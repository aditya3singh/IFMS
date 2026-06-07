using IFMS.Inventory.Application.DTOs;
using IFMS.Inventory.Application.Interfaces;
using IFMS.Inventory.Domain.Entities;

namespace IFMS.Inventory.Application.Commands;

public class StockDeliveryCommandHandler
{
    private readonly IStockDeliveryRepository _repository;
    private readonly ISupplierRepository _supplierRepository;
    private readonly IFuelStockRepository _stockRepository;
    private readonly IStockTransactionRepository _transactionRepository;

    public StockDeliveryCommandHandler(
        IStockDeliveryRepository repository,
        ISupplierRepository supplierRepository,
        IFuelStockRepository stockRepository,
        IStockTransactionRepository transactionRepository)
    {
        _repository = repository;
        _supplierRepository = supplierRepository;
        _stockRepository = stockRepository;
        _transactionRepository = transactionRepository;
    }

    public async Task<StockDeliveryResponse> CreateAsync(CreateStockDeliveryRequest request, Guid createdByUserId)
    {
        // Verify supplier exists
        var supplier = await _supplierRepository.GetByIdAsync(request.SupplierId)
            ?? throw new InvalidOperationException("Supplier not found.");

        var delivery = StockDelivery.Create(
            request.StationId,
            request.SupplierId,
            request.FuelType,
            request.Quantity,
            request.PricePerLitre,
            request.ScheduledDate,
            createdByUserId,
            request.Notes
        );

        await _repository.AddAsync(delivery);
        await _repository.SaveChangesAsync();

        return MapToResponse(delivery, supplier.Name);
    }

    public async Task<StockDeliveryResponse?> GetByIdAsync(Guid id)
    {
        var delivery = await _repository.GetByIdAsync(id);
        return delivery == null ? null : MapToResponse(delivery, delivery.Supplier?.Name ?? "Unknown");
    }

    public async Task<List<StockDeliveryResponse>> GetAllAsync()
    {
        var deliveries = await _repository.GetAllAsync();
        return deliveries.Select(d => MapToResponse(d, d.Supplier?.Name ?? "Unknown")).ToList();
    }

    public async Task<List<StockDeliveryResponse>> GetByStationIdAsync(Guid stationId)
    {
        var deliveries = await _repository.GetByStationIdAsync(stationId);
        return deliveries.Select(d => MapToResponse(d, d.Supplier?.Name ?? "Unknown")).ToList();
    }

    public async Task<List<StockDeliveryResponse>> GetByStatusAsync(string status)
    {
        var deliveries = await _repository.GetByStatusAsync(status);
        return deliveries.Select(d => MapToResponse(d, d.Supplier?.Name ?? "Unknown")).ToList();
    }

    public async Task<List<StockDeliveryResponse>> GetUpcomingAsync(int days = 7)
    {
        var deliveries = await _repository.GetUpcomingAsync(days);
        return deliveries.Select(d => MapToResponse(d, d.Supplier?.Name ?? "Unknown")).ToList();
    }

    public async Task<StockDeliveryResponse> UpdateAsync(Guid id, UpdateStockDeliveryRequest request)
    {
        var delivery = await _repository.GetByIdAsync(id)
            ?? throw new InvalidOperationException("Delivery not found.");

        delivery.Update(
            request.Quantity,
            request.PricePerLitre,
            request.ScheduledDate,
            request.Notes
        );

        await _repository.UpdateAsync(delivery);
        await _repository.SaveChangesAsync();

        return MapToResponse(delivery, delivery.Supplier?.Name ?? "Unknown");
    }

    public async Task<StockDeliveryResponse> UpdateStatusAsync(Guid id, UpdateDeliveryStatusRequest request, Guid? deliveredByUserId = null)
    {
        var delivery = await _repository.GetByIdAsync(id)
            ?? throw new InvalidOperationException("Delivery not found.");

        delivery.UpdateStatus(request.Status, deliveredByUserId);

        // If marked as delivered, automatically add stock and log transaction
        if (request.Status == "Delivered")
        {
            await ProcessDeliveredStockAsync(delivery, deliveredByUserId);
        }

        await _repository.UpdateAsync(delivery);
        await _repository.SaveChangesAsync();

        return MapToResponse(delivery, delivery.Supplier?.Name ?? "Unknown");
    }

    public async Task DeleteAsync(Guid id)
    {
        var delivery = await _repository.GetByIdAsync(id)
            ?? throw new InvalidOperationException("Delivery not found.");

        if (delivery.Status == "Delivered")
            throw new InvalidOperationException("Cannot delete a delivered delivery.");

        await _repository.DeleteAsync(delivery);
        await _repository.SaveChangesAsync();
    }

    private async Task ProcessDeliveredStockAsync(StockDelivery delivery, Guid? deliveredByUserId)
    {
        // Find or create fuel stock for this station and fuel type
        var stock = await _stockRepository.GetByStationAndFuelTypeAsync(delivery.StationId, delivery.FuelType);
        
        if (stock == null)
        {
            // Create new stock entry
            stock = FuelStock.Create(
                delivery.FuelType,
                delivery.Quantity,
                delivery.PricePerLitre,
                delivery.StationId
            );
            await _stockRepository.AddAsync(stock);
            await _stockRepository.SaveChangesAsync();

            // Log initial transaction
            var initialTransaction = StockTransaction.Create(
                stock.Id,
                stock.StationId,
                stock.FuelType,
                "Delivery",
                delivery.Quantity,
                0,
                stock.Quantity,
                delivery.PricePerLitre,
                deliveredByUserId,
                "Dealer",
                $"Initial stock from delivery {delivery.Id}",
                null,
                delivery.Id
            );
            await _transactionRepository.AddAsync(initialTransaction);
        }
        else
        {
            // Update existing stock
            var quantityBefore = stock.Quantity;
            stock.UpdatePrice(delivery.PricePerLitre);
            stock.UpdateStock(stock.Quantity + delivery.Quantity);
            await _stockRepository.UpdateAsync(stock);

            // Log transaction
            var transaction = StockTransaction.Create(
                stock.Id,
                stock.StationId,
                stock.FuelType,
                "Delivery",
                delivery.Quantity,
                quantityBefore,
                stock.Quantity,
                delivery.PricePerLitre,
                deliveredByUserId,
                "Dealer",
                $"Stock delivered from supplier {delivery.Supplier?.Name ?? "Unknown"}",
                null,
                delivery.Id
            );
            await _transactionRepository.AddAsync(transaction);
        }

        await _stockRepository.SaveChangesAsync();
        await _transactionRepository.SaveChangesAsync();
    }

    private static StockDeliveryResponse MapToResponse(StockDelivery delivery, string supplierName) => new(
        delivery.Id,
        delivery.StationId,
        delivery.SupplierId,
        supplierName,
        delivery.FuelType,
        delivery.Quantity,
        delivery.PricePerLitre,
        delivery.TotalAmount,
        delivery.Status,
        delivery.ScheduledDate,
        delivery.ActualDeliveryDate,
        delivery.Notes,
        delivery.CreatedByUserId,
        delivery.DeliveredByUserId,
        delivery.CreatedAt,
        delivery.UpdatedAt
    );
}
