using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TobacoBackend.Domain.IServices;
using TobacoBackend.DTOs;
using System.Security.Claims;

namespace TobacoBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AsistenciaController : ControllerBase
    {
        private readonly IAsistenciaService _asistenciaService;
        private readonly IUserService _userService;

        public AsistenciaController(IAsistenciaService asistenciaService, IUserService userService)
        {
            _asistenciaService = asistenciaService;
            _userService = userService;
        }

        [HttpPost("registrar-entrada")]
        public async Task<ActionResult<AsistenciaDTO>> RegistrarEntrada([FromBody] RegistrarEntradaDTO registrarEntradaDto)
        {
            try
            {
                if (registrarEntradaDto == null)
                    return BadRequest(new { message = "Los datos de entrada no pueden ser nulos." });

                var asistencia = await _asistenciaService.RegistrarEntradaAsync(registrarEntradaDto);
                return Ok(asistencia);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error al registrar la entrada: {ex.Message}" });
            }
        }

        [HttpPost("registrar-salida")]
        public async Task<ActionResult<AsistenciaDTO>> RegistrarSalida([FromBody] RegistrarSalidaDTO registrarSalidaDto)
        {
            try
            {
                if (registrarSalidaDto == null)
                    return BadRequest(new { message = "Los datos de salida no pueden ser nulos." });

                var asistencia = await _asistenciaService.RegistrarSalidaAsync(registrarSalidaDto);
                
                if (asistencia == null)
                    return NotFound(new { message = "Asistencia no encontrada." });

                return Ok(asistencia);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error al registrar la salida: {ex.Message}" });
            }
        }

        [HttpGet("activa/{userId}")]
        public async Task<ActionResult<AsistenciaDTO>> GetAsistenciaActiva(int userId)
        {
            try
            {
                var asistencia = await _asistenciaService.GetAsistenciaActivaByUserIdAsync(userId);
                
                if (asistencia == null)
                    return NotFound(new { message = "No hay asistencia activa para este usuario." });

                return Ok(asistencia);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error al obtener la asistencia activa: {ex.Message}" });
            }
        }

        [HttpGet("usuario/{userId}")]
        public async Task<ActionResult<IEnumerable<AsistenciaDTO>>> GetAsistenciasByUserId(int userId)
        {
            try
            {
                var asistencias = await _asistenciaService.GetAsistenciasByUserIdAsync(userId);
                return Ok(asistencias);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error al obtener las asistencias: {ex.Message}" });
            }
        }

        [HttpGet("usuario/{userId}/rango")]
        public async Task<ActionResult<IEnumerable<AsistenciaDTO>>> GetAsistenciasByUserIdAndDateRange(
            int userId, 
            [FromQuery] DateTime fechaInicio, 
            [FromQuery] DateTime fechaFin)
        {
            try
            {
                var asistencias = await _asistenciaService.GetAsistenciasByUserIdAndDateRangeAsync(userId, fechaInicio, fechaFin);
                return Ok(asistencias);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error al obtener las asistencias: {ex.Message}" });
            }
        }

        [HttpGet("todas")]
        public async Task<ActionResult<IEnumerable<AsistenciaDTO>>> GetAllAsistencias()
        {
            try
            {
                // Verificar si el usuario es admin
                var subClaim = User.FindFirst("sub")?.Value;
                var nameIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userIdClaim = subClaim ?? nameIdClaim;
                
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                    return Unauthorized(new { message = "Token inválido." });

                var isAdmin = await _userService.IsAdminAsync(userId);
                if (!isAdmin)
                    return Forbid("Solo los administradores pueden acceder a esta funcionalidad.");

                var asistencias = await _asistenciaService.GetAllAsistenciasAsync();
                return Ok(asistencias);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error al obtener todas las asistencias: {ex.Message}" });
            }
        }

        [HttpGet("rango")]
        public async Task<ActionResult<IEnumerable<AsistenciaDTO>>> GetAsistenciasByDateRange(
            [FromQuery] DateTime fechaInicio, 
            [FromQuery] DateTime fechaFin)
        {
            try
            {
                // Verificar si el usuario es admin
                var subClaim = User.FindFirst("sub")?.Value;
                var nameIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userIdClaim = subClaim ?? nameIdClaim;
                
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                    return Unauthorized(new { message = "Token inválido." });

                var isAdmin = await _userService.IsAdminAsync(userId);
                if (!isAdmin)
                    return Forbid("Solo los administradores pueden acceder a esta funcionalidad.");

                var asistencias = await _asistenciaService.GetAsistenciasByDateRangeAsync(fechaInicio, fechaFin);
                return Ok(asistencias);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error al obtener las asistencias: {ex.Message}" });
            }
        }
    }
}

