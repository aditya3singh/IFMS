using FsCheck;
using FsCheck.Xunit;
using Xunit;
using StationEntity = IFMS.Station.Domain.Entities.Station;

namespace IFMS.Station.Domain.Tests;

/// <summary>
/// Property-based tests for input validation
/// **Validates: Requirements 6.5, 6.6, 6.8, 9.5, 9.6**
/// </summary>
public class InputValidationPropertyTests
{
    /// <summary>
    /// Property 3: Input Validation Enforces Field Constraints
    /// 
    /// For any station creation or update request, if the name is empty or exceeds 200 characters, 
    /// or the license number is empty or exceeds 50 characters, or the city or state is empty, 
    /// the operation should fail with a validation error.
    /// </summary>
    [Property(MaxTest = 100)]
    [Trait("Feature", "station-management-service")]
    [Trait("Property", "Property 3: Input Validation Enforces Field Constraints")]
    public void StationCreate_RejectsEmptyName()
    {
        // Arrange: Generate valid inputs for other fields
        var licenseNumber = GenerateValidString(1, 50);
        var city = GenerateValidString(1, 100);
        var state = GenerateValidString(1, 100);
        var latitude = GenerateValidLatitude();
        var longitude = GenerateValidLongitude();
        
        // Act & Assert: Verify ArgumentException is thrown for empty name
        var exception = Assert.Throws<ArgumentException>(() =>
            StationEntity.Create("", licenseNumber, city, state, latitude, longitude));
        
        Assert.Contains("Name", exception.Message, StringComparison.OrdinalIgnoreCase);
    }
    
    [Property(MaxTest = 100)]
    [Trait("Feature", "station-management-service")]
    [Trait("Property", "Property 3: Input Validation Enforces Field Constraints")]
    public void StationCreate_RejectsNameExceeding200Characters(PositiveInt extraLength)
    {
        // Arrange: Generate a name that exceeds 200 characters
        var invalidName = new string('A', 201 + (extraLength.Get % 100));
        var licenseNumber = GenerateValidString(1, 50);
        var city = GenerateValidString(1, 100);
        var state = GenerateValidString(1, 100);
        var latitude = GenerateValidLatitude();
        var longitude = GenerateValidLongitude();
        
        // Act & Assert: Verify ArgumentException is thrown
        var exception = Assert.Throws<ArgumentException>(() =>
            StationEntity.Create(invalidName, licenseNumber, city, state, latitude, longitude));
        
        Assert.Contains("Name", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("200", exception.Message);
    }
    
    [Property(MaxTest = 100)]
    [Trait("Feature", "station-management-service")]
    [Trait("Property", "Property 3: Input Validation Enforces Field Constraints")]
    public void StationCreate_RejectsEmptyLicenseNumber()
    {
        // Arrange: Generate valid inputs for other fields
        var name = GenerateValidString(1, 200);
        var city = GenerateValidString(1, 100);
        var state = GenerateValidString(1, 100);
        var latitude = GenerateValidLatitude();
        var longitude = GenerateValidLongitude();
        
        // Act & Assert: Verify ArgumentException is thrown for empty license number
        var exception = Assert.Throws<ArgumentException>(() =>
            StationEntity.Create(name, "", city, state, latitude, longitude));
        
        Assert.Contains("LicenseNumber", exception.Message, StringComparison.OrdinalIgnoreCase);
    }
    
    [Property(MaxTest = 100)]
    [Trait("Feature", "station-management-service")]
    [Trait("Property", "Property 3: Input Validation Enforces Field Constraints")]
    public void StationCreate_RejectsLicenseNumberExceeding50Characters(PositiveInt extraLength)
    {
        // Arrange: Generate a license number that exceeds 50 characters
        var invalidLicenseNumber = new string('B', 51 + (extraLength.Get % 50));
        var name = GenerateValidString(1, 200);
        var city = GenerateValidString(1, 100);
        var state = GenerateValidString(1, 100);
        var latitude = GenerateValidLatitude();
        var longitude = GenerateValidLongitude();
        
        // Act & Assert: Verify ArgumentException is thrown
        var exception = Assert.Throws<ArgumentException>(() =>
            StationEntity.Create(name, invalidLicenseNumber, city, state, latitude, longitude));
        
        Assert.Contains("LicenseNumber", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("50", exception.Message);
    }
    
    [Property(MaxTest = 100)]
    [Trait("Feature", "station-management-service")]
    [Trait("Property", "Property 3: Input Validation Enforces Field Constraints")]
    public void StationCreate_RejectsEmptyCity()
    {
        // Arrange: Generate valid inputs for other fields
        var name = GenerateValidString(1, 200);
        var licenseNumber = GenerateValidString(1, 50);
        var state = GenerateValidString(1, 100);
        var latitude = GenerateValidLatitude();
        var longitude = GenerateValidLongitude();
        
        // Act & Assert: Verify ArgumentException is thrown for empty city
        var exception = Assert.Throws<ArgumentException>(() =>
            StationEntity.Create(name, licenseNumber, "", state, latitude, longitude));
        
        Assert.Contains("City", exception.Message, StringComparison.OrdinalIgnoreCase);
    }
    
    [Property(MaxTest = 100)]
    [Trait("Feature", "station-management-service")]
    [Trait("Property", "Property 3: Input Validation Enforces Field Constraints")]
    public void StationCreate_RejectsEmptyState()
    {
        // Arrange: Generate valid inputs for other fields
        var name = GenerateValidString(1, 200);
        var licenseNumber = GenerateValidString(1, 50);
        var city = GenerateValidString(1, 100);
        var latitude = GenerateValidLatitude();
        var longitude = GenerateValidLongitude();
        
        // Act & Assert: Verify ArgumentException is thrown for empty state
        var exception = Assert.Throws<ArgumentException>(() =>
            StationEntity.Create(name, licenseNumber, city, "", latitude, longitude));
        
        Assert.Contains("State", exception.Message, StringComparison.OrdinalIgnoreCase);
    }
    
    [Property(MaxTest = 100)]
    [Trait("Feature", "station-management-service")]
    [Trait("Property", "Property 3: Input Validation Enforces Field Constraints")]
    public void StationUpdate_RejectsEmptyName()
    {
        // Arrange: Create a valid station first
        var name = GenerateValidString(1, 200);
        var licenseNumber = GenerateValidString(1, 50);
        var city = GenerateValidString(1, 100);
        var state = GenerateValidString(1, 100);
        var latitude = GenerateValidLatitude();
        var longitude = GenerateValidLongitude();
        
        var station = StationEntity.Create(name, licenseNumber, city, state, latitude, longitude);
        
        // Act & Assert: Verify ArgumentException is thrown for empty name on update
        var exception = Assert.Throws<ArgumentException>(() =>
            station.Update("", licenseNumber, city, state, latitude, longitude));
        
        Assert.Contains("Name", exception.Message, StringComparison.OrdinalIgnoreCase);
    }
    
    [Property(MaxTest = 100)]
    [Trait("Feature", "station-management-service")]
    [Trait("Property", "Property 3: Input Validation Enforces Field Constraints")]
    public void StationUpdate_RejectsNameExceeding200Characters(PositiveInt extraLength)
    {
        // Arrange: Create a valid station first
        var name = GenerateValidString(1, 200);
        var licenseNumber = GenerateValidString(1, 50);
        var city = GenerateValidString(1, 100);
        var state = GenerateValidString(1, 100);
        var latitude = GenerateValidLatitude();
        var longitude = GenerateValidLongitude();
        
        var station = StationEntity.Create(name, licenseNumber, city, state, latitude, longitude);
        
        // Generate a name that exceeds 200 characters
        var invalidName = new string('A', 201 + (extraLength.Get % 100));
        
        // Act & Assert: Verify ArgumentException is thrown
        var exception = Assert.Throws<ArgumentException>(() =>
            station.Update(invalidName, licenseNumber, city, state, latitude, longitude));
        
        Assert.Contains("Name", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("200", exception.Message);
    }
    
    [Property(MaxTest = 100)]
    [Trait("Feature", "station-management-service")]
    [Trait("Property", "Property 3: Input Validation Enforces Field Constraints")]
    public void StationUpdate_RejectsEmptyLicenseNumber()
    {
        // Arrange: Create a valid station first
        var name = GenerateValidString(1, 200);
        var licenseNumber = GenerateValidString(1, 50);
        var city = GenerateValidString(1, 100);
        var state = GenerateValidString(1, 100);
        var latitude = GenerateValidLatitude();
        var longitude = GenerateValidLongitude();
        
        var station = StationEntity.Create(name, licenseNumber, city, state, latitude, longitude);
        
        // Act & Assert: Verify ArgumentException is thrown for empty license number on update
        var exception = Assert.Throws<ArgumentException>(() =>
            station.Update(name, "", city, state, latitude, longitude));
        
        Assert.Contains("LicenseNumber", exception.Message, StringComparison.OrdinalIgnoreCase);
    }
    
    [Property(MaxTest = 100)]
    [Trait("Feature", "station-management-service")]
    [Trait("Property", "Property 3: Input Validation Enforces Field Constraints")]
    public void StationUpdate_RejectsLicenseNumberExceeding50Characters(PositiveInt extraLength)
    {
        // Arrange: Create a valid station first
        var name = GenerateValidString(1, 200);
        var licenseNumber = GenerateValidString(1, 50);
        var city = GenerateValidString(1, 100);
        var state = GenerateValidString(1, 100);
        var latitude = GenerateValidLatitude();
        var longitude = GenerateValidLongitude();
        
        var station = StationEntity.Create(name, licenseNumber, city, state, latitude, longitude);
        
        // Generate a license number that exceeds 50 characters
        var invalidLicenseNumber = new string('B', 51 + (extraLength.Get % 50));
        
        // Act & Assert: Verify ArgumentException is thrown
        var exception = Assert.Throws<ArgumentException>(() =>
            station.Update(name, invalidLicenseNumber, city, state, latitude, longitude));
        
        Assert.Contains("LicenseNumber", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("50", exception.Message);
    }
    
    // Helper methods to generate valid test data
    private static string GenerateValidString(int minLength, int maxLength)
    {
        var random = new Random();
        var length = random.Next(minLength, Math.Min(maxLength, 20)); // Keep strings reasonably short for testing
        return new string(Enumerable.Range(0, length).Select(_ => (char)random.Next('A', 'Z' + 1)).ToArray());
    }
    
    private static decimal GenerateValidLatitude()
    {
        var random = new Random();
        return (decimal)(random.NextDouble() * 180 - 90);
    }
    
    private static decimal GenerateValidLongitude()
    {
        var random = new Random();
        return (decimal)(random.NextDouble() * 360 - 180);
    }
}
