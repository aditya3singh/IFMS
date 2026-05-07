using IFMS.Messaging.Events;
using IFMS.Sales.Application.DTOs;
using IFMS.Sales.Application.Interfaces;
using IFMS.Sales.Domain.Entities;
using MassTransit;
using System.Net.Http.Json;

namespace IFMS.Sales.Application.Commands;

public class TransactionCommandHandler
{
    private readonly ITransactionRepository _repository;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IPublishEndpoint _publish;

    public TransactionCommandHandler(
        ITransactionRepository repository,
        IHttpClientFactory httpClientFactory,
        IPublishEndpoint publish)
    {
        _repository = repository;
        _httpClientFactory = httpClientFactory;
        _publish = publish;
    }

    public async Task<TransactionResponse> CreateAsync(CreateTransactionRequest request)
    {
        // First, deduct inventory
        await DeductInventoryAsync(request.StationId, request.FuelType, request.Quantity);

        var transaction = Transaction.Create(
            request.StationId,
            request.FuelType,
            request.Quantity,
            request.PricePerLitre,
            request.PaymentMethod,
            request.CustomerName);

        await _repository.AddAsync(transaction);
        await _repository.SaveChangesAsync();

        // Publish SaleRecorded event → Notification API consumes via RabbitMQ
        _ = _publish.Publish(new SaleRecorded(
            TransactionId:   transaction.Id,
            StationId:       transaction.StationId,
            FuelType:        transaction.FuelType,
            Quantity:        transaction.Quantity,
            TotalAmount:     transaction.TotalAmount,
            CustomerName:    transaction.CustomerName,
            TransactionDate: transaction.TransactionDate
        ));

        return MapToResponse(transaction);
    }

    private async Task DeductInventoryAsync(Guid stationId, string fuelType, decimal quantity)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("InventoryAPI");
            
            // Get current inventory for this station and fuel type
            var inventoryResponse = await httpClient.GetAsync($"/api/Inventory/station/{stationId}");
            
            if (!inventoryResponse.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Failed to fetch inventory: {inventoryResponse.StatusCode}");
            }

            var inventoryList = await inventoryResponse.Content.ReadFromJsonAsync<List<InventoryStockDto>>();
            var currentStock = inventoryList?.FirstOrDefault(s => s.FuelType.Equals(fuelType, StringComparison.OrdinalIgnoreCase));

            if (currentStock == null)
            {
                throw new InvalidOperationException($"No inventory found for fuel type {fuelType} at station {stationId}");
            }

            if (currentStock.Quantity < quantity)
            {
                throw new InvalidOperationException($"Insufficient inventory. Available: {currentStock.Quantity}L, Required: {quantity}L");
            }

            // Deduct the quantity
            var newQuantity = currentStock.Quantity - quantity;
            var updateRequest = new
            {
                id = currentStock.Id,
                newQuantity = newQuantity
            };

            var updateResponse = await httpClient.PutAsJsonAsync("/api/Inventory/internal/deduct", updateRequest);
            
            if (!updateResponse.IsSuccessStatusCode)
            {
                var errorContent = await updateResponse.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Failed to update inventory: {updateResponse.StatusCode} - {errorContent}");
            }
        }
        catch (Exception ex)
        {
            // Log and rethrow - inventory deduction is critical
            Console.WriteLine($"Error deducting inventory: {ex.Message}");
            throw new InvalidOperationException($"Failed to deduct inventory: {ex.Message}", ex);
        }
    }

    // Helper DTO for inventory response
    private class InventoryStockDto
    {
        public Guid Id { get; set; }
        public string FuelType { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal PricePerLitre { get; set; }
        public string Status { get; set; } = string.Empty;
        public Guid StationId { get; set; }
        public DateTime LastUpdated { get; set; }
        public bool IsLowStock { get; set; }
    }

    public async Task<List<TransactionResponse>> GetAllAsync()
    {
        var transactions = await _repository.GetAllAsync();
        return transactions.Select(MapToResponse).ToList();
    }

    public async Task<List<TransactionResponse>> GetByStationIdsAsync(IReadOnlyCollection<Guid> stationIds)
    {
        var transactions = await _repository.GetByStationIdsAsync(stationIds);
        return transactions.Select(MapToResponse).ToList();
    }

    public async Task<List<TransactionResponse>> GetByStationAsync(Guid stationId)
    {
        var transactions = await _repository.GetByStationIdAsync(stationId);
        return transactions.Select(MapToResponse).ToList();
    }

    public async Task<List<TransactionResponse>> GetByDateRangeAsync(DateTime from, DateTime to)
    {
        var transactions = await _repository.GetByDateRangeAsync(from, to);
        return transactions.Select(MapToResponse).ToList();
    }

    public async Task<List<TransactionResponse>> GetByDateRangeForStationsAsync(
        DateTime from,
        DateTime to,
        IReadOnlyCollection<Guid>? stationIds)
    {
        var transactions = await _repository.GetByDateRangeAsync(from, to, stationIds);
        return transactions.Select(MapToResponse).ToList();
    }

    public async Task<decimal> GetTotalRevenueAsync(Guid stationId)
    {
        var transactions = await _repository.GetByStationIdAsync(stationId);
        return transactions.Sum(t => t.TotalAmount);
    }

    public async Task<TransactionResponse?> GetByIdAsync(Guid id)
    {
        var t = await _repository.GetByIdAsync(id);
        return t == null ? null : MapToResponse(t);
    }

    public async Task<object> GetSalesSummaryAsync(IReadOnlyCollection<Guid> stationIds)
    {
        var transactions = await _repository.GetByStationIdsAsync(stationIds);
        return new
        {
            totalTransactions = transactions.Count,
            totalRevenue = transactions.Sum(t => t.TotalAmount),
            totalLitres = transactions.Sum(t => t.Quantity),
            byFuelType = transactions
                .GroupBy(t => t.FuelType)
                .Select(g => new
                {
                    fuelType = g.Key,
                    transactions = g.Count(),
                    litres = g.Sum(x => x.Quantity),
                    revenue = g.Sum(x => x.TotalAmount)
                })
                .OrderByDescending(x => x.revenue)
                .ToList(),
            byPaymentMethod = transactions
                .GroupBy(t => t.PaymentMethod)
                .Select(g => new
                {
                    method = g.Key,
                    count = g.Count(),
                    revenue = g.Sum(x => x.TotalAmount)
                })
                .OrderByDescending(x => x.revenue)
                .ToList()
        };
    }

    public async Task<List<object>> GetRevenueTrendAsync(IReadOnlyCollection<Guid> stationIds, DateTime from, DateTime to, string groupBy = "day")
    {
        var transactions = await _repository.GetByDateRangeAsync(from, to, stationIds);

        IEnumerable<IGrouping<string, IFMS.Sales.Domain.Entities.Transaction>> grouped = groupBy.ToLower() switch
        {
            "month" => transactions.GroupBy(t => t.TransactionDate.ToString("yyyy-MM")),
            "week"  => transactions.GroupBy(t => $"{t.TransactionDate.Year}-W{System.Globalization.ISOWeek.GetWeekOfYear(t.TransactionDate):D2}"),
            _       => transactions.GroupBy(t => t.TransactionDate.ToString("yyyy-MM-dd"))
        };

        return grouped
            .Select(g => (object)new
            {
                period = g.Key,
                transactions = g.Count(),
                revenue = g.Sum(x => x.TotalAmount),
                litres = g.Sum(x => x.Quantity)
            })
            .OrderBy(x => ((dynamic)x).period)
            .ToList();
    }

    public async Task<List<object>> GetExportAsync(IReadOnlyCollection<Guid> stationIds, DateTime from, DateTime to)
    {
        var transactions = await _repository.GetByDateRangeAsync(from, to, stationIds);
        return transactions.Select(t => (object)new
        {
            t.Id, t.StationId, t.FuelType, t.Quantity,
            t.PricePerLitre, t.TotalAmount, t.PaymentMethod,
            t.Status, t.TransactionDate, t.CustomerName
        }).ToList();
    }

    private static TransactionResponse MapToResponse(Transaction t) => new(
        t.Id,
        t.StationId,
        t.FuelType,
        t.Quantity,
        t.PricePerLitre,
        t.TotalAmount,
        t.PaymentMethod,
        t.Status,
        t.TransactionDate,
        t.CustomerName
    );
}