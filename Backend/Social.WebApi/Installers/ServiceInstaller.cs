using Social.JwtServices.Interface;
using Social.JwtServices.Services;

namespace Social.WebApi.Installers
{
    public class ServiceInstaller : IInstaller
    {
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IJwtService, JwtService>();
        }
    }
}
