using IFMS.Sales.Application.Interfaces;
using IFMS.Sales.Domain.Entities;
using IFMS.Sales.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IFMS.Sales.Infrastructure.Repositories;

public class ComplaintRepository : IComplaintRepository
{
    private readonly SalesDbContext _context;

    public ComplaintRepository(SalesDbContext context)
    {
        _context = context;
    }

    public async Task<Complaint?> GetByIdAsync(Guid id)
        => await _context.Complaints.FirstOrDefaultAsync(c => c.Id == id);

    public async Task<List<Complaint>> GetByCustomerIdAsync(Guid customerId)
        => await _context.Complaints
            .Where(c => c.CustomerId == customerId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

    public async Task<List<Complaint>> GetAllAsync()
        => await _context.Complaints
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

    public async Task AddAsync(Complaint complaint)
        => await _context.Complaints.AddAsync(complaint);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
