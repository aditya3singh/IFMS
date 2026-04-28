using IFMS.Messaging.Events;
using IFMS.Notification.API.DTOs;
using IFMS.Notification.API.Services;
using MassTransit;

namespace IFMS.Notification.API.Consumers;

/// <summary>Consumes BookingConfirmed — fuel was dispensed at the pump.</summary>
public class BookingConfirmedConsumer : IConsumer<BookingConfirmed>
{
    private readonly INotificationService _notify;
    private readonly NotificationStore _store;
    private readonly ILogger<BookingConfirmedConsumer> _logger;

    public BookingConfirmedConsumer(INotificationService notify, NotificationStore store, ILogger<BookingConfirmedConsumer> logger)
    {
        _notify = notify;
        _store = store;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<BookingConfirmed> context)
    {
        var e = context.Message;
        _logger.LogInformation("BookingConfirmed received — Token: {Token}", e.TokenCode);

        // SMS to customer
        if (!string.IsNullOrWhiteSpace(e.CustomerPhone))
        {
            var phone = e.CustomerPhone.StartsWith('+') ? e.CustomerPhone : "+91" + e.CustomerPhone;
            await _notify.SendSmsAsync(phone,
                $"IFMS: Your fuel has been dispensed! Token {e.TokenCode} — {e.QuantityLiters}L {e.FuelType}. Thank you. — Bharat Kinetic");
        }

        // In-app for customer
        _store.Add(new AppNotification
        {
            Type = "success",
            Title = "Fuel Dispensed",
            Message = $"Your token {e.TokenCode} was used. {e.QuantityLiters}L of {e.FuelType} dispensed successfully.",
            Icon = "local_gas_station",
            TargetRole = "Customer",
            TargetUserId = e.CustomerId.ToString()
        });
    }
}
