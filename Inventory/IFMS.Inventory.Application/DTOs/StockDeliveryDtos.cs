namespace IFMS.Inventory.Application.DTOs;

// Response
public record StockDeliveryResponse(
    Guid Id,
    Guid StationId,
    Guid SupplierId,
    string SupplierName,
    string FuelType,
    decimal Quantity,
    decimal PricePerLitre,
    decimal TotalAmount,
    string Status,
    DateTime ScheduledDate,
    DateTime? ActualDeliveryDate,
    string? Notes,
    Guid CreatedByUserId,
    Guid? DeliveredByUserId,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

// Create
public record CreateStockDeliveryRequest(
    Guid StationId,
    Guid SupplierId,
    string FuelType,
    decimal Quantity,
    decimal PricePerLitre,
    DateTime ScheduledDate,
    string? Notes = null
);

// Update
public record UpdateStockDeliveryRequest(
    decimal Quantity,
    decimal PricePerLitre,
    DateTime ScheduledDate,
    string? Notes
);

// Update Status
public record UpdateDeliveryStatusRequest(
    string Status
);
