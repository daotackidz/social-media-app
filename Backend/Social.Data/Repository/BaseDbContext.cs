using Microsoft.EntityFrameworkCore;
using Social.Common.Constants;
using Social.Common.Functions;

namespace Social.Data.Repository
{
    public class BaseDbContext : DbContext
    {
        public BaseDbContext(DbContextOptions options) : base(options)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseLowerCaseNamingConvention();

        public static string GetDefaultSchemaName(DbContext db)
        {
            return EntityHelperFunction.GetAppSettingValueByKey(SocialConstant.DefaultSchema);
        }

        public static string GetDefaultSchemaName()
        {
            return EntityHelperFunction.GetAppSettingValueByKey(SocialConstant.DefaultSchema);
        }
    }
}
