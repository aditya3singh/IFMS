namespace IFMS.Station.Application.DTOs;

// ── Response ──────────────────────────────────────────────────────────────────
public record StaffMemberResponseDto(
    Guid Id,
    Guid StationId,
    string Name,
    string Role,
    string Shift,
    string Phone,
    string Email,
    string Status,
    string JoinDate,
    DateTime CreatedAt
);

// ── Create (Dealer only) ──────────────────────────────────────────────────────
public record CreateStaffMemberDto(
    string Name,
    string Role,
    string Shift,
    string Phone,
    string Email,
    string Status,
    string JoinDate
);

// ── Update (Dealer only) ──────────────────────────────────────────────────────
public record UpdateStaffMemberDto(
    string Name,
    string Role,
    string Shift,
    string Phone,
    string Email,
    string Status,
    string JoinDate
);

// ── Status-only patch (Dealer only) ───────────────────────────────────────────
public record UpdateStaffStatusDto(string Status);
