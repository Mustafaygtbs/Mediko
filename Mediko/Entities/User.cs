using Microsoft.AspNetCore.Identity;

namespace Mediko.Entities
{
    public class User : IdentityUser
    {
        public string AdSoyad { get; set; } = string.Empty;
        public bool IsLdapUser { get; set; } = false;

    }
}