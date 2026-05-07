namespace IFMS.Identity.Application.Interfaces;

/// <summary>
/// Enforces which roles may be created via public signup.
/// </summary>
public interface ISelfRegistrationPolicy
{
    void EnsureAllowedForPublicSignup(string role);
}
