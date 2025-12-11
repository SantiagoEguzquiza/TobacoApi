namespace TobacoBackend.Domain.Models
{
    public class JwtSettings
    {
        public string Secret { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int ExpirationDays { get; set; } = 30; // Duración del token en días, por defecto 30 días
        public int AccessTokenExpirationMinutes { get; set; } = 30; // Duración del access token en minutos, por defecto 30 minutos
        public int RefreshTokenExpirationDays { get; set; } = 30; // Duración del refresh token en días, por defecto 30 días
    }
}
