using IFMS.Sales.Application.DTOs;
using IFMS.Sales.Application.Interfaces;
using IFMS.Sales.Domain.Entities;

namespace IFMS.Sales.Application.Commands;

public class ComplaintCommandHandler
{
    private readonly IComplaintRepository _repository;

    public ComplaintCommandHandler(IComplaintRepository repository)
    {
        _repository = repository;
    }

    public async Task<ComplaintResponse> RaiseAsync(Guid customerId, RaiseComplaintRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Category))
            throw new InvalidOperationException("Category is required.");
        if (string.IsNullOrWhiteSpace(request.Subject))
            throw new InvalidOperationException("Subject is required.");
        if (string.IsNullOrWhiteSpace(request.Description))
            throw new InvalidOperationException("Description is required.");

        var complaint = Complaint.Create(
            customerId,
            request.CustomerName,
            request.CustomerEmail,
            request.CustomerPhone,
            request.Category,
            request.Subject,
            request.Description,
            request.ReferenceId);

        await _repository.AddAsync(complaint);
        await _repository.SaveChangesAsync();
        return Map(complaint);
    }

    public async Task<List<ComplaintResponse>> GetMyComplaintsAsync(Guid customerId)
    {
        var list = await _repository.GetByCustomerIdAsync(customerId);
        return list.Select(Map).ToList();
    }

    public async Task<ComplaintResponse?> GetByIdAsync(Guid id)
    {
        var c = await _repository.GetByIdAsync(id);
        return c == null ? null : Map(c);
    }

    public async Task<List<ComplaintResponse>> GetAllAsync()
    {
        var list = await _repository.GetAllAsync();
        return list.Select(Map).ToList();
    }

    public async Task<ComplaintResponse> UpdateStatusAsync(Guid id, UpdateComplaintStatusRequest request)
    {
        var complaint = await _repository.GetByIdAsync(id)
            ?? throw new InvalidOperationException("Complaint not found.");

        var allowed = new[] { "Open", "InProgress", "Resolved", "Closed" };
        if (!allowed.Contains(request.Status))
            throw new InvalidOperationException($"Invalid status. Allowed: {string.Join(", ", allowed)}");

        complaint.UpdateStatus(request.Status, request.ResolutionNote);
        await _repository.SaveChangesAsync();
        return Map(complaint);
    }

    private static ComplaintResponse Map(Complaint c) => new(
        c.Id, c.CustomerId, c.CustomerName, c.CustomerEmail, c.CustomerPhone,
        c.Category, c.Subject, c.Description, c.ReferenceId,
        c.Status, c.ResolutionNote, c.CreatedAt, c.UpdatedAt, c.ResolvedAt);
}
