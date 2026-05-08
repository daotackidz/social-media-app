using Microsoft.AspNetCore.Mvc;
using Social.Data.Model.Request.Login;
using Social.Service.Social.Auth.Interface;

namespace Social.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var (success, message) = await _authService.RegisterAsync(request);
            return success ? Ok(new { message }) : BadRequest(new { message });
        }

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
        {
            var (success, message) = await _authService.VerifyEmailAsync(request.Email, request.OtpCode);
            return success ? Ok(new { message }) : BadRequest(new { message });
        }

        [HttpPost("resend-otp")]
        public async Task<IActionResult> ResendOtp([FromBody] ResendOtpRequest request)
        {
            var (success, message) = await _authService.ResendOtpAsync(request.Email);
            return success ? Ok(new { message }) : BadRequest(new { message });
        }
    }
}
