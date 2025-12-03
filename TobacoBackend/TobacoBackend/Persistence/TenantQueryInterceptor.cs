using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace TobacoBackend.Persistence
{
    public class TenantQueryInterceptor : IQueryExpressionInterceptor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string TenantIdClaim = "tenant_id";

        public TenantQueryInterceptor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private int? GetCurrentTenantId()
        {
            if (_httpContextAccessor?.HttpContext == null)
                return null;

            var tenantIdClaim = _httpContextAccessor.HttpContext.User?.FindFirst(TenantIdClaim)?.Value;
            if (!string.IsNullOrEmpty(tenantIdClaim) && int.TryParse(tenantIdClaim, out int tenantId))
            {
                return tenantId;
            }

            return null;
        }

        // Este interceptor se ejecutará antes de cada consulta
        // Por ahora, vamos a usar un enfoque más simple: aplicar el filtro en los repositorios
        // Este archivo queda como placeholder para futuras mejoras
    }
}

