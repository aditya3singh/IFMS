namespace IFMS.Admin.Infrastructure.Models;

public class TransactionView
{
    public Guid Id { get; set; }
    public Guid StationId { get; set; }
    public string FuelType { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime TransactionDate { get; set; }
}