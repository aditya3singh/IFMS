using IFMS.Sales.Domain.Entities;

namespace IFMS.Sales.Application.Interfaces;

public interface ITransactionRepository
{
    Task<Transaction?> GetByIdAsync(Guid id);
    Task<List<Transaction>> GetAllAsync();
    Task<List<Transaction>> GetByStationIdsAsync(IReadOnlyCollection<Guid> stationIds);
    Task<List<Transaction>> GetByStationIdAsync(Guid stationId);
    Task<List<Transaction>> GetByDateRangeAsync(DateTime from, DateTime to);
    Task<List<Transaction>> GetByDateRangeAsync(DateTime from, DateTime to, IReadOnlyCollection<Guid>? stationIds);
    Task AddAsync(Transaction transaction);
    Task SaveChangesAsync();
}