using System.Text;
using System.Text.RegularExpressions;

namespace TobacoBackend.Middleware
{
    /// <summary>
    /// Middleware para validación y sanitización de entrada
    /// </summary>
    public class InputValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<InputValidationMiddleware> _logger;

        public InputValidationMiddleware(RequestDelegate next, ILogger<InputValidationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Validar tamaño de request
            if (context.Request.ContentLength > 10_485_760) // 10MB
            {
                context.Response.StatusCode = 413; // Payload Too Large
                await context.Response.WriteAsync("Request too large. Maximum size is 10MB.");
                return;
            }

            // Validar headers sospechosos
            if (HasSuspiciousHeaders(context.Request))
            {
                var ipAddress = GetClientIpAddress(context);
                _logger.LogWarning("Request con headers sospechosos desde IP: {IpAddress}", ipAddress);
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Invalid request headers.");
                return;
            }

            await _next(context);
        }

        private bool HasSuspiciousHeaders(HttpRequest request)
        {
            // Detectar patrones sospechosos en headers
            var suspiciousPatterns = new[]
            {
                @"<script",
                @"javascript:",
                @"onerror=",
                @"onload=",
                @"eval\(",
                @"expression\("
            };

            foreach (var header in request.Headers)
            {
                var headerValue = header.Value.ToString();
                foreach (var pattern in suspiciousPatterns)
                {
                    if (Regex.IsMatch(headerValue, pattern, RegexOptions.IgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static string? GetClientIpAddress(HttpContext context)
        {
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

    public static class InputValidationMiddlewareExtensions
    {
        public static IApplicationBuilder UseInputValidation(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<InputValidationMiddleware>();
        }
    }
}

