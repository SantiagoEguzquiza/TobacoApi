using System.Security.Claims;

namespace TobacoBackend.Services
{
    /// <summary>
    /// Servicio para auditoría de operaciones críticas
    /// </summary>
    public class AuditService
    {
        private readonly ILogger<AuditService> _logger;

        public AuditService(ILogger<AuditService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Registra una operación de auditoría
        /// </summary>
        public void LogAuditEvent(
            string action,
            string entityType,
            int? entityId,
            ClaimsPrincipal? user,
            string? details = null,
            string? ipAddress = null)
        {
            var userName = user?.Identity?.Name ?? "Sistema";
            var userId = user?.FindFirst("sub")?.Value ?? "N/A";

            _logger.LogInformation(
                "[AUDIT] Acción: {Action}, Entidad: {EntityType}, Id: {EntityId}, Usuario: {UserName} (Id: {UserId}), IP: {IpAddress}, Detalles: {Details}, Fecha: {Timestamp}",
                action,
                entityType,
                entityId?.ToString() ?? "N/A",
                userName,
                userId,
                ipAddress ?? "Desconocida",
                details ?? "N/A",
                DateTime.UtcNow
            );
        }

        /// <summary>
        /// Registra creación de entidad
        /// </summary>
        public void LogCreate(string entityType, int entityId, ClaimsPrincipal? user, string? ipAddress = null)
        {
            LogAuditEvent("CREATE", entityType, entityId, user, null, ipAddress);
        }

        /// <summary>
        /// Registra actualización de entidad
        /// </summary>
        public void LogUpdate(string entityType, int entityId, ClaimsPrincipal? user, string? ipAddress = null, string? details = null)
        {
            LogAuditEvent("UPDATE", entityType, entityId, user, details, ipAddress);
        }

        /// <summary>
        /// Registra eliminación de entidad
        /// </summary>
        public void LogDelete(string entityType, int entityId, ClaimsPrincipal? user, string? ipAddress = null)
        {
            LogAuditEvent("DELETE", entityType, entityId, user, null, ipAddress);
        }

        /// <summary>
        /// Registra acceso a datos sensibles
        /// </summary>
        public void LogSensitiveAccess(string resource, ClaimsPrincipal? user, string? ipAddress = null)
        {
            LogAuditEvent("SENSITIVE_ACCESS", resource, null, user, null, ipAddress);
        }
    }
}

