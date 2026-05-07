using IFMS.Messaging.Events;
using IFMS.Notification.API.DTOs;
using IFMS.Notification.API.Services;
using MassTransit;

namespace IFMS.Notification.API.Consumers;

public class BookingCancelledConsumer : IConsumer<BookingCancelled>
{
    private readonly INotificationService _notify;
    private readonly NotificationStore _store;
    private readonly ILogger<BookingCancelledConsumer> _logger;

    public BookingCancelledConsumer(INotificationService notify, NotificationStore store, ILogger<BookingCancelledConsumer> logger)
    {
        _notify = notify;
        _store = store;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<BookingCancelled> context)
    {
        var e = context.Message;
        _logger.LogInformation("BookingCancelled received — Token: {Token}", e.TokenCode);

        if (!string.IsNullOrWhiteSpace(e.CustomerPhone))
        {
            var phone = e.CustomerPhone.StartsWith('+') ? e.CustomerPhone : "+91" + e.CustomerPhone;
            await _notify.SendSmsAsync(phone,
                $"IFMS: Your booking ({e.TokenCode}) has been cancelled. If this was not you, contact support. — Bharat Kinetic");
        }

        _store.Add(new AppNotification
        {
            Type = "warning",
            Title = "Booking Cancelled",
            Message = $"Your booking ({e.TokenCode}) has been cancelled.",
            Icon = "cancel",
            TargetRole = "Customer",
            TargetUserId = e.CustomerId.ToString()
        });
    }
}
