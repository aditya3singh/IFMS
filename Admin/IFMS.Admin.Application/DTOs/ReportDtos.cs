namespace IFMS.Admin.Application.DTOs;

public record SystemOverviewResponse(
    int TotalStations,
    int TotalUsers,
    decimal TotalRevenue,
    int TotalTransactions,
    List<StationSummary> Stations
);

public record StationSummary(
    Guid StationId,
    string StationName,
    decimal Revenue,
    int TransactionCount
);

public record DailyReportResponse(
    DateTime Date,
    int TotalTransactions,
    decimal TotalRevenue,
    decimal PetrolSold,
    decimal DieselSold,
    decimal CngSold
);