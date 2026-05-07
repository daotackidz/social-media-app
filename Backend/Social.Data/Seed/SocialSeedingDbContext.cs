using Microsoft.EntityFrameworkCore;
using Social.Data.Repository;

namespace Social.Data.Seed
{
    internal class SocialSeedingDbContext : SocialDbContext
    {
        public SocialSeedingDbContext(DbContextOptions<SocialDbContext> options) : base(options)
        {
        }
    }
}