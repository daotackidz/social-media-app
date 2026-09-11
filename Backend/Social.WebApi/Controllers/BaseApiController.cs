using Microsoft.AspNetCore.Mvc;
using Social.Common.Constants;
using Social.Data.Model.Response.Base;
using Social.WebApi.Infrastructure.Services;

[ApiController]
public class BaseApiController : ControllerBase
{
    private readonly ICurrentUserService _currentUserService;

    public BaseApiController(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    protected Guid? CurrentUserId => _currentUserService?.UserId;
    protected string? CurrentUserEmail => _currentUserService?.Email;
    protected string? CurrentUserName => _currentUserService?.UserName;
    protected bool IsCurrentUserAuthenticated => _currentUserService?.IsAuthenticated ?? false;

    protected IActionResult ApiOk<T>(T data, string message = "Success")
        => StatusCode(200, new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            StatusCode = 200
        });

    protected IActionResult ApiCreated<T>(T data, string message = "Created")
        => StatusCode(201, new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            StatusCode = 201
        });

    protected IActionResult ApiBadRequest(string message, ErrorCode errorCode = ErrorCode.VALIDATION_ERROR)
        => StatusCode(400, new ApiResponse<object>
        {
            Success = false,
            Message = message,
            StatusCode = 400,
            ErrorCode = errorCode
        });

    protected IActionResult ApiUnauthorized(string message = "Bạn cần đăng nhập để thực hiện thao tác này.", ErrorCode errorCode = ErrorCode.UNAUTHORIZED)
        => StatusCode(401, new ApiResponse<object>
        {
            Success = false,
            Message = message,
            StatusCode = 401,
            ErrorCode = errorCode
        });

    protected IActionResult ApiNotFound(string message, ErrorCode errorCode = ErrorCode.USER_NOT_FOUND)
        => StatusCode(404, new ApiResponse<object>
        {
            Success = false,
            Message = message,
            StatusCode = 404,
            ErrorCode = errorCode
        });

    protected IActionResult ApiConflict(string message, ErrorCode errorCode = ErrorCode.EMAIL_EXISTS)
        => StatusCode(409, new ApiResponse<object>
        {
            Success = false,
            Message = message,
            StatusCode = 409,
            ErrorCode = errorCode
        });
}
