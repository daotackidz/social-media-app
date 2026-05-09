using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Social.Data.Model.Request.User;
using Social.Data.Model.Response.User;
using Social.Service.Social.Jwt.Interface;

namespace Social.WebApi.Controllers
{
    [ApiController]
    [Route("api/account")]
    public class AccountController(IJwtService jwtService) : ControllerBase
    {
        private readonly IJwtService _jwtService = jwtService;

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
        {
            var result = await _jwtService.Authenticate(request);

            if (result is null)
            {
                return Unauthorized();
            }
            return Ok(result);
        }
    }
}
