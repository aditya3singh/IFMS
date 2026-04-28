using System.Security.Cryptography;

namespace IFMS.Booking.Domain.Entities;

public class Booking
{
    public Guid BookingId { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid StationId { get; private set; }
    public string FuelType { get; private set; } = string.Empty;
    public decimal QuantityLiters { get; private set; }
    public decimal TotalPaid { get; private set; }
    public string TokenCode { get; private set; } = string.Empty;
    public string TokenStatus { get; private set; } = "PENDING";
    public string PaymentId { get; private set; } = string.Empty;
    public DateTime BookedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? UsedAt { get; private set; }

    /// <summary>Customer's registered phone number — stored so SMS can be sent at confirm/cancel time.</summary>
    public string CustomerPhone { get; private set; } = string.Empty;

    /// <summary>Customer's email — stored for notification delivery at confirm/cancel time.</summary>
    public string CustomerEmail { get; private set; } = string.Empty;

    private Booking() { }

    public static Booking Create(
        Guid customerId,
        Guid stationId,
        string fuelType,
        decimal quantityLiters,
        decimal totalPaid,
        string paymentId,
        int stationNumber,
        string customerPhone = "",
        string customerEmail = "")
    {
        return new Booking
        {
            BookingId = Guid.NewGuid(),
            CustomerId = customerId,
            StationId = stationId,
            FuelType = fuelType,
            QuantityLiters = quantityLiters,
            TotalPaid = totalPaid,
            TokenCode = GenerateToken(stationNumber),
            TokenStatus = "PENDING",
            PaymentId = paymentId,
            BookedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            CustomerPhone = customerPhone,
            CustomerEmail = customerEmail
        };
    }

    /// <summary>
    /// Generates cryptographically secure token: IFM-{stationId:D2}-{8 random digits}
    /// Uses RandomNumberGenerator — NOT System.Random (which is predictable/hackable)
    /// </summary>
    public static string GenerateToken(int stationId)
    {
        var bytes = new byte[4];
        RandomNumberGenerator.Fill(bytes);
        var randomPart = (BitConverter.ToUInt32(bytes) % 100_000_000).ToString("D8");
        return $"IFM-{stationId:D2}-{randomPart}";
    }

    public void MarkUsed()
    {
        TokenStatus = "USED";
        UsedAt = DateTime.UtcNow;
    }

    public void MarkExpired()
    {
        TokenStatus = "EXPIRED";
    }

    public void MarkCancelled()
    {
        TokenStatus = "CANCELLED";
    }
}
