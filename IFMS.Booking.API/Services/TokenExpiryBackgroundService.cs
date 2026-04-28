using IFMS.Booking.Application.Commands;

namespace IFMS.Booking.API.Services;

/// <summary>
/// Background job that runs every 5 minutes to find PENDING bookings
/// past their 24h expiry and marks them as EXPIRED in the database.
/// Redis keys auto-expire via TTL, but DB status needs explicit update.
/// </summary>
public class TokenExpiryBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TokenExpiryBackgroundService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5);

    public TokenExpiryBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<TokenExpiryBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Token Expiry Background Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<BookingCommandHandler>();

                var expiredCount = await handler.ExpirePendingBookingsAsync();

                if (expiredCount > 0)
                {
                    _logger.LogInformation(
                        "Token Expiry Job: Marked {Count} booking(s) as EXPIRED", expiredCount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Token Expiry Background Service");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("Token Expiry Background Service stopped");
    }
}
