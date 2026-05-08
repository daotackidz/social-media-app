using Social.Data.Model.Request.Login;
using Social.Data.Model.Response.Login;

namespace Social.Service.Social.Jwt.Interface
{
    public interface IJwtService
    {
        Task<LoginResponse?> Authenticate(LoginRequest request);
    }
}
