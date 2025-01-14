using SchedulingTasks.Models;
using System.Text.Json;

namespace SchedulingTasks.Interfaces
{
    public interface ITaskExecutionService
    {
        Task ExecuteTaskAsync(ScheduledTask task, TaskExecution execution, string correlationId, CancellationToken stoppingToken);
        Task<TaskExecution> CreateExecutionAsync(int scheduledTaskId, string correlationId = null, CancellationToken cancellationToken = default);
        Task UpdateExecutionAsync(TaskExecution execution, string correlationId = null, CancellationToken cancellationToken = default);
        Task<TaskExecution> GetExecutionByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<TaskExecution>> GetExecutionsForTaskAsync(int scheduledTaskId, CancellationToken cancellationToken = default);
        Task<IEnumerable<TaskExecution>> GetRecentExecutionsAsync(int count = 100, CancellationToken cancellationToken = default);
    }
}