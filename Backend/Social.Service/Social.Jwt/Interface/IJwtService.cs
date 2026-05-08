using Social.Data.Model.Response.Login;
using Social.WebApi.Models.Api.Login;

namespace Social.Service.Social.Jwt.Interface
{
    public interface IJwtService
    {
        Task<LoginResponseModel?> Authenticate(LoginRequestModel request);
    }
}
