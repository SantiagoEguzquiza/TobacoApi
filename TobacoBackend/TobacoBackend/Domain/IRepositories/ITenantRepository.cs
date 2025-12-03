using TobacoBackend.Domain.Models;

namespace TobacoBackend.Domain.IRepositories
{
    public interface ITenantRepository
    {
        Task<List<Tenant>> GetAllTenantsAsync();
        Task<Tenant?> GetTenantByIdAsync(int id);
        Task<Tenant> CreateTenantAsync(Tenant tenant);
        Task<Tenant> UpdateTenantAsync(Tenant tenant);
        Task<bool> DeleteTenantAsync(int id);
        Task<bool> TenantExistsAsync(int id);
        Task<bool> TenantNameExistsAsync(string nombre, int? excludeId = null);
    }
}

