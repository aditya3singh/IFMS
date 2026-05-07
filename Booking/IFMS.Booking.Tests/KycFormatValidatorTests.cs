using IFMS.Booking.Application.Support;

namespace IFMS.Booking.Tests;

public class KycFormatValidatorTests
{
    [Theory]
    [InlineData("ABCDE1234F", true)]
    [InlineData("abcde1234f", true)]
    [InlineData("ABCDE 1234 F", true)]
    [InlineData("ABCDE-1234-F", true)]
    [InlineData("ABCD1234F", false)]
    [InlineData("ABCDE12345", false)]
    [InlineData("", false)]
    public void Pan_Format(string pan, bool expected) =>
        Assert.Equal(expected, KycFormatValidator.IsValidPanFormat(pan));

    [Fact]
    public void Aadhaar_RejectsLeadingZeroOrOne()
    {
        Assert.False(KycFormatValidator.IsValidAadhaarFormat("012345678901"));
        Assert.False(KycFormatValidator.IsValidAadhaarFormat("112345678901"));
    }

    [Fact]
    public void Aadhaar_CleansSeparators()
    {
        Assert.Equal("234523452342", KycFormatValidator.CleanAadhaarDigits("2345-2345-2342"));
    }
}
