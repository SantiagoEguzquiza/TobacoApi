using TobacoBackend.Domain.Models;

namespace TobacoBackend.Domain.IRepositories
{
    public interface IUserRepository
    {
        Task<User?> GetByUserNameAsync(string userName);
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByIdWithoutTenantFilterAsync(int id);
        Task<User> AddAsync(User user);
        Task<User> CreateAsync(User user);
        Task UpdateAsync(User user);
        Task<bool> ExistsAsync(string userName);
        Task<IEnumerable<User>> GetAllAsync();
        Task<IEnumerable<User>> GetUsersByTenantIdAsync(int tenantId);
        Task DeleteAsync(int id);
        Task DeleteAsyncWithoutTenantFilter(int id);
        Task<IEnumerable<User>> GetSubUsersByCreatedByIdAsync(int createdById);
        Task UpdateSubUsersPlanAsync(int createdById, PlanType newPlan);
    }
}
