using IFMS.Inventory.Application.Interfaces;
using IFMS.Inventory.Domain.Entities;
using IFMS.Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IFMS.Inventory.Infrastructure.Repositories;

public class StockDeliveryRepository : IStockDeliveryRepository
{
    private readonly InventoryDbContext _context;

    public StockDeliveryRepository(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<StockDelivery?> GetByIdAsync(Guid id)
        => await _context.StockDeliveries
            .Include(sd => sd.Supplier)
            .FirstOrDefaultAsync(sd => sd.Id == id);

    public async Task<List<StockDelivery>> GetAllAsync()
        => await _context.StockDeliveries
            .Include(sd => sd.Supplier)
            .OrderByDescending(sd => sd.CreatedAt)
            .ToListAsync();

    public async Task<List<StockDelivery>> GetByStationIdAsync(Guid stationId)
        => await _context.StockDeliveries
            .Include(sd => sd.Supplier)
            .Where(sd => sd.StationId == stationId)
            .OrderByDescending(sd => sd.ScheduledDate)
            .ToListAsync();

    public async Task<List<StockDelivery>> GetBySupplerIdAsync(Guid supplierId)
        => await _context.StockDeliveries
            .Include(sd => sd.Supplier)
            .Where(sd => sd.SupplierId == supplierId)
            .OrderByDescending(sd => sd.ScheduledDate)
            .ToListAsync();

    public async Task<List<StockDelivery>> GetByStatusAsync(string status)
        => await _context.StockDeliveries
            .Include(sd => sd.Supplier)
            .Where(sd => sd.Status == status)
            .OrderBy(sd => sd.ScheduledDate)
            .ToListAsync();

    public async Task<List<StockDelivery>> GetUpcomingAsync(int days = 7)
    {
        var end = DateTime.UtcNow.AddDays(days);
        return await _context.StockDeliveries
            .Include(sd => sd.Supplier)
            .Where(sd => sd.Status == "Scheduled" && sd.ScheduledDate <= end)
            .OrderBy(sd => sd.ScheduledDate)
            .ToListAsync();
    }

    public async Task AddAsync(StockDelivery delivery)
        => await _context.StockDeliveries.AddAsync(delivery);

    public Task UpdateAsync(StockDelivery delivery)
    {
        _context.StockDeliveries.Update(delivery);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(StockDelivery delivery)
    {
        _context.StockDeliveries.Remove(delivery);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
