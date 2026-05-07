using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace Social.Common.Functions
{
    public static class EntityHelperFunction
    {
        public static string GetAppSettingValueByKey(string key)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            return configuration[key] ?? "";
        }

        public static string GetTableName<T>()
        {
            var attr = typeof(T).GetCustomAttribute<TableAttribute>();
            return attr?.Name ?? typeof(T).Name; // nếu không có attribute thì trả về tên class
        }

        public static string GetSchemaName<T>()
        {
            var attr = typeof(T).GetCustomAttribute<TableAttribute>();
            return attr?.Schema ?? "";
        }
    }
}
