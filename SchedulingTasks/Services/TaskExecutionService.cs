using SchedulingTasks.Data;
using SchedulingTasks.Interfaces;
using SchedulingTasks.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace SchedulingTasks.Services
{
    public class TaskExecutionService(ApplicationDbContext dbContext, IHttpClientFactory httpClientFactory, ILogger<TaskExecutionService> logger, ITaskExecutionLogService logService) : ITaskExecutionService
    {
        public async Task ExecuteTaskAsync(ScheduledTask task, TaskExecution execution, string correlationId, CancellationToken stoppingToken)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                using var client = httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(task.TimeoutSeconds);

                var request = new HttpRequestMessage(new HttpMethod(task.Endpoint.HttpMethod), task.Endpoint.FullUrl);
                request.Headers.Add("X-Correlation-ID", correlationId);

                logger.LogInformation($"Attempting to execute task: {task.Name} at endpoint: {$"{task.Endpoint.BaseUrl}{task.Endpoint.Path}"}");

                var response = await client.SendAsync(request, stoppingToken);

                logger.LogInformation($"Received response with status code {response.StatusCode}");

                stopwatch.Stop();
                execution.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
                execution.HttpStatusCode = (int)response.StatusCode;

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(stoppingToken);
                    execution.Status = "Completed";
                    execution.CompletedAt = DateTime.UtcNow;
                    execution.ResponseBody = content;

                    await logService.LogAsync(execution.Id, "Information", $"Task completed successfully with status code {response.StatusCode}", correlationId,
                        new
                        {
                            response.StatusCode,
                            ExecutionTime = stopwatch.ElapsedMilliseconds,
                            Content = content
                        },
                        stoppingToken);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync(stoppingToken);
                    execution.Status = "Failed";
                    execution.ErrorMessage = $"HTTP {response.StatusCode}: {errorContent}";

                    await logService.LogAsync(
                        execution.Id,
                        "Error",
                        $"Task failed with status code {response.StatusCode}",
                        correlationId,
                        new
                        {
                            response.StatusCode,
                            ErrorContent = errorContent
                        },
                        stoppingToken);

                    throw new HttpRequestException($"Task failed with status code {response.StatusCode}");  // will remove test both
                }

                await UpdateExecutionAsync(execution, correlationId, stoppingToken);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                execution.Status = "Failed";
                execution.CompletedAt = DateTime.UtcNow;
                execution.ErrorMessage = ex.Message;
                execution.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;

                await logService.LogAsync(execution.Id, "Error", "Task execution failed with exception", correlationId,
                    new
                    {
                        Error = ex.Message,
                        ex.StackTrace
                    },
                    stoppingToken);

                await UpdateExecutionAsync(execution, correlationId, stoppingToken);
                throw;
            }
            finally
            {
                await UpdateExecutionAsync(execution, correlationId, stoppingToken);
            }
        }

        public async Task<TaskExecution> CreateExecutionAsync(int scheduledTaskId, string correlationId = null, CancellationToken cancellationToken = default)
        {
            var execution = new TaskExecution
            {
                ScheduledTaskId = scheduledTaskId,
                StartedAt = DateTime.UtcNow,
                Status = "Running",
                AttemptNumber = 1,
                CorrelationId = correlationId
            };

            dbContext.TaskExecutions.Add(execution);
            await dbContext.SaveChangesAsync(cancellationToken);

            return execution;
        }

        public async Task UpdateExecutionAsync(TaskExecution execution, string correlationId = null, CancellationToken cancellationToken = default)
        {
            dbContext.TaskExecutions.Update(execution);
            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Updated task execution {ExecutionId} with status {Status}", execution.Id, execution.Status);
        }

        public async Task<TaskExecution> GetExecutionByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await dbContext.TaskExecutions.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<TaskExecution>> GetExecutionsForTaskAsync(int scheduledTaskId, CancellationToken cancellationToken = default)
        {
            return await dbContext.TaskExecutions
                .Where(x => x.ScheduledTaskId == scheduledTaskId)
                .OrderByDescending(x => x.StartedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<TaskExecution>> GetRecentExecutionsAsync(int count = 100, CancellationToken cancellationToken = default)
        {
            return await dbContext.TaskExecutions
                .OrderByDescending(x => x.StartedAt)
                .Take(count)
                .ToListAsync(cancellationToken);
        }
    }
}