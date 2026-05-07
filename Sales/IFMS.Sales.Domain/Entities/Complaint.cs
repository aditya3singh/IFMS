namespace IFMS.Sales.Domain.Entities;

/// <summary>
/// Customer complaint raised against a booking or transaction.
/// Stored in IFMS_SalesDB alongside transactions.
/// </summary>
public class Complaint
{
    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public string CustomerName { get; private set; } = string.Empty;
    public string CustomerEmail { get; private set; } = string.Empty;
    public string CustomerPhone { get; private set; } = string.Empty;

    /// <summary>Category: FuelQuality | QuantityDispute | PaymentIssue | BookingFailed | Other</summary>
    public string Category { get; private set; } = string.Empty;

    /// <summary>Optional reference to a booking token or transaction ID</summary>
    public string? ReferenceId { get; private set; }

    public string Subject { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    /// <summary>Open | InProgress | Resolved | Closed</summary>
    public string Status { get; private set; } = "Open";

    public string? ResolutionNote { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? ResolvedAt { get; private set; }

    private Complaint() { }

    public static Complaint Create(
        Guid customerId,
        string customerName,
        string customerEmail,
        string customerPhone,
        string category,
        string subject,
        string description,
        string? referenceId = null)
    {
        return new Complaint
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            CustomerName = customerName,
            CustomerEmail = customerEmail,
            CustomerPhone = customerPhone,
            Category = category,
            Subject = subject,
            Description = description,
            ReferenceId = referenceId,
            Status = "Open",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void UpdateStatus(string status, string? resolutionNote = null)
    {
        Status = status;
        ResolutionNote = resolutionNote ?? ResolutionNote;
        UpdatedAt = DateTime.UtcNow;
        if (status is "Resolved" or "Closed")
            ResolvedAt = DateTime.UtcNow;
    }
}
