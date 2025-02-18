using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Mediko.Entities
{
    public class JwtSettings
    {
        public string? Key { get; set; }
        public string? Issuer { get; set; }
        public string? Audience { get; set; }
    }

    public static class MyTokenHandler
    {
        public static ClaimsPrincipal? ValidateToken(string token, JwtSettings jwtSettings)
        {
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(jwtSettings.Key))
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(jwtSettings.Key);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),

                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,

                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,

                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            try
            {
                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                return principal;
            }
            catch (SecurityTokenExpiredException)
            {
                Console.WriteLine("Token süresi dolmuş.");
                return null;
            }
            catch (SecurityTokenValidationException ex)
            {
                Console.WriteLine($"Token doğrulama hatası: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Bilinmeyen bir hata oluştu: {ex.Message}");
                return null;
            }
        }
    }
}
