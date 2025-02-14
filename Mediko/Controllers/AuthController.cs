using Mediko.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Mediko.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,User")]
    public class AuthController : ControllerBase
    {
        private readonly JwtSettings _jwtSettings;

        public AuthController(IOptions<JwtSettings> jwtSettings)

        {
            _jwtSettings = jwtSettings.Value;
        }

        [AllowAnonymous]
        [HttpPost("Giris")]
        public IActionResult Login([FromBody] User user)
        {
            var kullanici = GeciciKullaniciBilgileri.Users.Find(x => x.KullaniciAdi == user.KullaniciAdi && x.Sifre == user.Sifre);
            if (kullanici == null)
            {
                return Unauthorized();
            }
            var token = JwtTokenOlustur(kullanici);
            return Ok(token);
        }

        [Authorize]
        [HttpGet("GetUserInfo")]
        public IActionResult GetUserInfo()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;

            if (identity == null || !identity.Claims.Any())
            {
                return Unauthorized("Kullanıcı bilgileri alınamadı.");
            }

            var claims = identity.Claims;

            var userInfo = new
            {
                KullaniciAdi = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value,
                Rol = claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value
            };

            return Ok(userInfo);
        }





        private object JwtTokenOlustur(User user)
        {
            if (_jwtSettings.Key is null)
            {
                throw new Exception("Jwt ayarındaki key değeri null olamaz");
            }
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.KullaniciAdi!),
                new Claim(ClaimTypes.Role,user.Rol!)
            };

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: credentials
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
