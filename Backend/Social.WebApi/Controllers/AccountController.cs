using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Social.Data.Model.Request.User;
using Social.Data.Model.Response.User;
using Social.Service.Social.Jwt.Interface;
using Social.WebApi.Infrastructure.Services;

namespace Social.WebApi.Controllers
{
    [ApiController]
    [Route("api/account")]
    public class AccountController : BaseApiController
    {
        private readonly IJwtService _jwtService;

        public AccountController(IJwtService jwtService, ICurrentUserService currentUserService) : base(currentUserService)
        {
            _jwtService = jwtService;
        }

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
