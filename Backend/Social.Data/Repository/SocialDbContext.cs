using Microsoft.EntityFrameworkCore;
using Social.Common.Constants;
using Social.Common.Functions;

namespace Social.Data.Repository
{
    public class SocialDbContext() : DbContext
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //modelBuilder.HasDefaultSchema(_schema);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
            .UseNpgsql()
            .UseLowerCaseNamingConvention();

        public static string CalcDefaultSchemaName(DbContext db)
        {
            return SchemaFunction.GetAppSettingValueByKey(SocialConstant.DefaultSchema);
        }
        public static string CalcDefaultSchemaName()
        {
            return SchemaFunction.GetAppSettingValueByKey(SocialConstant.DefaultSchema);
        }

        public static string CalAdminSchema()
        {
            return SchemaFunction.GetAppSettingValueByKey(SocialConstant.AdminSchema);
        }

        public static string CalDanhMucChungSchema()
        {
            return SchemaFunction.GetAppSettingValueByKey(SocialConstant.DanhMucChungSchema);
        }
    }
}
