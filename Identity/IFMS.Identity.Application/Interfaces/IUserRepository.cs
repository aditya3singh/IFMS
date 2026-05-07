using IFMS.Identity.Domain.Entities;

namespace IFMS.Identity.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByPhoneAsync(string normalizedPhoneDigits);
    Task<User?> GetByIdAsync(Guid id);
    Task<bool> ExistsAsync(string email);
    Task<bool> ExistsPhoneAsync(string normalizedPhoneDigits);
    Task AddAsync(User user);
    Task SaveChangesAsync();
    Task<List<User>> GetAllAsync(string? role = null);
    Task UpdateAsync(User user);
}