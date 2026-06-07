using IFMS.Inventory.Application.DTOs;
using IFMS.Inventory.Application.Interfaces;
using IFMS.Inventory.Domain.Entities;

namespace IFMS.Inventory.Application.Commands;

public class SupplierCommandHandler
{
    private readonly ISupplierRepository _repository;

    public SupplierCommandHandler(ISupplierRepository repository)
    {
        _repository = repository;
    }

    public async Task<SupplierResponse> CreateAsync(CreateSupplierRequest request)
    {
        // Check for duplicate name
        var existing = await _repository.GetByNameAsync(request.Name);
        if (existing != null)
            throw new InvalidOperationException($"Supplier '{request.Name}' already exists.");

        var supplier = Supplier.Create(
            request.Name,
            request.ContactPerson,
            request.Phone,
            request.Email,
            request.Address,
            request.Rating
        );

        await _repository.AddAsync(supplier);
        await _repository.SaveChangesAsync();

        return MapToResponse(supplier);
    }

    public async Task<SupplierResponse?> GetByIdAsync(Guid id)
    {
        var supplier = await _repository.GetByIdAsync(id);
        return supplier == null ? null : MapToResponse(supplier);
    }

    public async Task<List<SupplierResponse>> GetAllAsync()
    {
        var suppliers = await _repository.GetAllAsync();
        return suppliers.Select(MapToResponse).ToList();
    }

    public async Task<List<SupplierResponse>> GetActiveAsync()
    {
        var suppliers = await _repository.GetActiveAsync();
        return suppliers.Select(MapToResponse).ToList();
    }

    public async Task<SupplierResponse> UpdateAsync(Guid id, UpdateSupplierRequest request)
    {
        var supplier = await _repository.GetByIdAsync(id)
            ?? throw new InvalidOperationException("Supplier not found.");

        // Check for duplicate name (excluding current supplier)
        var existing = await _repository.GetByNameAsync(request.Name);
        if (existing != null && existing.Id != id)
            throw new InvalidOperationException($"Supplier '{request.Name}' already exists.");

        supplier.Update(
            request.Name,
            request.ContactPerson,
            request.Phone,
            request.Email,
            request.Address,
            request.Rating
        );

        await _repository.UpdateAsync(supplier);
        await _repository.SaveChangesAsync();

        return MapToResponse(supplier);
    }

    public async Task<SupplierResponse> UpdateStatusAsync(Guid id, UpdateSupplierStatusRequest request)
    {
        var supplier = await _repository.GetByIdAsync(id)
            ?? throw new InvalidOperationException("Supplier not found.");

        supplier.UpdateStatus(request.Status);

        await _repository.UpdateAsync(supplier);
        await _repository.SaveChangesAsync();

        return MapToResponse(supplier);
    }

    public async Task DeleteAsync(Guid id)
    {
        var supplier = await _repository.GetByIdAsync(id)
            ?? throw new InvalidOperationException("Supplier not found.");

        await _repository.DeleteAsync(supplier);
        await _repository.SaveChangesAsync();
    }

    private static SupplierResponse MapToResponse(Supplier supplier) => new(
        supplier.Id,
        supplier.Name,
        supplier.ContactPerson,
        supplier.Phone,
        supplier.Email,
        supplier.Address,
        supplier.Rating,
        supplier.Status,
        supplier.CreatedAt,
        supplier.UpdatedAt
    );
}
