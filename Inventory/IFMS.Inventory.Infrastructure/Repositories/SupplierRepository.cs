using IFMS.Inventory.Application.Interfaces;
using IFMS.Inventory.Domain.Entities;
using IFMS.Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IFMS.Inventory.Infrastructure.Repositories;

public class SupplierRepository : ISupplierRepository
{
    private readonly InventoryDbContext _context;

    public SupplierRepository(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<Supplier?> GetByIdAsync(Guid id)
        => await _context.Suppliers.FirstOrDefaultAsync(s => s.Id == id);

    public async Task<List<Supplier>> GetAllAsync()
        => await _context.Suppliers
            .OrderBy(s => s.Name)
            .ToListAsync();

    public async Task<List<Supplier>> GetActiveAsync()
        => await _context.Suppliers
            .Where(s => s.Status == "Active")
            .OrderBy(s => s.Name)
            .ToListAsync();

    public async Task<Supplier?> GetByNameAsync(string name)
        => await _context.Suppliers
            .FirstOrDefaultAsync(s => s.Name.ToLower() == name.ToLower());

    public async Task AddAsync(Supplier supplier)
        => await _context.Suppliers.AddAsync(supplier);

    public Task UpdateAsync(Supplier supplier)
    {
        _context.Suppliers.Update(supplier);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Supplier supplier)
    {
        _context.Suppliers.Remove(supplier);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
