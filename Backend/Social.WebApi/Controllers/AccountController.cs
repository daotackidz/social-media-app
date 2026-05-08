using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Social.Data.Model.Response.Login;
using Social.JwtServices.Interface;
using Social.WebApi.Models.Api.Login;

namespace Social.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController(IJwtService jwtService) : ControllerBase
    {
        private readonly IJwtService _jwtService = jwtService;

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult<LoginResponseModel>> Login(LoginRequestModel request)
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
