namespace IFMS.Inventory.Domain.Entities;

/// <summary>
/// Fuel supplier/vendor information
/// </summary>
public class Supplier
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string ContactPerson { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Address { get; private set; } = string.Empty;
    
    /// <summary>Gold, Silver, Bronze</summary>
    public string Rating { get; private set; } = "Silver";
    
    /// <summary>Active, Inactive, Suspended</summary>
    public string Status { get; private set; } = "Active";
    
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Supplier() { }

    public static Supplier Create(
        string name,
        string contactPerson,
        string phone,
        string email,
        string address,
        string rating = "Silver")
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Supplier name is required.");
        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException("Phone is required.");

        return new Supplier
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            ContactPerson = contactPerson?.Trim() ?? string.Empty,
            Phone = phone.Trim(),
            Email = email?.Trim() ?? string.Empty,
            Address = address?.Trim() ?? string.Empty,
            Rating = ValidateRating(rating),
            Status = "Active",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string contactPerson, string phone, string email, string address, string rating)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Supplier name is required.");
        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException("Phone is required.");

        Name = name.Trim();
        ContactPerson = contactPerson?.Trim() ?? ContactPerson;
        Phone = phone.Trim();
        Email = email?.Trim() ?? Email;
        Address = address?.Trim() ?? Address;
        Rating = ValidateRating(rating);
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateStatus(string status)
    {
        Status = status switch
        {
            "Active" or "Inactive" or "Suspended" => status,
            _ => Status
        };
        UpdatedAt = DateTime.UtcNow;
    }

    private static string ValidateRating(string rating) =>
        rating switch
        {
            "Gold" or "Silver" or "Bronze" => rating,
            _ => "Silver"
        };
}
