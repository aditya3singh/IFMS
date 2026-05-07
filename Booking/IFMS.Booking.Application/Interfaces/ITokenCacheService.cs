namespace IFMS.Booking.Application.Interfaces;

public interface ITokenCacheService
{
    Task StoreTokenAsync(string tokenCode, object bookingDetails, TimeSpan expiry);
    Task<string?> GetTokenAsync(string tokenCode);
    Task DeleteTokenAsync(string tokenCode);
}
