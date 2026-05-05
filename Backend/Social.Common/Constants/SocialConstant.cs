namespace Social.Common.Constants
{
    public partial class SocialConstant
    {
        public const string MainConnString = "MAIN_CONN_STRING";
        public const string MainConnStringFull = "ConnectionStrings:MAIN_CONN_STRING";
        public const string DbContextCustom = "ConnectionStrings:DbContextCustomUser";
        public const string DbContext = "ConnectionStrings:DbContext";
        public const string DefaultSchema = "DefaultSchema";
        public const string AdminSchema = "AdminSchema";
        public const string DanhMucChungSchema = "DanhMucChungSchema";
        public const string DefaultPassword = "NO_PASSWORD_CREATED";
    }

    public class SocialStatusConstant
    {
        public const int SystemUsing = 0;
        public const int Active = 1;
        public const int Deactive = 2;
        public const int Deleted = 3;
    }
}