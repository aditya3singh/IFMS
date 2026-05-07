namespace IFMS.Messaging.Events;

/// <summary>Published by Inventory API when stock drops below threshold.</summary>
public record LowStockAlert(
    Guid StationId,
    string FuelType,
    decimal RemainingQuantity,
    decimal Threshold
);
