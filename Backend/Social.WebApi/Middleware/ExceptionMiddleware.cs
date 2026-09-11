using Social.Common.Constants;
using Social.Data.Model.Response.Base;

namespace Social.WebApi.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");
                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";

                var response = new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Lỗi hệ thống, vui lòng thử lại.",
                        StatusCode = 500,
                        ErrorCode = ErrorCode.INTERNAL_ERROR
                    };
                await context.Response.WriteAsJsonAsync(response);
            }
        }
    }
}