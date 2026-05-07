namespace IFMS.Messaging.Events;

/// <summary>Published by Booking API when a customer successfully creates a booking.</summary>
public record BookingCreated(
    Guid BookingId,
    Guid CustomerId,
    Guid StationId,
    string FuelType,
    decimal QuantityLiters,
    decimal TotalPaid,
    string TokenCode,
    string StationName,
    string CustomerName,
    string CustomerEmail,
    string CustomerPhone,
    DateTime BookedAt,
    DateTime ExpiresAt
);

/// <summary>Published by Booking API when a dealer confirms fuel dispensing.</summary>
public record BookingConfirmed(
    Guid BookingId,
    Guid CustomerId,
    Guid StationId,
    string FuelType,
    decimal QuantityLiters,
    decimal TotalPaid,
    string TokenCode,
    string CustomerPhone,
    string CustomerEmail
);

/// <summary>Published by Booking API when a customer cancels a booking.</summary>
public record BookingCancelled(
    Guid BookingId,
    Guid CustomerId,
    string TokenCode,
    string CustomerPhone
);
