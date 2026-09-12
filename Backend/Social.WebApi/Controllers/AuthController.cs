using Microsoft.AspNetCore.Mvc;
using Social.Common.Constants;
using Social.Data.Model.Request.User;
using Social.Service.Social.Auth.Interface;
using Social.WebApi.Infrastructure.Services;

[Route("api/auth")]
public class AuthController : BaseApiController
{
    private readonly IAuthService _authService;
    public AuthController(IAuthService authService, ICurrentUserService currentUserService)
        : base(currentUserService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var (success, message, data, code) = await _authService.RegisterAsync(request);

        return success
            ? ApiCreated(data, message)
            : ApiBadRequest(message, code ?? ErrorCode.VALIDATION_ERROR);
    }

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
    {
        var (success, message, data, code) = await _authService.VerifyEmailAsync(request);

        return success
            ? ApiOk(data, message)
            : ApiBadRequest(message, code ?? ErrorCode.VALIDATION_ERROR);
    }

    [HttpPost("resend-otp")]
    public async Task<IActionResult> ResendOtp([FromBody] ResendOtpRequest request)
    {
        var (success, message, code) = await _authService.ResendOtpAsync(request.Email);

        return success
            ? ApiOk<object>(null!, message)
            : ApiBadRequest(message, code ?? ErrorCode.VALIDATION_ERROR);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var (success, message, data, code) = await _authService.ForgotPasswordAsync(request);

        return success
            ? ApiOk(data, message)
            : ApiBadRequest(message, code ?? ErrorCode.VALIDATION_ERROR);
    }

    [HttpPost("verify-forgot-password-otp")]
    public async Task<IActionResult> VerifyForgotPasswordOtp([FromBody] VerifyForgotPasswordOtpRequest request)
    {
        var (success, message, code) = await _authService.VerifyForgotPasswordOtpAsync(request);

        return success
            ? ApiOk<object>(null!, message)
            : ApiBadRequest(message, code ?? ErrorCode.VALIDATION_ERROR);
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var (success, message, code) = await _authService.ResetPasswordAsync(request);

        return success
            ? ApiOk<object>(null!, message)
            : ApiBadRequest(message, code ?? ErrorCode.VALIDATION_ERROR);
    }
}
