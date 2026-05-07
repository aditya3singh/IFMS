using IFMS.Identity.Application.Interfaces;
using IFMS.Identity.Domain.Entities;
using IFMS.Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IFMS.Identity.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IdentityDbContext _context;

    public UserRepository(IdentityDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByEmailAsync(string email)
        => await _context.Users.FirstOrDefaultAsync(
            u => u.Email.ToLower() == email.ToLower());

    public async Task<User?> GetByPhoneAsync(string normalizedPhoneDigits)
        => await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == normalizedPhoneDigits);

    public async Task<User?> GetByIdAsync(Guid id)
        => await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

    public async Task<bool> ExistsAsync(string email)
        => await _context.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower());

    public async Task<bool> ExistsPhoneAsync(string normalizedPhoneDigits)
        => await _context.Users.AnyAsync(u => u.PhoneNumber == normalizedPhoneDigits);

    public async Task AddAsync(User user)
        => await _context.Users.AddAsync(user);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();

    public async Task<List<User>> GetAllAsync(string? role = null)
    {
        var q = _context.Users.AsQueryable();
        if (!string.IsNullOrWhiteSpace(role))
            q = q.Where(u => u.Role == role);
        return await q.OrderBy(u => u.FullName).ToListAsync();
    }

    public Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        return Task.CompletedTask;
    }
}