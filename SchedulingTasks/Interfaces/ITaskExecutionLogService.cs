using SchedulingTasks.Models;

namespace SchedulingTasks.Interfaces
{
    public interface ITaskExecutionLogService
    {
        Task LogAsync(int taskExecutionId, string level, string message, string correlationId = null, object data = null, CancellationToken cancellationToken = default);
        Task<IEnumerable<TaskExecutionLog>> GetLogsForExecutionAsync(int taskExecutionId, CancellationToken cancellationToken = default);
        Task<IEnumerable<TaskExecutionLog>> GetRecentLogsAsync(int count = 100, CancellationToken cancellationToken = default);
    }
}
