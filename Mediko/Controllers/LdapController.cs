using Mediko.Services;
using Microsoft.AspNetCore.Mvc;

namespace Mediko.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LdapController : Controller
    {
        private readonly LdapApiService _ldapApiService;

        public LdapController(LdapApiService ldapApiService)
        {
            _ldapApiService = ldapApiService;
        }


        [HttpPost]
        [Route("Authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] LoginModel loginModel)
        {

            if (string.IsNullOrWhiteSpace(loginModel.Username) || string.IsNullOrWhiteSpace(loginModel.Password))
            {
                return BadRequest("Kullanıcı adı ve şifre gereklidir.");
            }

            bool isAuthenticated = await _ldapApiService.AuthenticateAsync(loginModel.Username, loginModel.Password);

            if (isAuthenticated)
            {
                return Ok("Kimlik doğrulaması başarılı.");
            }


            return Unauthorized("Kimlik doğrulaması başarısız.");

        }
    }
    public class LoginModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
