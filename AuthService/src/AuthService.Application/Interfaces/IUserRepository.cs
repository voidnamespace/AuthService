using AuthService.Domain.Entities;

namespace AuthService.Application.Interfaces;


public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);

    Task<User?> GetByEmailAsync(string email);

    Task<User> CreateAsync(User user);

    Task UpdateAsync(User user);

    Task DeleteAsync(Guid id);

    Task<bool> ExistsByEmailAsync(string email);

    Task<IEnumerable<User>> GetAllAsync();
}
