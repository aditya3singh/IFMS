using IFMS.Inventory.Application.Interfaces;
using IFMS.Inventory.Domain.Entities;
using IFMS.Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IFMS.Inventory.Infrastructure.Repositories;

public class FuelStockRepository : IFuelStockRepository
{
    private readonly InventoryDbContext _context;

    public FuelStockRepository(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<FuelStock?> GetByIdAsync(Guid id)
        => await _context.FuelStocks.FirstOrDefaultAsync(f => f.Id == id);

    public async Task<FuelStock?> GetByStationAndFuelTypeAsync(Guid stationId, string fuelType)
        => await _context.FuelStocks.FirstOrDefaultAsync(f => f.StationId == stationId && f.FuelType == fuelType);

    public async Task<List<FuelStock>> GetAllAsync()
        => await _context.FuelStocks.ToListAsync();

    public async Task<List<FuelStock>> GetByStationIdsAsync(IReadOnlyCollection<Guid> stationIds)
    {
        if (stationIds == null || stationIds.Count == 0)
            return new List<FuelStock>();

        return await _context.FuelStocks.Where(f => stationIds.Contains(f.StationId)).ToListAsync();
    }

    public async Task<List<FuelStock>> GetByStationIdAsync(Guid stationId)
        => await _context.FuelStocks.Where(f => f.StationId == stationId).ToListAsync();

    public async Task AddAsync(FuelStock fuelStock)
        => await _context.FuelStocks.AddAsync(fuelStock);

    public Task UpdateAsync(FuelStock fuelStock)
    {
        _context.FuelStocks.Update(fuelStock);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(FuelStock fuelStock)
    {
        _context.FuelStocks.Remove(fuelStock);
        return Task.CompletedTask;
    }

    public async Task<List<FuelStock>> GetLowStockAsync(decimal threshold = 500)
        => await _context.FuelStocks.Where(f => f.Quantity < threshold).ToListAsync();

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}