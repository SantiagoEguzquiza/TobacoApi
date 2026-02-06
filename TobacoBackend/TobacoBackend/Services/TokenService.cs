using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TobacoBackend.Domain.Models;

namespace TobacoBackend.Services
{
    public class TokenService
    {
        private readonly JwtSettings _jwtSettings;

        public TokenService(IOptions<JwtSettings> jwtSettings)
        {
            _jwtSettings = jwtSettings.Value;
        }

        /// <summary>
        /// Exponer la duración de refresh token configurada para que otros servicios la usen.
        /// </summary>
        public int RefreshTokenExpirationDays => _jwtSettings.RefreshTokenExpirationDays > 0
            ? _jwtSettings.RefreshTokenExpirationDays
            : 30;

        public string GenerateToken(string userId, string? userName = null, int? tenantId = null)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            if (!string.IsNullOrEmpty(userName))
            {
                claims.Add(new Claim(ClaimTypes.Name, userName));
                claims.Add(new Claim("username", userName));
            }

            if (tenantId.HasValue)
            {
                claims.Add(new Claim("tenant_id", tenantId.Value.ToString()));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Usar AccessTokenExpirationMinutes en lugar de ExpirationDays para access tokens
            var expirationMinutes = _jwtSettings.AccessTokenExpirationMinutes > 0 
                ? _jwtSettings.AccessTokenExpirationMinutes 
                : 30; // Default 30 minutos

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            return Guid.NewGuid().ToString() + "-" + Guid.NewGuid().ToString();
        }

        public ClaimsPrincipal ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.Secret);

            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return principal;
            }
            catch
            {
                return null;
            }
        }

        public string GetUserIdFromToken(string token)
        {
            var principal = ValidateToken(token);
            return principal?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        }

        public bool IsTokenValid(string token)
        {
            return ValidateToken(token) != null;
        }

        public DateTime GetTokenExpiration(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            return jwtToken.ValidTo;
        }
    }
}