namespace IFMS.Station.Application.Pricing;

/// <summary>
/// Fixed fuel pricing for Indian states (April 2026 rates).
/// Prices in ₹ per litre for Petrol/Diesel, ₹ per kg for CNG, ₹ per kWh for EV.
/// </summary>
public static class RegionalFuelPriceQuote
{
    public sealed record Quote(decimal PricePerUnit, string UnitLabel, string AreaSummary);

    /// <summary>Fixed Petrol prices by state (₹/litre)</summary>
    private static readonly Dictionary<string, decimal> StatePetrolBase = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Maharashtra"] = 106.31m,    // Mumbai, Pune
        ["Delhi"] = 96.72m,           // New Delhi
        ["Karnataka"] = 102.86m,      // Bengaluru
        ["Tamil Nadu"] = 102.63m,     // Chennai
        ["Gujarat"] = 96.32m,         // Ahmedabad
        ["Rajasthan"] = 108.48m,      // Jaipur
        ["Uttar Pradesh"] = 96.57m,   // Lucknow
        ["West Bengal"] = 106.03m,    // Kolkata
        ["Telangana"] = 109.66m,      // Hyderabad
        ["Kerala"] = 107.71m,         // Kochi
        ["Punjab"] = 101.68m,         // Chandigarh
        ["Haryana"] = 96.79m,         // Gurgaon
        ["Madhya Pradesh"] = 107.23m, // Indore
        ["Bihar"] = 107.24m,          // Patna
        ["Odisha"] = 103.19m          // Bhubaneswar
    };

    /// <summary>Fixed Diesel prices by state (₹/litre)</summary>
    private static readonly Dictionary<string, decimal> StateDieselBase = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Maharashtra"] = 94.27m,     // Mumbai, Pune
        ["Delhi"] = 89.62m,           // New Delhi
        ["Karnataka"] = 88.94m,       // Bengaluru
        ["Tamil Nadu"] = 94.24m,      // Chennai
        ["Gujarat"] = 92.37m,         // Ahmedabad
        ["Rajasthan"] = 93.72m,       // Jaipur
        ["Uttar Pradesh"] = 89.87m,   // Lucknow
        ["West Bengal"] = 92.76m,     // Kolkata
        ["Telangana"] = 97.82m,       // Hyderabad
        ["Kerala"] = 96.00m,          // Kochi
        ["Punjab"] = 86.26m,          // Chandigarh
        ["Haryana"] = 89.55m,         // Gurgaon
        ["Madhya Pradesh"] = 93.90m,  // Indore
        ["Bihar"] = 94.04m,           // Patna
        ["Odisha"] = 92.26m           // Bhubaneswar
    };

    /// <summary>Fixed CNG prices by state (₹/kg)</summary>
    private static readonly Dictionary<string, decimal> StateCNGBase = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Maharashtra"] = 75.00m,     // Mumbai, Pune
        ["Delhi"] = 75.61m,           // New Delhi
        ["Karnataka"] = 80.00m,       // Bengaluru
        ["Tamil Nadu"] = 82.00m,      // Chennai
        ["Gujarat"] = 72.00m,         // Ahmedabad
        ["Rajasthan"] = 78.00m,       // Jaipur
        ["Uttar Pradesh"] = 79.00m,   // Lucknow
        ["West Bengal"] = 81.00m,     // Kolkata
        ["Telangana"] = 83.00m,       // Hyderabad
        ["Kerala"] = 85.00m,          // Kochi
        ["Punjab"] = 76.00m,          // Chandigarh
        ["Haryana"] = 75.50m,         // Gurgaon
        ["Madhya Pradesh"] = 77.00m,  // Indore
        ["Bihar"] = 80.00m,           // Patna
        ["Odisha"] = 79.00m           // Bhubaneswar
    };

    /// <summary>Get Petrol price for state (₹/litre)</summary>
    private static decimal GetPetrolPrice(string? state)
    {
        if (!string.IsNullOrWhiteSpace(state) && StatePetrolBase.TryGetValue(state.Trim(), out var price))
            return price;
        return 102.50m; // National average
    }

    /// <summary>Get Diesel price for state (₹/litre)</summary>
    private static decimal GetDieselPrice(string? state)
    {
        if (!string.IsNullOrWhiteSpace(state) && StateDieselBase.TryGetValue(state.Trim(), out var price))
            return price;
        return 92.00m; // National average
    }

    /// <summary>Get CNG price for state (₹/kg)</summary>
    private static decimal GetCNGPrice(string? state)
    {
        if (!string.IsNullOrWhiteSpace(state) && StateCNGBase.TryGetValue(state.Trim(), out var price))
            return price;
        return 78.00m; // National average
    }

    /// <summary>Get EV charging price (₹/kWh) - uniform across India</summary>
    private static decimal GetEVPrice()
    {
        return 8.50m; // Fixed rate for EV charging
    }

    public static Quote GetQuote(string? state, string? city, string? fuelType)
    {
        var ft = (fuelType ?? "Petrol").Trim();
        var stateName = state?.Trim() ?? "India";
        var cityName = city?.Trim() ?? "—";
        var area = $"{stateName} / {cityName}";

        if (string.Equals(ft, "EV", StringComparison.OrdinalIgnoreCase))
        {
            return new Quote(GetEVPrice(), "kWh", area);
        }

        if (string.Equals(ft, "Diesel", StringComparison.OrdinalIgnoreCase))
        {
            return new Quote(GetDieselPrice(state), "litre", area);
        }

        if (string.Equals(ft, "CNG", StringComparison.OrdinalIgnoreCase))
        {
            return new Quote(GetCNGPrice(state), "kg", area);
        }

        // Petrol default
        return new Quote(GetPetrolPrice(state), "litre", area);
    }
}
