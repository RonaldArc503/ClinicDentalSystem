using Identity.Application.Interfaces;
using Identity.Domain.Entities;
using Identity.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Identity.Persistence.Repositories
{
    public sealed class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly IdentityDbContext _dbContext;

        public RefreshTokenRepository(IdentityDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            return await _dbContext.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == token);
        }

        public async Task<User?> GetUserByRefreshTokenAsync(string token)
        {
            var refreshToken = await _dbContext.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == token);

            return refreshToken?.User;
        }

        public async Task AddAsync(RefreshToken refreshToken)
        {
            await _dbContext.RefreshTokens.AddAsync(refreshToken);
        }

        public Task UpdateAsync(RefreshToken refreshToken)
        {
            _dbContext.RefreshTokens.Update(refreshToken);
            return Task.CompletedTask;
        }

        public async Task RevokeAsync(string token)
        {
            var refreshToken = await _dbContext.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == token);

            refreshToken?.Revoke();
        }

        public async Task RevokeAllForUserAsync(Guid userId)
        {
            var activeTokens = await _dbContext.RefreshTokens
                .Where(rt => rt.UserId == userId && rt.IsActive)
                .ToListAsync();

            foreach (var token in activeTokens)
            {
                token.Revoke();
            }
        }
    }
}
