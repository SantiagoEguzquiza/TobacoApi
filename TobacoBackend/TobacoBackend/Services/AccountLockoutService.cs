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
            var lockoutKey = GetLockoutKey(userName);
            
            // Usar TryGetValue para evitar problemas de concurrencia
            if (!_cache.TryGetValue(cacheKey, out int? currentAttempts))
            {
                currentAttempts = 0;
            }
            
            var attempts = (currentAttempts ?? 0) + 1;

            // Si alcanza el máximo, guardar el tiempo de bloqueo
            if (attempts >= MaxFailedAttempts)
            {
                var lockoutTime = DateTime.UtcNow;
                _cache.Set(lockoutKey, lockoutTime, TimeSpan.FromMinutes(LockoutDurationMinutes));
                
                _logger.LogError(
                    "Cuenta bloqueada por {Duration} minutos - Usuario: {UserName}, IP: {IpAddress}",
                    LockoutDurationMinutes, userName, ipAddress ?? "Desconocida");
            }

            // Guardar intentos con expiración - usar opciones de cache para asegurar consistencia
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(LockoutDurationMinutes),
                SlidingExpiration = null // No usar sliding expiration para evitar que se resetee
            };
            _cache.Set(cacheKey, attempts, cacheOptions);

            _logger.LogWarning(
                "Intento de login fallido #{Attempt}/{MaxAttempts} para usuario: {UserName}, IP: {IpAddress}. Intentos restantes: {Remaining}",
                attempts, MaxFailedAttempts, userName, ipAddress ?? "Desconocida", Math.Max(0, MaxFailedAttempts - attempts));
        }

        /// <summary>
        /// Limpia los intentos fallidos después de un login exitoso
        /// </summary>
        public void ClearFailedAttempts(string userName)
        {
            var cacheKey = GetCacheKey(userName);
            var lockoutKey = GetLockoutKey(userName);
            _cache.Remove(cacheKey);
            _cache.Remove(lockoutKey);
        }

        /// <summary>
        /// Verifica si una cuenta está bloqueada
        /// </summary>
        public bool IsAccountLocked(string userName)
        {
            var cacheKey = GetCacheKey(userName);
            
            if (!_cache.TryGetValue(cacheKey, out int? attempts))
            {
                return false;
            }
            
            return (attempts ?? 0) >= MaxFailedAttempts;
        }

        /// <summary>
        /// Obtiene el número de intentos fallidos restantes
        /// </summary>
        public int GetRemainingAttempts(string userName)
        {
            var cacheKey = GetCacheKey(userName);
            
            // Si la cuenta está bloqueada, retornar 0
            if (IsAccountLocked(userName))
            {
                return 0;
            }
            
            // Obtener intentos actuales
            if (!_cache.TryGetValue(cacheKey, out int? attempts))
            {
                attempts = 0;
            }
            
            var remaining = Math.Max(0, MaxFailedAttempts - (attempts ?? 0));
            
            _logger.LogDebug(
                "Intentos restantes para usuario {UserName}: {Remaining} (Intentos fallidos: {Attempts}/{MaxAttempts})",
                userName, remaining, attempts ?? 0, MaxFailedAttempts);
            
            return remaining;
        }

        /// <summary>
        /// Obtiene el tiempo de bloqueo restante en minutos
        /// </summary>
        public int? GetLockoutRemainingMinutes(string userName)
        {
            if (!IsAccountLocked(userName))
                return null;

            var lockoutKey = GetLockoutKey(userName);
            if (_cache.TryGetValue(lockoutKey, out DateTime? lockoutTime) && lockoutTime.HasValue)
            {
                var elapsed = DateTime.UtcNow - lockoutTime.Value;
                var remaining = LockoutDurationMinutes - (int)elapsed.TotalMinutes;
                return Math.Max(0, remaining);
            }

            return null;
        }

        private string GetCacheKey(string userName)
        {
            return $"failed_login_attempts_{userName.ToLowerInvariant()}";
        }

        private string GetLockoutKey(string userName)
        {
            return $"lockout_time_{userName.ToLowerInvariant()}";
        }
    }
}

