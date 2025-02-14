namespace Mediko.Entities
{
    public class GeciciKullaniciBilgileri
    {
        public static List<User> Users= new()
        {
            new User { Id = 1, KullaniciAdi = "admin", Sifre = "admin", Rol = "Admin" },
            new User { Id = 2, KullaniciAdi = "user", Sifre = "user", Rol = "User" }
        };
    }
}
