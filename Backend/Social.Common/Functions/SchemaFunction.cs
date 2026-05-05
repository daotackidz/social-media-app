using Microsoft.Extensions.Configuration;

namespace Social.Common.Functions
{
    public static class SchemaFunction
    {
        public static string GetAppSettingValueByKey(string key)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            return configuration[key] ?? "";
        }
    }
}
