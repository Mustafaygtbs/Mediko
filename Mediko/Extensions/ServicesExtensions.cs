using Mediko.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Net.Http;
using System.Text;

namespace Mediko.Extensions
{
    public static class ServicesExtensions
    {
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = configuration["Jwt:Issuer"],
                        ValidAudience = configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"] ?? throw new ArgumentNullException("Jwt:Key", "JWT anahtar değeri belirtilmemiş.")))
                    };
                });

            return services;
        }




        public static IServiceCollection AddLdapAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient<ILdapAuthService, LdapAuthService>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(5); // 5 saniye içinde yanıt alamazsa hata fırlatma kodu eklendi
            });

            services.AddSingleton<JwtTokenService>();

            return services;
        }
    }
}