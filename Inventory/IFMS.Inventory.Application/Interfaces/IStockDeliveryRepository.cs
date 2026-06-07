using IFMS.Inventory.Domain.Entities;

namespace IFMS.Inventory.Application.Interfaces;

public interface IStockDeliveryRepository
{
    Task<StockDelivery?> GetByIdAsync(Guid id);
    Task<List<StockDelivery>> GetAllAsync();
    Task<List<StockDelivery>> GetByStationIdAsync(Guid stationId);
    Task<List<StockDelivery>> GetBySupplerIdAsync(Guid supplierId);
    Task<List<StockDelivery>> GetByStatusAsync(string status);
    Task<List<StockDelivery>> GetUpcomingAsync(int days = 7);
    Task AddAsync(StockDelivery delivery);
    Task UpdateAsync(StockDelivery delivery);
    Task DeleteAsync(StockDelivery delivery);
    Task SaveChangesAsync();
}
