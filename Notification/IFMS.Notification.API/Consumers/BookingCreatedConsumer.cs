using IFMS.Messaging.Events;
using IFMS.Notification.API.DTOs;
using IFMS.Notification.API.Services;
using MassTransit;

namespace IFMS.Notification.API.Consumers;

/// <summary>
/// Consumes BookingCreated events from RabbitMQ and sends SMS + Email to the customer.
/// </summary>
public class BookingCreatedConsumer : IConsumer<BookingCreated>
{
    private readonly INotificationService _notify;
    private readonly NotificationStore _store;
    private readonly ILogger<BookingCreatedConsumer> _logger;

    public BookingCreatedConsumer(
        INotificationService notify,
        NotificationStore store,
        ILogger<BookingCreatedConsumer> logger)
    {
        _notify = notify;
        _store = store;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<BookingCreated> context)
    {
        var e = context.Message;
        _logger.LogInformation("BookingCreated received — Token: {Token}, Customer: {Email}", e.TokenCode, e.CustomerEmail);

        // 1. SMS to customer
        if (!string.IsNullOrWhiteSpace(e.CustomerPhone))
        {
            var phone = e.CustomerPhone.StartsWith('+') ? e.CustomerPhone : "+91" + e.CustomerPhone;
            var sms = $"IFMS: Your fuel booking is confirmed! Token: {e.TokenCode}. " +
                      $"{e.QuantityLiters}L {e.FuelType} at {e.StationName}. " +
                      $"Amount: ₹{e.TotalPaid:F2}. Valid 24 hours. — Bharat Kinetic";
            await _notify.SendSmsAsync(phone, sms);
        }

        // 2. Email to customer
        if (!string.IsNullOrWhiteSpace(e.CustomerEmail))
        {
            var emailBody = $@"
                <h2 style='color:#1a237e;'>IFMS Booking Confirmation</h2>
                <p>Dear {e.CustomerName},</p>
                <p>Your fuel booking has been confirmed!</p>
                <table style='border-collapse:collapse;width:100%;max-width:500px;'>
                    <tr><td style='padding:8px;border:1px solid #ddd;'><b>Token Code</b></td>
                        <td style='padding:8px;border:1px solid #ddd;font-family:monospace;font-size:18px;'><b>{e.TokenCode}</b></td></tr>
                    <tr><td style='padding:8px;border:1px solid #ddd;'>Station</td>
                        <td style='padding:8px;border:1px solid #ddd;'>{e.StationName}</td></tr>
                    <tr><td style='padding:8px;border:1px solid #ddd;'>Fuel Type</td>
                        <td style='padding:8px;border:1px solid #ddd;'>{e.FuelType}</td></tr>
                    <tr><td style='padding:8px;border:1px solid #ddd;'>Quantity</td>
                        <td style='padding:8px;border:1px solid #ddd;'>{e.QuantityLiters} Litres</td></tr>
                    <tr><td style='padding:8px;border:1px solid #ddd;'>Amount Paid</td>
                        <td style='padding:8px;border:1px solid #ddd;'>₹{e.TotalPaid:F2}</td></tr>
                    <tr><td style='padding:8px;border:1px solid #ddd;'>Valid Until</td>
                        <td style='padding:8px;border:1px solid #ddd;'>{e.ExpiresAt:dd MMM yyyy HH:mm} UTC</td></tr>
                </table>
                <p>Show this token at the pump to collect your fuel.</p>
                <p style='color:#666;font-size:12px;'>— Bharat Kinetic IFMS</p>";

            await _notify.SendEmailAsync(e.CustomerEmail, "IFMS Booking Confirmed — " + e.TokenCode, emailBody);
        }

        // 3. In-app notification for customer
        _store.Add(new AppNotification
        {
            Type = "success",
            Title = "Booking Confirmed",
            Message = $"Your fuel booking ({e.TokenCode}) is confirmed. {e.QuantityLiters}L {e.FuelType} at {e.StationName}. Valid 24 hours.",
            Icon = "confirmation_number",
            TargetRole = "Customer",
            TargetUserId = e.CustomerId.ToString()
        });

        // 4. In-app notification for dealer
        _store.Add(new AppNotification
        {
            Type = "info",
            Title = "New Booking",
            Message = $"New {e.FuelType} booking ({e.QuantityLiters}L) at your station. Token: {e.TokenCode}.",
            Icon = "local_gas_station",
            TargetRole = "Dealer"
        });
    }
}
