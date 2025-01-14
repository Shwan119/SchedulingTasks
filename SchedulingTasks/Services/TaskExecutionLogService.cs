using SchedulingTasks.Data;
using SchedulingTasks.Interfaces;
using SchedulingTasks.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Text.Json;

namespace SchedulingTasks.Services
{
    public class TaskExecutionLogService(ApplicationDbContext dbContext, ILogger<TaskExecutionLogService> logger) : ITaskExecutionLogService
    {
        public async Task LogAsync(int taskExecutionId, string level, string message, string correlationId = null, object data = null, CancellationToken cancellationToken = default)
        {
            var log = new TaskExecutionLog
            {
                TaskExecutionId = taskExecutionId,
                Timestamp = DateTime.UtcNow,
                Level = level,
                Message = message,
                CorrelationId = correlationId,
                Data = data != null ? JsonSerializer.Serialize(data) : null
            };

            dbContext.TaskExecutionLogs.Add(log);
            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation($"Added log entry {log.Id} for execution {taskExecutionId}: {message}. CorrelationId: {correlationId}");
        }

        public async Task<IEnumerable<TaskExecutionLog>> GetLogsForExecutionAsync(int taskExecutionId, CancellationToken cancellationToken = default)
        {
            return await dbContext.TaskExecutionLogs
                .Where(x => x.TaskExecutionId == taskExecutionId)
                .OrderBy(x => x.Timestamp)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<TaskExecutionLog>> GetRecentLogsAsync(int count = 100, CancellationToken cancellationToken = default)
        {
            return await dbContext.TaskExecutionLogs
                .OrderByDescending(x => x.Timestamp)
                .Take(count)
                .ToListAsync(cancellationToken);
        }
    }
}
