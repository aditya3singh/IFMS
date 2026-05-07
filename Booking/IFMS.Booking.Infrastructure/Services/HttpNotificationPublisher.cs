using System.Net.Http.Json;
using IFMS.Booking.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace IFMS.Booking.Infrastructure.Services;

/// <summary>
/// Calls the Notification API over HTTP.
/// All failures are swallowed so booking operations are never blocked.
/// </summary>
public class HttpNotificationPublisher : INotificationPublisher
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<HttpNotificationPublisher> _logger;

    public HttpNotificationPublisher(
        IHttpClientFactory httpFactory,
        IConfiguration config,
        ILogger<HttpNotificationPublisher> logger)
    {
        _httpFactory = httpFactory;
        _config = config;
        _logger = logger;
    }

    public async Task SendBookingConfirmedAsync(
        string customerEmail,
        string customerPhone,
        string customerName,
        string tokenCode,
        string stationName,
        string fuelType,
        decimal quantityLiters,
        decimal totalPaid,
        CancellationToken ct = default)
    {
        try
        {
            var client = BuildClient();
            var resp = await client.PostAsJsonAsync("/api/notify/token", new
            {
                customerEmail,
                customerPhone,
                customerName,
                tokenCode,
                stationName,
                fuelType,
                quantity = quantityLiters,
                totalPaid
            }, ct);

            if (!resp.IsSuccessStatusCode)
                _logger.LogWarning("Booking token notification failed: {Status}", (int)resp.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Booking token notification error (non-fatal)");
        }
    }

    public async Task SendSmsAsync(string toPhone, string message, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(toPhone))
        {
            _logger.LogDebug("SendSmsAsync skipped — no phone number provided");
            return;
        }

        try
        {
            var client = BuildClient();
            var resp = await client.PostAsJsonAsync("/api/internal/sms", new
            {
                toPhone,
                message
            }, ct);

            if (!resp.IsSuccessStatusCode)
                _logger.LogWarning("SMS notification failed: {Status}", (int)resp.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SMS notification error (non-fatal)");
        }
    }

    public async Task PushInAppAsync(
        string type,
        string title,
        string message,
        string icon,
        string targetRole,
        string? targetUserId = null,
        CancellationToken ct = default)
    {
        try
        {
            var client = BuildClient();
            var resp = await client.PostAsJsonAsync("/api/internal/push", new
            {
                type,
                title,
                message,
                icon,
                targetRole,
                targetUserId
            }, ct);

            if (!resp.IsSuccessStatusCode)
                _logger.LogWarning("In-app push notification failed: {Status}", (int)resp.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "In-app push notification error (non-fatal)");
        }
    }

    private HttpClient BuildClient()
    {
        var client = _httpFactory.CreateClient("NotificationAPI");
        var key = _config["Services:InternalNotifyKey"];
        if (!string.IsNullOrEmpty(key))
        {
            client.DefaultRequestHeaders.Remove("X-Internal-Key");
            client.DefaultRequestHeaders.Add("X-Internal-Key", key);
        }
        return client;
    }
}
