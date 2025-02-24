using Mediko.DataAccess;
using Mediko.Entities;
using Mediko.Entities.DTOs.AuthDTOs;
using Mediko.Entities.DTOs.UserDTOs;
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
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class UserController : ControllerBase
    {
       
            private readonly UserManager<User> _userManager;
            private readonly SignInManager<User> _signInManager;
            private readonly JwtSettings _jwtSettings;
            private readonly MedikoDbContext _context;

            public UserController(
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

            [HttpPost("Register")]
            public async Task<IActionResult> Register([FromBody] RegisterDto model)
            {
                if (model == null)
                    return BadRequest(new { Message = "Boş model." });

                var ogrno = await _userManager.FindByNameAsync(model.OgrenciNo);
                if (ogrno != null)
                    return Conflict(new { Message = "Bu öğrenci numarası zaten kayıtlı." });

                var newUser = new User
                {
                    UserName = model.OgrenciNo,
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

                var createResult = await _userManager.CreateAsync(newUser);
                if (!createResult.Succeeded)
                    return BadRequest(new
                    {
                        Message = "Kullanıcı oluşturulamadı.",
                        Errors = createResult.Errors.Select(e => e.Description)
                    });

                await _userManager.AddToRoleAsync(newUser, "User");

                return Ok(new { Message = "Kayıt başarılı." });
            }


        //[HttpPost("RegisterAdmin")]
        //public async Task<IActionResult> RegisterAdmin([FromBody] AdminRegisterDto model, [FromServices] MailIslemleri mailIslemleri)
        //{
        //    try
        //    {
        //        if (model == null)
        //            return BadRequest(new { Message = "Boş model." });

        //        // TC Kimlik No ile Kullanıcı Var mı Kontrol Et
        //        var existingUser = await _userManager.FindByNameAsync(model.Tc);
        //        if (existingUser != null)
        //            return Conflict(new { Message = "Bu TC numarası zaten kayıtlı." });

        //        var newAdmin = new User
        //        {
        //            UserName = model.UserName,
        //            Email = model.Email,
        //            AdSoyad = model.AdSoyad,
        //            TcKimlikNo = model.Tc,
        //            TelNo = model.TelNo,
        //            EmailConfirmed = true,
        //            PasswordHash = null 
        //        };

        //        var createResult = await _userManager.CreateAsync(newAdmin);


        //        // **Oluşturma Başarısızsa Hata Dön**
        //        if (!createResult.Succeeded)
        //        {
        //            var errors = createResult.Errors.Select(e => e.Description).ToList();
        //            Console.WriteLine($"🛑 Kullanıcı oluşturma başarısız. Hatalar: {string.Join(", ", errors)}");
        //            return BadRequest(new
        //            {
        //                Message = "Admin oluşturulamadı.",
        //                Errors = errors
        //            });
        //        }


        //        // **Admin Rolü Ata**
        //        await _userManager.AddToRoleAsync(newAdmin, "Admin");

        //        // 📧 **E-Posta Gönder**
        //        try
        //        {
        //            await mailIslemleri.SendAdminRegistrationEmail(newAdmin);
        //        }
        //        catch (Exception emailEx)
        //        {
        //            Console.WriteLine($"📧 Mail Gönderme Hatası: {emailEx.Message}");
        //        }

        //        return Ok(new { Message = "Admin kayıt başarılı. Bilgilendirme e-postası gönderildi." });
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"🛑 Hata Oluştu: {ex.Message}");
        //        return StatusCode(500, new { Message = "Sunucu hatası: " + ex.Message });
        //    }
        //}


        [HttpPost("GetUserInfo")]
            public async Task<IActionResult> GetUserInfo()
            {
                var authHeader = Request.Headers["Authorization"].ToString();
                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                {
                    return BadRequest(new { Message = "Authorization header eksik veya geçersiz." });
                }

                var token = authHeader.Substring("Bearer ".Length).Trim();

                try
                {
                    var handler = new JwtSecurityTokenHandler();

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

                    var principal = handler.ValidateToken(token, validations, out _);

                    var userName = principal.Identity?.Name;
                    if (string.IsNullOrEmpty(userName))
                        return Unauthorized(new { Message = "Token'dan kullanıcı bilgisi okunamadı." });


                    var user = await _userManager.FindByNameAsync(userName);
                    if (user == null)
                        return NotFound(new { Message = "Kullanıcı veritabanında bulunamadı." });

                    var roles = await _userManager.GetRolesAsync(user);


                    var adSoyad = principal.FindFirst("AdSoyad")?.Value;
                    var ogrenciNo = principal.FindFirst("OgrenciNo")?.Value;
                    var tcKimlikNo = principal.FindFirst("TcKimlikNo")?.Value;
                    var dogumTarihi = principal.FindFirst("DogumTarihi")?.Value;
                    var dogumYeri = principal.FindFirst("DogumYeri")?.Value;
                    var anneAdi = principal.FindFirst("AnneAdi")?.Value;
                    var babaAdi = principal.FindFirst("BabaAdi")?.Value;
                    var telNo = principal.FindFirst("TelNo")?.Value;

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



        }
    }
    
