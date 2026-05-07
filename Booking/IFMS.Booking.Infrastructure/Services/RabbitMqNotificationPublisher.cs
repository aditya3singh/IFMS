using IFMS.Booking.Application.Interfaces;
using IFMS.Messaging.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace IFMS.Booking.Infrastructure.Services;

/// <summary>
/// Publishes booking events to RabbitMQ via MassTransit.
/// The Notification API consumes these events and sends SMS/Email/in-app.
/// </summary>
public class RabbitMqNotificationPublisher : INotificationPublisher
{
    private readonly IPublishEndpoint _publish;
    private readonly ILogger<RabbitMqNotificationPublisher> _logger;

    public RabbitMqNotificationPublisher(IPublishEndpoint publish, ILogger<RabbitMqNotificationPublisher> logger)
    {
        _publish = publish;
        _logger = logger;
    }

    public async Task SendBookingConfirmedAsync(
        string customerEmail, string customerPhone, string customerName,
        string tokenCode, string stationName, string fuelType,
        decimal quantityLiters, decimal totalPaid, CancellationToken ct = default)
    {
        try
        {
            await _publish.Publish(new BookingCreated(
                BookingId:      Guid.NewGuid(), // placeholder — real ID set by handler
                CustomerId:     Guid.Empty,
                StationId:      Guid.Empty,
                FuelType:       fuelType,
                QuantityLiters: quantityLiters,
                TotalPaid:      totalPaid,
                TokenCode:      tokenCode,
                StationName:    stationName,
                CustomerName:   customerName,
                CustomerEmail:  customerEmail,
                CustomerPhone:  customerPhone,
                BookedAt:       DateTime.UtcNow,
                ExpiresAt:      DateTime.UtcNow.AddHours(24)
            ), ct);
            _logger.LogInformation("[RabbitMQ] BookingCreated published — Token: {Token}", tokenCode);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[RabbitMQ] Failed to publish BookingCreated (non-fatal)");
        }
    }

    public async Task SendSmsAsync(string toPhone, string message, CancellationToken ct = default)
    {
        // Direct SMS (fuel dispensed / cancelled) — still published as events
        // handled by BookingConfirmedConsumer / BookingCancelledConsumer
        // This method is a no-op here since those events carry the phone number
        await Task.CompletedTask;
    }

    public async Task PushInAppAsync(
        string type, string title, string message, string icon,
        string targetRole, string? targetUserId = null, CancellationToken ct = default)
    {
        // In-app pushes are handled by the consumers after they receive the events
        await Task.CompletedTask;
    }
}
