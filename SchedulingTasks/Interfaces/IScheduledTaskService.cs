using SchedulingTasks.Dto;
using SchedulingTasks.Models;

namespace SchedulingTasks.Interfaces
{
    public interface IScheduledTaskService
    {
        Task<IEnumerable<ScheduledTaskResponseDto>> GetAllTasksAsync();
        Task<IEnumerable<ScheduledTask>> GetActiveTasksAsync(CancellationToken cancellationToken = default);
        Task<ScheduledTask> GetTaskByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<ScheduledTask> UpdateTaskAsync(ScheduledTask task, CancellationToken cancellationToken = default);

        Task<ScheduledTask> CreateTaskAsync(ScheduledTask task);
        Task<bool> DeleteTaskAsync(int id);
        Task<bool> ToggleTaskAsync(int id);
    }
}
