using Microsoft.EntityFrameworkCore;
using Social.Data.Repository;

namespace Social.Data.Seed
{
    internal class SocialSeedingDbContext : SocialDbContext
    {
        public SocialSeedingDbContext() : base()
        {
        }
    }
}