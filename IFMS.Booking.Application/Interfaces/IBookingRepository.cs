using IFMS.Booking.Domain.Entities;

namespace IFMS.Booking.Application.Interfaces;

public interface IBookingRepository
{
    Task<Domain.Entities.Booking?> GetByIdAsync(Guid bookingId);
    Task<Domain.Entities.Booking?> GetByTokenCodeAsync(string tokenCode);
    Task<List<Domain.Entities.Booking>> GetByCustomerIdAsync(Guid customerId);
    Task<List<Domain.Entities.Booking>> GetByStationIdAsync(Guid stationId);
    Task AddAsync(Domain.Entities.Booking booking);
    Task UpdateAsync(Domain.Entities.Booking booking);
    Task<List<Domain.Entities.Booking>> GetExpiredPendingBookingsAsync();
    Task SaveChangesAsync();
    Task<List<Domain.Entities.Booking>> GetByStationIdWithDateFilterAsync(Guid stationId, DateTime? from, DateTime? to);
    Task<List<Domain.Entities.Booking>> GetByCustomerIdWithDateFilterAsync(Guid customerId, DateTime? from, DateTime? to);
}
