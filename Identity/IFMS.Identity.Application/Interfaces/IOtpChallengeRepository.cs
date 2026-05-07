using IFMS.Identity.Domain.Entities;

namespace IFMS.Identity.Application.Interfaces;

public interface IOtpChallengeRepository
{
    Task RemoveForKeyAndPurposeAsync(string normalizedKey, string purpose);
    Task AddAsync(OtpChallenge challenge);
    Task<OtpChallenge?> GetLatestAsync(string normalizedKey, string purpose);
    Task RemoveAsync(Guid id);
    Task SaveChangesAsync();
}
