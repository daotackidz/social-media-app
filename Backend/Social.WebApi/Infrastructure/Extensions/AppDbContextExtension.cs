using Microsoft.EntityFrameworkCore;
using Social.Common.Constants;
using Social.Data.Repository;

namespace Social.WebApi.Infrastructure.Extensions
{
    public static class AppDbContextExtension
    {
        public static IServiceCollection AddAppDbContext(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            string? connectionString =
                configuration.GetConnectionString(
                    SocialConstant.MainConnString);

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            services.AddDbContext<SocialDbContext>(options =>
                options.UseNpgsql(connectionString));

            return services;
        }
    }
}