using TobacoBackend.Domain.Models;

namespace TobacoBackend.Domain.IRepositories
{
    public interface IUserRepository
    {
        Task<User?> GetByUserNameAsync(string userName);
        Task<User?> GetByIdAsync(int id);
        Task<User> AddAsync(User user);
        Task<User> CreateAsync(User user);
        Task UpdateAsync(User user);
        Task<bool> ExistsAsync(string userName);
        Task<IEnumerable<User>> GetAllAsync();
        Task DeleteAsync(int id);
    }
}
