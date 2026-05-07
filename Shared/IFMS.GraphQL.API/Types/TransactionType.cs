namespace IFMS.GraphQL.API.Types;

public class TransactionType
{
    public Guid Id { get; set; }
    public Guid StationId { get; set; }
    public string FuelType { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal PricePerLitre { get; set; }
    public decimal TotalAmount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public string CustomerName { get; set; } = string.Empty;
}
