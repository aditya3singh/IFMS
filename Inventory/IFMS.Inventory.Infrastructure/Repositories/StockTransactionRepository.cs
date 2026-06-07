using IFMS.Inventory.Application.Interfaces;
using IFMS.Inventory.Domain.Entities;
using IFMS.Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IFMS.Inventory.Infrastructure.Repositories;

public class StockTransactionRepository : IStockTransactionRepository
{
    private readonly InventoryDbContext _context;

    public StockTransactionRepository(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<StockTransaction?> GetByIdAsync(Guid id)
        => await _context.StockTransactions
            .Include(st => st.FuelStock)
            .FirstOrDefaultAsync(st => st.Id == id);

    public async Task<List<StockTransaction>> GetByFuelStockIdAsync(Guid fuelStockId)
        => await _context.StockTransactions
            .Where(st => st.FuelStockId == fuelStockId)
            .OrderByDescending(st => st.CreatedAt)
            .ToListAsync();

    public async Task<List<StockTransaction>> GetByStationIdAsync(Guid stationId, int limit = 100)
        => await _context.StockTransactions
            .Where(st => st.StationId == stationId)
            .OrderByDescending(st => st.CreatedAt)
            .Take(limit)
            .ToListAsync();

    public async Task<List<StockTransaction>> GetBySaleTransactionIdAsync(Guid saleTransactionId)
        => await _context.StockTransactions
            .Where(st => st.SaleTransactionId == saleTransactionId)
            .ToListAsync();

    public async Task<List<StockTransaction>> GetByDeliveryIdAsync(Guid deliveryId)
        => await _context.StockTransactions
            .Where(st => st.DeliveryId == deliveryId)
            .ToListAsync();

    public async Task<List<StockTransaction>> GetRecentAsync(int days = 30, int limit = 1000)
    {
        var cutoff = DateTime.UtcNow.AddDays(-days);
        return await _context.StockTransactions
            .Where(st => st.CreatedAt >= cutoff)
            .OrderByDescending(st => st.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task AddAsync(StockTransaction transaction)
        => await _context.StockTransactions.AddAsync(transaction);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
