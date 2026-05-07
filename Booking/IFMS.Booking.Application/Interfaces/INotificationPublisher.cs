namespace IFMS.Booking.Application.Interfaces;

/// <summary>
/// Fire-and-forget notification publisher used by the Booking service.
/// Implementations call the Notification API; failures are logged but never
/// propagate to the caller so booking operations are never blocked.
/// </summary>
public interface INotificationPublisher
{
    /// <summary>Send booking-token confirmation (SMS + Email) to the customer.</summary>
    Task SendBookingConfirmedAsync(
        string customerEmail,
        string customerPhone,
        string customerName,
        string tokenCode,
        string stationName,
        string fuelType,
        decimal quantityLiters,
        decimal totalPaid,
        CancellationToken ct = default);

    /// <summary>Send a plain SMS to a customer's registered mobile number.</summary>
    Task SendSmsAsync(
        string toPhone,
        string message,
        CancellationToken ct = default);

    /// <summary>Push an in-app notification to a specific user or role.</summary>
    Task PushInAppAsync(
        string type,
        string title,
        string message,
        string icon,
        string targetRole,
        string? targetUserId = null,
        CancellationToken ct = default);
}
