using TobacoBackend.Domain.Models;

namespace TobacoBackend.Domain.IRepositories
{
    public interface IPermisosEmpleadoRepository
    {
        Task<PermisosEmpleado?> GetByUserIdAsync(int userId);
        Task<PermisosEmpleado> CreateAsync(PermisosEmpleado permisos);
        Task<PermisosEmpleado> UpdateAsync(PermisosEmpleado permisos);
        Task<bool> DeleteAsync(int id);
    }
}

