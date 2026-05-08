using Microsoft.EntityFrameworkCore;
using Social.Data.Model.Base;
using Social.Data.Model.User;
using Social.Data.Seed.Models.Base;
using Social.Data.Seed.Models.User;
using Social.Data.Seed.RepositorySeed;

namespace Social.Data.Seed
{
    public class PostgreSQLSeedData
    {
        public static void Seed(DbContext dbContext)
        {
            #region SCRIPTS

            if (System.Diagnostics.Debugger.IsAttached == false)
            {
                System.Diagnostics.Debugger.Launch();
            }

            #endregion SCRIPTS

            #region Data

            var schema = SocialSeedingDbContext.GetDefaultSchemaName(dbContext);

            PostgreSqlSeedObject<RecordStatusSeed, RecordStatus>.Seed(dbContext, schema);
            PostgreSqlSeedObject<UserSeed, Users>.Seed(dbContext, schema);

            #endregion Data
        }
    }
}