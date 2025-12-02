using System.Security.Claims;

namespace TobacoBackend.Services
{
    /// <summary>
    /// Servicio para logging de eventos de seguridad
    /// </summary>
    public class SecurityLoggingService
    {
        private readonly ILogger<SecurityLoggingService> _logger;

        public SecurityLoggingService(ILogger<SecurityLoggingService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Registra un intento de login fallido
        /// </summary>
        public void LogFailedLoginAttempt(string userName, string? ipAddress = null)
        {
            _logger.LogWarning(
                "Intento de login fallido - Usuario: {UserName}, IP: {IpAddress}, Fecha: {Timestamp}",
                userName, ipAddress ?? "Desconocida", DateTime.UtcNow
            );
        }

        /// <summary>
        /// Registra un login exitoso
        /// </summary>
        public void LogSuccessfulLogin(string userName, int userId, string? ipAddress = null)
        {
            _logger.LogInformation(
                "Login exitoso - Usuario: {UserName}, UserId: {UserId}, IP: {IpAddress}, Fecha: {Timestamp}",
                userName, userId, ipAddress ?? "Desconocida", DateTime.UtcNow
            );
        }

        /// <summary>
        /// Registra un intento de acceso no autorizado
        /// </summary>
        public void LogUnauthorizedAccess(string? userName, string endpoint, string? ipAddress = null)
        {
            _logger.LogWarning(
                "Acceso no autorizado - Usuario: {UserName}, Endpoint: {Endpoint}, IP: {IpAddress}, Fecha: {Timestamp}",
                userName ?? "Anónimo", endpoint, ipAddress ?? "Desconocida", DateTime.UtcNow
            );
        }

        /// <summary>
        /// Registra una operación sensible (crear, actualizar, eliminar)
        /// </summary>
        public void LogSensitiveOperation(string operation, string entity, int? entityId, ClaimsPrincipal? user, string? ipAddress = null)
        {
            var userName = user?.Identity?.Name ?? "Desconocido";
            var userId = user?.FindFirst("sub")?.Value ?? "Desconocido";
            
            _logger.LogInformation(
                "Operación sensible - Operación: {Operation}, Entidad: {Entity}, Id: {EntityId}, Usuario: {UserName} (Id: {UserId}), IP: {IpAddress}, Fecha: {Timestamp}",
                operation, entity, entityId?.ToString() ?? "N/A", userName, userId, ipAddress ?? "Desconocida", DateTime.UtcNow
            );
        }

        /// <summary>
        /// Obtiene la dirección IP del cliente desde el HttpContext
        /// </summary>
        public static string? GetClientIpAddress(HttpContext? context)
        {
            if (context == null) return null;

            // Intentar obtener la IP real (puede estar detrás de un proxy/load balancer)
            var ipAddress = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            
            if (string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            }
            
            if (string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = context.Connection.RemoteIpAddress?.ToString();
            }

            return ipAddress;
        }
    }
}

