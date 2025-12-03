using TobacoBackend.DTOs;

namespace TobacoBackend.Domain.IServices
{
    public interface ITenantService
    {
        Task<List<TenantDTO>> GetAllTenantsAsync();
        Task<TenantDTO?> GetTenantByIdAsync(int id);
        Task<TenantDTO> CreateTenantAsync(CreateTenantDTO createTenantDto);
        Task<TenantDTO> UpdateTenantAsync(int id, UpdateTenantDTO updateTenantDto);
        Task<bool> DeleteTenantAsync(int id);
    }
}

