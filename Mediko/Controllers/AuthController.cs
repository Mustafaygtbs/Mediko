using Mediko.DataAccess;
using Mediko.Entities;
using Mediko.Entities.DTOs.AuthDTOs;
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
     

    }    
}
