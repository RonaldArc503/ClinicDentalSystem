using Identity.Domain.Entities;

namespace Identity.Application.Interfaces
{
    public interface IJwtTokenGenerator
    {
        Task<string> GenerateAccessTokenAsync(User user);
        string GenerateRefreshToken();
    }
}
