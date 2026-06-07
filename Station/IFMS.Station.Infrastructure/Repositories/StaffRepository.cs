using IFMS.Station.Application.Interfaces;
using IFMS.Station.Domain.Entities;
using IFMS.Station.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IFMS.Station.Infrastructure.Repositories;

public class StaffRepository : IStaffRepository
{
    private readonly StationDbContext _context;

    public StaffRepository(StationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<StaffMember>> GetByStationIdAsync(Guid stationId)
        => await _context.StaffMembers
            .Where(s => s.StationId == stationId)
            .OrderBy(s => s.Name)
            .ToListAsync();

    public async Task<StaffMember?> GetByIdAsync(Guid id)
        => await _context.StaffMembers.FirstOrDefaultAsync(s => s.Id == id);

    public async Task AddAsync(StaffMember member)
        => await _context.StaffMembers.AddAsync(member);

    public Task UpdateAsync(StaffMember member)
    {
        _context.StaffMembers.Update(member);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(StaffMember member)
    {
        _context.StaffMembers.Remove(member);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
