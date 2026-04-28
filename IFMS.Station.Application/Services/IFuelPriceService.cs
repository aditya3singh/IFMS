namespace IFMS.Station.Application.Services;

/// <summary>
/// Service for fetching real-time fuel prices from external API
/// </summary>
public interface IFuelPriceService
{
    /// <summary>
    /// Get current fuel price for a specific state and district
    /// </summary>
    Task<FuelPriceResponse?> GetFuelPriceAsync(string state, string district, string fuelType, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get all fuel prices for a state
    /// </summary>
    Task<List<FuelPriceResponse>> GetStateFuelPricesAsync(string state, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get list of available states
    /// </summary>
    Task<List<string>> GetAvailableStatesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get list of districts in a state
    /// </summary>
    Task<List<string>> GetDistrictsAsync(string state, CancellationToken cancellationToken = default);
}

public record FuelPriceResponse(
    string District,
    string State,
    decimal PetrolPrice,
    decimal DieselPrice,
    decimal? CngPrice,
    string Currency,
    DateTime FetchedAt,
    string Source
);

public record FuelProduct(
    string ProductName,
    string ProductPrice,
    string ProductCurrency,
    string? PriceChange,
    string? PriceChangeSign
);

public record ApiFuelPriceResponse(
    string District,
    List<FuelProduct> Products
);
