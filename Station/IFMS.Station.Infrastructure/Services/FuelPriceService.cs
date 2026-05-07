using IFMS.Station.Application.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace IFMS.Station.Infrastructure.Services;

/// <summary>
/// Implementation of fuel price service that fetches real-time prices from Indian fuel price API
/// </summary>
public class FuelPriceService : IFuelPriceService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<FuelPriceService> _logger;
    private const string BaseUrl = "https://fuelprice-api-india.herokuapp.com";
    private const string FallbackBaseUrl = "https://fuel-price-india-api.onrender.com"; // Backup API
    private const int CacheExpirationHours = 6; // Cache for 6 hours (prices update daily at 6 AM)

    public FuelPriceService(
        HttpClient httpClient,
        IMemoryCache cache,
        ILogger<FuelPriceService> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _logger = logger;
        _httpClient.Timeout = TimeSpan.FromSeconds(10);
    }

    public async Task<FuelPriceResponse?> GetFuelPriceAsync(
        string state,
        string district,
        string fuelType,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"fuel_price_{state}_{district}_{fuelType}".ToLowerInvariant();
        
        if (_cache.TryGetValue<FuelPriceResponse>(cacheKey, out var cachedPrice))
        {
            _logger.LogInformation("Returning cached fuel price for {State}/{District}", state, district);
            return cachedPrice;
        }

        try
        {
            var normalizedState = NormalizeStateName(state);
            var normalizedDistrict = NormalizeDistrictName(district);
            
            var url = $"{BaseUrl}/price/{normalizedState}/{normalizedDistrict}";
            _logger.LogInformation("Fetching fuel price from: {Url}", url);

            var response = await _httpClient.GetAsync(url, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch fuel price: {StatusCode}", response.StatusCode);
                return GetFallbackPrice(state, district);
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var apiResponse = JsonSerializer.Deserialize<ApiFuelPriceResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (apiResponse == null || apiResponse.Products == null || !apiResponse.Products.Any())
            {
                _logger.LogWarning("Empty response from fuel price API");
                return GetFallbackPrice(state, district);
            }

            var petrolProduct = apiResponse.Products.FirstOrDefault(p => 
                p.ProductName.Equals("Petrol", StringComparison.OrdinalIgnoreCase));
            var dieselProduct = apiResponse.Products.FirstOrDefault(p => 
                p.ProductName.Equals("Diesel", StringComparison.OrdinalIgnoreCase));
            var cngProduct = apiResponse.Products.FirstOrDefault(p => 
                p.ProductName.Equals("CNG", StringComparison.OrdinalIgnoreCase));

            var priceResponse = new FuelPriceResponse(
                District: apiResponse.District,
                State: state,
                PetrolPrice: decimal.TryParse(petrolProduct?.ProductPrice, out var petrol) ? petrol : 95.50m,
                DieselPrice: decimal.TryParse(dieselProduct?.ProductPrice, out var diesel) ? diesel : 88.50m,
                CngPrice: decimal.TryParse(cngProduct?.ProductPrice, out var cng) ? cng : 75.00m,
                Currency: petrolProduct?.ProductCurrency ?? "INR",
                FetchedAt: DateTime.UtcNow,
                Source: "Indian Fuel Price API"
            );

            // Cache for 6 hours
            _cache.Set(cacheKey, priceResponse, TimeSpan.FromHours(CacheExpirationHours));
            
            _logger.LogInformation("Successfully fetched fuel prices for {State}/{District}: Petrol={Petrol}, Diesel={Diesel}", 
                state, district, priceResponse.PetrolPrice, priceResponse.DieselPrice);

            return priceResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching fuel price for {State}/{District}", state, district);
            return GetFallbackPrice(state, district);
        }
    }

    public async Task<List<FuelPriceResponse>> GetStateFuelPricesAsync(
        string state,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"fuel_prices_state_{state}".ToLowerInvariant();
        
        if (_cache.TryGetValue<List<FuelPriceResponse>>(cacheKey, out var cachedPrices))
        {
            return cachedPrices;
        }

        try
        {
            var normalizedState = NormalizeStateName(state);
            var url = $"{BaseUrl}/price/{normalizedState}";
            
            var response = await _httpClient.GetAsync(url, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                return new List<FuelPriceResponse> { GetFallbackPrice(state, "Default") };
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var apiResponses = JsonSerializer.Deserialize<List<ApiFuelPriceResponse>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (apiResponses == null || !apiResponses.Any())
            {
                return new List<FuelPriceResponse> { GetFallbackPrice(state, "Default") };
            }

            var prices = apiResponses.Select(apiResponse =>
            {
                var petrolProduct = apiResponse.Products.FirstOrDefault(p => 
                    p.ProductName.Equals("Petrol", StringComparison.OrdinalIgnoreCase));
                var dieselProduct = apiResponse.Products.FirstOrDefault(p => 
                    p.ProductName.Equals("Diesel", StringComparison.OrdinalIgnoreCase));
                var cngProduct = apiResponse.Products.FirstOrDefault(p => 
                    p.ProductName.Equals("CNG", StringComparison.OrdinalIgnoreCase));

                return new FuelPriceResponse(
                    District: apiResponse.District,
                    State: state,
                    PetrolPrice: decimal.TryParse(petrolProduct?.ProductPrice, out var petrol) ? petrol : 95.50m,
                    DieselPrice: decimal.TryParse(dieselProduct?.ProductPrice, out var diesel) ? diesel : 88.50m,
                    CngPrice: decimal.TryParse(cngProduct?.ProductPrice, out var cng) ? cng : 75.00m,
                    Currency: petrolProduct?.ProductCurrency ?? "INR",
                    FetchedAt: DateTime.UtcNow,
                    Source: "Indian Fuel Price API"
                );
            }).ToList();

            _cache.Set(cacheKey, prices, TimeSpan.FromHours(CacheExpirationHours));
            
            return prices;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching state fuel prices for {State}", state);
            return new List<FuelPriceResponse> { GetFallbackPrice(state, "Default") };
        }
    }

    public async Task<List<string>> GetAvailableStatesAsync(CancellationToken cancellationToken = default)
    {
        var cacheKey = "fuel_price_states";
        
        if (_cache.TryGetValue<List<string>>(cacheKey, out var cachedStates))
        {
            return cachedStates;
        }

        try
        {
            var url = $"{BaseUrl}/states";
            var response = await _httpClient.GetAsync(url, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                return GetDefaultStates();
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var states = JsonSerializer.Deserialize<List<string>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (states == null || !states.Any())
            {
                return GetDefaultStates();
            }

            _cache.Set(cacheKey, states, TimeSpan.FromDays(7)); // States don't change often
            
            return states;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching available states");
            return GetDefaultStates();
        }
    }

    public async Task<List<string>> GetDistrictsAsync(string state, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"fuel_price_districts_{state}".ToLowerInvariant();
        
        if (_cache.TryGetValue<List<string>>(cacheKey, out var cachedDistricts))
        {
            return cachedDistricts;
        }

        try
        {
            var normalizedState = NormalizeStateName(state);
            var url = $"{BaseUrl}/{normalizedState}/districts";
            
            var response = await _httpClient.GetAsync(url, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                return new List<string> { "Default" };
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var districts = JsonSerializer.Deserialize<List<string>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (districts == null || !districts.Any())
            {
                return new List<string> { "Default" };
            }

            _cache.Set(cacheKey, districts, TimeSpan.FromDays(7));
            
            return districts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching districts for {State}", state);
            return new List<string> { "Default" };
        }
    }

    private string NormalizeStateName(string state)
    {
        // Convert to URL-friendly format
        return state.Replace(" ", "%20").Replace("&", "%26");
    }

    private string NormalizeDistrictName(string district)
    {
        // Convert to URL-friendly format
        return district.Replace(" ", "%20").ToUpperInvariant();
    }

    private FuelPriceResponse GetFallbackPrice(string state, string district)
    {
        // Fallback prices based on major Indian cities (approximate averages)
        var fallbackPrices = new Dictionary<string, (decimal petrol, decimal diesel, decimal cng)>
        {
            { "delhi", (94.77m, 87.67m, 75.61m) },
            { "mumbai", (106.31m, 94.27m, 76.00m) },
            { "bangalore", (102.86m, 88.94m, 80.00m) },
            { "chennai", (100.75m, 92.34m, 78.00m) },
            { "kolkata", (104.95m, 89.79m, 79.00m) },
            { "hyderabad", (109.66m, 97.82m, 82.00m) },
            { "pune", (106.61m, 92.28m, 76.50m) },
            { "ahmedabad", (96.23m, 89.23m, 74.00m) }
        };

        var stateKey = state.ToLowerInvariant();
        var prices = fallbackPrices.ContainsKey(stateKey) 
            ? fallbackPrices[stateKey] 
            : (95.50m, 88.50m, 75.00m); // National average

        _logger.LogInformation("Using fallback prices for {State}/{District}", state, district);

        return new FuelPriceResponse(
            District: district,
            State: state,
            PetrolPrice: prices.Item1,
            DieselPrice: prices.Item2,
            CngPrice: prices.Item3,
            Currency: "INR",
            FetchedAt: DateTime.UtcNow,
            Source: "Fallback (Approximate)"
        );
    }

    private List<string> GetDefaultStates()
    {
        return new List<string>
        {
            "Delhi", "Maharashtra", "Karnataka", "Tamil Nadu", "West Bengal",
            "Telangana", "Gujarat", "Rajasthan", "Uttar Pradesh", "Madhya Pradesh",
            "Punjab", "Haryana", "Kerala", "Andhra Pradesh", "Odisha"
        };
    }
}
