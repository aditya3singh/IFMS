namespace IFMS.Inventory.Application.DTOs;

public record CreateFuelStockRequest(
    string FuelType,
    decimal Quantity,
    decimal PricePerLitre,
    Guid StationId
);

public record UpdateStockRequest(
    Guid Id,
    decimal NewQuantity
);

public record FuelStockResponse(
    Guid Id,
    string FuelType,
    decimal Quantity,
    decimal PricePerLitre,
    string Status,
    Guid StationId,
    DateTime LastUpdated,
    bool IsLowStock
);