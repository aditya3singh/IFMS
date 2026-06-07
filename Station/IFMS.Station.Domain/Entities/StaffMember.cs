namespace IFMS.Station.Domain.Entities;

public class StaffMember
{
    public Guid Id { get; private set; }
    public Guid StationId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Role { get; private set; } = string.Empty;
    public string Shift { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Status { get; private set; } = "Active";
    public string JoinDate { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    // Navigation
    public Station? Station { get; private set; }

    private StaffMember() { }

    public static StaffMember Create(
        Guid stationId,
        string name,
        string role,
        string shift,
        string phone,
        string email,
        string status,
        string joinDate)
    {
        if (stationId == Guid.Empty)
            throw new ArgumentException("StationId is required.");
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.");
        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException("Phone is required.");

        return new StaffMember
        {
            Id = Guid.NewGuid(),
            StationId = stationId,
            Name = name.Trim(),
            Role = string.IsNullOrWhiteSpace(role) ? "Pump Operator" : role.Trim(),
            Shift = string.IsNullOrWhiteSpace(shift) ? "Morning (6AM-2PM)" : shift.Trim(),
            Phone = phone.Trim(),
            Email = email?.Trim() ?? string.Empty,
            Status = ValidStatus(status),
            JoinDate = string.IsNullOrWhiteSpace(joinDate)
                ? DateTime.UtcNow.ToString("yyyy-MM-dd")
                : joinDate.Trim(),
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string role, string shift, string phone, string email, string status, string joinDate)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.");
        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException("Phone is required.");

        Name = name.Trim();
        Role = string.IsNullOrWhiteSpace(role) ? Role : role.Trim();
        Shift = string.IsNullOrWhiteSpace(shift) ? Shift : shift.Trim();
        Phone = phone.Trim();
        Email = email?.Trim() ?? Email;
        Status = ValidStatus(status);
        JoinDate = string.IsNullOrWhiteSpace(joinDate) ? JoinDate : joinDate.Trim();
    }

    public void UpdateStatus(string status) => Status = ValidStatus(status);

    private static string ValidStatus(string status) =>
        status is "Active" or "Off Duty" or "On Leave" ? status : "Active";
}
