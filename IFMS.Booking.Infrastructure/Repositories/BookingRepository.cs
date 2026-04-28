using IFMS.Booking.Application.Interfaces;
using IFMS.Booking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IFMS.Booking.Infrastructure.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly BookingDbContext _context;

    public BookingRepository(BookingDbContext context)
    {
        _context = context;
    }

    public async Task<Domain.Entities.Booking?> GetByIdAsync(Guid bookingId)
    {
        return await _context.Bookings.FindAsync(bookingId);
    }

    public async Task<Domain.Entities.Booking?> GetByTokenCodeAsync(string tokenCode)
    {
        return await _context.Bookings
            .FirstOrDefaultAsync(b => b.TokenCode == tokenCode);
    }

    public async Task<List<Domain.Entities.Booking>> GetByCustomerIdAsync(Guid customerId)
    {
        return await _context.Bookings
            .Where(b => b.CustomerId == customerId)
            .OrderByDescending(b => b.BookedAt)
            .ToListAsync();
    }

    public async Task<List<Domain.Entities.Booking>> GetByStationIdAsync(Guid stationId)
    {
        return await _context.Bookings
            .Where(b => b.StationId == stationId)
            .OrderByDescending(b => b.BookedAt)
            .ToListAsync();
    }

    public async Task AddAsync(Domain.Entities.Booking booking)
    {
        await _context.Bookings.AddAsync(booking);
    }

    public async Task UpdateAsync(Domain.Entities.Booking booking)
    {
        _context.Bookings.Update(booking);
        await Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<List<Domain.Entities.Booking>> GetExpiredPendingBookingsAsync()
    {
        return await _context.Bookings
            .Where(b => b.TokenStatus == "PENDING" && b.ExpiresAt <= DateTime.UtcNow)
            .ToListAsync();
    }

    public async Task<List<Domain.Entities.Booking>> GetByStationIdWithDateFilterAsync(Guid stationId, DateTime? from, DateTime? to)
    {
        var q = _context.Bookings.Where(b => b.StationId == stationId);
        if (from.HasValue) q = q.Where(b => b.BookedAt >= from.Value);
        if (to.HasValue) q = q.Where(b => b.BookedAt <= to.Value);
        return await q.OrderByDescending(b => b.BookedAt).ToListAsync();
    }

    public async Task<List<Domain.Entities.Booking>> GetByCustomerIdWithDateFilterAsync(Guid customerId, DateTime? from, DateTime? to)
    {
        var q = _context.Bookings.Where(b => b.CustomerId == customerId);
        if (from.HasValue) q = q.Where(b => b.BookedAt >= from.Value);
        if (to.HasValue) q = q.Where(b => b.BookedAt <= to.Value);
        return await q.OrderByDescending(b => b.BookedAt).ToListAsync();
    }
}
