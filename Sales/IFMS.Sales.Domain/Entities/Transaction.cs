namespace IFMS.Sales.Domain.Entities;

public class Transaction
{
    public Guid Id { get; private set; }
    public Guid StationId { get; private set; }
    public string FuelType { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }
    public decimal PricePerLitre { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string PaymentMethod { get; private set; } = string.Empty;
    public string Status { get; private set; } = string.Empty;
    public DateTime TransactionDate { get; private set; }
    public string CustomerName { get; private set; } = string.Empty;

    private Transaction() { }

    public static Transaction Create(
        Guid stationId,
        string fuelType,
        decimal quantity,
        decimal pricePerLitre,
        string paymentMethod,
        string customerName)
    {
        return new Transaction
        {
            Id = Guid.NewGuid(),
            StationId = stationId,
            FuelType = fuelType,
            Quantity = quantity,
            PricePerLitre = pricePerLitre,
            TotalAmount = quantity * pricePerLitre,
            PaymentMethod = paymentMethod,
            Status = "Completed",
            TransactionDate = DateTime.UtcNow,
            CustomerName = customerName
        };
    }
}