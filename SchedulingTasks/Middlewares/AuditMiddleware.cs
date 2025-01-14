namespace SchedulingTasks.Middlewares
{
    public class AuditMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuditMiddleware> _logger;

        public AuditMiddleware(RequestDelegate next, ILogger<AuditMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var startTime = DateTime.UtcNow;
            var correlationId = Guid.NewGuid().ToString();
            context.Items["CorrelationId"] = correlationId;

            try
            {
                await _next(context);
            }
            finally
            {
                var duration = DateTime.UtcNow - startTime;
                _logger.LogInformation(
                    "Request {Method} {Path} completed in {Duration}ms with status {StatusCode}. CorrelationId: {CorrelationId}",
                    context.Request.Method,
                    context.Request.Path,
                    duration.TotalMilliseconds,
                    context.Response.StatusCode,
                    correlationId);
            }
        }
    }
}