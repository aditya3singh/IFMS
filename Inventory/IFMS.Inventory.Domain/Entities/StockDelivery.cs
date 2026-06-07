namespace IFMS.Inventory.Domain.Entities;

/// <summary>
/// Scheduled/tracked fuel deliveries from suppliers to stations
/// </summary>
public class StockDelivery
{
    public Guid Id { get; private set; }
    public Guid StationId { get; private set; }
    public Guid SupplierId { get; private set; }
    public string FuelType { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }
    public decimal PricePerLitre { get; private set; }
    public decimal TotalAmount { get; private set; }
    
    /// <summary>Scheduled, InTransit, Delivered, Cancelled</summary>
    public string Status { get; private set; } = "Scheduled";
    
    public DateTime ScheduledDate { get; private set; }
    public DateTime? ActualDeliveryDate { get; private set; }
    
    public string? Notes { get; private set; }
    
    /// <summary>User who created the delivery order (dealer)</summary>
    public Guid CreatedByUserId { get; private set; }
    
    /// <summary>User who marked as delivered (if any)</summary>
    public Guid? DeliveredByUserId { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Navigation
    public Supplier? Supplier { get; private set; }

    private StockDelivery() { }

    public static StockDelivery Create(
        Guid stationId,
        Guid supplierId,
        string fuelType,
        decimal quantity,
        decimal pricePerLitre,
        DateTime scheduledDate,
        Guid createdByUserId,
        string? notes = null)
    {
        if (stationId == Guid.Empty)
            throw new ArgumentException("StationId is required.");
        if (supplierId == Guid.Empty)
            throw new ArgumentException("SupplierId is required.");
        if (string.IsNullOrWhiteSpace(fuelType))
            throw new ArgumentException("FuelType is required.");
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive.");
        if (pricePerLitre <= 0)
            throw new ArgumentException("PricePerLitre must be positive.");

        return new StockDelivery
        {
            Id = Guid.NewGuid(),
            StationId = stationId,
            SupplierId = supplierId,
            FuelType = fuelType.Trim(),
            Quantity = quantity,
            PricePerLitre = pricePerLitre,
            TotalAmount = quantity * pricePerLitre,
            Status = "Scheduled",
            ScheduledDate = scheduledDate,
            Notes = notes,
            CreatedByUserId = createdByUserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void UpdateStatus(string status, Guid? deliveredByUserId = null)
    {
        Status = status switch
        {
            "Scheduled" or "InTransit" or "Delivered" or "Cancelled" => status,
            _ => Status
        };

        if (status == "Delivered")
        {
            ActualDeliveryDate = DateTime.UtcNow;
            DeliveredByUserId = deliveredByUserId;
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(decimal quantity, decimal pricePerLitre, DateTime scheduledDate, string? notes)
    {
        if (Status == "Delivered" || Status == "Cancelled")
            throw new InvalidOperationException("Cannot update a delivered or cancelled delivery.");

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive.");
        if (pricePerLitre <= 0)
            throw new ArgumentException("PricePerLitre must be positive.");

        Quantity = quantity;
        PricePerLitre = pricePerLitre;
        TotalAmount = quantity * pricePerLitre;
        ScheduledDate = scheduledDate;
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;
    }
}
