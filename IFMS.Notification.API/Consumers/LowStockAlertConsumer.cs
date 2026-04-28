using IFMS.Messaging.Events;
using IFMS.Notification.API.DTOs;
using IFMS.Notification.API.Services;
using MassTransit;

namespace IFMS.Notification.API.Consumers;

public class LowStockAlertConsumer : IConsumer<LowStockAlert>
{
    private readonly NotificationStore _store;
    private readonly ILogger<LowStockAlertConsumer> _logger;

    public LowStockAlertConsumer(NotificationStore store, ILogger<LowStockAlertConsumer> logger)
    {
        _store = store;
        _logger = logger;
    }

    public Task Consume(ConsumeContext<LowStockAlert> context)
    {
        var e = context.Message;
        _logger.LogInformation("LowStockAlert received — Station: {StationId}, Fuel: {Fuel}, Qty: {Qty}L",
            e.StationId, e.FuelType, e.RemainingQuantity);

        _store.Add(new AppNotification
        {
            Type = "warning",
            Title = "Low Stock Alert",
            Message = $"{e.FuelType} stock is low ({e.RemainingQuantity:F0}L remaining). Please schedule a tanker delivery.",
            Icon = "warning",
            TargetRole = "Dealer"
        });

        _store.Add(new AppNotification
        {
            Type = "warning",
            Title = "Low Stock Alert",
            Message = $"{e.FuelType} stock at station {e.StationId} is low ({e.RemainingQuantity:F0}L remaining).",
            Icon = "warning",
            TargetRole = "Admin"
        });

        return Task.CompletedTask;
    }
}
