using IFMS.Station.Domain.Entities;

namespace IFMS.Station.Application.Interfaces;

public interface IStaffRepository
{
    Task<IReadOnlyList<StaffMember>> GetByStationIdAsync(Guid stationId);
    Task<StaffMember?> GetByIdAsync(Guid id);
    Task AddAsync(StaffMember member);
    Task UpdateAsync(StaffMember member);
    Task RemoveAsync(StaffMember member);
    Task SaveChangesAsync();
}
