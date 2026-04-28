using IFMS.Station.Domain.Entities;

namespace IFMS.Station.Application.Interfaces;

public interface IDealerAssignmentRepository
{
    Task<DealerAssignment?> GetByStationIdAsync(Guid stationId);
    Task<bool> StationHasAssignmentAsync(Guid stationId);
    Task<IReadOnlyList<Guid>> GetStationIdsForUserAsync(Guid userId);
    Task<bool> UserIsAssignedToStationAsync(Guid userId, Guid stationId);
    Task AddAsync(DealerAssignment assignment);
    Task RemoveAsync(DealerAssignment assignment);
    Task SaveChangesAsync();
}
