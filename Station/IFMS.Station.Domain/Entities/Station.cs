namespace IFMS.Station.Domain.Entities;

public class Station
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string LicenseNumber { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public string State { get; private set; } = string.Empty;
    public decimal Latitude { get; private set; }
    public decimal Longitude { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    
    // Navigation property
    public DealerAssignment? DealerAssignment { get; private set; }
    
    private Station() { }
    
    public static Station Create(
        string name, 
        string licenseNumber, 
        string city, 
        string state, 
        decimal latitude, 
        decimal longitude)
    {
        ValidateInputs(name, licenseNumber, city, state);
        ValidateCoordinates(latitude, longitude);
        
        return new Station
        {
            Id = Guid.NewGuid(),
            Name = name,
            LicenseNumber = licenseNumber,
            City = city,
            State = state,
            Latitude = latitude,
            Longitude = longitude,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
    
    public void Update(
        string name, 
        string licenseNumber, 
        string city, 
        string state, 
        decimal latitude, 
        decimal longitude)
    {
        ValidateInputs(name, licenseNumber, city, state);
        ValidateCoordinates(latitude, longitude);
        
        Name = name;
        LicenseNumber = licenseNumber;
        City = city;
        State = state;
        Latitude = latitude;
        Longitude = longitude;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void SoftDelete()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
    
    private static void ValidateCoordinates(decimal latitude, decimal longitude)
    {
        if (latitude < -90 || latitude > 90)
            throw new ArgumentException("Latitude must be between -90 and 90");
        if (longitude < -180 || longitude > 180)
            throw new ArgumentException("Longitude must be between -180 and 180");
    }
    
    private static void ValidateInputs(string name, string licenseNumber, string city, string state)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required and cannot be empty");
        if (name.Length > 200)
            throw new ArgumentException("Name cannot exceed 200 characters");
        
        if (string.IsNullOrWhiteSpace(licenseNumber))
            throw new ArgumentException("LicenseNumber is required and cannot be empty");
        if (licenseNumber.Length > 50)
            throw new ArgumentException("LicenseNumber cannot exceed 50 characters");
        
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City is required and cannot be empty");
        
        if (string.IsNullOrWhiteSpace(state))
            throw new ArgumentException("State is required and cannot be empty");
    }
}
