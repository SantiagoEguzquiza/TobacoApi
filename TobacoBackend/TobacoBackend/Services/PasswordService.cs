using BCrypt.Net;
using System.Security.Cryptography;

namespace TobacoBackend.Services
{
    /// <summary>
    /// Servicio para el manejo seguro de contraseñas usando BCrypt
    /// </summary>
    public static class PasswordService
    {
        /// <summary>
        /// Genera un hash BCrypt de la contraseña con salt automático
        /// </summary>
        /// <param name="password">Contraseña en texto plano</param>
        /// <returns>Hash BCrypt de la contraseña</returns>
        public static string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("La contraseña no puede estar vacía", nameof(password));

            // BCrypt genera automáticamente un salt único para cada hash
            // WorkFactor de 12 es un buen balance entre seguridad y rendimiento
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        }

        /// <summary>
        /// Verifica si una contraseña coincide con su hash BCrypt
        /// </summary>
        /// <param name="password">Contraseña en texto plano a verificar</param>
        /// <param name="hashedPassword">Hash BCrypt almacenado</param>
        /// <returns>True si la contraseña es correcta, False en caso contrario</returns>
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hashedPassword))
                return false;

            try
            {
                // BCrypt.Verify maneja automáticamente el salt y la comparación segura
                // Usa comparación de tiempo constante para prevenir timing attacks
                return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            }
            catch
            {
                // Si hay algún error en la verificación (formato inválido, etc.), retornar false
                return false;
            }
        }

        /// <summary>
        /// Verifica si un hash es del tipo antiguo SHA256 (para migración)
        /// </summary>
        /// <param name="hashedPassword">Hash a verificar</param>
        /// <returns>True si es un hash SHA256 antiguo</returns>
        public static bool IsOldPasswordHash(string hashedPassword)
        {
            if (string.IsNullOrWhiteSpace(hashedPassword))
                return false;

            // Los hashes BCrypt empiezan con $2a$, $2b$, $2y$, etc.
            // Los hashes SHA256 en Base64 no empiezan con $
            return !hashedPassword.StartsWith("$2");
        }

        /// <summary>
        /// Valida una contraseña contra un hash SHA256 antiguo (para migración)
        /// </summary>
        /// <param name="password">Contraseña en texto plano</param>
        /// <param name="oldHash">Hash SHA256 antiguo</param>
        /// <returns>True si la contraseña es correcta</returns>
        public static bool ValidateOldPassword(string password, string oldHash)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(oldHash))
                return false;

            try
            {
                using (var sha256 = System.Security.Cryptography.SHA256.Create())
                {
                    var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                    var hashedString = Convert.ToBase64String(hashedBytes);
                    // Comparación de tiempo constante para prevenir timing attacks
                    return CryptographicOperations.FixedTimeEquals(
                        System.Text.Encoding.UTF8.GetBytes(hashedString),
                        System.Text.Encoding.UTF8.GetBytes(oldHash)
                    );
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Verifica si un hash necesita ser actualizado (por ejemplo, si se cambió el workFactor)
        /// </summary>
        /// <param name="hashedPassword">Hash BCrypt a verificar</param>
        /// <param name="workFactor">WorkFactor deseado (por defecto 12)</param>
        /// <returns>True si el hash necesita ser actualizado</returns>
        public static bool NeedsRehash(string hashedPassword, int workFactor = 12)
        {
            if (string.IsNullOrWhiteSpace(hashedPassword))
                return true;

            try
            {
                // BCrypt almacena el workFactor en el hash (formato: $2a$workFactor$...)
                // Extraer el workFactor del hash
                if (hashedPassword.StartsWith("$2") && hashedPassword.Length > 4)
                {
                    var parts = hashedPassword.Split('$');
                    if (parts.Length >= 3 && int.TryParse(parts[2], out int currentWorkFactor))
                    {
                        return currentWorkFactor < workFactor;
                    }
                }
                // Si no es un hash BCrypt válido, necesita ser actualizado
                return true;
            }
            catch
            {
                // Si hay algún error, necesita ser actualizado
                return true;
            }
        }
    }
}

