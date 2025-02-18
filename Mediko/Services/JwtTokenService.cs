using Mediko.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Mediko.Services
{
    public class JwtTokenService
    {
        private readonly IConfiguration _config;

        public JwtTokenService(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public string GenerateJwtToken(string username, IList<string> roles, string userId, bool IsLdapUser = false)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Geçersiz kullanıcı adı.", nameof(username));

            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("Geçersiz kullanıcı ID'si.", nameof(userId));

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim("IsLdapUser", IsLdapUser.ToString())
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = _config["Jwt:Key"];
            if (string.IsNullOrWhiteSpace(key))
                throw new InvalidOperationException("JWT anahtar değeri belirtilmemiş.");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // LDAP Kullanıcıları İçin Özel Oturum Süresi Ayarlandı
            double expiresMinutes;
            if (IsLdapUser && double.TryParse(_config["Jwt:LdapExpires"], out double ldapExpiresMinutes))
            {
                expiresMinutes = ldapExpiresMinutes;
            }
            else if (!double.TryParse(_config["Jwt:Expires"], out expiresMinutes))
            {
                expiresMinutes = 30;
            }

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}