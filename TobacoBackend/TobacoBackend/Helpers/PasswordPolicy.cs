using System.Text.RegularExpressions;

namespace TobacoBackend.Helpers
{
    /// <summary>
    /// Políticas y validaciones de contraseñas
    /// </summary>
    public static class PasswordPolicy
    {
        /// <summary>
        /// Longitud mínima de contraseña
        /// </summary>
        public const int MinLength = 8;

        /// <summary>
        /// Longitud máxima de contraseña
        /// </summary>
        public const int MaxLength = 100;

        /// <summary>
        /// Valida que una contraseña cumpla con las políticas de seguridad
        /// </summary>
        /// <param name="password">Contraseña a validar</param>
        /// <param name="errorMessage">Mensaje de error si la validación falla</param>
        /// <returns>True si la contraseña es válida, False en caso contrario</returns>
        public static bool IsValid(string? password, out string? errorMessage)
        {
            errorMessage = null;

            if (string.IsNullOrWhiteSpace(password))
            {
                errorMessage = "La contraseña no puede estar vacía.";
                return false;
            }

            if (password.Length < MinLength)
            {
                errorMessage = $"La contraseña debe tener al menos {MinLength} caracteres.";
                return false;
            }

            if (password.Length > MaxLength)
            {
                errorMessage = $"La contraseña no puede exceder {MaxLength} caracteres.";
                return false;
            }

            // Debe contener al menos una letra mayúscula
            if (!Regex.IsMatch(password, @"[A-Z]"))
            {
                errorMessage = "La contraseña debe contener al menos una letra mayúscula.";
                return false;
            }

            // Debe contener al menos una letra minúscula
            if (!Regex.IsMatch(password, @"[a-z]"))
            {
                errorMessage = "La contraseña debe contener al menos una letra minúscula.";
                return false;
            }

            // Debe contener al menos un número
            if (!Regex.IsMatch(password, @"[0-9]"))
            {
                errorMessage = "La contraseña debe contener al menos un número.";
                return false;
            }

            // Opcional: Debe contener al menos un carácter especial
            // if (!Regex.IsMatch(password, @"[!@#$%^&*(),.?\"":{}|<>]"))
            // {
            //     errorMessage = "La contraseña debe contener al menos un carácter especial.";
            //     return false;
            // }

            // No debe contener espacios
            if (password.Contains(' '))
            {
                errorMessage = "La contraseña no puede contener espacios.";
                return false;
            }

            // No debe ser una contraseña común
            if (IsCommonPassword(password))
            {
                errorMessage = "La contraseña es demasiado común. Por favor, elige una contraseña más segura.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Verifica si una contraseña está en la lista de contraseñas comunes
        /// </summary>
        private static bool IsCommonPassword(string password)
        {
            var commonPasswords = new[]
            {
                "password", "12345678", "123456789", "1234567890",
                "qwerty", "abc123", "password1", "Password1",
                "admin123", "root123", "user123", "test123"
            };

            return commonPasswords.Contains(password.ToLowerInvariant());
        }

        /// <summary>
        /// Calcula la fortaleza de una contraseña (0-100)
        /// </summary>
        public static int CalculateStrength(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return 0;

            int strength = 0;

            // Longitud
            if (password.Length >= 8) strength += 20;
            if (password.Length >= 12) strength += 10;
            if (password.Length >= 16) strength += 10;

            // Complejidad
            if (Regex.IsMatch(password, @"[a-z]")) strength += 10;
            if (Regex.IsMatch(password, @"[A-Z]")) strength += 10;
            if (Regex.IsMatch(password, @"[0-9]")) strength += 10;
            if (Regex.IsMatch(password, @"[!@#$%^&*(),.?\"":{}|<>]")) strength += 10;

            // Variedad de caracteres
            var uniqueChars = password.Distinct().Count();
            if (uniqueChars >= password.Length * 0.7) strength += 10;

            return Math.Min(100, strength);
        }
    }
}

