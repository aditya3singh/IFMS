namespace IFMS.Inventory.Application.DTOs;

// Response
public record SupplierResponse(
    Guid Id,
    string Name,
    string ContactPerson,
    string Phone,
    string Email,
    string Address,
    string Rating,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

// Create
public record CreateSupplierRequest(
    string Name,
    string ContactPerson,
    string Phone,
    string Email,
    string Address,
    string Rating = "Silver"
);

// Update
public record UpdateSupplierRequest(
    string Name,
    string ContactPerson,
    string Phone,
    string Email,
    string Address,
    string Rating
);

// Update Status
public record UpdateSupplierStatusRequest(
    string Status
);
