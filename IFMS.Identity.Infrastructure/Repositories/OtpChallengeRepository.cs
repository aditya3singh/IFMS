using IFMS.Identity.Application.Interfaces;
using IFMS.Identity.Domain.Entities;
using IFMS.Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IFMS.Identity.Infrastructure.Repositories;

public class OtpChallengeRepository : IOtpChallengeRepository
{
    private readonly IdentityDbContext _db;

    public OtpChallengeRepository(IdentityDbContext db)
    {
        _db = db;
    }

    public async Task RemoveForKeyAndPurposeAsync(string normalizedKey, string purpose)
    {
        await _db.OtpChallenges
            .Where(o => o.NormalizedKey == normalizedKey && o.Purpose == purpose)
            .ExecuteDeleteAsync();
    }

    public async Task AddAsync(OtpChallenge challenge)
        => await _db.OtpChallenges.AddAsync(challenge);

    public async Task<OtpChallenge?> GetLatestAsync(string normalizedKey, string purpose)
        => await _db.OtpChallenges
            .Where(o => o.NormalizedKey == normalizedKey && o.Purpose == purpose)
            .OrderByDescending(o => o.CreatedAtUtc)
            .FirstOrDefaultAsync();

    public async Task RemoveAsync(Guid id)
    {
        var row = await _db.OtpChallenges.FindAsync(id);
        if (row != null)
            _db.OtpChallenges.Remove(row);
    }

    public Task SaveChangesAsync() => _db.SaveChangesAsync();
}
