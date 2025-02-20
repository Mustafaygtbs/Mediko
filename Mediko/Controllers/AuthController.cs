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
  //  [Authorize(Roles = "Admin")]
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
        [HttpPost("Ldapsız-Giris")]
        public async Task<IActionResult> Login([FromBody] LdapsızLoginDto model)
        {
            if (string.IsNullOrWhiteSpace(model.KullaniciAdi))
                return BadRequest(new { Message = "Kullanıcı adı gereklidir." });

            var user = await _userManager.FindByNameAsync(model.KullaniciAdi);
            if (user == null)
                return Unauthorized(new { Message = "Kullanıcı bulunamadı." });


            var token = await JwtTokenOlustur(user);
            return Ok(new { Token = token});
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
                return Ok(new { Token = token});
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

                // Token içinde Name (kullanıcı adı) claim’i var mı?
                var userName = principal.Identity?.Name;

                if (string.IsNullOrEmpty(userName))
                {
                    return Unauthorized(new { Message = "Token'dan kullanıcı bilgisi alınamadı." });
                }

                // IdentityUser’dan tekrar okumak isterseniz (Email vs. gibi),
                // veritabanındaki user’ı da getirebilirsiniz:
                var user = await _userManager.FindByNameAsync(userName);
                if (user == null)
                {
                    return NotFound(new { Message = "Kullanıcı bulunamadı." });
                }

                var roles = await _userManager.GetRolesAsync(user);


                var adSoyad = principal.FindFirst("AdSoyad")?.Value;
                var ogrenciNo = principal.FindFirst("OgrenciNo")?.Value;
                var tcKimlikNo = principal.FindFirst("TcKimlikNo")?.Value;
                var dogumTarihi = principal.FindFirst("DogumTarihi")?.Value;
                var dogumYeri = principal.FindFirst("DogumYeri")?.Value;
                var anneAdi = principal.FindFirst("AnneAdi")?.Value;
                var babaAdi = principal.FindFirst("BabaAdi")?.Value;
                var telNo = principal.FindFirst("TelNo")?.Value;

                // Geriye istediğiniz biçimde dönün; örneğin hepsini tek bir json objesi olarak:
                return Ok(new
                {
                    userName = userName,
                    email = user.Email,
                    roles = roles,
                    adSoyad,
                    ogrenciNo,
                    tcKimlikNo,
                    dogumTarihi,
                    dogumYeri,
                    anneAdi,
                    babaAdi,
                    telNo
                });
            }
            catch (SecurityTokenException)
            {
                return Unauthorized(new { Message = "Geçersiz token." });
            }
        }


        private async Task<string> JwtTokenOlustur(User user)
        {
            if (string.IsNullOrWhiteSpace(_jwtSettings.Key))
                throw new Exception("JWT ayarındaki Key değeri null veya boş olamaz.");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Kullanıcıyla ilgili tüm alanları ekliyoruz
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id),
        new Claim(ClaimTypes.Name, user.UserName ?? ""),
        new Claim(ClaimTypes.Email, user.Email ?? ""),
        new Claim("AdSoyad", user.AdSoyad ?? ""),
        new Claim("OgrenciNo", user.OgrenciNo ?? ""),
        new Claim("TcKimlikNo", user.TcKimlikNo ?? ""),
        new Claim("DogumTarihi", user.DogumTarihi.ToString("yyyy-MM-dd")), 
        new Claim("DogumYeri", user.DogumYeri ?? ""),
        new Claim("AnneAdi", user.AnneAdi ?? ""),
        new Claim("BabaAdi", user.BabaAdi ?? ""),
        new Claim("TelNo", user.TelNo ?? "")
    };

            var roles = await _userManager.GetRolesAsync(user) ?? new List<string>();
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));


            double expiresMinutes = 30;
            if (!string.IsNullOrWhiteSpace(_jwtSettings.Expires)
                && double.TryParse(_jwtSettings.Expires, out double result))
            {
                expiresMinutes = result;
            }

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }



        //private async Task<string> JwtTokenOlustur(User user)
        //{
        //    if (_jwtSettings.Key is null)
        //    {
        //        throw new Exception("JWT ayarındaki Key değeri null olamaz");
        //    }

        //    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        //    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        //    var claims = new List<Claim>
        //    {
        //        new Claim(ClaimTypes.NameIdentifier, user.Id),
        //        new Claim(ClaimTypes.Name, user.UserName ?? ""),
        //        new Claim(ClaimTypes.Email, user.Email ?? "")
        //    };

        //    var roles = await _userManager.GetRolesAsync(user);
        //    claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        //    var token = new JwtSecurityToken(
        //        issuer: _jwtSettings.Issuer,
        //        audience: _jwtSettings.Audience,
        //        claims: claims,
        //        expires: DateTime.UtcNow.AddMinutes(30),
        //        signingCredentials: credentials
        //    );

        //    return new JwtSecurityTokenHandler().WriteToken(token);
        //}
    }

    public class LdapsızLoginDto
    {
        public string? KullaniciAdi { get; set; }

    }

    public class LdapLoginDto
    {
        public string? KullaniciAdi { get; set; }
        public string? Sifre { get; set; }
    }
}
