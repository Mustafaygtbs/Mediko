using Mediko.DataAccess;
using Mediko.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Mediko.Services
{
    public static class CustomTokenHandler
    {

        public static (string accessToken, DateTime accessTokenExpiration, string refreshToken, DateTime refreshTokenExpiration)
            CreateToken(User user, JwtSettings jwtSettings, MedikoDbContext context)
        {

            if (string.IsNullOrWhiteSpace(jwtSettings.Key))
                throw new Exception("JWT ayarındaki Key değeri null veya boş olamaz.");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            double expiresMinutes = 30;
            if (!string.IsNullOrWhiteSpace(jwtSettings.Expires)
                && double.TryParse(jwtSettings.Expires, out double result))
            {
                expiresMinutes = result;
            }

            var nowUtc = DateTime.UtcNow;
            var accessTokenExpiration = nowUtc.AddMinutes(expiresMinutes);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim("AdSoyad", user.AdSoyad ?? string.Empty),
                new Claim("OgrenciNo", user.OgrenciNo ?? string.Empty),
                new Claim("TcKimlikNo", user.TcKimlikNo ?? string.Empty),
                new Claim("DogumTarihi", user.DogumTarihi.ToString("yyyy-MM-dd")),
                new Claim("DogumYeri", user.DogumYeri ?? string.Empty),
                new Claim("AnneAdi", user.AnneAdi ?? string.Empty),
                new Claim("BabaAdi", user.BabaAdi ?? string.Empty),
                new Claim("TelNo", user.TelNo ?? string.Empty)
            };

            // Roles?
            // Rollerinizi userManager üzerinden çekmeyeceğiniz için buraya parametre olarak rolleri geçebilir
            // veya AuthController içinden rolleri çekip ekleyebilirsiniz. Burada kalsın örnek:
             claims.Add(new Claim(ClaimTypes.Role, "Admin"));

            var jwtToken = new JwtSecurityToken(
                issuer: jwtSettings.Issuer,
                audience: jwtSettings.Audience,
                claims: claims,
                notBefore: nowUtc,
                expires: accessTokenExpiration,
                signingCredentials: credentials
            );

            var accessToken = new JwtSecurityTokenHandler().WriteToken(jwtToken);


            var refreshTokenExpiration = nowUtc.AddDays(7); 
            byte[] refreshTokenBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(refreshTokenBytes);
            }
            var refreshToken = Convert.ToBase64String(refreshTokenBytes);


            var newRefreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                ExpirationDate = refreshTokenExpiration
            };

            context.RefreshTokens.Add(newRefreshTokenEntity);
            context.SaveChanges(); 


            return (accessToken, accessTokenExpiration, refreshToken, refreshTokenExpiration);
        }


        public static (string accessToken, DateTime accessTokenExpiration, string refreshToken, DateTime refreshTokenExpiration)
            RefreshAccessToken(string oldRefreshToken, JwtSettings jwtSettings, MedikoDbContext context, User user)
        {
            var existingToken = context.RefreshTokens
                .FirstOrDefault(t => t.Token == oldRefreshToken && t.ExpirationDate > DateTime.UtcNow);
            if (existingToken == null)
                throw new SecurityTokenException("Geçersiz veya süresi dolmuş refresh token.");


            if (existingToken.UserId != user.Id)
                throw new SecurityTokenException("Refresh token, farklı bir kullanıcıya ait.");

            context.RefreshTokens.Remove(existingToken);
            context.SaveChanges();

            return CreateToken(user, jwtSettings, context);
        }
    }
}
