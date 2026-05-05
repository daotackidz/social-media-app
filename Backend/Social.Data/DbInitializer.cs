using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Social.Data.Repository;
using Social.Data.Seed;

namespace Social.Data
{
    public class DbInitializer(IServiceScopeFactory scopeFactory) : IDbInitializer
    {
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;

        public void Initialize()
        {
            using var serviceScope = _scopeFactory.CreateScope();
            using var context = serviceScope.ServiceProvider.GetService<SocialDbContext>();
            context.Database.Migrate();
        }

        public void SeedData()
        {
            using var serviceScope = _scopeFactory.CreateScope();
            using var context = serviceScope.ServiceProvider.GetService<SocialSeedingDbContext>();
            PostgreSQLSeedData.Seed(context);
        }
    }
}
