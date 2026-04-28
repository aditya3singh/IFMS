using System.Security.Cryptography;
using System.Text;
using IFMS.Identity.Application.DTOs;
using IFMS.Identity.Application.Interfaces;
using IFMS.Identity.Application.Support;
using IFMS.Identity.Domain.Constants;
using IFMS.Identity.Domain.Entities;

namespace IFMS.Identity.Application.Commands;

public class VerifyLoginOtpCommandHandler
{
    private readonly IUserRepository _users;
    private readonly IOtpChallengeRepository _otps;
    private readonly IJwtTokenService _jwt;

    public VerifyLoginOtpCommandHandler(
        IUserRepository users,
        IOtpChallengeRepository otps,
        IJwtTokenService jwt)
    {
        _users = users;
        _otps = otps;
        _jwt = jwt;
    }

    public async Task<AuthResponse> HandleAsync(VerifyOtpLoginRequest request)
    {
        var key = AuthIdentifier.ToOtpStorageKey(request.Identifier);
        var row = await _otps.GetLatestAsync(key, OtpPurposes.Login)
            ?? throw new InvalidOperationException("Invalid or expired code.");

        if (row.ExpiresAtUtc < DateTime.UtcNow)
            throw new InvalidOperationException("Invalid or expired code.");

        if (!ConstantTimeEquals(row.CodeHash, Hash(request.Code.Trim())))
            throw new InvalidOperationException("Invalid or expired code.");

        var user = await ResolveUserAsync(request.Identifier)
            ?? throw new InvalidOperationException("Invalid or expired code.");

        if (!user.IsActive)
            throw new InvalidOperationException("Account is disabled.");

        await _otps.RemoveAsync(row.Id);
        await _otps.SaveChangesAsync();

        var token = _jwt.GenerateToken(user);
        return new AuthResponse(token, user.FullName, user.Email, user.Role);
    }

    private async Task<User?> ResolveUserAsync(string identifier)
    {
        if (AuthIdentifier.LooksLikeEmail(identifier))
            return await _users.GetByEmailAsync(AuthIdentifier.NormalizeEmail(identifier));
        return await _users.GetByPhoneAsync(AuthIdentifier.NormalizePhone(identifier));
    }

    private static string Hash(string code)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(code));
        return Convert.ToHexString(bytes);
    }

    private static bool ConstantTimeEquals(string a, string b)
    {
        if (a.Length != b.Length) return false;
        var diff = 0;
        for (var i = 0; i < a.Length; i++)
            diff |= a[i] ^ b[i];
        return diff == 0;
    }
}
