using IFMS.Booking.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace IFMS.Booking.Infrastructure.Services;

public class MemoryKycSessionStore : IKycSessionStore
{
    private readonly IMemoryCache _cache;

    public MemoryKycSessionStore(IMemoryCache cache) => _cache = cache;

    private sealed record SessionEntry(Guid CustomerId, string ReferenceId);

    public Task<string> CreateSessionAsync(Guid customerId, string verificationReferenceId, TimeSpan ttl,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var sessionId = Guid.NewGuid().ToString("N");
        var key = CacheKey(sessionId);
        _cache.Set(key, new SessionEntry(customerId, verificationReferenceId),
            new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl });
        return Task.FromResult(sessionId);
    }

    public Task<bool> TryConsumeAsync(string sessionId, Guid customerId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var key = CacheKey(sessionId);
        if (!_cache.TryGetValue(key, out SessionEntry? entry) || entry is null)
            return Task.FromResult(false);

        if (entry.CustomerId != customerId)
            return Task.FromResult(false);

        _cache.Remove(key);
        return Task.FromResult(true);
    }

    private static string CacheKey(string sessionId) => $"kyc-session:{sessionId}";
}
