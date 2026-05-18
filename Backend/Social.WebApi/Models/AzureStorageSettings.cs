namespace Social.WebApi.Models
{
    public class AzureStorageSettings
    {
        public const string SectionName = "AzureStorageSettings";

        public string ConnectionString { get; set; } = string.Empty;
    }
}
