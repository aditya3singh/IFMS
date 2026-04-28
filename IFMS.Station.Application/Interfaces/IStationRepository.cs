using IFMS.Station.Domain.Entities;

namespace IFMS.Station.Application.Interfaces;

public interface IStationRepository
{
    Task<IEnumerable<Domain.Entities.Station>> GetAllAsync();
    Task<IReadOnlyList<Domain.Entities.Station>> GetByIdsAsync(IReadOnlyCollection<Guid> ids);
    Task<Domain.Entities.Station?> GetByIdAsync(Guid id);
    Task<Domain.Entities.Station?> GetByLicenseNumberAsync(string licenseNumber);
    Task<bool> LicenseNumberExistsAsync(string licenseNumber, Guid? excludeStationId = null);
    Task AddAsync(Domain.Entities.Station station);
    Task UpdateAsync(Domain.Entities.Station station);
    Task SaveChangesAsync();
}
