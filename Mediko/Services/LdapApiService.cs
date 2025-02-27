namespace Mediko.Services
{
    using System.Net.Http;
    using System.Threading.Tasks;

    public class LdapApiService
    {
        private readonly HttpClient _httpClient;

        public LdapApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }


        public async Task<bool> AuthenticateAsync(string username, string password)
        {
            var form = new MultipartFormDataContent();
            form.Add(new StringContent(username), "username");
            form.Add(new StringContent(password), "pass");

            var response = await _httpClient.PostAsync("http://192.168.2.16:8181/login_ldap", form);

            return response.IsSuccessStatusCode;


        }


    }


}
