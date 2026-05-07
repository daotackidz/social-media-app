namespace Social.WebApi.Infrastructure.Helper
{
    public static class LinkHelper
    {
        public static string FormatLink(string linkTemplate, params (string Key, string Value)[] parameters)
        {
            foreach (var (Key, Value) in parameters)
            {
                linkTemplate = linkTemplate.Replace($"{{{Key}}}", Value);
            }
            return linkTemplate;
        }
    }
}
