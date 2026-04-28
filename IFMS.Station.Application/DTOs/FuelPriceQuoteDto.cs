namespace IFMS.Station.Application.DTOs;

public record FuelPriceQuoteDto(
    decimal PricePerUnit,
    string UnitLabel,
    string AreaSummary,
    DateTimeOffset AsOfUtc);
