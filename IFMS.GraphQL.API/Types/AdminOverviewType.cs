namespace IFMS.GraphQL.API.Types;

public class AdminOverviewType
{
    public int TotalTransactions { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal PetrolSold { get; set; }
    public decimal DieselSold { get; set; }
    public decimal CngSold { get; set; }
}

public class RevenueTrendPoint
{
    public string Period { get; set; } = string.Empty;
    public int Transactions { get; set; }
    public decimal Revenue { get; set; }
    public decimal Litres { get; set; }
}

public class BookingsOverviewType
{
    public int Total { get; set; }
    public int Pending { get; set; }
    public int Used { get; set; }
    public int Cancelled { get; set; }
    public int Expired { get; set; }
    public decimal TotalRevenue { get; set; }
}
