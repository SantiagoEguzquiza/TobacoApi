using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using TobacoBackend.Domain.IServices;
using TobacoBackend.Domain.Models;

namespace TobacoBackend.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<bool> SendPasswordResetEmailAsync(string toEmail, string userName, string resetLink)
        {
            if (string.IsNullOrWhiteSpace(_settings.SmtpUser) || string.IsNullOrWhiteSpace(_settings.SmtpPassword))
            {
                _logger.LogWarning(
                    "Email no configurado. Define Email__SmtpUser y Email__SmtpPassword como variables de entorno " +
                    "(SmtpUser = tu Gmail, SmtpPassword = contraseña de aplicación de Google).");
                return false;
            }

            var from = string.IsNullOrWhiteSpace(_settings.FromAddress) ? _settings.SmtpUser : _settings.FromAddress;
            var baseUrl = _settings.BaseUrlForEmails?.TrimEnd('/') ?? string.Empty;
            var logoUrl = $"{baseUrl}/images/logo.png";

            var html = $@"<!DOCTYPE html>
<html lang=""es"">
<head>
  <meta charset=""utf-8"">
  <meta name=""viewport"" content=""width=device-width,initial-scale=1"">
  <title>Recuperar contraseña</title>
</head>
<body style=""margin:0;padding:0;background:#f4f4f4;font-family:'Helvetica Neue',Arial,sans-serif;"">
  <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#f4f4f4;padding:40px 0;"">
    <tr>
      <td align=""center"">
        <table width=""520"" cellpadding=""0"" cellspacing=""0"" style=""background:#ffffff;border-radius:16px;overflow:hidden;box-shadow:0 4px 24px rgba(0,0,0,0.08);"">

          <!-- Header -->
          <tr>
            <td align=""center"" style=""background:linear-gradient(135deg,#0D47A1,#1565C0);padding:32px 40px 24px;"">
              <img src=""{logoUrl}"" width=""64"" height=""64"" alt=""Provider"" style=""display:block;border-radius:12px;"">
              <p style=""margin:14px 0 0;color:#ffffff;font-size:22px;font-weight:700;letter-spacing:0.5px;"">Provider</p>
              <p style=""margin:4px 0 0;color:rgba(255,255,255,0.75);font-size:13px;"">Sistema de Gestión Comercial</p>
            </td>
          </tr>

          <!-- Body -->
          <tr>
            <td style=""padding:36px 40px 28px;"">
              <p style=""margin:0 0 8px;font-size:20px;font-weight:700;color:#1a1a1a;"">Recuperar contraseña</p>
              <p style=""margin:0 0 24px;font-size:15px;color:#555;"">Hola <strong>{userName}</strong>, recibimos una solicitud para restablecer tu contraseña.</p>

              <p style=""margin:0 0 20px;font-size:15px;color:#555;line-height:1.6;"">
                Hacé clic en el botón para elegir una nueva contraseña. El enlace es válido durante <strong>1 hora</strong>.
              </p>

              <!-- CTA Button -->
              <table cellpadding=""0"" cellspacing=""0"" style=""margin:0 auto 28px;"">
                <tr>
                  <td align=""center"" style=""background:#1565C0;border-radius:10px;"">
                    <a href=""{resetLink}"" target=""_blank""
                       style=""display:inline-block;padding:14px 36px;color:#ffffff;font-size:15px;font-weight:700;text-decoration:none;letter-spacing:0.3px;"">
                      Restablecer contraseña
                    </a>
                  </td>
                </tr>
              </table>

              <p style=""margin:0 0 6px;font-size:13px;color:#888;"">Si el botón no funciona, copiá y pegá este enlace en tu navegador:</p>
              <p style=""margin:0 0 28px;font-size:12px;color:#1565C0;word-break:break-all;""><a href=""{resetLink}"" style=""color:#1565C0;"">{resetLink}</a></p>

              <hr style=""border:none;border-top:1px solid #eeeeee;margin:0 0 20px;"">

              <p style=""margin:0;font-size:13px;color:#aaa;line-height:1.6;"">
                Si no solicitaste este correo podés ignorarlo. Tu contraseña no cambiará.<br>
                Por seguridad, nunca compartás este enlace con nadie.
              </p>
            </td>
          </tr>

          <!-- Footer -->
          <tr>
            <td align=""center"" style=""background:#f8f8f8;padding:16px 40px;border-top:1px solid #eeeeee;"">
              <p style=""margin:0;font-size:12px;color:#bbb;"">© {DateTime.UtcNow.Year} Provider · Sistema de Gestión Comercial</p>
            </td>
          </tr>

        </table>
      </td>
    </tr>
  </table>
</body>
</html>";

            try
            {
                using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(_settings.SmtpUser, _settings.SmtpPassword),
                    Timeout = 15_000,
                };

                using var message = new MailMessage
                {
                    From = new MailAddress(from, _settings.FromName),
                    Subject = "Recuperar contraseña - Provider",
                    Body = html,
                    IsBodyHtml = true,
                };
                message.To.Add(toEmail);

                await client.SendMailAsync(message);
                _logger.LogInformation("Correo de recuperación enviado a {Email} vía Gmail SMTP", toEmail);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar correo de recuperación a {Email} vía Gmail SMTP", toEmail);
                return false;
            }
        }
    }
}
