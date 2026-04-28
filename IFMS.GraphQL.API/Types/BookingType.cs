namespace IFMS.GraphQL.API.Types;

public class BookingType
{
    public Guid BookingId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid StationId { get; set; }
    public string FuelType { get; set; } = string.Empty;
    public decimal QuantityLiters { get; set; }
    public decimal TotalPaid { get; set; }
    public string TokenCode { get; set; } = string.Empty;
    public string TokenStatus { get; set; } = string.Empty;
    public string PaymentId { get; set; } = string.Empty;
    public DateTime BookedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }
}
