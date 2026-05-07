using IFMS.Identity.Domain.Constants;

namespace IFMS.Identity.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? PasswordHash { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? GoogleSubjectId { get; private set; }
    public string Role { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private User() { }

    public static User Create(string fullName, string email, string passwordHash, string role, string? phoneNumber = null)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            FullName = fullName,
            Email = email,
            PasswordHash = passwordHash,
            PhoneNumber = phoneNumber,
            Role = role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static User CreateFromGoogle(string fullName, string email, string googleSubjectId, string role = Roles.Customer)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            FullName = fullName,
            Email = email,
            PasswordHash = null,
            GoogleSubjectId = googleSubjectId,
            Role = role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void LinkGoogle(string googleSubjectId) => GoogleSubjectId = googleSubjectId;

    public void SetPhoneNumber(string? normalizedPhoneDigits) => PhoneNumber = normalizedPhoneDigits;

    public void SetPasswordHash(string passwordHash) => PasswordHash = passwordHash;

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;

    public void UpdateProfile(string fullName, string? phoneNumber)
    {
        if (!string.IsNullOrWhiteSpace(fullName))
            FullName = fullName;
        PhoneNumber = phoneNumber;
    }

    public void ChangeRole(string newRole) => Role = newRole;
}
