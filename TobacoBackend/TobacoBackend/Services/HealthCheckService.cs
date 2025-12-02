using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace TobacoBackend.Services
{
    /// <summary>
    /// Health check para verificar el estado de la base de datos
    /// </summary>
    public class DatabaseHealthCheck : IHealthCheck
    {
        private readonly AplicationDbContext _dbContext;

        public DatabaseHealthCheck(AplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Intentar ejecutar una consulta simple
                var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);
                
                if (!canConnect)
                {
                    return HealthCheckResult.Unhealthy("No se puede conectar a la base de datos.");
                }

                // Ejecutar una consulta simple para verificar que funciona
                await _dbContext.Database.ExecuteSqlRawAsync("SELECT 1", cancellationToken);

                return HealthCheckResult.Healthy("Base de datos está disponible y respondiendo.");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Error al verificar la base de datos.", ex);
            }
        }
    }

    /// <summary>
    /// Health check para verificar el estado del sistema
    /// </summary>
    public class SystemHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Verificar memoria disponible
                var process = System.Diagnostics.Process.GetCurrentProcess();
                var memoryUsage = process.WorkingSet64 / (1024.0 * 1024.0); // MB

                // Verificar espacio en disco
                var drive = new DriveInfo(AppDomain.CurrentDomain.BaseDirectory);
                var freeSpaceGB = drive.AvailableFreeSpace / (1024.0 * 1024.0 * 1024.0);

                var data = new Dictionary<string, object>
                {
                    { "MemoryUsageMB", memoryUsage },
                    { "FreeDiskSpaceGB", freeSpaceGB },
                    { "Uptime", DateTime.UtcNow - process.StartTime.ToUniversalTime() }
                };

                // Considerar unhealthy si hay menos de 1GB de espacio libre
                if (freeSpaceGB < 1)
                {
                    return Task.FromResult(HealthCheckResult.Degraded(
                        "Espacio en disco bajo.",
                        data: data));
                }

                return Task.FromResult(HealthCheckResult.Healthy("Sistema funcionando correctamente.", data));
            }
            catch (Exception ex)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy("Error al verificar el sistema.", ex));
            }
        }
    }
}

