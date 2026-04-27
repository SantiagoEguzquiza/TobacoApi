namespace TobacoBackend.Domain.Models
{
    public class EmailSettings
    {
        public const string SectionName = "Email";

        /// <summary>Servidor SMTP. Gmail: smtp.gmail.com</summary>
        public string SmtpHost { get; set; } = "smtp.gmail.com";

        /// <summary>Puerto SMTP. Gmail TLS: 587</summary>
        public int SmtpPort { get; set; } = 587;

        /// <summary>Usuario SMTP (tu dirección Gmail). Variable de entorno: Email__SmtpUser</summary>
        public string SmtpUser { get; set; } = string.Empty;

        /// <summary>Contraseña de aplicación de Gmail (no la contraseña normal). Variable de entorno: Email__SmtpPassword</summary>
        public string SmtpPassword { get; set; } = string.Empty;

        public string FromAddress { get; set; } = string.Empty;
        public string FromName { get; set; } = "Provider";

        /// <summary>URL base para enlaces en correos (ej. https://tu-api.railway.app). Sin barra final.</summary>
        public string? BaseUrlForEmails { get; set; }
    }
}
