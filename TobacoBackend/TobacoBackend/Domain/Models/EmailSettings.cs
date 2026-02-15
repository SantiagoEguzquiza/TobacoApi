namespace TobacoBackend.Domain.Models
{
    /// <summary>
    /// Configuración para envío de correo vía Resend API (recomendado en producción; no depende de SMTP).
    /// </summary>
    public class EmailSettings
    {
        public const string SectionName = "Email";

        /// <summary>API Key de Resend (resend.com). Variable de entorno: Email__ResendApiKey.</summary>
        public string? ResendApiKey { get; set; }
        public string FromAddress { get; set; } = "onboarding@resend.dev";
        public string FromName { get; set; } = "Provider";
        /// <summary>URL base para enlaces en correos (ej. https://tu-api.railway.app). Sin barra final.</summary>
        public string? BaseUrlForEmails { get; set; }
    }
}
