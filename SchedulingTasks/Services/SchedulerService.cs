using Cronos;
using SchedulingTasks.Interfaces;
using SchedulingTasks.Models;
using System.Diagnostics;

namespace SchedulingTasks.Services
{
    public class SchedulerService(IServiceScopeFactory serviceScopeFactory, ILogger<SchedulerService> logger) : BackgroundService
    {
        private const int DEFAULT_MAX_EXECUTION_MINUTES = 30;
        private const int MAX_DRIFT_MINUTES_WARNING = 5;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = serviceScopeFactory.CreateScope();
                var taskRepository = scope.ServiceProvider.GetRequiredService<IScheduledTaskService>();
                var taskExecutionService = scope.ServiceProvider.GetRequiredService<ITaskExecutionService>();
                var taskExecutionLogService = scope.ServiceProvider.GetRequiredService<ITaskExecutionLogService>();
                var correlationProvider = scope.ServiceProvider.GetRequiredService<ICorrelationIdProvider>();

                try
                {
                    await ProcessScheduledTasks(taskRepository, taskExecutionService, correlationProvider, stoppingToken);
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    logger.LogError(ex, "Error in scheduler");
                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                }
            }
        }

        private async Task ProcessScheduledTasks(
            IScheduledTaskService taskRepository,
            ITaskExecutionService taskExecutionService,
            ICorrelationIdProvider correlationProvider,
            CancellationToken stoppingToken)
        {
            var activeTasks = await  taskRepository.GetActiveTasksAsync(stoppingToken);


            // Order tasks by priority and next run time
            var orderedTasks = activeTasks
                .OrderBy(t => t.Priority)
                .ThenBy(t => GetNextRunTimeWithTimeZone(t, DateTime.UtcNow));

            var previousTaskEndTime = DateTime.UtcNow; // for test

            foreach (var task in orderedTasks)
            {
                var correlationId = correlationProvider.GetCorrelationId();
                using var logScope = logger.BeginScope(new Dictionary<string, object>
                {
                    ["CorrelationId"] = correlationId,
                    ["TaskId"] = task.Id,
                    ["TaskName"] = task.Name,
                    ["Priority"] = task.Priority
                });

                try
                {
                    var utcNow = DateTime.UtcNow;
                    var nextRun = GetNextRunTimeWithTimeZone(task, utcNow);

                    if (!nextRun.HasValue)
                    {
                        logger.LogWarning("Could not determine next run for task {TaskName}", task.Name);
                        continue;
                    }

                    var cumulativeDelay = nextRun.Value - previousTaskEndTime; // for test

                    var delay = nextRun.Value - utcNow;
                    if (delay <= TimeSpan.FromMinutes(1))
                    {
                        var drift = utcNow - nextRun.Value;

                        if (drift > TimeSpan.FromMinutes(MAX_DRIFT_MINUTES_WARNING))
                        {
                            logger.LogWarning($"Task {task.Name} started {drift.TotalMinutes} minutes later than scheduled");
                        }

                        // Create linked cancellation token with timeout
                        using var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                        var maxExecutionMinutes = task.MaxExecutionMinutes > 0 ? task.MaxExecutionMinutes : DEFAULT_MAX_EXECUTION_MINUTES;
                        cts.CancelAfter(TimeSpan.FromMinutes(maxExecutionMinutes));

                        logger.LogInformation($"Executing task {task.Name} (Priority: {task.Priority}). Scheduled for: {nextRun.Value}, Starting at: {utcNow}");

                        try
                        {
                            await ExecuteTaskWithRetries(task, taskExecutionService, correlationId, cts.Token);
                        }
                        catch (OperationCanceledException) when (!stoppingToken.IsCancellationRequested)
                        {
                            logger.LogWarning($"Task {task.Name} exceeded maximum execution time of {maxExecutionMinutes} minutes");
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing scheduled task {TaskName}", task.Name);
                }
            }
        }

        private async Task ExecuteTaskWithRetries(ScheduledTask task, ITaskExecutionService taskExecutionService, string correlationId, CancellationToken stoppingToken)
        {
            var execution = await taskExecutionService.CreateExecutionAsync(task.Id, correlationId, stoppingToken);
            var delay = TimeSpan.FromSeconds(task.RetryDelaySeconds);

            while (execution.AttemptNumber <= task.RetryAttempts)
            {
                using var logScope = logger.BeginScope(new Dictionary<string, object>
                {
                    ["CorrelationId"] = correlationId,
                    ["TaskId"] = task.Id,
                    ["TaskName"] = task.Name,
                    ["ExecutionId"] = execution.Id,
                    ["AttemptNumber"] = execution.AttemptNumber
                });

                try
                {
                    var taskTimeZone = TimeZoneInfo.FindSystemTimeZoneById(task.TimeZoneId ?? "UTC");
                    var nextRunLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, taskTimeZone);

                    logger.LogInformation($"Starting task execution {task.Name} (Attempt: {execution.AttemptNumber}/{task.RetryAttempts}) at {nextRunLocal}");

                    await taskExecutionService.ExecuteTaskAsync(task, execution, correlationId, stoppingToken);
                    return; // Success
                }
                catch (Exception ex) when (execution.AttemptNumber < task.RetryAttempts && task.RetryIfFailed)
                {
                    execution.Status = "Retrying";
                    execution.AttemptNumber++;

                    await taskExecutionService.UpdateExecutionAsync(execution, correlationId, stoppingToken);

                    logger.LogWarning(ex, $"Task {task.Name} failed on attempt {execution.AttemptNumber} of {task.RetryAttempts}. Retrying after {delay.TotalSeconds} seconds");

                    try
                    {
                        await Task.Delay(delay, stoppingToken);
                    }
                    catch (OperationCanceledException)
                    {
                        logger.LogInformation(
                            "Retry delayed cancelled for task {TaskName} after attempt {Attempt}",
                            task.Name,
                            execution.AttemptNumber);
                        throw;
                    }

                    delay *= 2;
                }
                catch (Exception ex)
                {
                    // Last attempt or RetryIfFailed is false
                    execution.Status = "Failed";
                    execution.ErrorMessage = ex.Message;
                    execution.CompletedAt = DateTime.UtcNow;

                    await taskExecutionService.UpdateExecutionAsync(execution, correlationId, stoppingToken);

                    logger.LogError(ex, $"Task {task.Name} failed on final attempt {execution.AttemptNumber}. No more retries.");

                    throw;
                }
            }
        }

        private DateTime? GetNextRunTimeWithTimeZone(ScheduledTask task, DateTime utcNow)
        {
            try
            {
                var cronExpression = CronExpression.Parse(task.CronExpression);
                var timeZone = TimeZoneInfo.FindSystemTimeZoneById(task.TimeZoneId ?? "UTC");
                return cronExpression.GetNextOccurrence(utcNow, timeZone);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error calculating next run time for task {task.Name}. Invalid cron expression: {task.CronExpression}");
                return null;
            }
        }
    }
}
