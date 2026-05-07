using System.Security.Cryptography;
using System.Text;
using IFMS.Identity.Application.DTOs;
using IFMS.Identity.Application.Interfaces;
using IFMS.Identity.Application.Support;
using IFMS.Identity.Domain.Constants;
using IFMS.Identity.Domain.Entities;

namespace IFMS.Identity.Application.Commands;

public class RequestLoginOtpCommandHandler
{
    private readonly IUserRepository _users;
    private readonly IOtpChallengeRepository _otps;
    private readonly IOtpDeliveryService _delivery;

    public RequestLoginOtpCommandHandler(
        IUserRepository users,
        IOtpChallengeRepository otps,
        IOtpDeliveryService delivery)
    {
        _users = users;
        _otps = otps;
        _delivery = delivery;
    }

    public async Task<string?> HandleAsync(RequestOtpRequest request, int expiryMinutes)
    {
        var key = AuthIdentifier.ToOtpStorageKey(request.Identifier);
        User? user = await ResolveUserAsync(request.Identifier);
        if (user == null)
            return null;

        if (!user.IsActive)
            return null;

        var code = GenerateSixDigitCode();
        var hash = Hash(code);
        var expires = DateTime.UtcNow.AddMinutes(expiryMinutes);

        await _otps.RemoveForKeyAndPurposeAsync(key, OtpPurposes.Login);
        await _otps.AddAsync(OtpChallenge.Create(key, OtpPurposes.Login, hash, expires));
        await _otps.SaveChangesAsync();

        await _delivery.SendLoginCodeAsync(user.Email, user.PhoneNumber, code);

        // Return the code so the API layer can expose it in dev/mock mode
        return code;
    }

    private async Task<User?> ResolveUserAsync(string identifier)
    {
        if (AuthIdentifier.LooksLikeEmail(identifier))
            return await _users.GetByEmailAsync(AuthIdentifier.NormalizeEmail(identifier));
        var phone = AuthIdentifier.NormalizePhone(identifier);
        return await _users.GetByPhoneAsync(phone);
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
