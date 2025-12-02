using System.Security.Claims;
using TobacoBackend.Domain.IServices;
using Microsoft.AspNetCore.Http;

namespace TobacoBackend.Helpers
{
    /// <summary>
    /// Helper para validar permisos de usuarios
    /// </summary>
    public static class PermissionHelper
    {
        /// <summary>
        /// Obtiene el ID del usuario desde los claims del token
        /// </summary>
        public static int? GetUserIdFromClaims(ClaimsPrincipal? user)
        {
            if (user == null) return null;

            var userIdClaim = user.FindFirst("sub")?.Value 
                ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return null;

            return userId;
        }

        /// <summary>
        /// Verifica si el usuario tiene un permiso específico
        /// </summary>
        public static async Task<bool> HasPermissionAsync(
            ClaimsPrincipal? user,
            IServiceProvider serviceProvider,
            string permission)
        {
            var userId = GetUserIdFromClaims(user);
            if (!userId.HasValue)
                return false;

            var userService = serviceProvider.GetRequiredService<IUserService>();
            var userDto = await userService.GetUserByIdAsync(userId.Value);
            
            // Los admins siempre tienen todos los permisos
            if (userDto?.Role == "Admin")
                return true;

            var permisosService = serviceProvider.GetRequiredService<IPermisosEmpleadoService>();
            return await permisosService.HasPermissionAsync(userId.Value, permission);
        }

        /// <summary>
        /// Verifica si el usuario puede visualizar un módulo
        /// </summary>
        public static async Task<bool> CanViewModuleAsync(
            ClaimsPrincipal? user,
            IServiceProvider serviceProvider,
            string module)
        {
            var permission = $"{module}_Visualizar";
            return await HasPermissionAsync(user, serviceProvider, permission);
        }
    }
}

