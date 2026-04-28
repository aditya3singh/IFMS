using System.Net.Http.Json;

namespace IFMS.Sales.API.Services;

public class HttpSalesNotificationPublisher : ISalesNotificationPublisher
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<HttpSalesNotificationPublisher> _logger;

    public HttpSalesNotificationPublisher(
        IHttpClientFactory httpFactory,
        IConfiguration config,
        ILogger<HttpSalesNotificationPublisher> logger)
    {
        _httpFactory = httpFactory;
        _config = config;
        _logger = logger;
    }

    public async Task PushSaleRecordedAsync(
        Guid stationId,
        string fuelType,
        decimal quantity,
        decimal totalAmount,
        string customerName,
        CancellationToken ct = default)
    {
        await PushInAppAsync(
            type: "success",
            title: "Sale Recorded",
            message: $"Sale of ₹{totalAmount:F0} recorded for {customerName} — {quantity}L {fuelType}.",
            icon: "point_of_sale",
            targetRole: "Dealer",
            ct: ct);

        // Also notify Admin for visibility
        await PushInAppAsync(
            type: "info",
            title: "New Transaction",
            message: $"Transaction: {quantity}L {fuelType} at station {stationId} — ₹{totalAmount:F0}.",
            icon: "receipt_long",
            targetRole: "Admin",
            ct: ct);
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
