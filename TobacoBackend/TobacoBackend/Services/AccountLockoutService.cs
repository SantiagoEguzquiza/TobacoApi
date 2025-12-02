using Microsoft.Extensions.Caching.Memory;

namespace TobacoBackend.Services
{
    /// <summary>
    /// Servicio para manejar el bloqueo de cuentas después de múltiples intentos fallidos
    /// </summary>
    public class AccountLockoutService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<AccountLockoutService> _logger;
        private const int MaxFailedAttempts = 5;
        private const int LockoutDurationMinutes = 15;

        public AccountLockoutService(IMemoryCache cache, ILogger<AccountLockoutService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// Registra un intento fallido de login
        /// </summary>
        public void RecordFailedAttempt(string userName, string? ipAddress = null)
        {
            var cacheKey = GetCacheKey(userName);
            var attempts = _cache.Get<int?>(cacheKey) ?? 0;
            attempts++;

            _cache.Set(cacheKey, attempts, TimeSpan.FromMinutes(LockoutDurationMinutes));

            _logger.LogWarning(
                "Intento de login fallido #{Attempt} para usuario: {UserName}, IP: {IpAddress}",
                attempts, userName, ipAddress ?? "Desconocida");

            if (attempts >= MaxFailedAttempts)
            {
                _logger.LogError(
                    "Cuenta bloqueada por {Duration} minutos - Usuario: {UserName}, IP: {IpAddress}",
                    LockoutDurationMinutes, userName, ipAddress ?? "Desconocida");
            }
        }

        /// <summary>
        /// Limpia los intentos fallidos después de un login exitoso
        /// </summary>
        public void ClearFailedAttempts(string userName)
        {
            var cacheKey = GetCacheKey(userName);
            _cache.Remove(cacheKey);
        }

        /// <summary>
        /// Verifica si una cuenta está bloqueada
        /// </summary>
        public bool IsAccountLocked(string userName)
        {
            var cacheKey = GetCacheKey(userName);
            var attempts = _cache.Get<int?>(cacheKey) ?? 0;
            return attempts >= MaxFailedAttempts;
        }

        /// <summary>
        /// Obtiene el número de intentos fallidos restantes
        /// </summary>
        public int GetRemainingAttempts(string userName)
        {
            var cacheKey = GetCacheKey(userName);
            var attempts = _cache.Get<int?>(cacheKey) ?? 0;
            return Math.Max(0, MaxFailedAttempts - attempts);
        }

        /// <summary>
        /// Obtiene el tiempo de bloqueo restante en minutos
        /// </summary>
        public int? GetLockoutRemainingMinutes(string userName)
        {
            if (!IsAccountLocked(userName))
                return null;

            var cacheKey = GetCacheKey(userName);
            if (_cache.TryGetValue(cacheKey, out _))
            {
                // El cache expira en LockoutDurationMinutes, así que retornamos ese tiempo
                return LockoutDurationMinutes;
            }

            return null;
        }

        private string GetCacheKey(string userName)
        {
            return $"failed_login_attempts_{userName.ToLowerInvariant()}";
        }
    }
}

