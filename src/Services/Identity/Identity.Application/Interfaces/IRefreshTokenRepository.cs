using Identity.Domain.Entities;

namespace Identity.Application.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken?> GetByTokenAsync(string token);
        Task<User?> GetUserByRefreshTokenAsync(string token);
        Task AddAsync(RefreshToken refreshToken);
        Task UpdateAsync(RefreshToken refreshToken);
        Task RevokeAsync(string token);
        Task RevokeAllForUserAsync(Guid userId);
    }
}
