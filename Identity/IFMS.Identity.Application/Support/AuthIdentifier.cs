namespace IFMS.Identity.Application.Support;

public static class AuthIdentifier
{
    public static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }

    /// <summary>Digits only; suitable for matching stored PhoneNumber.</summary>
    public static string NormalizePhone(string input)
    {
        var digits = new string(input.Where(char.IsDigit).ToArray());
        if (digits.Length >= 12 && digits.StartsWith("91", StringComparison.Ordinal))
            return digits[2..];
        if (digits.Length == 11 && digits[0] == '0')
            return digits[1..];
        return digits;
    }

    public static bool LooksLikeEmail(string identifier) => identifier.Contains('@', StringComparison.Ordinal);

    /// <summary>Key used in OTP storage: "e:email" or "p:digits".</summary>
    public static string ToOtpStorageKey(string identifier)
    {
        if (LooksLikeEmail(identifier))
            return "e:" + NormalizeEmail(identifier);
        var phone = NormalizePhone(identifier);
        if (string.IsNullOrEmpty(phone))
            throw new InvalidOperationException("Enter a valid email or mobile number.");
        return "p:" + phone;
    }
}
