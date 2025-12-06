using TobacoBackend.DTOs;

namespace TobacoBackend.Domain.IServices
{
    public interface IPermisosEmpleadoService
    {
        Task<PermisosEmpleadoDTO?> GetPermisosByUserIdAsync(int userId);
        Task<PermisosEmpleadoDTO> UpdatePermisosAsync(int userId, UpdatePermisosEmpleadoDTO updateDto);
        Task<PermisosEmpleadoDTO> CreateDefaultPermisosAsync(int userId);
        Task<bool> HasPermissionAsync(int userId, string permission);
    }
}

