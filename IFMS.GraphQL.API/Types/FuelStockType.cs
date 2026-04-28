namespace IFMS.GraphQL.API.Types;

public class FuelStockType
{
    public Guid Id { get; set; }
    public string FuelType { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal PricePerLitre { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid StationId { get; set; }
    public DateTime LastUpdated { get; set; }
    public bool IsLowStock { get; set; }
}
