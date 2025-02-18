namespace Mediko.Entities
{
    public class LdapSettings
    {
        public string Url { get; set; }
        public string UserAttribute { get; set; }
        public string Domain { get; set; }
        public int Timeout { get; set; } = 5;
        public int RetryCount { get; set; } = 3;
    }

}
