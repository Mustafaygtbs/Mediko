namespace Mediko.Entities.DTOs.AuthDTOs
{
    public class RegisterDto
    {
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
}
