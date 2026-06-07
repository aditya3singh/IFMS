using IFMS.Inventory.Domain.Entities;

namespace IFMS.Inventory.Application.Interfaces;

public interface ISupplierRepository
{
    Task<Supplier?> GetByIdAsync(Guid id);
    Task<List<Supplier>> GetAllAsync();
    Task<List<Supplier>> GetActiveAsync();
    Task<Supplier?> GetByNameAsync(string name);
    Task AddAsync(Supplier supplier);
    Task UpdateAsync(Supplier supplier);
    Task DeleteAsync(Supplier supplier);
    Task SaveChangesAsync();
}
