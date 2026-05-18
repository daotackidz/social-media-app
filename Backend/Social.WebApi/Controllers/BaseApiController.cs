using Microsoft.AspNetCore.Mvc;
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

    protected IActionResult ApiBadRequest(string message)
        => StatusCode(400, new ApiResponse<object>
        {
            Success = false,
            Message = message,
            StatusCode = 400
        });

    protected IActionResult ApiNotFound(string message)
        => StatusCode(404, new ApiResponse<object>
        {
            Success = false,
            Message = message,
            StatusCode = 404
        });

    protected IActionResult ApiConflict(string message)
        => StatusCode(409, new ApiResponse<object>
        {
            Success = false,
            Message = message,
            StatusCode = 409
        });
}