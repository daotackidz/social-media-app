using Social.Service.Social.Auth.Interface;
using Social.Service.Social.Auth.Services;
using Social.Service.Social.Email.Interface;
using Social.Service.Social.Email.Services;
using Social.Service.Social.Jwt.Interface;
using Social.Service.Social.Jwt.Services;

namespace Social.WebApi.Installers
{
    public class ServiceInstaller : IInstaller
    {
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IAuthService, AuthService>();
        }
    }
}
