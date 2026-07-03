using Identity.Domain.Entities;
using Identity.Domain.ValueObjects;

namespace Identity.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByEmailAsync(Email email);
        Task<User?> GetByUsernameAsync(Username username);
        Task<bool> ExistsByEmailAsync(Email email);
        Task<bool> ExistsByUsernameAsync(Username username);
        Task AddAsync(User user);
        Task UpdateAsync(User user);
    }
}
