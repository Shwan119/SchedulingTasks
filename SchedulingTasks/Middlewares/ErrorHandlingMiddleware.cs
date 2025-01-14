using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace SchedulingTasks.Middlewares
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate _next, ILogger<ErrorHandlingMiddleware> logger)
        {
            this._next = _next;
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
                _logger.LogError(ex, "Unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";

            var result = JsonSerializer.Serialize(new
            {
                Error = ex.Message,
                CorrelationId = context.Items["CorrelationId"]?.ToString()
            });

            context.Response.StatusCode = ex switch
            {
                //NotFoundException => StatusCodes.Status404NotFound,
                ValidationException => StatusCodes.Status400BadRequest,
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                _ => StatusCodes.Status500InternalServerError
            };

            await context.Response.WriteAsync(result);
        }
    }
}