using System.Text.RegularExpressions;

namespace TobacoBackend.Helpers
{
    /// <summary>
    /// Utilidades para sanitizar y validar entrada de usuario
    /// </summary>
    public static class InputSanitizer
    {
        /// <summary>
        /// Sanitiza una cadena de texto removiendo caracteres peligrosos
        /// </summary>
        public static string SanitizeString(string? input, int maxLength = 1000)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Limitar longitud
            if (input.Length > maxLength)
                input = input.Substring(0, maxLength);

            // Remover caracteres de control excepto saltos de línea y tabs
            input = Regex.Replace(input, @"[\x00-\x08\x0B-\x0C\x0E-\x1F]", "");

            // Remover scripts y tags HTML peligrosos
            input = Regex.Replace(input, @"<script[^>]*>.*?</script>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            input = Regex.Replace(input, @"<iframe[^>]*>.*?</iframe>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            input = Regex.Replace(input, @"javascript:", "", RegexOptions.IgnoreCase);
            input = Regex.Replace(input, @"on\w+\s*=", "", RegexOptions.IgnoreCase);

            return input.Trim();
        }

        /// <summary>
        /// Valida que una cadena no contenga SQL injection patterns
        /// </summary>
        public static bool ContainsSqlInjection(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            var sqlPatterns = new[]
            {
                @"(\b(SELECT|INSERT|UPDATE|DELETE|DROP|CREATE|ALTER|EXEC|EXECUTE)\b)",
                @"(--|;|\*|')",
                @"(UNION\s+SELECT)",
                @"(/\*|\*/)",
                @"(xp_\w+)",
                @"(sp_\w+)"
            };

            foreach (var pattern in sqlPatterns)
            {
                if (Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Valida que una cadena no contenga XSS patterns
        /// </summary>
        public static bool ContainsXss(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            var xssPatterns = new[]
            {
                @"<script[^>]*>",
                @"javascript:",
                @"on\w+\s*=",
                @"<iframe[^>]*>",
                @"<object[^>]*>",
                @"<embed[^>]*>",
                @"eval\s*\(",
                @"expression\s*\("
            };

            foreach (var pattern in xssPatterns)
            {
                if (Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Valida y sanitiza un nombre de usuario
        /// </summary>
        public static string SanitizeUserName(string? userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
                return string.Empty;

            // Solo permitir letras, números, guiones y guiones bajos
            userName = Regex.Replace(userName, @"[^a-zA-Z0-9_-]", "");
            
            // Limitar longitud
            if (userName.Length > 50)
                userName = userName.Substring(0, 50);

            return userName.Trim();
        }

        /// <summary>
        /// Valida un email básico
        /// </summary>
        public static bool IsValidEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
                return emailRegex.IsMatch(email) && email.Length <= 100;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Valida que un número esté en un rango seguro
        /// </summary>
        public static bool IsValidNumber(decimal? value, decimal min = decimal.MinValue, decimal max = decimal.MaxValue)
        {
            if (!value.HasValue)
                return false;

            return value.Value >= min && value.Value <= max;
        }
    }
}

