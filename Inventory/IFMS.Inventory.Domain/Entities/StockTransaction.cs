namespace IFMS.Inventory.Domain.Entities;

/// <summary>
/// Audit log for all inventory changes (additions, sales deductions, adjustments).
/// Immutable — never updated or deleted.
/// </summary>
public class StockTransaction
{
    public Guid Id { get; private set; }
    public Guid FuelStockId { get; private set; }
    public Guid StationId { get; private set; }
    public string FuelType { get; private set; } = string.Empty;
    
    /// <summary>Addition, Deduction, Adjustment, Delivery</summary>
    public string TransactionType { get; private set; } = string.Empty;
    
    /// <summary>Quantity change (positive for additions, negative for deductions)</summary>
    public decimal QuantityChange { get; private set; }
    
    /// <summary>Stock level before this transaction</summary>
    public decimal QuantityBefore { get; private set; }
    
    /// <summary>Stock level after this transaction</summary>
    public decimal QuantityAfter { get; private set; }
    
    /// <summary>Price per litre at time of transaction</summary>
    public decimal PricePerLitre { get; private set; }
    
    /// <summary>User who performed the action (null for system actions)</summary>
    public Guid? UserId { get; private set; }
    
    /// <summary>Admin, Dealer, System, Sale</summary>
    public string PerformedBy { get; private set; } = string.Empty;
    
    /// <summary>Optional notes/reason for adjustment</summary>
    public string? Notes { get; private set; }
    
    /// <summary>Related sale transaction ID (if deduction was from a sale)</summary>
    public Guid? SaleTransactionId { get; private set; }
    
    /// <summary>Related delivery ID (if addition was from a scheduled delivery)</summary>
    public Guid? DeliveryId { get; private set; }
    
    public DateTime CreatedAt { get; private set; }

    // Navigation
    public FuelStock? FuelStock { get; private set; }

    private StockTransaction() { }

    public static StockTransaction Create(
        Guid fuelStockId,
        Guid stationId,
        string fuelType,
        string transactionType,
        decimal quantityChange,
        decimal quantityBefore,
        decimal quantityAfter,
        decimal pricePerLitre,
        Guid? userId,
        string performedBy,
        string? notes = null,
        Guid? saleTransactionId = null,
        Guid? deliveryId = null)
    {
        if (string.IsNullOrWhiteSpace(transactionType))
            throw new ArgumentException("TransactionType is required.");
        if (string.IsNullOrWhiteSpace(fuelType))
            throw new ArgumentException("FuelType is required.");
        if (string.IsNullOrWhiteSpace(performedBy))
            throw new ArgumentException("PerformedBy is required.");

        return new StockTransaction
        {
            Id = Guid.NewGuid(),
            FuelStockId = fuelStockId,
            StationId = stationId,
            FuelType = fuelType,
            TransactionType = transactionType,
            QuantityChange = quantityChange,
            QuantityBefore = quantityBefore,
            QuantityAfter = quantityAfter,
            PricePerLitre = pricePerLitre,
            UserId = userId,
            PerformedBy = performedBy,
            Notes = notes,
            SaleTransactionId = saleTransactionId,
            DeliveryId = deliveryId,
            CreatedAt = DateTime.UtcNow
        };
    }
}
