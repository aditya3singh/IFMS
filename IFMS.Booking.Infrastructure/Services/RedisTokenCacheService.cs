using System.Text.Json;
using IFMS.Booking.Application.Interfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace IFMS.Booking.Infrastructure.Services;

public class RedisTokenCacheService : ITokenCacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisTokenCacheService> _logger;

    public RedisTokenCacheService(IConnectionMultiplexer redis, ILogger<RedisTokenCacheService> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task StoreTokenAsync(string tokenCode, object bookingDetails, TimeSpan expiry)
    {
        var db = _redis.GetDatabase();
        var key = $"booking:token:{tokenCode}";
        var value = JsonSerializer.Serialize(bookingDetails);

        await db.StringSetAsync(key, value, expiry);
        _logger.LogInformation("Token {TokenCode} stored in Redis with {Hours}h TTL", tokenCode, expiry.TotalHours);
    }

    public async Task<string?> GetTokenAsync(string tokenCode)
    {
        var db = _redis.GetDatabase();
        var key = $"booking:token:{tokenCode}";
        var value = await db.StringGetAsync(key);

        if (value.IsNull)
        {
            _logger.LogInformation("Token {TokenCode} not found in Redis (expired or invalid)", tokenCode);
            return null;
        }

        return value.ToString();
    }

    public async Task DeleteTokenAsync(string tokenCode)
    {
        var db = _redis.GetDatabase();
        var key = $"booking:token:{tokenCode}";
        await db.KeyDeleteAsync(key);
        _logger.LogInformation("Token {TokenCode} deleted from Redis", tokenCode);
    }
}
