using IFMS.Identity.Domain.Entities;

namespace IFMS.Identity.Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(User user);
}