using System.Security.Cryptography;
using System.Text;
using IFMS.Identity.Application.DTOs;
using IFMS.Identity.Application.Interfaces;
using IFMS.Identity.Application.Support;
using IFMS.Identity.Domain.Constants;
using IFMS.Identity.Domain.Entities;

namespace IFMS.Identity.Application.Commands;

public class RequestPasswordResetOtpCommandHandler
{
    private readonly IUserRepository _users;
    private readonly IOtpChallengeRepository _otps;
    private readonly IOtpDeliveryService _delivery;

    public RequestPasswordResetOtpCommandHandler(
        IUserRepository users,
        IOtpChallengeRepository otps,
        IOtpDeliveryService delivery)
    {
        _users = users;
        _otps = otps;
        _delivery = delivery;
    }

    public async Task<string?> HandleAsync(RequestPasswordResetOtpRequest request, int expiryMinutes)
    {
        var key = AuthIdentifier.ToOtpStorageKey(request.Identifier);
        var user = await ResolveUserAsync(request.Identifier);
        if (user == null) return null;
        if (!user.IsActive) return null;
        if (string.IsNullOrEmpty(user.Email) && string.IsNullOrEmpty(user.PhoneNumber)) return null;

        var code = GenerateSixDigitCode();
        var hash = Hash(code);
        var expires = DateTime.UtcNow.AddMinutes(expiryMinutes);

        await _otps.RemoveForKeyAndPurposeAsync(key, OtpPurposes.PasswordReset);
        await _otps.AddAsync(OtpChallenge.Create(key, OtpPurposes.PasswordReset, hash, expires));
        await _otps.SaveChangesAsync();

        await _delivery.SendPasswordResetCodeAsync(user.Email, user.PhoneNumber, code);
        return code;
    }

    private async Task<User?> ResolveUserAsync(string identifier)
    {
        if (AuthIdentifier.LooksLikeEmail(identifier))
            return await _users.GetByEmailAsync(AuthIdentifier.NormalizeEmail(identifier));
        return await _users.GetByPhoneAsync(AuthIdentifier.NormalizePhone(identifier));
    }

    private static string GenerateSixDigitCode()
    {
        var n = RandomNumberGenerator.GetInt32(0, 1_000_000);
        return n.ToString("D6");
    }

    private static string Hash(string code)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(code));
        return Convert.ToHexString(bytes);
    }
}
