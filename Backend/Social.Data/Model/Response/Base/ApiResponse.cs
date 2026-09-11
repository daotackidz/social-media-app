using Social.Common.Constants;

namespace Social.Data.Model.Response.Base
{
    /// <summary>
    /// The one response envelope every API endpoint returns — success or error,
    /// including framework-generated ones (model validation, unhandled
    /// exceptions). See BaseApiController's ApiOk/ApiCreated/ApiBadRequest/...
    /// helpers, Program.cs's InvalidModelStateResponseFactory, and
    /// Middleware/ExceptionMiddleware for where each field gets set.
    /// </summary>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public int StatusCode { get; set; }

        /// <summary>
        /// Machine-readable failure reason, null on success. The frontend
        /// switches on this (never on Message) to decide what to do next.
        /// </summary>
        public ErrorCode? ErrorCode { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
