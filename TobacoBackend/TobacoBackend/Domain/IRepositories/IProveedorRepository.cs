using TobacoBackend.Domain.Models;

namespace TobacoBackend.Domain.IRepositories
{
    public interface IProveedorRepository
    {
        Task<List<Proveedor>> GetAllByTenantAsync();
        Task<Proveedor?> GetByIdAsync(int id);
        Task<Proveedor> CreateAsync(Proveedor proveedor);
        Task<bool> ExistsNombreAsync(string nombre, int? excludeId = null);
    }
}
