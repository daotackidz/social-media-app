namespace Social.Data.Model.Response.User
{
    public class LoginResponse
    {
        public string? Email { get; set; }
        public string? Username { get; set; }
        public string? AccessToken { get; set; }
        public int ExpiresIn { get; set; }
    }
}
