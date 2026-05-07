using System.Text.RegularExpressions;

namespace IFMS.Booking.Application.Support;

/// <summary>
/// KYC document format validator with regex patterns for PAN and Aadhaar.
/// </summary>
public static class KycFormatValidator
{
    /// <summary>
    /// PAN Card Regex Pattern: ^[A-Z]{5}[0-9]{4}[A-Z]$
    /// Format: 5 uppercase letters + 4 digits + 1 uppercase letter
    /// Example: ABCDE1234F
    /// 
    /// Structure:
    /// - First 3 letters: Alphabetic series (AAA to ZZZ)
    /// - 4th letter: Type of holder (C=Company, P=Person, H=HUF, F=Firm, etc.)
    /// - 5th letter: First letter of surname/name
    /// - Next 4 digits: Sequential number (0001-9999)
    /// - Last letter: Alphabetic check digit
    /// </summary>
    private static readonly Regex PanRegex = new("^[A-Z]{5}[0-9]{4}[A-Z]$", RegexOptions.Compiled);

    /// <summary>
    /// Aadhaar Number Regex Pattern: ^[2-9][0-9]{11}$
    /// Format: 12 digits, first digit cannot be 0 or 1
    /// Example: 234567890123
    /// 
    /// Rules:
    /// - Total 12 digits
    /// - First digit must be 2-9 (not 0 or 1)
    /// - Last digit is Verhoeff checksum
    /// - No alphabets or special characters
    /// </summary>
    private static readonly Regex AadhaarBasicRegex = new("^[2-9][0-9]{11}$", RegexOptions.Compiled);

    /// <summary>
    /// Normalize PAN: Uppercase letters and digits only (removes spaces, hyphens, etc.).
    /// Input: "abcde-1234-f" or "ABCDE 1234 F"
    /// Output: "ABCDE1234F"
    /// </summary>
    public static string NormalizePan(string pan) =>
        new string((pan ?? string.Empty).Trim().ToUpperInvariant()
            .Where(char.IsAsciiLetterOrDigit)
            .ToArray());

    /// <summary>
    /// Clean Aadhaar: Extract only digits (removes spaces, hyphens, etc.).
    /// Input: "2345-6789-0123" or "2345 6789 0123"
    /// Output: "234567890123"
    /// </summary>
    public static string CleanAadhaarDigits(string input) =>
        new string(input.Where(char.IsDigit).ToArray());

    /// <summary>
    /// Validate PAN format using regex.
    /// Returns true if PAN matches pattern: ^[A-Z]{5}[0-9]{4}[A-Z]$
    /// 
    /// Valid examples:
    /// - ABCDE1234F
    /// - BNZAA2318J
    /// - AFZPK7190K
    /// 
    /// Invalid examples:
    /// - ABC123 (too short)
    /// - ABCDE12345 (wrong format)
    /// - abcde1234f (lowercase - will be normalized first)
    /// </summary>
    public static bool IsValidPanFormat(string pan)
    {
        var n = NormalizePan(pan);
        return n.Length == 10 && PanRegex.IsMatch(n);
    }

    /// <summary>
    /// Validate Aadhaar format: 12-digit pattern + Verhoeff checksum (offline).
    /// Does not prove UIDAI issuance - only validates format and checksum.
    /// 
    /// Valid examples:
    /// - 234567890123 (if checksum passes)
    /// - 999999990019 (if checksum passes)
    /// 
    /// Invalid examples:
    /// - 123456789012 (starts with 1)
    /// - 023456789012 (starts with 0)
    /// - 23456789012 (only 11 digits)
    /// - 2345678901234 (13 digits)
    /// 
    /// Returns true if:
    /// 1. Exactly 12 digits
    /// 2. First digit is 2-9
    /// 3. Verhoeff checksum is valid
    /// </summary>
    public static bool IsValidAadhaarFormat(string aadhaar)
    {
        var d = CleanAadhaarDigits(aadhaar);
        if (d.Length != 12 || !AadhaarBasicRegex.IsMatch(d))
            return false;
        return VerhoeffChecksum.IsValid(d);
    }
}
