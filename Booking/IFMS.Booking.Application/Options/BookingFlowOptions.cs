namespace IFMS.Booking.Application.Options;

public class BookingFlowOptions
{
    public const string SectionName = "Booking";

    /// <summary>When true, create booking requires a fresh KYC session from POST verify-kyc.</summary>
    public bool RequireKycVerification { get; set; } = true;
}
