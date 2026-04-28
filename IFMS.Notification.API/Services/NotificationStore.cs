using IFMS.Notification.API.DTOs;
using System.Collections.Concurrent;

namespace IFMS.Notification.API.Services;

/// <summary>
/// Thread-safe in-memory notification store.
/// Keeps last 200 notifications. Replace with DB persistence for production.
/// </summary>
public class NotificationStore
{
    private readonly ConcurrentQueue<AppNotification> _notifications = new();
    private const int MaxNotifications = 200;

    public void Add(AppNotification notification)
    {
        _notifications.Enqueue(notification);
        // Trim to max size
        while (_notifications.Count > MaxNotifications)
            _notifications.TryDequeue(out _);
    }

    public IReadOnlyList<AppNotification> GetAll() =>
        _notifications.OrderByDescending(n => n.CreatedAt).ToList();

    public IReadOnlyList<AppNotification> GetForRole(string role, string? userId = null)
    {
        return _notifications
            .Where(n => IsVisibleTo(n, role, userId))
            .OrderByDescending(n => n.CreatedAt)
            .ToList();
    }

    public bool MarkRead(string id)
    {
        var n = _notifications.FirstOrDefault(x => x.Id == id);
        if (n == null) return false;
        n.IsRead = true;
        return true;
    }

    public void MarkAllRead(string role, string? userId = null)
    {
        foreach (var n in _notifications)
        {
            if (IsVisibleTo(n, role, userId))
                n.IsRead = true;
        }
    }

    public int UnreadCount(string role, string? userId = null) =>
        _notifications.Count(n => !n.IsRead && IsVisibleTo(n, role, userId));

    /// <summary>Shared visibility predicate — keeps all filter logic in one place.</summary>
    private static bool IsVisibleTo(AppNotification n, string role, string? userId)
    {
        if (n.TargetRole == "All") return true;
        if (n.TargetRole != role) return false;

        // Customers only see their own notifications
        if (role == "Customer")
            return userId != null && n.TargetUserId == userId;

        // Admin/Dealer see role-wide + their own
        return n.TargetUserId == null || n.TargetUserId == string.Empty || n.TargetUserId == userId;
    }

    /// <summary>Seed some initial notifications for demo purposes</summary>
    public void SeedDemo()
    {
        if (_notifications.Count > 0) return;

        var seeds = new[]
        {
            new AppNotification { Type = "success", Title = "System Online", Message = "IFMS platform is running. All services healthy.", Icon = "check_circle", TargetRole = "Admin", CreatedAt = DateTime.UtcNow.AddMinutes(-5) },
            new AppNotification { Type = "warning", Title = "Low Stock Alert", Message = "Diesel stock at HITEC City Fuels is below 20%. Schedule a tanker delivery.", Icon = "warning", TargetRole = "Dealer", CreatedAt = DateTime.UtcNow.AddMinutes(-15) },
            new AppNotification { Type = "info", Title = "New Booking", Message = "Customer John Doe booked 20L Petrol at HITEC City Fuels.", Icon = "local_gas_station", TargetRole = "Dealer", CreatedAt = DateTime.UtcNow.AddMinutes(-30) },
            new AppNotification { Type = "error", Title = "Fraud Alert", Message = "Suspicious transaction detected at NCR Central Energy. Amount: ₹75,000.", Icon = "gpp_bad", TargetRole = "Admin", CreatedAt = DateTime.UtcNow.AddHours(-1) },
            new AppNotification { Type = "success", Title = "Booking Confirmed", Message = "Your fuel booking (IFM-44-ABCD1234) is confirmed. Valid for 24 hours.", Icon = "confirmation_number", TargetRole = "Customer", CreatedAt = DateTime.UtcNow.AddHours(-2) },
            new AppNotification { Type = "info", Title = "Dealer Assigned", Message = "Dealer User has been assigned to HITEC City Fuels station.", Icon = "manage_accounts", TargetRole = "Admin", CreatedAt = DateTime.UtcNow.AddHours(-3) },
            new AppNotification { Type = "success", Title = "Sale Recorded", Message = "Sale of ₹1,063 recorded for customer Ramesh Kumar.", Icon = "point_of_sale", TargetRole = "Dealer", CreatedAt = DateTime.UtcNow.AddHours(-4) },
            new AppNotification { Type = "info", Title = "Token Validated", Message = "Token IFM-44-XY789 validated and fuel dispensed successfully.", Icon = "qr_code_scanner", TargetRole = "Dealer", CreatedAt = DateTime.UtcNow.AddHours(-5) },
        };

        foreach (var s in seeds)
            _notifications.Enqueue(s);
    }
}
