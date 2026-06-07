using IFMS.Inventory.Domain.Entities;

namespace IFMS.Inventory.Application.Interfaces;

public interface IStockTransactionRepository
{
    Task<StockTransaction?> GetByIdAsync(Guid id);
    Task<List<StockTransaction>> GetByFuelStockIdAsync(Guid fuelStockId);
    Task<List<StockTransaction>> GetByStationIdAsync(Guid stationId, int limit = 100);
    Task<List<StockTransaction>> GetBySaleTransactionIdAsync(Guid saleTransactionId);
    Task<List<StockTransaction>> GetByDeliveryIdAsync(Guid deliveryId);
    Task<List<StockTransaction>> GetRecentAsync(int days = 30, int limit = 1000);
    Task AddAsync(StockTransaction transaction);
    Task SaveChangesAsync();
}
