using System.Threading.Tasks;

namespace Mediko.Services
{
    public interface ILdapAuthService
    {
        Task<string?> AuthenticateAndGenerateTokenAsync(string username, string password);
    }
}
