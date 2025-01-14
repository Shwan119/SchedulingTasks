using SchedulingTasks.Interfaces;

namespace SchedulingTasks.Services
{
    public class CorrelationIdProvider(IHttpContextAccessor httpContextAccessor) : ICorrelationIdProvider
    {
        public string GetCorrelationId() => httpContextAccessor.HttpContext?.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();
    }
}