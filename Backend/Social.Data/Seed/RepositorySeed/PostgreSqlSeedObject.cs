using Microsoft.EntityFrameworkCore;
using Social.Common.Extensions;
using Social.Data.Seed.Interface;
using System.Reflection;

namespace Social.Data.Seed.RepositorySeed
{
    public static class PostgreSqlSeedObject<TObjectSeed, TObjectMappingDatabase>
         where TObjectSeed : class, new()
         where TObjectMappingDatabase : class
    {
        public static void Seed(DbContext context, string schema = "")
        {
            var dbSet = context.Set<TObjectMappingDatabase>();
            var seed = new TObjectSeed() as ISeed<TObjectMappingDatabase>;
            if (seed != null && !seed.GetSeedData.IsNullIsEmptyOrFirstIsNull())
            {
                AddOrUpdateModel dataAddOrUpdate = CheckFromDb(dbSet, seed.GetSeedData);
                try
                {
                    var entityType = context.Model.FindEntityType(typeof(TObjectMappingDatabase));
                    if (entityType == null)
                        throw new InvalidOperationException($"Type {typeof(TObjectMappingDatabase).Name} is not part of the DbContext model.");

                    var keyProps = entityType.FindPrimaryKey()?.Properties;
                    if (keyProps == null || keyProps.Count == 0)
                        throw new InvalidOperationException($"Entity {typeof(TObjectMappingDatabase).Name} does not have a primary key defined.");

                    var duplicates = dataAddOrUpdate.Adds
                        .GroupBy(entity =>
                            string.Join("|", keyProps.Select(p =>
                                p.PropertyInfo.GetValue(entity)?.ToString() ?? "null"
                            ))
                        )
                        .Where(g => g.Count() > 1)
                        .SelectMany(g => g)
                        .ToList();

                    if (dataAddOrUpdate.Adds.Count != 0)
                    {
                        dataAddOrUpdate.Adds = dataAddOrUpdate.Adds.Where(x => x != null).ToList();
                        context.BulkInsert<TObjectMappingDatabase>(dataAddOrUpdate.Adds);
                    }
                    if (dataAddOrUpdate.Updates.Any())
                    {
                        dataAddOrUpdate.Updates = dataAddOrUpdate.Updates.Where(x => x != null).ToList();
                        context.BulkUpdate<TObjectMappingDatabase>(dataAddOrUpdate.Updates);
                    }
                }
                catch (Exception exception)
                {
                    var a = dataAddOrUpdate;
                    throw;
                }
            }
        }

        public static AddOrUpdateModel CheckFromDb(DbSet<TObjectMappingDatabase> dbSet, TObjectMappingDatabase[] seedData)
        {
            var result = new AddOrUpdateModel();
            var context = dbSet.GetContext();
            var ids = context.Model.FindEntityType(typeof(TObjectMappingDatabase)).FindPrimaryKey().Properties.Select(x => x.Name);
            var t = typeof(TObjectMappingDatabase);
            List<PropertyInfo> keyFields = new List<PropertyInfo>();
            Parallel.ForEach(t.GetProperties(), new ParallelOptions() { MaxDegreeOfParallelism = 10 }, async propt =>
            {
                var keyAttr = ids.Contains(propt.Name);
                if (keyAttr)
                {
                    keyFields.Add(propt);
                }
            });
            if (!keyFields.Any())
            {
                return result;
            }
          
            var maxDegreeOfParallelism = 1;
            var entities = dbSet.AsNoTracking().ToList();

            if (seedData.Length <= 500)
            {
                maxDegreeOfParallelism = 1;
            }
            else
            {
                maxDegreeOfParallelism = 10;
            }
            Parallel.ForEach(seedData, new ParallelOptions() { MaxDegreeOfParallelism = maxDegreeOfParallelism }, async item =>
            {
                var env = entities;
                foreach (var keyField in keyFields)
                {
                    var keyVal = keyField.GetValue(item);
                    env = entities.Where(p => p.GetType().GetProperty(keyField.Name).GetValue(p).Equals(keyVal)).ToList();
                }
                var dbVal = env.FirstOrDefault();
                if (dbVal != null)
                {
                    result.Updates.Add(item);
                }
                else
                {
                    result.Adds.Add(item);
                }
            });

            return result;
        }

        public class AddOrUpdateModel
        {
            public List<TObjectMappingDatabase> Adds { get; set; }
            public List<TObjectMappingDatabase> Updates { get; set; }

            public AddOrUpdateModel()
            {
                Adds = new List<TObjectMappingDatabase>();
                Updates = new List<TObjectMappingDatabase>();
            }
        }
    }
}
