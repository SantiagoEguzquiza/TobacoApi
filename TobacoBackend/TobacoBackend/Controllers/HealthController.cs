using Microsoft.AspNetCore.Mvc;

namespace TobacoBackend.Controllers
{
    /// <summary>
    /// Controlador para verificar el estado de salud del API
    /// Utilizado por el sistema offline para detectar disponibilidad del backend
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        /// <summary>
        /// Endpoint de health check
        /// </summary>
        /// <returns>Estado de salud del servidor</returns>
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                status = "healthy",
                timestamp = DateTime.UtcNow,
                service = "TobacoBackend"
            });
        }
    }
}

