using IFMS.Booking.Application.Commands;

namespace IFMS.Booking.API.Services;

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
