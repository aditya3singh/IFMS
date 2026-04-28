using IFMS.Messaging.Events;
using IFMS.Notification.API.DTOs;
using IFMS.Notification.API.Services;
using MassTransit;

namespace IFMS.Notification.API.Consumers;

public class SaleRecordedConsumer : IConsumer<SaleRecorded>
{
    private readonly NotificationStore _store;
    private readonly ILogger<SaleRecordedConsumer> _logger;

    public SaleRecordedConsumer(NotificationStore store, ILogger<SaleRecordedConsumer> logger)
    {
        _store = store;
        _logger = logger;
    }

    public Task Consume(ConsumeContext<SaleRecorded> context)
    {
        var e = context.Message;
        _logger.LogInformation("SaleRecorded received — Station: {StationId}, Amount: {Amount}", e.StationId, e.TotalAmount);

        _store.Add(new AppNotification
        {
            Type = "success",
            Title = "Sale Recorded",
            Message = $"Sale of ₹{e.TotalAmount:F0} recorded for {e.CustomerName} — {e.Quantity}L {e.FuelType}.",
            Icon = "point_of_sale",
            TargetRole = "Dealer"
        });

        _store.Add(new AppNotification
        {
            Type = "info",
            Title = "New Transaction",
            Message = $"Transaction: {e.Quantity}L {e.FuelType} at station {e.StationId} — ₹{e.TotalAmount:F0}.",
            Icon = "receipt_long",
            TargetRole = "Admin"
        });

        return Task.CompletedTask;
    }
}
