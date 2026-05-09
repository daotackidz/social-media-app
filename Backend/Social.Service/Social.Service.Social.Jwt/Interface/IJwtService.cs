using Social.Data.Model.Request.User;
using Social.Data.Model.Response.User;

namespace Social.Service.Social.Jwt.Interface
{
    public interface IJwtService
    {
        Task<LoginResponse?> Authenticate(LoginRequest request);
    }
}
