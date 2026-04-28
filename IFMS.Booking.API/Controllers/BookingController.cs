using IFMS.Booking.Application.Commands;
using IFMS.Booking.Application.DTOs;
using IFMS.Booking.Application.Interfaces;
using IFMS.Station.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IFMS.Booking.API.Controllers;

[ApiController]
[Route("api/booking")]
[Authorize]
public class BookingController : ControllerBase
{
    private readonly BookingCommandHandler _handler;
    private readonly KycVerificationHandler _kycHandler;
    private readonly IDealerAssignmentRepository _dealerAssignments;
    private readonly INotificationPublisher _notifications;
    private readonly ILogger<BookingController> _logger;

    public BookingController(
        BookingCommandHandler handler,
        KycVerificationHandler kycHandler,
        IDealerAssignmentRepository dealerAssignments,
        INotificationPublisher notifications,
        ILogger<BookingController> logger)
    {
        _handler = handler;
        _kycHandler = kycHandler;
        _dealerAssignments = dealerAssignments;
        _notifications = notifications;
        _logger = logger;
    }

    /// <summary>
    /// KYC: customer sends <c>documentType</c> <c>Pan</c> or <c>Aadhaar</c> and the matching field only. Stub (or future provider) verification; returns one-time kycSessionId for create booking.
    /// </summary>
    [HttpPost("verify-kyc")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> VerifyKyc([FromBody] VerifyKycRequest request, CancellationToken cancellationToken)
    {
        var customerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(customerIdClaim) || !Guid.TryParse(customerIdClaim, out var customerId))
            return Unauthorized(new { error = "Invalid customer identity" });

        try
        {
            var result = await _kycHandler.VerifyForBookingAsync(customerId, request, cancellationToken);
            if (!result.Verified)
            {
                return BadRequest(new
                {
                    error = result.Message,
                    flags = result.RiskFlags
                });
            }

            return Ok(new
            {
                verified = true,
                kycSessionId = result.SessionId,
                referenceId = result.ReferenceId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "KYC verification error");
            return StatusCode(500, new { error = "Verification could not be completed." });
        }
    }

    [HttpPost("create")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequest request, CancellationToken cancellationToken)
    {
        var customerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(customerIdClaim) || !Guid.TryParse(customerIdClaim, out var customerId))
            return Unauthorized(new { error = "Invalid customer identity" });

        // Enrich with identity claims so the handler can send notifications
        var customerEmail = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        var customerPhone = User.FindFirstValue(ClaimTypes.MobilePhone)
                         ?? User.FindFirstValue("phone_number")
                         ?? string.Empty;
        var customerName  = User.FindFirstValue(ClaimTypes.Name)
                         ?? User.FindFirstValue("name")
                         ?? string.Empty;

        var body = request with
        {
            CustomerId    = customerId,
            CustomerEmail = customerEmail,
            CustomerPhone = customerPhone,
            CustomerName  = customerName
        };

        try
        {
            _logger.LogInformation("Creating booking for customer {CustomerId} at station {StationId}",
                customerId, body.StationId);

            var result = await _handler.CreateBookingAsync(body, customerId, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating booking");

            // SMS: notify customer on their registered mobile that the booking failed
            // This covers the "fuel unavailable" case and any other booking failure.
            var reason = ex.Message.Contains("nventor", StringComparison.OrdinalIgnoreCase)
                         || ex.Message.Contains("stock", StringComparison.OrdinalIgnoreCase)
                         || ex.Message.Contains("fuel", StringComparison.OrdinalIgnoreCase)
                ? $"IFMS: Sorry, your fuel booking for {body.FuelType} could not be completed — fuel is currently unavailable at the selected station. Please try another station."
                : $"IFMS: Your fuel booking could not be completed. Reason: {ex.Message}. Please try again or contact support.";

            _ = _notifications.SendSmsAsync(
                toPhone: customerPhone,
                message: reason,
                ct: CancellationToken.None); // use None so cancellation doesn't suppress the SMS

            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("validate")]
    [Authorize(Roles = "Dealer")]
    public async Task<IActionResult> ValidateToken([FromBody] ValidateTokenRequest request)
    {
        try
        {
            _logger.LogInformation("Validating token {TokenCode}", request.TokenCode);

            var result = await _handler.ValidateTokenAsync(request.TokenCode);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Token validation failed: {Message}", ex.Message);
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpPost("confirm")]
    [Authorize(Roles = "Dealer")]
    public async Task<IActionResult> ConfirmBooking([FromBody] ConfirmBookingRequest request)
    {
        try
        {
            _logger.LogInformation("Confirming booking for token {TokenCode}", request.TokenCode);

            var result = await _handler.ConfirmBookingAsync(request.TokenCode);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Booking confirmation failed: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Customer: own bookings; optional <paramref name="stationId"/> filters to one outlet.</summary>
    [HttpGet("my-bookings")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> GetMyBookings([FromQuery] Guid? stationId = null)
    {
        try
        {
            var customerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(customerIdClaim) || !Guid.TryParse(customerIdClaim, out var customerId))
            {
                return Unauthorized(new { error = "Invalid customer identity" });
            }

            var result = await _handler.GetCustomerBookingsAsync(customerId, stationId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer bookings");
            return StatusCode(500, new { error = "An unexpected error occurred" });
        }
    }

    /// <summary>Dealer only: customer bookings for an assigned station.</summary>
    [HttpGet("station/{stationId:guid}/bookings")]
    [Authorize(Roles = "Dealer")]
    public async Task<IActionResult> GetStationBookings(Guid stationId)
    {
        try
        {
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized(new { error = "Invalid identity" });

            if (!await _dealerAssignments.UserIsAssignedToStationAsync(userId.Value, stationId))
                return Forbid();

            var result = await _handler.GetBookingsForStationAsync(stationId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving station bookings");
            return StatusCode(500, new { error = "An unexpected error occurred" });
        }
    }

    [HttpPost("cancel")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> CancelBooking([FromBody] ConfirmBookingRequest request)
    {
        try
        {
            _logger.LogInformation("Cancelling booking for token {TokenCode}", request.TokenCode);

            var result = await _handler.CancelBookingAsync(request.TokenCode);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Booking cancellation failed: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Get a single booking by its ID. Customer: own bookings only. Dealer/Admin: any.</summary>
    [HttpGet("{bookingId:guid}")]
    [Authorize]
    public async Task<IActionResult> GetBookingById(Guid bookingId)
    {
        var result = await _handler.GetByIdAsync(bookingId);
        if (result == null)
            return NotFound(new { error = "Booking not found" });

        // Customers can only see their own bookings
        if (User.IsInRole("Customer"))
        {
            var userId = GetUserId();
            if (result.CustomerId != userId)
                return Forbid();
        }

        return Ok(result);
    }

    /// <summary>Look up a booking by token code. Useful for customer support.</summary>
    [HttpGet("token/{tokenCode}")]
    [Authorize]
    public async Task<IActionResult> GetByToken(string tokenCode)
    {
        var result = await _handler.GetByTokenAsync(tokenCode);
        if (result == null)
            return NotFound(new { error = "Booking not found" });

        if (User.IsInRole("Customer"))
        {
            var userId = GetUserId();
            if (result.CustomerId != userId)
                return Forbid();
        }

        return Ok(result);
    }

    /// <summary>Customer: booking summary stats (total spent, counts by status, most used fuel).</summary>
    [HttpGet("my-stats")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> GetMyStats()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized(new { error = "Invalid identity" });

        var stats = await _handler.GetCustomerStatsAsync(userId.Value);
        return Ok(stats);
    }

    /// <summary>Dealer: bookings for assigned station with optional date filter.</summary>
    [HttpGet("station/{stationId:guid}/bookings/filtered")]
    [Authorize(Roles = "Dealer,Admin")]
    public async Task<IActionResult> GetStationBookingsFiltered(
        Guid stationId,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        if (User.IsInRole("Dealer"))
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new { error = "Invalid identity" });
            if (!await _dealerAssignments.UserIsAssignedToStationAsync(userId.Value, stationId))
                return Forbid();
        }

        var result = await _handler.GetBookingsForStationFilteredAsync(stationId, from, to);
        return Ok(result);
    }

    private Guid? GetUserId()
    {
        var v = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(v, out var g) ? g : null;
    }
}
