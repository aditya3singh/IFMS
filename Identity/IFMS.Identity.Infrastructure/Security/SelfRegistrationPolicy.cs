using IFMS.Identity.Application.Interfaces;
using IFMS.Identity.Domain.Constants;
using Microsoft.Extensions.Configuration;

namespace IFMS.Identity.Infrastructure.Security;

public class SelfRegistrationPolicy : ISelfRegistrationPolicy
{
    private readonly HashSet<string> _allowed;

    public SelfRegistrationPolicy(IConfiguration configuration)
    {
        var configured = configuration.GetSection("Auth:AllowedSelfRegistrationRoles").Get<string[]>();
        if (configured is { Length: > 0 })
        {
            _allowed = new HashSet<string>(configured, StringComparer.OrdinalIgnoreCase);
            return;
        }

        _allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Roles.Customer,
            Roles.Dealer
        };
    }

    public void EnsureAllowedForPublicSignup(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
            throw new InvalidOperationException("Invalid role.");

        if (!_allowed.Contains(role))
            throw new InvalidOperationException(
                "This account type is not available for self-registration. Contact an administrator.");
    }
}
