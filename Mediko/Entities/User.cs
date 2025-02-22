using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Mediko.Entities
{
    public class User : IdentityUser
    {
        public string AdSoyad { get; set; } = string.Empty;
        public string OgrenciNo { get; set; } = string.Empty;

        [StringLength(11, MinimumLength = 11, ErrorMessage = "T.C. Kimlik Numarası 11 haneli olmalıdır.")]
        public string TcKimlikNo { get; set; } = string.Empty;

        public DateTime DogumTarihi { get; set; }

        public string DogumYeri { get; set; } = string.Empty;

        public string AnneAdi { get; set; } = string.Empty;

        public string BabaAdi { get; set; } = string.Empty;

        public string TelNo { get; set; } = string.Empty;


    }
}