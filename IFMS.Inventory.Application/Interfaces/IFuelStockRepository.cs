using IFMS.Inventory.Domain.Entities;

namespace IFMS.Inventory.Application.Interfaces;

public interface IFuelStockRepository
{
    Task<FuelStock?> GetByIdAsync(Guid id);
    Task<FuelStock?> GetByStationAndFuelTypeAsync(Guid stationId, string fuelType);
    Task<List<FuelStock>> GetAllAsync();
    Task<List<FuelStock>> GetByStationIdsAsync(IReadOnlyCollection<Guid> stationIds);
    Task<List<FuelStock>> GetByStationIdAsync(Guid stationId);
    Task AddAsync(FuelStock fuelStock);
    Task UpdateAsync(FuelStock fuelStock);
    Task DeleteAsync(FuelStock fuelStock);
    Task<List<FuelStock>> GetLowStockAsync(decimal threshold = 500);
    Task SaveChangesAsync();
}