using FsCheck;
using FsCheck.Xunit;
using Xunit;
using StationEntity = IFMS.Station.Domain.Entities.Station;

namespace IFMS.Station.Domain.Tests;

/// <summary>
/// Property-based tests for Station creation
/// **Validates: Requirements 4.9, 6.1, 6.2, 6.3, 6.4**
/// </summary>
public class StationCreationPropertyTests
{
    /// <summary>
    /// Property 1: Station Creation Initializes All Required Fields
    /// 
    /// For any valid station creation request with name, license number, city, state, 
    /// latitude, and longitude, creating a station should result in a new station entity 
    /// with a non-empty Guid ID, IsActive set to true, and both CreatedAt and UpdatedAt 
    /// set to the current timestamp.
    /// </summary>
    [Property(MaxTest = 100)]
    [Trait("Feature", "station-management-service")]
    [Trait("Property", "Property 1: Station Creation Initializes All Required Fields")]
    public void StationCreation_InitializesAllRequiredFields(
        NonEmptyString nameWrapper,
        NonEmptyString licenseNumberWrapper,
        NonEmptyString cityWrapper,
        NonEmptyString stateWrapper)
    {
        // Generate valid inputs within constraints
        var name = new string(nameWrapper.Get.Take(200).ToArray()).Trim();
        var licenseNumber = new string(licenseNumberWrapper.Get.Take(50).ToArray()).Trim();
        var city = new string(cityWrapper.Get.Take(100).ToArray()).Trim();
        var state = new string(stateWrapper.Get.Take(100).ToArray()).Trim();
        
        // Generate valid coordinates
        var random = new Random();
        var latitude = (decimal)(random.NextDouble() * 180 - 90);
        var longitude = (decimal)(random.NextDouble() * 360 - 180);
        
        // Skip if any string is empty after trimming
        if (string.IsNullOrWhiteSpace(name) || 
            string.IsNullOrWhiteSpace(licenseNumber) || 
            string.IsNullOrWhiteSpace(city) || 
            string.IsNullOrWhiteSpace(state))
        {
            return;
        }
        
        // Record the time before creation
        var beforeCreation = DateTime.UtcNow;
        
        // Act: Create the station
        var station = StationEntity.Create(name, licenseNumber, city, state, latitude, longitude);
        
        // Record the time after creation
        var afterCreation = DateTime.UtcNow;
        
        // Assert: Verify all required fields are initialized correctly
        Assert.NotEqual(Guid.Empty, station.Id);
        Assert.True(station.IsActive);
        Assert.InRange(station.CreatedAt, beforeCreation, afterCreation);
        Assert.InRange(station.UpdatedAt, beforeCreation, afterCreation);
        Assert.Equal(name, station.Name);
        Assert.Equal(licenseNumber, station.LicenseNumber);
        Assert.Equal(city, station.City);
        Assert.Equal(state, station.State);
        Assert.Equal(latitude, station.Latitude);
        Assert.Equal(longitude, station.Longitude);
    }
}
