using IFMS.Booking.Application.DTOs;
using IFMS.Booking.Application.Interfaces;
using IFMS.Booking.Application.Options;
using IFMS.Messaging.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace IFMS.Booking.Application.Commands;

public class BookingCommandHandler
{
    private readonly IBookingRepository _bookingRepository;
    private readonly ITokenCacheService _tokenCacheService;
    private readonly IKycSessionStore _kycSessionStore;
    private readonly IOptions<BookingFlowOptions> _bookingFlowOptions;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IPublishEndpoint _publish;
    private readonly ILogger<BookingCommandHandler> _logger;

    public BookingCommandHandler(
        IBookingRepository bookingRepository,
        ITokenCacheService tokenCacheService,
        IKycSessionStore kycSessionStore,
        IOptions<BookingFlowOptions> bookingFlowOptions,
        IHttpClientFactory httpClientFactory,
        IPublishEndpoint publish,
        ILogger<BookingCommandHandler> logger)
    {
        _bookingRepository = bookingRepository;
        _tokenCacheService = tokenCacheService;
        _kycSessionStore = kycSessionStore;
        _bookingFlowOptions = bookingFlowOptions;
        _httpClientFactory = httpClientFactory;
        _publish = publish;
        _logger = logger;
    }

    public async Task<BookingResponse> CreateBookingAsync(
        CreateBookingRequest request,
        Guid authenticatedCustomerId,
        CancellationToken cancellationToken = default)
    {
        if (request.CustomerId != authenticatedCustomerId)
            throw new InvalidOperationException("Customer identity mismatch.");

        if (_bookingFlowOptions.Value.RequireKycVerification)
        {
            if (string.IsNullOrWhiteSpace(request.KycSessionId))
            {
                throw new InvalidOperationException(
                    "Identity verification is required before booking. Verify PAN and Aadhaar, then continue.");
            }

            var consumed = await _kycSessionStore.TryConsumeAsync(request.KycSessionId.Trim(), authenticatedCustomerId, cancellationToken);
            if (!consumed)
            {
                throw new InvalidOperationException(
                    "Verification session is invalid or expired. Please verify your identity again.");
            }
        }

        var totalPaid = request.QuantityLiters * request.PricePerLitre;

        var booking = Domain.Entities.Booking.Create(
            request.CustomerId,
            request.StationId,
            request.FuelType,
            request.QuantityLiters,
            totalPaid,
            request.PaymentId,
            request.StationNumber,
            customerPhone: request.CustomerPhone ?? string.Empty,
            customerEmail: request.CustomerEmail ?? string.Empty
        );

        await _bookingRepository.AddAsync(booking);
        await _bookingRepository.SaveChangesAsync();

        // Store in Redis with 24h TTL for fast dealer validation
        var cacheData = new
        {
            booking.BookingId,
            booking.CustomerId,
            booking.StationId,
            booking.FuelType,
            booking.QuantityLiters,
            booking.TotalPaid,
            booking.BookedAt,
            booking.ExpiresAt
        };
        await _tokenCacheService.StoreTokenAsync(
            booking.TokenCode, cacheData, TimeSpan.FromHours(24));

        // Publish BookingCreated event → Notification API consumes it via RabbitMQ
        _ = _publish.Publish(new BookingCreated(
            BookingId:      booking.BookingId,
            CustomerId:     booking.CustomerId,
            StationId:      booking.StationId,
            FuelType:       booking.FuelType,
            QuantityLiters: booking.QuantityLiters,
            TotalPaid:      booking.TotalPaid,
            TokenCode:      booking.TokenCode,
            StationName:    request.StationName ?? request.StationId.ToString(),
            CustomerName:   request.CustomerName ?? $"Customer-{authenticatedCustomerId.ToString()[..8]}",
            CustomerEmail:  request.CustomerEmail ?? string.Empty,
            CustomerPhone:  request.CustomerPhone ?? string.Empty,
            BookedAt:       booking.BookedAt,
            ExpiresAt:      booking.ExpiresAt
        ), cancellationToken);

        return MapToResponse(booking);
    }

    public async Task<TokenValidationResponse> ValidateTokenAsync(string tokenCode)
    {
        if (string.IsNullOrWhiteSpace(tokenCode))
            throw new InvalidOperationException("Token invalid or expired");

        tokenCode = tokenCode.Trim();

        static void EnsureUsable(Domain.Entities.Booking b)
        {
            if (b.ExpiresAt <= DateTime.UtcNow)
                throw new InvalidOperationException("Token expired");
            if (b.TokenStatus == "USED")
                throw new InvalidOperationException("Token already used");
            if (b.TokenStatus == "CANCELLED")
                throw new InvalidOperationException("Token cancelled");
            if (b.TokenStatus == "EXPIRED")
                throw new InvalidOperationException("Token expired");
            if (b.TokenStatus != "PENDING")
                throw new InvalidOperationException($"Token not valid (status: {b.TokenStatus})");
        }

        // Try Redis first (fast path)
        var cached = await _tokenCacheService.GetTokenAsync(tokenCode);
        if (cached != null)
        {
            var booking = await _bookingRepository.GetByTokenCodeAsync(tokenCode);
            if (booking == null)
                throw new InvalidOperationException("Token not found in database");

            EnsureUsable(booking);

            return new TokenValidationResponse(
                booking.TokenCode,
                booking.CustomerId,
                booking.FuelType,
                booking.QuantityLiters,
                booking.TotalPaid,
                booking.BookedAt,
                booking.ExpiresAt
            );
        }

        // Fallback to DB when Redis is empty/unavailable.
        // This avoids false "invalid" when Redis wasn't populated or has been flushed.
        var dbBooking = await _bookingRepository.GetByTokenCodeAsync(tokenCode);
        if (dbBooking == null)
            throw new InvalidOperationException("Token invalid or expired");

        EnsureUsable(dbBooking);

        // Best-effort: repopulate cache for next validation.
        try
        {
            var cacheData = new
            {
                dbBooking.BookingId,
                dbBooking.CustomerId,
                dbBooking.StationId,
                dbBooking.FuelType,
                dbBooking.QuantityLiters,
                dbBooking.TotalPaid,
                dbBooking.BookedAt,
                dbBooking.ExpiresAt
            };
            var ttl = dbBooking.ExpiresAt - DateTime.UtcNow;
            if (ttl < TimeSpan.Zero) ttl = TimeSpan.Zero;
            await _tokenCacheService.StoreTokenAsync(dbBooking.TokenCode, cacheData, ttl);
        }
        catch
        {
            // Don't block validation if Redis is down.
        }

        return new TokenValidationResponse(
            dbBooking.TokenCode,
            dbBooking.CustomerId,
            dbBooking.FuelType,
            dbBooking.QuantityLiters,
            dbBooking.TotalPaid,
            dbBooking.BookedAt,
            dbBooking.ExpiresAt
        );
    }

    public async Task<BookingResponse> ConfirmBookingAsync(string tokenCode)
    {
        var booking = await _bookingRepository.GetByTokenCodeAsync(tokenCode);
        if (booking == null)
            throw new InvalidOperationException("Booking not found");

        if (booking.TokenStatus != "PENDING")
            throw new InvalidOperationException($"Booking cannot be confirmed. Current status: {booking.TokenStatus}");

        booking.MarkUsed();
        await _bookingRepository.UpdateAsync(booking);
        await _bookingRepository.SaveChangesAsync();

        // Remove from Redis
        await _tokenCacheService.DeleteTokenAsync(tokenCode);

        // Auto-create sales transaction
        await CreateSalesTransactionAsync(booking);

        // Publish BookingConfirmed event → Notification API sends SMS to customer
        _ = _publish.Publish(new BookingConfirmed(
            BookingId:      booking.BookingId,
            CustomerId:     booking.CustomerId,
            StationId:      booking.StationId,
            FuelType:       booking.FuelType,
            QuantityLiters: booking.QuantityLiters,
            TotalPaid:      booking.TotalPaid,
            TokenCode:      booking.TokenCode,
            CustomerPhone:  booking.CustomerPhone,
            CustomerEmail:  booking.CustomerEmail
        ));

        return MapToResponse(booking);
    }

    /// <summary>
    /// Automatically create sales transaction when booking is confirmed
    /// </summary>
    private async Task CreateSalesTransactionAsync(Domain.Entities.Booking booking)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("SalesAPI");
            
            var salesRequest = new
            {
                stationId = booking.StationId,
                fuelType = booking.FuelType,
                quantity = booking.QuantityLiters,
                pricePerLitre = booking.TotalPaid / booking.QuantityLiters,
                paymentMethod = "Token",
                customerName = !string.IsNullOrWhiteSpace(booking.CustomerEmail) 
                    ? booking.CustomerEmail 
                    : $"Customer-{booking.CustomerId.ToString().Substring(0, 8)}"
            };

            // Use the internal endpoint that doesn't require auth
            var response = await httpClient.PostAsJsonAsync("/api/Sales/internal/from-booking", salesRequest);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to create sales transaction for booking {BookingId}: {StatusCode} - {Error}", 
                    booking.BookingId, response.StatusCode, errorBody);
            }
            else
            {
                _logger.LogInformation("Sales transaction created successfully for booking {BookingId}", booking.BookingId);
            }
        }
        catch (Exception ex)
        {
            // Log error but don't fail the booking confirmation
            _logger.LogError(ex, "Error creating sales transaction for booking {BookingId}", booking.BookingId);
        }
    }

    public async Task<List<BookingResponse>> GetCustomerBookingsAsync(Guid customerId, Guid? stationId = null)
    {
        var bookings = await _bookingRepository.GetByCustomerIdAsync(customerId);
        if (stationId.HasValue)
            bookings = bookings.Where(b => b.StationId == stationId.Value).ToList();
        return bookings.Select(MapToResponse).ToList();
    }

    public async Task<List<BookingResponse>> GetBookingsForStationAsync(Guid stationId)
    {
        var bookings = await _bookingRepository.GetByStationIdAsync(stationId);
        return bookings.Select(MapToResponse).ToList();
    }

    public async Task<BookingResponse> CancelBookingAsync(string tokenCode)
    {
        var booking = await _bookingRepository.GetByTokenCodeAsync(tokenCode);
        if (booking == null)
            throw new InvalidOperationException("Booking not found");

        if (booking.TokenStatus != "PENDING")
            throw new InvalidOperationException($"Booking cannot be cancelled. Current status: {booking.TokenStatus}");

        booking.MarkCancelled();
        await _bookingRepository.UpdateAsync(booking);
        await _bookingRepository.SaveChangesAsync();

        // Remove from Redis
        await _tokenCacheService.DeleteTokenAsync(tokenCode);

        // Publish BookingCancelled event → Notification API sends SMS to customer
        _ = _publish.Publish(new BookingCancelled(
            BookingId:     booking.BookingId,
            CustomerId:    booking.CustomerId,
            TokenCode:     booking.TokenCode,
            CustomerPhone: booking.CustomerPhone
        ));

        return MapToResponse(booking);
    }

    public async Task<int> ExpirePendingBookingsAsync()
    {
        var expiredBookings = await _bookingRepository.GetExpiredPendingBookingsAsync();
        foreach (var booking in expiredBookings)
        {
            booking.MarkExpired();
            await _bookingRepository.UpdateAsync(booking);
            await _tokenCacheService.DeleteTokenAsync(booking.TokenCode);
        }

        if (expiredBookings.Count > 0)
            await _bookingRepository.SaveChangesAsync();

        return expiredBookings.Count;
    }

    public async Task<BookingResponse?> GetByIdAsync(Guid bookingId)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId);
        return booking == null ? null : MapToResponse(booking);
    }

    public async Task<BookingResponse?> GetByTokenAsync(string tokenCode)
    {
        var booking = await _bookingRepository.GetByTokenCodeAsync(tokenCode.Trim());
        return booking == null ? null : MapToResponse(booking);
    }

    public async Task<List<BookingResponse>> GetBookingsForStationFilteredAsync(Guid stationId, DateTime? from, DateTime? to)
    {
        var bookings = await _bookingRepository.GetByStationIdWithDateFilterAsync(stationId, from, to);
        return bookings.Select(MapToResponse).ToList();
    }

    public async Task<object> GetCustomerStatsAsync(Guid customerId)
    {
        var bookings = await _bookingRepository.GetByCustomerIdAsync(customerId);
        return new
        {
            totalBookings = bookings.Count,
            totalSpent = bookings.Where(b => b.TokenStatus == "USED").Sum(b => b.TotalPaid),
            pendingBookings = bookings.Count(b => b.TokenStatus == "PENDING"),
            usedBookings = bookings.Count(b => b.TokenStatus == "USED"),
            cancelledBookings = bookings.Count(b => b.TokenStatus == "CANCELLED"),
            expiredBookings = bookings.Count(b => b.TokenStatus == "EXPIRED"),
            mostUsedFuelType = bookings
                .GroupBy(b => b.FuelType)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault() ?? "N/A",
            mostVisitedStationId = bookings
                .GroupBy(b => b.StationId)
                .OrderByDescending(g => g.Count())
                .Select(g => (Guid?)g.Key)
                .FirstOrDefault()
        };
    }

    private static BookingResponse MapToResponse(Domain.Entities.Booking booking)
    {
        return new BookingResponse(
            booking.BookingId,
            booking.CustomerId,
            booking.StationId,
            booking.FuelType,
            booking.QuantityLiters,
            booking.TotalPaid,
            booking.TokenCode,
            booking.TokenStatus,
            booking.PaymentId,
            booking.BookedAt,
            booking.ExpiresAt,
            booking.UsedAt
        );
    }
}
