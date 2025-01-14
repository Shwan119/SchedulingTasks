using SchedulingTasks.Interfaces;
using SchedulingTasks.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using SchedulingTasks.Data;
using SchedulingTasks.Exceptions;
using SchedulingTasks.Dto;

namespace SchedulingTasks.Services
{
    public class ScheduledTaskService(ApplicationDbContext dbContext, ILogger<ScheduledTaskService> logger) : IScheduledTaskService
    {
        private readonly Cronos.CronExpression _cronParser;

        public async Task<IEnumerable<ScheduledTaskResponseDto>> GetAllTasksAsync()
        {
            try
            {
                var scheduledTasks = await dbContext.ScheduledTasks
                    .AsNoTracking()
                    .Select(task => new ScheduledTaskResponseDto
                    {
                        Id = task.Id,
                        Name = task.Name,
                        Description = task.Description,
                        CronExpression = task.CronExpression,
                        LastRun = task.LastRun,
                        IsEnabled = task.IsEnabled,
                        Status = task.Status,
                        RetryAttempts = task.RetryAttempts,
                        RetryDelaySeconds = task.RetryDelaySeconds,
                        RetryIfFailed = task.RetryIfFailed,
                        TimeoutSeconds = task.TimeoutSeconds,
                        TimeZoneId = task.TimeZoneId,
                        Priority = task.Priority,
                        MaxExecutionMinutes = task.MaxExecutionMinutes,
                        NotificationEmail = task.NotificationEmail,
                        EndpointName = task.Endpoint.Name
                    })
                    .ToListAsync();

                return scheduledTasks;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving all tasks");
                throw;
            }
        }

        public async Task<IEnumerable<ScheduledTask>> GetActiveTasksAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await dbContext.ScheduledTasks
                .Include(s => s.Endpoint)
                .Where(t => t.IsEnabled)
                .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving all active tasks");
                throw;
            }
        }

        public async Task<ScheduledTask> GetTaskByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var task = await dbContext.ScheduledTasks
                    .Include(s => s.Endpoint)
                    .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

                if (task == null)
                    throw new NotFoundException($"Task with ID {id} not found");

                return task;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving task {TaskId}", id);
                throw;
            }
        }

        public async Task<ScheduledTask> CreateTaskAsync(ScheduledTask task)
        {
            try
            {
                // Validate cron expression
                if (!IsValidCronExpression(task.CronExpression))
                    throw new ValidationException("Invalid cron expression");

                // Calculate next run

                dbContext.ScheduledTasks.Add(task);
                await dbContext.SaveChangesAsync();

                return task;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating task");
                throw;
            }
        }

        public async Task<ScheduledTask> UpdateTaskAsync(ScheduledTask task, CancellationToken cancellationToken = default)
        {
            try
            {
                var existingTask = await dbContext.ScheduledTasks.FindAsync([task.Id, cancellationToken], cancellationToken: cancellationToken);

                if (existingTask == null)
                    throw new NotFoundException($"Task with ID {task.Id} not found");

                // Validate cron expression
                if (!IsValidCronExpression(task.CronExpression))
                    throw new ValidationException("Invalid cron expression");

                dbContext.Entry(existingTask).CurrentValues.SetValues(task);
                await dbContext.SaveChangesAsync(cancellationToken);

                return existingTask;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating task {TaskId}", task.Id);
                throw;
            }
        }

        public async Task<bool> DeleteTaskAsync(int id)
        {
            try
            {
                var task = await dbContext.ScheduledTasks.FindAsync(id);
                if (task == null) return false;

                dbContext.ScheduledTasks.Remove(task);
                await dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting task {TaskId}", id);
                throw;
            }
        }

        public async Task<bool> ToggleTaskAsync(int id)
        {
            try
            {
                var task = await dbContext.ScheduledTasks.FindAsync(id);
                if (task == null) return false;

                task.IsEnabled = !task.IsEnabled;
                await dbContext.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error toggling task {TaskId}", id);
                throw;
            }
        }

        private bool IsValidCronExpression(string cronExpression)
        {
            try
            {
                Cronos.CronExpression.Parse(cronExpression);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }    
}