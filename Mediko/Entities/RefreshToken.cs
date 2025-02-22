namespace Mediko.Entities
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; } = string.Empty;

        public string UserId { get; set; } = string.Empty;
        public User User { get; set; } 

        public DateTime ExpirationDate { get; set; }
    }
}
