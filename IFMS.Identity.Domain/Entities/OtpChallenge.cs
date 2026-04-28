namespace IFMS.Identity.Domain.Entities;

public class OtpChallenge
{
    public Guid Id { get; private set; }
    public string NormalizedKey { get; private set; } = string.Empty;
    public string Purpose { get; private set; } = string.Empty;
    public string CodeHash { get; private set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private OtpChallenge() { }

    public static OtpChallenge Create(string normalizedKey, string purpose, string codeHash, DateTime expiresAtUtc)
    {
        return new OtpChallenge
        {
            Id = Guid.NewGuid(),
            NormalizedKey = normalizedKey,
            Purpose = purpose,
            CodeHash = codeHash,
            ExpiresAtUtc = expiresAtUtc,
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}
