using IFMS.Inventory.Application.DTOs;
using IFMS.Inventory.Application.Interfaces;
using IFMS.Inventory.Domain.Entities;

namespace IFMS.Inventory.Application.Commands;

public class FuelStockCommandHandler
{
    private readonly IFuelStockRepository _repository;
    private readonly IStockTransactionRepository _transactionRepository;

    public FuelStockCommandHandler(
        IFuelStockRepository repository,
        IStockTransactionRepository transactionRepository)
    {
        _repository = repository;
        _transactionRepository = transactionRepository;
    }

    public async Task<FuelStockResponse> CreateAsync(CreateFuelStockRequest request, Guid? userId = null, string performedBy = "Dealer")
    {
        var existing = await _repository.GetByStationAndFuelTypeAsync(request.StationId, request.FuelType);
        if (existing != null)
        {
            // Treat "add stock" as an upsert to avoid unique constraint failures.
            var quantityBefore = existing.Quantity;
            existing.UpdatePrice(request.PricePerLitre);
            existing.UpdateStock(existing.Quantity + request.Quantity);
            await _repository.UpdateAsync(existing);
            
            // Log transaction
            var transaction = StockTransaction.Create(
                existing.Id,
                existing.StationId,
                existing.FuelType,
                "Addition",
                request.Quantity,
                quantityBefore,
                existing.Quantity,
                request.PricePerLitre,
                userId,
                performedBy,
                "Stock added via API"
            );
            await _transactionRepository.AddAsync(transaction);
            
            await _repository.SaveChangesAsync();
            await _transactionRepository.SaveChangesAsync();
            
            return MapToResponse(existing);
        }

        var created = FuelStock.Create(
            request.FuelType,
            request.Quantity,
            request.PricePerLitre,
            request.StationId);

        await _repository.AddAsync(created);
        await _repository.SaveChangesAsync();

        // Log initial stock transaction
        var initialTransaction = StockTransaction.Create(
            created.Id,
            created.StationId,
            created.FuelType,
            "Addition",
            request.Quantity,
            0,
            created.Quantity,
            request.PricePerLitre,
            userId,
            performedBy,
            "Initial stock creation"
        );
        await _transactionRepository.AddAsync(initialTransaction);
        await _transactionRepository.SaveChangesAsync();

        return MapToResponse(created);
    }

    public async Task<FuelStockResponse?> GetByIdAsync(Guid id)
    {
        var stock = await _repository.GetByIdAsync(id);
        return stock == null ? null : MapToResponse(stock);
    }

    public async Task<FuelStockResponse> UpdateStockAsync(UpdateStockRequest request, Guid? userId = null, string performedBy = "System", string? notes = null, Guid? saleTransactionId = null)
    {
        var stock = await _repository.GetByIdAsync(request.Id)
            ?? throw new InvalidOperationException("Fuel stock not found.");

        var quantityBefore = stock.Quantity;
        var quantityChange = request.NewQuantity - quantityBefore;
        var transactionType = quantityChange > 0 ? "Addition" : (quantityChange < 0 ? "Deduction" : "Adjustment");

        stock.UpdateStock(request.NewQuantity);
        await _repository.UpdateAsync(stock);
        
        // Log transaction
        var transaction = StockTransaction.Create(
            stock.Id,
            stock.StationId,
            stock.FuelType,
            transactionType,
            quantityChange,
            quantityBefore,
            stock.Quantity,
            stock.PricePerLitre,
            userId,
            performedBy,
            notes,
            saleTransactionId
        );
        await _transactionRepository.AddAsync(transaction);

        await _repository.SaveChangesAsync();
        await _transactionRepository.SaveChangesAsync();

        return MapToResponse(stock);
    }

    public async Task<List<FuelStockResponse>> GetAllAsync()
    {
        var stocks = await _repository.GetAllAsync();
        return stocks.Select(MapToResponse).ToList();
    }

    public async Task<List<FuelStockResponse>> GetByStationIdsAsync(IReadOnlyCollection<Guid> stationIds)
    {
        var stocks = await _repository.GetByStationIdsAsync(stationIds);
        return stocks.Select(MapToResponse).ToList();
    }

    public async Task<List<FuelStockResponse>> GetByStationAsync(Guid stationId)
    {
        var stocks = await _repository.GetByStationIdAsync(stationId);
        return stocks.Select(MapToResponse).ToList();
    }

    public async Task DeleteAsync(Guid id)
    {
        var stock = await _repository.GetByIdAsync(id)
            ?? throw new InvalidOperationException("Fuel stock not found.");
        await _repository.DeleteAsync(stock);
        await _repository.SaveChangesAsync();
    }

    public async Task<List<FuelStockResponse>> GetLowStockAsync(decimal threshold = 500)
    {
        var stocks = await _repository.GetLowStockAsync(threshold);
        return stocks.Select(MapToResponse).ToList();
    }

    private static FuelStockResponse MapToResponse(FuelStock stock) => new(
        stock.Id,
        stock.FuelType,
        stock.Quantity,
        stock.PricePerLitre,
        stock.Status,
        stock.StationId,
        stock.LastUpdated,
        stock.IsLowStock()
    );
}