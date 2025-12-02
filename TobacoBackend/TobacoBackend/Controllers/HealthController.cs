using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;

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
        private readonly IServiceProvider _serviceProvider;

        public HealthController(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Endpoint de health check básico
        /// </summary>
        /// <returns>Estado de salud del servidor</returns>
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                status = "healthy",
                timestamp = DateTime.UtcNow,
                service = "TobacoBackend",
                version = "1.0.0"
            });
        }

        /// <summary>
        /// Endpoint de health check detallado
        /// </summary>
        /// <returns>Estado detallado de todos los componentes</returns>
        [HttpGet("detailed")]
        public async Task<IActionResult> GetDetailed()
        {
            var healthCheckService = _serviceProvider.GetRequiredService<HealthCheckService>();
            var healthReport = await healthCheckService.CheckHealthAsync();
            
            return Ok(new
            {
                status = healthReport.Status.ToString(),
                timestamp = DateTime.UtcNow,
                service = "TobacoBackend",
                version = "1.0.0",
                checks = healthReport.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description,
                    duration = e.Value.Duration.TotalMilliseconds,
                    data = e.Value.Data
                }),
                totalDuration = healthReport.TotalDuration.TotalMilliseconds
            });
        }
    }
}

