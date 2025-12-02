using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using TobacoBackend.Domain.IServices;
using TobacoBackend.Domain.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace TobacoBackend.Authorization
{
    /// <summary>
    /// Requisito de autorización basado en roles y tipos de vendedor
    /// </summary>
    public class RoleRequirement : IAuthorizationRequirement
    {
        public string[] AllowedRoles { get; }
        public TipoVendedor[]? AllowedTipoVendedor { get; }

        public RoleRequirement(string[] allowedRoles, TipoVendedor[]? allowedTipoVendedor = null)
        {
            AllowedRoles = allowedRoles;
            AllowedTipoVendedor = allowedTipoVendedor;
        }
    }

    /// <summary>
    /// Handler para verificar requisitos de rol
    /// </summary>
    public class RoleRequirementHandler : AuthorizationHandler<RoleRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<RoleRequirementHandler>? _logger;

        public RoleRequirementHandler(IHttpContextAccessor httpContextAccessor, ILogger<RoleRequirementHandler>? logger = null)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            RoleRequirement requirement)
        {
            try
            {
                // Intentar obtener el userId del claim "sub" o "NameIdentifier"
                var userIdClaim = context.User.FindFirst("sub")?.Value 
                    ?? context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    _logger?.LogWarning("No se pudo obtener el userId del token. Claims disponibles: {Claims}", 
                        string.Join(", ", context.User.Claims.Select(c => $"{c.Type}={c.Value}")));
                    context.Fail();
                    return;
                }

                // Obtener el HttpContext - usar el del contexto si está disponible, sino el del accessor
                var httpContext = context.Resource as HttpContext ?? _httpContextAccessor.HttpContext;
                if (httpContext == null)
                {
                    _logger?.LogWarning("HttpContext no disponible en el handler de autorización.");
                    context.Fail();
                    return;
                }

                // Obtener el usuario desde el repositorio directamente
                var userRepository = httpContext.RequestServices.GetRequiredService<Domain.IRepositories.IUserRepository>();
                var user = await userRepository.GetByIdAsync(userId);
                
                if (user == null)
                {
                    _logger?.LogWarning("Usuario con ID {UserId} no encontrado en la base de datos.", userId);
                    context.Fail();
                    return;
                }

                // Verificar si el usuario está activo
                if (!user.IsActive)
                {
                    _logger?.LogWarning("Usuario {UserId} ({UserName}) está inactivo.", userId, user.UserName);
                    context.Fail();
                    return;
                }

                // Verificar rol
                bool roleMatches = requirement.AllowedRoles.Contains(user.Role, StringComparer.OrdinalIgnoreCase);
                
                if (!roleMatches)
                {
                    _logger?.LogWarning("Usuario {UserId} ({UserName}) con rol {Role} no tiene permiso. Roles permitidos: {AllowedRoles}", 
                        userId, user.UserName, user.Role, string.Join(", ", requirement.AllowedRoles));
                    context.Fail();
                    return;
                }

                // Si es Admin, tiene acceso completo
                if (user.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                {
                    _logger?.LogDebug("Usuario Admin {UserId} ({UserName}) autorizado.", userId, user.UserName);
                    context.Succeed(requirement);
                    return;
                }

                // Si es Employee, verificar TipoVendedor si se especificó
                if (user.Role.Equals("Employee", StringComparison.OrdinalIgnoreCase))
                {
                    if (requirement.AllowedTipoVendedor == null || requirement.AllowedTipoVendedor.Length == 0)
                    {
                        // No hay restricción de TipoVendedor, cualquier Employee puede acceder
                        _logger?.LogDebug("Employee {UserId} ({UserName}) autorizado (sin restricción de TipoVendedor).", userId, user.UserName);
                        context.Succeed(requirement);
                        return;
                    }

                    // Verificar TipoVendedor
                    bool tipoVendedorMatches = requirement.AllowedTipoVendedor.Contains(user.TipoVendedor);
                    if (tipoVendedorMatches)
                    {
                        _logger?.LogDebug("Employee {UserId} ({UserName}) con TipoVendedor {TipoVendedor} autorizado.", 
                            userId, user.UserName, user.TipoVendedor);
                        context.Succeed(requirement);
                    }
                    else
                    {
                        _logger?.LogWarning("Employee {UserId} ({UserName}) con TipoVendedor {TipoVendedor} no autorizado. Tipos permitidos: {AllowedTypes}", 
                            userId, user.UserName, user.TipoVendedor, string.Join(", ", requirement.AllowedTipoVendedor));
                        context.Fail();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error en el handler de autorización.");
                context.Fail();
            }
        }
    }
}

