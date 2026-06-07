namespace IFMS.Inventory.Application.DTOs;

// Response
public record StockTransactionResponse(
    Guid Id,
    Guid FuelStockId,
    Guid StationId,
    string FuelType,
    string TransactionType,
    decimal QuantityChange,
    decimal QuantityBefore,
    decimal QuantityAfter,
    decimal PricePerLitre,
    Guid? UserId,
    string PerformedBy,
    string? Notes,
    Guid? SaleTransactionId,
    Guid? DeliveryId,
    DateTime CreatedAt
);

// Query filters
public record StockTransactionFilter(
    Guid? StationId = null,
    Guid? FuelStockId = null,
    string? TransactionType = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    int Limit = 100
);
