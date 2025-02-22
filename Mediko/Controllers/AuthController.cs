using Mediko.DataAccess;
using Mediko.Entities;
using Mediko.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace Mediko.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly JwtSettings _jwtSettings;
        private readonly MedikoDbContext _context;

        public AuthController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IOptions<JwtSettings> jwtSettings,
            MedikoDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtSettings = jwtSettings.Value;
            _context = context;
        }


        [AllowAnonymous]
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            if (model == null)
                return BadRequest(new { Message = "Boş model." });

            var userExists = await _userManager.FindByNameAsync(model.KullaniciAdi);
            if (userExists != null)
                return BadRequest(new { Message = "Kullanıcı zaten var." });

            var newUser = new User
            {
                UserName = model.KullaniciAdi,
                Email = model.Email,
                AdSoyad = model.AdSoyad,
                OgrenciNo = model.OgrenciNo,
                TcKimlikNo = model.TcKimlikNo,
                DogumTarihi = model.DogumTarihi,
                DogumYeri = model.DogumYeri,
                AnneAdi = model.AnneAdi,
                BabaAdi = model.BabaAdi,
                TelNo = model.TelNo,
                EmailConfirmed = true 
            };


            var createResult = await _userManager.CreateAsync(newUser, model.Sifre);
            if (!createResult.Succeeded)
                return BadRequest(createResult.Errors);

            await _userManager.AddToRoleAsync(newUser, "User");

            return Ok(new { Message = "Kayıt başarılı." });
        }


        [AllowAnonymous]
        [HttpPost("LdapsızLogin")]
        public async Task<IActionResult> Login([FromBody] LdapsizLoginDto model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.KullaniciAdi))
                return BadRequest(new { Message = "Kullanıcı adı veya şifre boş olamaz." });

            var user = await _userManager.FindByNameAsync(model.KullaniciAdi);
            if (user == null)
                return Unauthorized(new { Message = "Kullanıcı bulunamadı." });


            var (accessToken, accessExp, refreshToken, refreshExp) = CustomTokenHandler.CreateToken(user, _jwtSettings, _context);

            return Ok(new
            {
                accessToken = new
                {
                    token = accessToken,
                    expiration = accessExp
                },
                refreshToken = new
                {
                    token = refreshToken,
                    expiration = refreshExp
                }
            });
        }

        [AllowAnonymous]
        [HttpPost("Refresh-Token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            if (string.IsNullOrEmpty(request.RefreshToken))
                return BadRequest(new { Message = "Refresh token boş olamaz." });

            var existing = _context.RefreshTokens
                .FirstOrDefault(r => r.Token == request.RefreshToken && r.ExpirationDate > DateTime.UtcNow);

            if (existing == null)
                return Unauthorized(new { Message = "Geçersiz veya süresi dolmuş refresh token." });

            var user = await _userManager.FindByIdAsync(existing.UserId);
            if (user == null)
                return Unauthorized(new { Message = "Kullanıcı bulunamadı." });

            try
            {
                var (accessToken, accessExp, newRefreshToken, refreshExp) =
                    CustomTokenHandler.RefreshAccessToken(request.RefreshToken, _jwtSettings, _context, user);

                return Ok(new
                {
                    accessToken = new
                    {
                        token = accessToken,
                        expiration = accessExp
                    },
                    refreshToken = new
                    {
                        token = newRefreshToken,
                        expiration = refreshExp
                    }
                });
            }
            catch (SecurityTokenException ex)
            {
                return Unauthorized(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Beklenmeyen hata.", Error = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("Logout")]
        public IActionResult Logout([FromBody] RefreshTokenRequestDto request)
        {
            if (string.IsNullOrEmpty(request.RefreshToken))
                return BadRequest(new { Message = "Refresh token boş olamaz." });

            var existing = _context.RefreshTokens
                .FirstOrDefault(r => r.Token == request.RefreshToken);
            if (existing == null)
                return NotFound(new { Message = "Refresh token bulunamadı veya geçersiz." });

            _context.RefreshTokens.Remove(existing);
            _context.SaveChanges();

            return Ok(new { Message = "Başarıyla çıkış yapıldı. Refresh token silindi." });
        }



        [HttpPost("GetUserInfo")]
        public async Task<IActionResult> GetUserInfo()
        {
            // "Authorization" header'ını alıyoruz.
            var authHeader = Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return BadRequest(new { Message = "Authorization header eksik veya geçersiz." });
            }

            // "Bearer " kısmını kaldırarak token'ı elde ediyoruz.
            var token = authHeader.Substring("Bearer ".Length).Trim();

            try
            {
                var handler = new JwtSecurityTokenHandler();

                // JWT ayarları
                var key = Encoding.UTF8.GetBytes(_jwtSettings.Key ?? string.Empty);
                var validations = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidAudience = _jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                // Token'ı doğrula
                var principal = handler.ValidateToken(token, validations, out _);

                // Token'dan kullanıcı adı alalım
                var userName = principal.Identity?.Name;
                if (string.IsNullOrEmpty(userName))
                    return Unauthorized(new { Message = "Token'dan kullanıcı bilgisi okunamadı." });

                // Veritabanından kullanıcı bul
                var user = await _userManager.FindByNameAsync(userName);
                if (user == null)
                    return NotFound(new { Message = "Kullanıcı veritabanında bulunamadı." });

                var roles = await _userManager.GetRolesAsync(user);

                // Token içindeki claim'ler
                var adSoyad = principal.FindFirst("AdSoyad")?.Value;
                var ogrenciNo = principal.FindFirst("OgrenciNo")?.Value;
                var tcKimlikNo = principal.FindFirst("TcKimlikNo")?.Value;
                var dogumTarihi = principal.FindFirst("DogumTarihi")?.Value;
                var dogumYeri = principal.FindFirst("DogumYeri")?.Value;
                var anneAdi = principal.FindFirst("AnneAdi")?.Value;
                var babaAdi = principal.FindFirst("BabaAdi")?.Value;
                var telNo = principal.FindFirst("TelNo")?.Value;

                // Döneceğimiz cevap
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
                return Unauthorized(new { Message = "Geçersiz veya süresi dolmuş token." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Beklenmeyen bir hata oluştu.", Error = ex.Message });
            }
        }


        //[HttpPost("GetUserInfo")]
        //public async Task<IActionResult> GetUserInfo([FromBody] TokenBodyDto request)
        //{
        //    if (string.IsNullOrEmpty(request?.Token))
        //    {
        //        return BadRequest(new { Message = "Token alanı boş olamaz." });
        //    }

        //    try
        //    {
        //        var token = request.Token;
        //        var handler = new JwtSecurityTokenHandler();

        //        // JWT ayarları
        //        var key = Encoding.UTF8.GetBytes(_jwtSettings.Key ?? string.Empty);
        //        var validations = new TokenValidationParameters
        //        {
        //            ValidateIssuerSigningKey = true,
        //            IssuerSigningKey = new SymmetricSecurityKey(key),
        //            ValidateIssuer = true,
        //            ValidateAudience = true,
        //            ValidIssuer = _jwtSettings.Issuer,
        //            ValidAudience = _jwtSettings.Audience,
        //            ValidateLifetime = true,
        //            ClockSkew = TimeSpan.Zero
        //        };

        //        // Token'ı doğrula
        //        var principal = handler.ValidateToken(token, validations, out _);

        //        // Token’dan kullanıcı adı alalım
        //        var userName = principal.Identity?.Name;
        //        if (string.IsNullOrEmpty(userName))
        //            return Unauthorized(new { Message = "Token'dan kullanıcı bilgisi okunamadı." });

        //        // Veritabanından kullanıcı bul
        //        var user = await _userManager.FindByNameAsync(userName);
        //        if (user == null)
        //            return NotFound(new { Message = "Kullanıcı veritabanında bulunamadı." });

        //        var roles = await _userManager.GetRolesAsync(user);

        //        // Token içindeki claim’ler
        //        var adSoyad = principal.FindFirst("AdSoyad")?.Value;
        //        var ogrenciNo = principal.FindFirst("OgrenciNo")?.Value;
        //        var tcKimlikNo = principal.FindFirst("TcKimlikNo")?.Value;
        //        var dogumTarihi = principal.FindFirst("DogumTarihi")?.Value;
        //        var dogumYeri = principal.FindFirst("DogumYeri")?.Value;
        //        var anneAdi = principal.FindFirst("AnneAdi")?.Value;
        //        var babaAdi = principal.FindFirst("BabaAdi")?.Value;
        //        var telNo = principal.FindFirst("TelNo")?.Value;

        //        // Döneceğimiz cevap
        //        return Ok(new
        //        {
        //            userName = userName,
        //            email = user.Email,
        //            roles = roles,
        //            adSoyad,
        //            ogrenciNo,
        //            tcKimlikNo,
        //            dogumTarihi,
        //            dogumYeri,
        //            anneAdi,
        //            babaAdi,
        //            telNo
        //        });
        //    }
        //    catch (SecurityTokenException)
        //    {
        //        return Unauthorized(new { Message = "Geçersiz veya süresi dolmuş token." });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { Message = "Beklenmeyen bir hata oluştu.", Error = ex.Message });
        //    }
        //}
    }

    public class RegisterDto
    {
        public string? KullaniciAdi { get; set; }
        public string? Sifre { get; set; }
        public string? Email { get; set; }
        public string? AdSoyad { get; set; }
        public string? OgrenciNo { get; set; }
        public string? TcKimlikNo { get; set; }
        public DateTime DogumTarihi { get; set; }
        public string? DogumYeri { get; set; }
        public string? AnneAdi { get; set; }
        public string? BabaAdi { get; set; }
        public string? TelNo { get; set; }
    }

    public class LdapsizLoginDto
    {
        public string? KullaniciAdi { get; set; }
    }
    public class TokenBodyDto
    {
        public string? Token { get; set; }
    }

    public class LdapLoginDto
    {
        public string? KullaniciAdi { get; set; }
        public string? Sifre { get; set; }
    }

    public class RefreshTokenRequestDto
    {
        public string RefreshToken { get; set; }
    }
}
