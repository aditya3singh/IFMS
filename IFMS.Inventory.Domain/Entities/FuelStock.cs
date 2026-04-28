namespace IFMS.Inventory.Domain.Entities;

public class FuelStock
{
    public Guid Id { get; private set; }
    public string FuelType { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }
    public decimal PricePerLitre { get; private set; }
    public string Status { get; private set; } = string.Empty;
    public Guid StationId { get; private set; }
    public DateTime LastUpdated { get; private set; }

    private FuelStock() { }

    public static FuelStock Create(string fuelType, decimal quantity, decimal pricePerLitre, Guid stationId)
    {
        return new FuelStock
        {
            Id = Guid.NewGuid(),
            FuelType = fuelType,
            Quantity = quantity,
            PricePerLitre = pricePerLitre,
            StationId = stationId,
            Status = "Available",
            LastUpdated = DateTime.UtcNow
        };
    }

    public void UpdateStock(decimal newQuantity)
    {
        Quantity = newQuantity;
        Status = newQuantity > 0 ? "Available" : "OutOfStock";
        LastUpdated = DateTime.UtcNow;
    }

    public void UpdatePrice(decimal newPrice)
    {
        PricePerLitre = newPrice;
        LastUpdated = DateTime.UtcNow;
    }

    public bool IsLowStock() => Quantity < 500;
}
