using IFMS.Inventory.Application.DTOs;
using IFMS.Inventory.Application.Interfaces;
using IFMS.Inventory.Domain.Entities;

namespace IFMS.Inventory.Application.Commands;

public class FuelStockCommandHandler
{
    private readonly IFuelStockRepository _repository;

    public FuelStockCommandHandler(IFuelStockRepository repository)
    {
        _repository = repository;
    }

    public async Task<FuelStockResponse> CreateAsync(CreateFuelStockRequest request)
    {
        var existing = await _repository.GetByStationAndFuelTypeAsync(request.StationId, request.FuelType);
        if (existing != null)
        {
            // Treat "add stock" as an upsert to avoid unique constraint failures.
            existing.UpdatePrice(request.PricePerLitre);
            existing.UpdateStock(existing.Quantity + request.Quantity);
            await _repository.UpdateAsync(existing);
            await _repository.SaveChangesAsync();
            return MapToResponse(existing);
        }

        var created = FuelStock.Create(
            request.FuelType,
            request.Quantity,
            request.PricePerLitre,
            request.StationId);

        await _repository.AddAsync(created);
        await _repository.SaveChangesAsync();

        return MapToResponse(created);
    }

    public async Task<FuelStockResponse?> GetByIdAsync(Guid id)
    {
        var stock = await _repository.GetByIdAsync(id);
        return stock == null ? null : MapToResponse(stock);
    }

    public async Task<FuelStockResponse> UpdateStockAsync(UpdateStockRequest request)
    {
        var stock = await _repository.GetByIdAsync(request.Id)
            ?? throw new InvalidOperationException("Fuel stock not found.");

        stock.UpdateStock(request.NewQuantity);
        await _repository.UpdateAsync(stock);
        await _repository.SaveChangesAsync();

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