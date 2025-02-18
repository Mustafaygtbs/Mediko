using Mediko.Entities;
using Mediko.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace Mediko.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly JwtSettings _jwtSettings;
        private readonly ILdapAuthService _ldapAuthService;

        public AuthController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IOptions<JwtSettings> jwtSettings,
            ILdapAuthService ldapAuthService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtSettings = jwtSettings.Value;
            _ldapAuthService = ldapAuthService;
        }

        [AllowAnonymous]
        [HttpPost("Giris")]
        public async Task<IActionResult> Login([FromBody] LdapsızLoginDto model)
        {
            if (string.IsNullOrWhiteSpace(model.KullaniciAdi))
                return BadRequest(new { Message = "Kullanıcı adı gereklidir." });

            var user = await _userManager.FindByNameAsync(model.KullaniciAdi);
            if (user == null)
                return Unauthorized(new { Message = "Kullanıcı bulunamadı." });

            // Şifre doğrulama eklendi
            var result = await _signInManager.PasswordSignInAsync(model.KullaniciAdi, model.Sifre, false, false);
            if (!result.Succeeded)
                return Unauthorized(new { Message = "Geçersiz kullanıcı adı veya şifre." });

            var token = await JwtTokenOlustur(user);
            return Ok(new { Token = token, KullaniciAdi = user.UserName });
        }

        [AllowAnonymous]
        [HttpPost("LdapGiris")]
        public async Task<IActionResult> LdapLogin([FromBody] LdapLoginDto model)
        {
            if (string.IsNullOrWhiteSpace(model.KullaniciAdi) || string.IsNullOrWhiteSpace(model.Sifre))
                return BadRequest(new { Message = "Kullanıcı adı ve şifre gereklidir." });

            try
            {
                var token = await _ldapAuthService.AuthenticateAndGenerateTokenAsync(model.KullaniciAdi, model.Sifre);
                if (token == null)
                {
                    return Unauthorized(new { Message = "LDAP doğrulama başarısız. Kullanıcı adı veya şifre hatalı olabilir." });
                }

                // Kullanıcı bilgilerini de döndürmek için eklendi
                var user = await _userManager.FindByNameAsync(model.KullaniciAdi);
                var roles = user != null ? await _userManager.GetRolesAsync(user) : new List<string>();

                return Ok(new { Token = token, KullaniciAdi = model.KullaniciAdi, Roller = roles });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "LDAP giriş işlemi sırasında bir hata oluştu.", Error = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpGet("GetUserInfo")]
        public async Task<IActionResult> GetUserInfo()
        {
            var token = HttpContext.Request.Headers["Authorization"]
                .FirstOrDefault()?
                .Split(" ")
                .Last();

            if (string.IsNullOrEmpty(token))
            {
                return BadRequest(new { Message = "Token bulunamadı." });
            }

            var handler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.Key);
            var validations = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidAudience = _jwtSettings.Audience
            };

            try
            {
                var principal = handler.ValidateToken(token, validations, out _);
                var userName = principal.Identity?.Name;

                if (string.IsNullOrEmpty(userName))
                {
                    return Unauthorized(new { Message = "Token'dan kullanıcı bilgisi alınamadı." });
                }

                var user = await _userManager.FindByNameAsync(userName);
                if (user == null)
                {
                    return NotFound(new { Message = "Kullanıcı bulunamadı." });
                }

                var roles = await _userManager.GetRolesAsync(user);

                return Ok(new
                {
                    userName = user.UserName,
                    email = user.Email,
                    Roles = roles
                });
            }
            catch (SecurityTokenException)
            {
                return Unauthorized(new { Message = "Geçersiz token." });
            }
        }

        private async Task<string> JwtTokenOlustur(User user, bool isLdapUser = false)
        {
            if (_jwtSettings.Key is null)
            {
                throw new Exception("JWT ayarındaki Key değeri null olamaz");
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName ?? ""),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim("isLdapUser", isLdapUser.ToString())
            };

            var roles = await _userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class LdapsızLoginDto
    {
        public string? KullaniciAdi { get; set; }
        public string? Sifre { get; set; }  
    }

    public class LdapLoginDto
    {
        public string? KullaniciAdi { get; set; }
        public string? Sifre { get; set; }
    }
}
