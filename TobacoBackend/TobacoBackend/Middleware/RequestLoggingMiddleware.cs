using TobacoBackend.Services;

namespace TobacoBackend.Middleware
{
    /// <summary>
    /// Middleware para logging de requests (útil para debugging y auditoría)
    /// </summary>
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;
        private readonly MetricsService? _metricsService;

        public RequestLoggingMiddleware(
            RequestDelegate next, 
            ILogger<RequestLoggingMiddleware> logger,
            IServiceProvider serviceProvider)
        {
            _next = next;
            _logger = logger;
            _metricsService = serviceProvider.GetService<MetricsService>();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var startTime = DateTime.UtcNow;
            var path = context.Request.Path;
            var method = context.Request.Method;
            var ipAddress = GetClientIpAddress(context);

            // Log request
            _logger.LogInformation(
                "Request: {Method} {Path} desde IP: {IpAddress}",
                method, path, ipAddress);

            await _next(context);

            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            var statusCode = context.Response.StatusCode;
            var durationSeconds = duration / 1000.0;

            // Registrar métricas
            _metricsService?.RecordRequest(method, path, statusCode, durationSeconds);

            // Log response (solo errores o requests lentos)
            if (statusCode >= 400 || duration > 1000)
            {
                _logger.LogWarning(
                    "Response: {Method} {Path} - Status: {StatusCode} - Duration: {Duration}ms - IP: {IpAddress}",
                    method, path, statusCode, duration, ipAddress);
            }
        }

        private static string? GetClientIpAddress(HttpContext context)
        {
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

    public static class RequestLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestLoggingMiddleware>();
        }
    }
}

