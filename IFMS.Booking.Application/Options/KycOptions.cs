namespace IFMS.Booking.Application.Options;

public class KycOptions
{
    public const string SectionName = "Kyc";

    /// <summary>Stub = offline format + simulated provider. Replace with real provider name when integrated.</summary>
    public string Provider { get; set; } = "Stub";

    public int SessionTtlMinutes { get; set; } = 15;

    /// <summary>
    /// If set, normalized PAN equal to this value is rejected as simulated fraud (for QA).
    /// Clear or remove to disable demo rejection.
    /// </summary>
    public string? StubRejectPan { get; set; }
}
