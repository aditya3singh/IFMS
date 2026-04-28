using System.Net.Http.Json;

namespace IFMS.Inventory.API.Services;

public class HttpInventoryNotificationPublisher : IInventoryNotificationPublisher
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<HttpInventoryNotificationPublisher> _logger;

    public HttpInventoryNotificationPublisher(
        IHttpClientFactory httpFactory,
        IConfiguration config,
        ILogger<HttpInventoryNotificationPublisher> logger)
    {
        _httpFactory = httpFactory;
        _config = config;
        _logger = logger;
    }

    public async Task SendLowStockAlertAsync(
        Guid stationId,
        string fuelType,
        decimal remainingQuantity,
        CancellationToken ct = default)
    {
        // 1. Push in-app alert to Dealer role
        await PushInAppAsync(
            type: "warning",
            title: "Low Stock Alert",
            message: $"{fuelType} stock is low ({remainingQuantity:F0}L remaining). Please schedule a tanker delivery.",
            icon: "warning",
            targetRole: "Dealer",
            ct: ct);

        // 2. Also alert Admin
        await PushInAppAsync(
            type: "warning",
            title: "Low Stock Alert",
            message: $"{fuelType} stock at station {stationId} is low ({remainingQuantity:F0}L remaining).",
            icon: "warning",
            targetRole: "Admin",
            ct: ct);

        // 3. Call the dedicated low-stock SMS/Email endpoint (best-effort)
        try
        {
            var client = BuildClient();
            var resp = await client.PostAsJsonAsync("/api/notify/low-stock", new
            {
                dealerEmail = string.Empty,   // populated by real dealer lookup in future
                dealerPhone = string.Empty,
                stationName = stationId.ToString(),
                fuelType,
                remainingQuantity
            }, ct);

            if (!resp.IsSuccessStatusCode)
                _logger.LogWarning("Low-stock SMS/Email notification failed: {Status}", (int)resp.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Low-stock SMS/Email notification error (non-fatal)");
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
