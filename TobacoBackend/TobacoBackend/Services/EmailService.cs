using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MimeKit;
using TobacoBackend.Domain.IServices;
using TobacoBackend.Domain.Models;

namespace TobacoBackend.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> settings, IConfiguration configuration, ILogger<EmailService> logger)
        {
            _settings = settings.Value;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>Obtiene UserName y SmtpPassword: Options → IConfiguration → variables de entorno (Email__UserName, Email__SmtpPassword).</summary>
        private (string? userName, string? smtpPassword) GetCredentials()
        {
            var userName = !string.IsNullOrEmpty(_settings.UserName) ? _settings.UserName
                : _configuration["Email:UserName"]
                ?? Environment.GetEnvironmentVariable("Email__UserName");
            var smtpPassword = !string.IsNullOrEmpty(_settings.SmtpPassword) ? _settings.SmtpPassword
                : _configuration["Email:SmtpPassword"]
                ?? _configuration["Email:Password"]
                ?? Environment.GetEnvironmentVariable("Email__SmtpPassword");
            return (userName, smtpPassword);
        }

        public async Task<bool> SendPasswordResetEmailAsync(string toEmail, string userName, string resetLink)
        {
            var (emailUser, emailPassword) = GetCredentials();
            if (string.IsNullOrEmpty(emailUser) || string.IsNullOrEmpty(emailPassword))
            {
                var section = _configuration.GetSection("Email");
                var hasSection = section.Exists();
                var keys = hasSection ? string.Join(", ", section.GetChildren().Select(c => c.Key)) : "(no existe sección Email)";
                _logger.LogWarning(
                    "Email no configurado. Revisa: 1) appsettings Email:UserName y Email:SmtpPassword (o variables de entorno Email__UserName, Email__SmtpPassword). 2) Sección Email existe: {HasSection}. Claves vistas: {Keys}. 3) Para Gmail usa contraseña de aplicación.",
                    hasSection, keys);
                return false;
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromAddress));
            message.To.Add(new MailboxAddress(userName, toEmail));
            message.Subject = "Recuperar contraseña - Provider";

            var body = $@"Hola {userName},

Has solicitado restablecer tu contraseña en Provider.

Haz clic en el siguiente enlace para elegir una nueva contraseña (válido durante 1 hora):

{resetLink}

Si no solicitaste este correo, puedes ignorarlo. Tu contraseña no cambiará.

— Equipo Provider";

            message.Body = new TextPart("plain") { Text = body };

            try
            {
                using var client = new SmtpClient();
                await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort,
                    _settings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);
                await client.AuthenticateAsync(emailUser, emailPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
                _logger.LogInformation("Correo de recuperación enviado a {Email}", toEmail);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar correo de recuperación a {Email}. Revisa UserName/SmtpPassword (Gmail: contraseña de aplicación), SMTP y firewall.", toEmail);
                return false;
            }
        }
    }
}
