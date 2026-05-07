using IFMS.Notification.API.DTOs;
using IFMS.Notification.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IFMS.Notification.API.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class AppNotificationController : ControllerBase
{
    private readonly NotificationStore _store;
    private readonly ILogger<AppNotificationController> _logger;

    public AppNotificationController(NotificationStore store, ILogger<AppNotificationController> logger)
    {
        _store = store;
        _logger = logger;
    }

    /// <summary>Get notifications for the current user based on their role</summary>
    [HttpGet]
    public IActionResult GetMyNotifications([FromQuery] int limit = 20)
    {
        var role = User.FindFirstValue(ClaimTypes.Role) ??
                   User.FindFirstValue("http://schemas.microsoft.com/ws/2008/06/identity/claims/role") ?? "Customer";
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var notifications = _store.GetForRole(role, userId)
            .Take(limit)
            .Select(n => new
            {
                n.Id, n.Type, n.Title, n.Message, n.Icon,
                n.TargetRole, n.CreatedAt, n.IsRead
            });

        var unread = _store.UnreadCount(role, userId);

        return Ok(new { notifications, unreadCount = unread });
    }

    /// <summary>Mark a single notification as read</summary>
    [HttpPut("{id}/read")]
    public IActionResult MarkRead(string id)
    {
        _store.MarkRead(id);
        return Ok(new { message = "Marked as read" });
    }

    /// <summary>Mark all notifications as read for current user</summary>
    [HttpPut("read-all")]
    public IActionResult MarkAllRead()
    {
        var role = User.FindFirstValue(ClaimTypes.Role) ??
                   User.FindFirstValue("http://schemas.microsoft.com/ws/2008/06/identity/claims/role") ?? "Customer";
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _store.MarkAllRead(role, userId);
        return Ok(new { message = "All marked as read" });
    }

    /// <summary>Create a new notification (Admin only or internal)</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public IActionResult CreateNotification([FromBody] CreateNotificationRequest request)
    {
        var notification = new AppNotification
        {
            Type = request.Type,
            Title = request.Title,
            Message = request.Message,
            Icon = request.Icon,
            TargetRole = request.TargetRole,
            TargetUserId = request.TargetUserId
        };
        _store.Add(notification);
        _logger.LogInformation("Notification created: {Title} for {Role}", request.Title, request.TargetRole);
        return Ok(notification);
    }

    /// <summary>Get unread count only (lightweight polling)</summary>
    [HttpGet("unread-count")]
    public IActionResult GetUnreadCount()
    {
        var role = User.FindFirstValue(ClaimTypes.Role) ??
                   User.FindFirstValue("http://schemas.microsoft.com/ws/2008/06/identity/claims/role") ?? "Customer";
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Ok(new { count = _store.UnreadCount(role, userId) });
    }
}
