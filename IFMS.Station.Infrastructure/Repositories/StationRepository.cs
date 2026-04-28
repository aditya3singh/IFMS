using IFMS.Station.Application.Interfaces;
using IFMS.Station.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IFMS.Station.Infrastructure.Repositories;

public class StationRepository : IStationRepository
{
    private readonly StationDbContext _context;
    
    public StationRepository(StationDbContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<Domain.Entities.Station>> GetAllAsync()
        => await _context.Stations
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync();

    public async Task<IReadOnlyList<Domain.Entities.Station>> GetByIdsAsync(IReadOnlyCollection<Guid> ids)
    {
        if (ids == null || ids.Count == 0)
            return Array.Empty<Domain.Entities.Station>();

        return await _context.Stations
            .Include(s => s.DealerAssignment)
            .Where(s => s.IsActive && ids.Contains(s.Id))
            .OrderBy(s => s.Name)
            .ToListAsync();
    }
    
    public async Task<Domain.Entities.Station?> GetByIdAsync(Guid id)
        => await _context.Stations
            .Include(s => s.DealerAssignment)
            .FirstOrDefaultAsync(s => s.Id == id && s.IsActive);
    
    public async Task<Domain.Entities.Station?> GetByLicenseNumberAsync(string licenseNumber)
        => await _context.Stations
            .FirstOrDefaultAsync(s => s.LicenseNumber == licenseNumber);
    
    public async Task<bool> LicenseNumberExistsAsync(string licenseNumber, Guid? excludeStationId = null)
    {
        var query = _context.Stations.Where(s => s.LicenseNumber == licenseNumber);
        if (excludeStationId.HasValue)
            query = query.Where(s => s.Id != excludeStationId.Value);
        return await query.AnyAsync();
    }
    
    public async Task AddAsync(Domain.Entities.Station station)
        => await _context.Stations.AddAsync(station);
    
    public Task UpdateAsync(Domain.Entities.Station station)
    {
        _context.Stations.Update(station);
        return Task.CompletedTask;
    }
    
    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
