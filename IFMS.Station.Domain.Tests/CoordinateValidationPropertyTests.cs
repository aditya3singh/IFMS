using FsCheck;
using FsCheck.Xunit;
using Xunit;
using StationEntity = IFMS.Station.Domain.Entities.Station;

namespace IFMS.Station.Domain.Tests;

/// <summary>
/// Property-based tests for coordinate validation
/// **Validates: Requirements 6.9, 6.10, 9.8, 9.9**
/// </summary>
public class CoordinateValidationPropertyTests
{
    /// <summary>
    /// Property 2: Coordinate Validation Rejects Invalid Values
    /// 
    /// For any station creation or update request, if the latitude is outside the range [-90, 90] 
    /// or the longitude is outside the range [-180, 180], the operation should fail with a validation error.
    /// </summary>
    [Property(MaxTest = 100)]
    [Trait("Feature", "station-management-service")]
    [Trait("Property", "Property 2: Coordinate Validation Rejects Invalid Values")]
    public void StationCreate_RejectsInvalidLatitude(
        NonEmptyString nameWrapper,
        NonEmptyString licenseNumberWrapper,
        NonEmptyString cityWrapper,
        NonEmptyString stateWrapper)
    {
        // Generate valid inputs for other fields
        var name = new string(nameWrapper.Get.Take(200).ToArray()).Trim();
        var licenseNumber = new string(licenseNumberWrapper.Get.Take(50).ToArray()).Trim();
        var city = new string(cityWrapper.Get.Take(100).ToArray()).Trim();
        var state = new string(stateWrapper.Get.Take(100).ToArray()).Trim();
        
        // Skip if any string is empty after trimming
        if (string.IsNullOrWhiteSpace(name) || 
            string.IsNullOrWhiteSpace(licenseNumber) || 
            string.IsNullOrWhiteSpace(city) || 
            string.IsNullOrWhiteSpace(state))
        {
            return;
        }
        
        // Generate invalid latitude (outside [-90, 90])
        var random = new Random();
        var invalidLatitude = random.Next(0, 2) == 0 
            ? (decimal)(random.NextDouble() * 1000 + 90.000001)  // Above 90
            : (decimal)(random.NextDouble() * -1000 - 90.000001); // Below -90
        
        // Generate valid longitude
        var validLongitude = (decimal)(random.NextDouble() * 360 - 180);
        
        // Act & Assert: Verify ArgumentException is thrown
        var exception = Assert.Throws<ArgumentException>(() =>
            StationEntity.Create(name, licenseNumber, city, state, invalidLatitude, validLongitude));
        
        Assert.Contains("Latitude must be between -90 and 90", exception.Message);
    }
    
    [Property(MaxTest = 100)]
    [Trait("Feature", "station-management-service")]
    [Trait("Property", "Property 2: Coordinate Validation Rejects Invalid Values")]
    public void StationCreate_RejectsInvalidLongitude(
        NonEmptyString nameWrapper,
        NonEmptyString licenseNumberWrapper,
        NonEmptyString cityWrapper,
        NonEmptyString stateWrapper)
    {
        // Generate valid inputs for other fields
        var name = new string(nameWrapper.Get.Take(200).ToArray()).Trim();
        var licenseNumber = new string(licenseNumberWrapper.Get.Take(50).ToArray()).Trim();
        var city = new string(cityWrapper.Get.Take(100).ToArray()).Trim();
        var state = new string(stateWrapper.Get.Take(100).ToArray()).Trim();
        
        // Skip if any string is empty after trimming
        if (string.IsNullOrWhiteSpace(name) || 
            string.IsNullOrWhiteSpace(licenseNumber) || 
            string.IsNullOrWhiteSpace(city) || 
            string.IsNullOrWhiteSpace(state))
        {
            return;
        }
        
        // Generate valid latitude
        var random = new Random();
        var validLatitude = (decimal)(random.NextDouble() * 180 - 90);
        
        // Generate invalid longitude (outside [-180, 180])
        var invalidLongitude = random.Next(0, 2) == 0 
            ? (decimal)(random.NextDouble() * 1000 + 180.000001)  // Above 180
            : (decimal)(random.NextDouble() * -1000 - 180.000001); // Below -180
        
        // Act & Assert: Verify ArgumentException is thrown
        var exception = Assert.Throws<ArgumentException>(() =>
            StationEntity.Create(name, licenseNumber, city, state, validLatitude, invalidLongitude));
        
        Assert.Contains("Longitude must be between -180 and 180", exception.Message);
    }

    [Property(MaxTest = 100)]
    [Trait("Feature", "station-management-service")]
    [Trait("Property", "Property 2: Coordinate Validation Rejects Invalid Values")]
    public void StationUpdate_RejectsInvalidLatitude(
        NonEmptyString nameWrapper,
        NonEmptyString licenseNumberWrapper,
        NonEmptyString cityWrapper,
        NonEmptyString stateWrapper)
    {
        // Generate valid inputs for initial station creation
        var name = new string(nameWrapper.Get.Take(200).ToArray()).Trim();
        var licenseNumber = new string(licenseNumberWrapper.Get.Take(50).ToArray()).Trim();
        var city = new string(cityWrapper.Get.Take(100).ToArray()).Trim();
        var state = new string(stateWrapper.Get.Take(100).ToArray()).Trim();
        
        // Skip if any string is empty after trimming
        if (string.IsNullOrWhiteSpace(name) || 
            string.IsNullOrWhiteSpace(licenseNumber) || 
            string.IsNullOrWhiteSpace(city) || 
            string.IsNullOrWhiteSpace(state))
        {
            return;
        }
        
        // Create a valid station first
        var random = new Random();
        var validLatitude = (decimal)(random.NextDouble() * 180 - 90);
        var validLongitude = (decimal)(random.NextDouble() * 360 - 180);
        
        var station = StationEntity.Create(name, licenseNumber, city, state, validLatitude, validLongitude);
        
        // Generate invalid latitude for update (outside [-90, 90])
        var invalidLatitude = random.Next(0, 2) == 0 
            ? (decimal)(random.NextDouble() * 1000 + 90.000001)  // Above 90
            : (decimal)(random.NextDouble() * -1000 - 90.000001); // Below -90
        
        // Act & Assert: Verify ArgumentException is thrown on update
        var exception = Assert.Throws<ArgumentException>(() =>
            station.Update(name, licenseNumber, city, state, invalidLatitude, validLongitude));
        
        Assert.Contains("Latitude must be between -90 and 90", exception.Message);
    }
    
    [Property(MaxTest = 100)]
    [Trait("Feature", "station-management-service")]
    [Trait("Property", "Property 2: Coordinate Validation Rejects Invalid Values")]
    public void StationUpdate_RejectsInvalidLongitude(
        NonEmptyString nameWrapper,
        NonEmptyString licenseNumberWrapper,
        NonEmptyString cityWrapper,
        NonEmptyString stateWrapper)
    {
        // Generate valid inputs for initial station creation
        var name = new string(nameWrapper.Get.Take(200).ToArray()).Trim();
        var licenseNumber = new string(licenseNumberWrapper.Get.Take(50).ToArray()).Trim();
        var city = new string(cityWrapper.Get.Take(100).ToArray()).Trim();
        var state = new string(stateWrapper.Get.Take(100).ToArray()).Trim();
        
        // Skip if any string is empty after trimming
        if (string.IsNullOrWhiteSpace(name) || 
            string.IsNullOrWhiteSpace(licenseNumber) || 
            string.IsNullOrWhiteSpace(city) || 
            string.IsNullOrWhiteSpace(state))
        {
            return;
        }
        
        // Create a valid station first
        var random = new Random();
        var validLatitude = (decimal)(random.NextDouble() * 180 - 90);
        var validLongitude = (decimal)(random.NextDouble() * 360 - 180);
        
        var station = StationEntity.Create(name, licenseNumber, city, state, validLatitude, validLongitude);
        
        // Generate invalid longitude for update (outside [-180, 180])
        var invalidLongitude = random.Next(0, 2) == 0 
            ? (decimal)(random.NextDouble() * 1000 + 180.000001)  // Above 180
            : (decimal)(random.NextDouble() * -1000 - 180.000001); // Below -180
        
        // Act & Assert: Verify ArgumentException is thrown on update
        var exception = Assert.Throws<ArgumentException>(() =>
            station.Update(name, licenseNumber, city, state, validLatitude, invalidLongitude));
        
        Assert.Contains("Longitude must be between -180 and 180", exception.Message);
    }
}
