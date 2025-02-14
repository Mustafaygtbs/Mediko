using Mediko.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
    [Authorize]

    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly JwtSettings _jwtSettings;

        public AuthController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IOptions<JwtSettings> jwtSettings)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtSettings = jwtSettings.Value;
        }

        [AllowAnonymous]
        [HttpPost("Giris")]
        public async Task<IActionResult> Login([FromBody] LdapsızLoginDto model)
        {
            if (string.IsNullOrWhiteSpace(model.KullaniciAdi))
                return BadRequest("Kullanıcı adı gereklidir.");

            var user = await _userManager.FindByNameAsync(model.KullaniciAdi);
            if (user == null)
                return Unauthorized("Kullanıcı bulunamadı.");

            // LDAP şifre doğrulaması olduğu için şifre kontrolüne gerek yok
            var token = await JwtTokenOlustur(user);
            return Ok(new { Token = token });
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
                return BadRequest("Token bulunamadı.");
            }


            var principal = MyTokenHandler.ValidateToken(token, _jwtSettings);
            if (principal == null)
            {
                return Unauthorized("Geçersiz token.");
            }

            var userName = principal.Identity?.Name;
            if (string.IsNullOrEmpty(userName))
            {

                return Unauthorized("Token'dan kullanıcı bilgisi alınamadı.");
            }
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {

                return NotFound("Kullanıcı bulunamadı.");
            }


            var roles = await _userManager.GetRolesAsync(user);


            return Ok(new
            {
                userName = user.UserName,
              //  adSoyad = user.AdSoyad,
                email = user.Email,
                Roles = roles
            });
        }


        private async Task<string> JwtTokenOlustur(User user)
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
                new Claim(ClaimTypes.Email, user.Email ?? "")
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
    }
}
