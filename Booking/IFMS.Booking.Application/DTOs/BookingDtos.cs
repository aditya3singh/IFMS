namespace IFMS.Booking.Application.DTOs;

public record CreateBookingRequest(
    Guid CustomerId,
    Guid StationId,
    int StationNumber,
    string FuelType,
    decimal QuantityLiters,
    decimal PricePerLitre,
    string PaymentId,
    string? KycSessionId = null,
    // Optional contact info forwarded from the frontend for notification delivery.
    // The Booking API populates these from the JWT claims before passing to the handler.
    string? CustomerEmail = null,
    string? CustomerPhone = null,
    string? CustomerName = null,
    string? StationName = null
);

public record ValidateTokenRequest(
    string TokenCode
);

public record ConfirmBookingRequest(
    string TokenCode
);

public record BookingResponse(
    Guid BookingId,
    Guid CustomerId,
    Guid StationId,
    string FuelType,
    decimal QuantityLiters,
    decimal TotalPaid,
    string TokenCode,
    string TokenStatus,
    string PaymentId,
    DateTime BookedAt,
    DateTime ExpiresAt,
    DateTime? UsedAt
);

public record TokenValidationResponse(
    string TokenCode,
    Guid CustomerId,
    string FuelType,
    decimal QuantityLiters,
    decimal TotalPaid,
    DateTime BookedAt,
    DateTime ExpiresAt
);
