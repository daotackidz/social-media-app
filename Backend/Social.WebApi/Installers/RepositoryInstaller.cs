using Social.Repository.Social.User.Interface;
using Social.Repository.Social.User.Repository;

namespace Social.WebApi.Installers
{
    public class RepositoryInstaller : IInstaller
    {
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserPendingRepository, UserPendingRepository>();
        }
    }
}
