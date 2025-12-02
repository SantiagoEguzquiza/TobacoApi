using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TobacoBackend.Authorization;
using TobacoBackend.Domain.IServices;
using TobacoBackend.DTOs;
using TobacoBackend.Services;

namespace TobacoBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Todos los endpoints requieren autenticación
    public class PermisosEmpleadoController : ControllerBase
    {
        private readonly IPermisosEmpleadoService _permisosService;
        private readonly IUserService _userService;
        private readonly AuditService _auditService;

        public PermisosEmpleadoController(
            IPermisosEmpleadoService permisosService,
            IUserService userService,
            AuditService auditService)
        {
            _permisosService = permisosService;
            _userService = userService;
            _auditService = auditService;
        }

        /// <summary>
        /// Obtener permisos de un empleado (solo Admin)
        /// </summary>
        [HttpGet("usuario/{userId}")]
        [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
        public async Task<ActionResult<PermisosEmpleadoDTO>> GetPermisosByUserId(int userId)
        {
            try
            {
                // Verificar que el usuario existe y es un empleado
                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "Usuario no encontrado." });
                }

                if (user.Role == "Admin")
                {
                    return BadRequest(new { message = "Los administradores no tienen permisos restringidos." });
                }

                var permisos = await _permisosService.GetPermisosByUserIdAsync(userId);
                if (permisos == null)
                {
                    return NotFound(new { message = "Permisos no encontrados." });
                }

                return Ok(permisos);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error al obtener permisos: {ex.Message}" });
            }
        }

        /// <summary>
        /// Actualizar permisos de un empleado (solo Admin)
        /// </summary>
        [HttpPut("usuario/{userId}")]
        [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
        public async Task<ActionResult<PermisosEmpleadoDTO>> UpdatePermisos(int userId, [FromBody] UpdatePermisosEmpleadoDTO updateDto)
        {
            try
            {
                // Verificar que el usuario existe y es un empleado
                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "Usuario no encontrado." });
                }

                if (user.Role == "Admin")
                {
                    return BadRequest(new { message = "Los administradores no tienen permisos restringidos." });
                }

                // Obtener ID del admin que está haciendo el cambio
                var adminIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(adminIdClaim) || !int.TryParse(adminIdClaim, out int adminId))
                {
                    return Unauthorized(new { message = "Token inválido." });
                }

                var permisos = await _permisosService.UpdatePermisosAsync(userId, updateDto);

                // Auditoría
                _auditService.LogUpdate("PermisosEmpleado", permisos.Id, User,
                    SecurityLoggingService.GetClientIpAddress(HttpContext),
                    $"Permisos actualizados para usuario {user.UserName}");

                return Ok(permisos);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error al actualizar permisos: {ex.Message}" });
            }
        }

        /// <summary>
        /// Obtener mis propios permisos (para empleados)
        /// </summary>
        [HttpGet("mis-permisos")]
        public async Task<ActionResult<PermisosEmpleadoDTO>> GetMisPermisos()
        {
            try
            {
                var userIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Token inválido." });
                }

                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "Usuario no encontrado." });
                }

                // Si es Admin, retornar todos los permisos como true
                if (user.Role == "Admin")
                {
                    return Ok(new PermisosEmpleadoDTO
                    {
                        UserId = userId,
                        Productos_Visualizar = true,
                        Productos_Crear = true,
                        Productos_Editar = true,
                        Productos_Eliminar = true,
                        Clientes_Visualizar = true,
                        Clientes_Crear = true,
                        Clientes_Editar = true,
                        Clientes_Eliminar = true,
                        Ventas_Visualizar = true,
                        Ventas_Crear = true,
                        Ventas_EditarBorrador = true,
                        Ventas_Eliminar = true,
                        CuentaCorriente_Visualizar = true,
                        CuentaCorriente_RegistrarAbonos = true,
                        Entregas_Visualizar = true,
                        Entregas_ActualizarEstado = true
                    });
                }

                var permisos = await _permisosService.GetPermisosByUserIdAsync(userId);
                return Ok(permisos);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error al obtener permisos: {ex.Message}" });
            }
        }
    }
}

