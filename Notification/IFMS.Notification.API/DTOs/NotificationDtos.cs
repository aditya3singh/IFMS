namespace IFMS.Notification.API.DTOs;

public record SendTokenNotificationRequest(
    string CustomerPhone,
    string CustomerEmail,
    string CustomerName,
    string TokenCode,
    string StationName,
    string FuelType,
    decimal Quantity,
    decimal TotalPaid
);

public record SendLowStockAlertRequest(
    string DealerEmail,
    string DealerPhone,
    string StationName,
    string FuelType,
    decimal RemainingQuantity
);

public record SendFraudAlertRequest(
    string AdminEmail,
    string AdminPhone,
    string StationName,
    string Description,
    decimal SuspiciousAmount,
    DateTime TransactionDate
);

public record CreateNotificationRequest(
    string Type,
    string Title,
    string Message,
    string Icon,
    string TargetRole,
    string? TargetUserId = null
);

/// <summary>Payload for POST /api/internal/sms — service-to-service plain SMS.</summary>
public record SmsInternalRequest(
    string ToPhone,
    string Message
);

public class AppNotification
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Type { get; set; } = "info";
    public string Title { get; set; } = "";
    public string Message { get; set; } = "";
    public string Icon { get; set; } = "notifications";
    public string TargetRole { get; set; } = "All";
    public string? TargetUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; } = false;
}
