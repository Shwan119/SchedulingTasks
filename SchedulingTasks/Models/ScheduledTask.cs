namespace SchedulingTasks.Models
{
    public class ScheduledTask : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string CronExpression { get; set; }
        public DateTime? LastRun { get; set; }
        public bool IsEnabled { get; set; } = true;
        public string Status { get; set; } = "Pending";
        public int RetryAttempts { get; set; } = 3;
        public int RetryDelaySeconds { get; set; } = 500;
        public bool RetryIfFailed { get; set; } = true;
        public int TimeoutSeconds { get; set; } = 600;
        public string TimeZoneId { get; set; } = "UTC";  // Default to UTC
        public int Priority { get; set; } // 1 = High, 2 = Normal, 3 = Low
        public int MaxExecutionMinutes { get; set; }
        public string NotificationEmail { get; set; }


        // Navigation properties
        public int EndpointId { get; set; }
        public Endpoint Endpoint { get; set; }

        public ICollection<TaskExecution> Executions { get; set; }

        public DateTime? NextRun
        {
            get
            {
                try
                {
                    var expression = Cronos.CronExpression.Parse(CronExpression);
                    var timeZone = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);
                    var currentTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
                    var nextOccurrence = expression.GetNextOccurrence(currentTime, timeZone);

                    return nextOccurrence.HasValue
                        ? TimeZoneInfo.ConvertTimeToUtc(nextOccurrence.Value, timeZone)
                        : null;
                }
                catch
                {
                    return null;
                }
            }
        }       
    }
}