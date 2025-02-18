using Mediko.Entities;
using Microsoft.AspNetCore.Identity;
using System.Text.Json;
using Mediko.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace Mediko.Services
{
    public class LdapAuthService : ILdapAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly JwtTokenService _jwtTokenService;
        private readonly ILogger<LdapAuthService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _ldapUrl;

        public LdapAuthService(
            HttpClient httpClient,
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            JwtTokenService jwtTokenService,
            IConfiguration configuration,
            ILogger<LdapAuthService> logger)
        {
            _httpClient = httpClient;
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtTokenService = jwtTokenService;
            _logger = logger;
            _configuration = configuration;
            _ldapUrl = configuration["Ldap:Url"] ?? throw new ArgumentNullException("Ldap:Url", "LDAP URL tanımlanmamış.");
        }

        public async Task<string?> AuthenticateAndGenerateTokenAsync(string username, string password)
        {
            try
            {
                var formData = new MultipartFormDataContent
                {
                    { new StringContent(username), "username" },
                    { new StringContent(password), "pass" }
                };

                var response = await _httpClient.PostAsync(_ldapUrl, formData);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("LDAP doğrulama başarısız: {Username}. HTTP Kod: {StatusCode}, Yanıt: {ResponseContent}",
                                        username, response.StatusCode, responseContent);
                    return null;
                }

                LdapResponse? ldapResponse;
                try
                {
                    ldapResponse = JsonSerializer.Deserialize<LdapResponse>(responseContent);
                }
                catch (JsonException jsonEx)
                {
                    _logger.LogError(jsonEx, "❌ HATA: LDAP sunucusundan gelen yanıt JSON formatında değil. Yanıt: {ResponseContent}", responseContent);
                    return null;
                }

                if (ldapResponse == null || !ldapResponse.Success)
                {
                    _logger.LogWarning("LDAP doğrulama başarısız: {Username}. Yanıt: {ResponseContent}", username, responseContent);
                    return null;
                }

                var user = await _userManager.FindByNameAsync(username);
                if (user == null)
                {
                    var defaultEmailDomain = _configuration["Ldap:DefaultEmailDomain"] ?? "company.local";
                    var userEmail = !string.IsNullOrEmpty(ldapResponse.Email) ? ldapResponse.Email : $"{username}@{defaultEmailDomain}";

                    user = new User
                    {
                        UserName = username,
                        Email = userEmail,
                        EmailConfirmed = true
                    };


                    var result = await _userManager.CreateAsync(user);
                    if (!result.Succeeded)
                    {
                        _logger.LogError("LDAP kullanıcısı sisteme eklenirken hata oluştu: {Username}", username);
                        return null;
                    }
                }

                var existingRoles = await _userManager.GetRolesAsync(user);
                if (ldapResponse.Roles != null)
                {
                    foreach (var role in ldapResponse.Roles)
                    {
                        if (!await _roleManager.RoleExistsAsync(role))
                        {
                            await _roleManager.CreateAsync(new IdentityRole(role));
                        }

                        if (!existingRoles.Contains(role))
                        {
                            await _userManager.AddToRoleAsync(user, role);
                        }
                    }
                }
                else
                {
                    if (!existingRoles.Contains("User"))
                    {
                        await _userManager.AddToRoleAsync(user, "User");
                    }
                }

                var roles = await _userManager.GetRolesAsync(user);
                return _jwtTokenService.GenerateJwtToken(user.UserName, roles, user.Id, IsLdapUser: true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LDAP authentication işlemi sırasında hata oluştu.");
                return null;
            }
        }
    }

    public class LdapResponse
    {
        public bool Success { get; set; }
        public string? Email { get; set; }
        public List<string>? Roles { get; set; }
    }
}
