using IFMS.Sales.Domain.Entities;

namespace IFMS.Sales.Application.Interfaces;

public interface IComplaintRepository
{
    Task<Complaint?> GetByIdAsync(Guid id);
    Task<List<Complaint>> GetByCustomerIdAsync(Guid customerId);
    Task<List<Complaint>> GetAllAsync();
    Task AddAsync(Complaint complaint);
    Task SaveChangesAsync();
}
