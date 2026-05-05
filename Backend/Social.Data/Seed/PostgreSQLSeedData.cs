using Microsoft.EntityFrameworkCore;

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

            var schema = SocialSeedingDbContext.CalcDefaultSchemaName(dbContext);

            //PostgreSqlSeedObject<RuleSeed, Rule>.Seed(dbContext, adminSchema);

            #endregion Data


        }
    }
}