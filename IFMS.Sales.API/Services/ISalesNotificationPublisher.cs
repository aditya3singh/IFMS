namespace IFMS.Sales.API.Services;

/// <summary>
/// Sends sales-related notifications to the Notification API.
/// All failures are swallowed — sales operations are never blocked.
/// </summary>
public interface ISalesNotificationPublisher
{
    /// <summary>Push an in-app "sale recorded" notification to the dealer.</summary>
    Task PushSaleRecordedAsync(
        Guid stationId,
        string fuelType,
        decimal quantity,
        decimal totalAmount,
        string customerName,
        CancellationToken ct = default);

    /// <summary>Push a generic in-app notification.</summary>
    Task PushInAppAsync(
        string type,
        string title,
        string message,
        string icon,
        string targetRole,
        string? targetUserId = null,
        CancellationToken ct = default);
}
