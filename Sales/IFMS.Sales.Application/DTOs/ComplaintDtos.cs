namespace IFMS.Sales.Application.DTOs;

public record RaiseComplaintRequest(
    string Category,
    string Subject,
    string Description,
    string? ReferenceId = null,
    string CustomerName = "",
    string CustomerEmail = "",
    string CustomerPhone = ""
);

public record UpdateComplaintStatusRequest(
    string Status,
    string? ResolutionNote = null
);

public record ComplaintResponse(
    Guid Id,
    Guid CustomerId,
    string CustomerName,
    string CustomerEmail,
    string CustomerPhone,
    string Category,
    string Subject,
    string Description,
    string? ReferenceId,
    string Status,
    string? ResolutionNote,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? ResolvedAt
);
