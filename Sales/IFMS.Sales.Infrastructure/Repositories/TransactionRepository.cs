using IFMS.Sales.Application.Interfaces;
using IFMS.Sales.Domain.Entities;
using IFMS.Sales.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IFMS.Sales.Infrastructure.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly SalesDbContext _context;

    public TransactionRepository(SalesDbContext context)
    {
        _context = context;
    }

    public async Task<Transaction?> GetByIdAsync(Guid id)
        => await _context.Transactions.FirstOrDefaultAsync(t => t.Id == id);

    public async Task<List<Transaction>> GetAllAsync()
        => await _context.Transactions.OrderByDescending(t => t.TransactionDate).ToListAsync();

    public async Task<List<Transaction>> GetByStationIdsAsync(IReadOnlyCollection<Guid> stationIds)
    {
        if (stationIds == null || stationIds.Count == 0)
            return new List<Transaction>();

        return await _context.Transactions
            .Where(t => stationIds.Contains(t.StationId))
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
    }

    public async Task<List<Transaction>> GetByStationIdAsync(Guid stationId)
        => await _context.Transactions.Where(t => t.StationId == stationId).ToListAsync();

    public async Task<List<Transaction>> GetByDateRangeAsync(DateTime from, DateTime to)
        => await GetByDateRangeAsync(from, to, null);

    public async Task<List<Transaction>> GetByDateRangeAsync(DateTime from, DateTime to, IReadOnlyCollection<Guid>? stationIds)
    {
        var q = _context.Transactions.Where(t => t.TransactionDate >= from && t.TransactionDate <= to);
        if (stationIds != null && stationIds.Count > 0)
            q = q.Where(t => stationIds.Contains(t.StationId));
        return await q.OrderByDescending(t => t.TransactionDate).ToListAsync();
    }

    public async Task AddAsync(Transaction transaction)
        => await _context.Transactions.AddAsync(transaction);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}