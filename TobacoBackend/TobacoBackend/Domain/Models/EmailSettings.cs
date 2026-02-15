namespace TobacoBackend.Domain.Models
{
    /// <summary>
    /// Configuración para envío de correo (ej. Gmail SMTP).
    /// En Gmail: usar Contraseña de aplicación, no la contraseña normal.
    /// </summary>
    public class EmailSettings
    {
        public const string SectionName = "Email";

        public string SmtpHost { get; set; } = "smtp.gmail.com";
        public int SmtpPort { get; set; } = 587;
        public bool EnableSsl { get; set; } = true;
        public string? UserName { get; set; }
        /// <summary>Contraseña SMTP (en Gmail: contraseña de aplicación). Se usa "SmtpPassword" en appsettings para evitar que "Password" no se enlace en algunos entornos.</summary>
        public string? SmtpPassword { get; set; }
        public string FromAddress { get; set; } = "noreply@tobaco.com";
        public string FromName { get; set; } = "Provider";
        /// <summary>URL base para enlaces en correos (ej. https://tu-api.railway.app). Sin barra final.</summary>
        public string? BaseUrlForEmails { get; set; }
    }
}
