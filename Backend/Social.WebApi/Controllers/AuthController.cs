using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Social.AuthServices.Interface;
using Social.JwtServices.Interface;

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

        /// <summary>Bước 1: Nhập email + password, nhận OTP qua email</summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var (success, message) = await _authService.RegisterAsync(request.Email, request.Password);
            return success
                ? Ok(new ApiResponse<object>(true, message))
                : BadRequest(new ApiResponse<object>(false, message));
        }

        /// <summary>Bước 2: Xác nhận OTP để hoàn tất đăng ký</summary>
        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
        {
            var (success, message) = await _authService.VerifyEmailAsync(request.Email, request.OtpCode);
            return success
                ? Ok(new ApiResponse<object>(true, message))
                : BadRequest(new ApiResponse<object>(false, message));
        }

        /// <summary>Gửi lại OTP (nếu hết hạn hoặc không nhận được)</summary>
        [HttpPost("resend-otp")]
        public async Task<IActionResult> ResendOtp([FromBody] ResendOtpRequest request)
        {
            var (success, message) = await _authService.ResendOtpAsync(request.Email);
            return success
                ? Ok(new ApiResponse<object>(true, message))
                : BadRequest(new ApiResponse<object>(false, message));
        }
    }
}
