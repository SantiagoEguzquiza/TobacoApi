using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using TobacoBackend.Domain.IServices;
using TobacoBackend.Domain.Models;

namespace TobacoBackend.Services
{
    public class EmailService : IEmailService
    {
        private const string ResendApiUrl = "https://api.resend.com/emails";
        private readonly EmailSettings _settings;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            IOptions<EmailSettings> settings,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            ILogger<EmailService> logger)
        {
            _settings = settings.Value;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        private string? GetResendApiKey()
        {
            return !string.IsNullOrEmpty(_settings.ResendApiKey) ? _settings.ResendApiKey
                : _configuration["Email:ResendApiKey"]
                ?? Environment.GetEnvironmentVariable("Email__ResendApiKey");
        }

        private string GetFromAddress()
        {
            var from = !string.IsNullOrEmpty(_settings.FromAddress) ? _settings.FromAddress
                : _configuration["Email:FromAddress"]
                ?? Environment.GetEnvironmentVariable("Email__FromAddress");
            return !string.IsNullOrEmpty(from) ? from : "onboarding@resend.dev";
        }

        public async Task<bool> SendPasswordResetEmailAsync(string toEmail, string userName, string resetLink)
        {
            var apiKey = GetResendApiKey();
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning(
                    "Email no configurado. Define Email:ResendApiKey o variable de entorno Email__ResendApiKey. Obtén la API key en https://resend.com/api-keys");
                return false;
            }

            var fromAddress = GetFromAddress();
            var from = $"{_settings.FromName} <{fromAddress}>";
            var subject = "Recuperar contraseña - Provider";
            var body = $@"Hola {userName},

Has solicitado restablecer tu contraseña en Provider.

Haz clic en el siguiente enlace para elegir una nueva contraseña (válido durante 1 hora):

{resetLink}

Si no solicitaste este correo, puedes ignorarlo. Tu contraseña no cambiará.

— Equipo Provider";

            var payload = new Dictionary<string, object>
            {
                ["from"] = from,
                ["to"] = new[] { toEmail },
                ["subject"] = subject,
                ["text"] = body
            };

            try
            {
                var client = _httpClientFactory.CreateClient();
                using var request = new HttpRequestMessage(HttpMethod.Post, ResendApiUrl);
                request.Headers.Add("Authorization", "Bearer " + apiKey);
                request.Content = JsonContent.Create(payload);
                using var response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Correo de recuperación enviado a {Email} vía Resend", toEmail);
                    return true;
                }
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogError("Resend API error {StatusCode}: {Body}", response.StatusCode, errorBody);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar correo de recuperación a {Email} vía Resend", toEmail);
                return false;
            }
        }
    }
}
