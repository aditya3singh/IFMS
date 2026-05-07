namespace IFMS.Inventory.API.Services;

/// <summary>
/// Sends inventory-related notifications to the Notification API.
/// All failures are swallowed — inventory operations are never blocked.
/// </summary>
public interface IInventoryNotificationPublisher
{
    /// <summary>Push an in-app low-stock alert and optionally send SMS/Email to the dealer.</summary>
    Task SendLowStockAlertAsync(
        Guid stationId,
        string fuelType,
        decimal remainingQuantity,
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
