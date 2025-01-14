using System.Diagnostics;
using SchedulingTasks.Interfaces;

namespace SchedulingTasks.Middlewares
{
    public class PerformanceMiddleware1
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<PerformanceMiddleware1> _logger;

        public PerformanceMiddleware1(RequestDelegate next, ILogger<PerformanceMiddleware1> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                await _next(context);
            }
            finally
            {
                sw.Stop();
                var duration = sw.ElapsedMilliseconds;

                // Only log API endpoints performance issues
                if (duration > 1000) // 1 second threshold
                {
                    _logger.LogWarning(
                        "Long running API request detected: {Method} {Path} took {Duration}ms",
                        context.Request.Method,
                        context.Request.Path,
                        duration);
                }
            }
        }
    }

    public class PerformanceMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<PerformanceMiddleware> _logger;
        private readonly ITaskExecutionLogService _logService;

        public PerformanceMiddleware(
            RequestDelegate next,
            ILogger<PerformanceMiddleware> logger,
            ITaskExecutionLogService logService)
        {
            _next = next;
            _logger = logger;
            _logService = logService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var sw = Stopwatch.StartNew();
            var correlationId = context.Items["CorrelationId"]?.ToString();

            try
            {
                await _next(context);
            }
            finally
            {
                sw.Stop();
                var duration = sw.ElapsedMilliseconds;

                // Log to our custom log table
                await _logService.LogAsync(
                    taskExecutionId: GetTaskExecutionId(context),
                    level: "Information",
                    message: $"Request {context.Request.Method} {context.Request.Path} completed in {duration}ms",
                    correlationId: correlationId,
                    data: new
                    {
                        Path = context.Request.Path.Value,
                        context.Request.Method,
                        context.Response.StatusCode,
                        Duration = duration
                    });

                if (duration > 1000) // 1 second threshold
                {
                    _logger.LogWarning(
                        "Long running request detected: {Method} {Path} took {Duration}ms",
                        context.Request.Method,
                        context.Request.Path,
                        duration);
                }
            }
        }

        private int GetTaskExecutionId(HttpContext context)
        {
            // Try to get TaskExecutionId from route or query string
            if (context.Request.RouteValues.TryGetValue("taskExecutionId", out var taskExecutionId))
            {
                return (int)taskExecutionId;
            }

            // If not found, return a default value or handle as needed
            return 0; // You might want to handle this differently
        }
    }
}