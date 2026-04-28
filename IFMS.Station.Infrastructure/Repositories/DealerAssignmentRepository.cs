using IFMS.Station.Application.Interfaces;
using IFMS.Station.Domain.Entities;
using IFMS.Station.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IFMS.Station.Infrastructure.Repositories;

public class DealerAssignmentRepository : IDealerAssignmentRepository
{
    private readonly StationDbContext _context;
    
    public DealerAssignmentRepository(StationDbContext context)
    {
        _context = context;
    }
    
    public async Task<DealerAssignment?> GetByStationIdAsync(Guid stationId)
        => await _context.DealerAssignments
            .Include(da => da.Station)
            .FirstOrDefaultAsync(da => da.StationId == stationId);
    
    public async Task<bool> StationHasAssignmentAsync(Guid stationId)
        => await _context.DealerAssignments
            .AnyAsync(da => da.StationId == stationId);

    public async Task<IReadOnlyList<Guid>> GetStationIdsForUserAsync(Guid userId)
        => await _context.DealerAssignments
            .Where(da => da.UserId == userId)
            .Select(da => da.StationId)
            .ToListAsync();

    public async Task<bool> UserIsAssignedToStationAsync(Guid userId, Guid stationId)
        => await _context.DealerAssignments
            .AnyAsync(da => da.UserId == userId && da.StationId == stationId);
    
    public async Task AddAsync(DealerAssignment assignment)
        => await _context.DealerAssignments.AddAsync(assignment);

    public Task RemoveAsync(DealerAssignment assignment)
    {
        _context.DealerAssignments.Remove(assignment);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
