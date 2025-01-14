using SchedulingTasks.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchedulingTasks.Dto
{
    public class ScheduledTaskResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CronExpression { get; set; }
        public DateTime? LastRun { get; set; }
        public bool IsEnabled { get; set; }
        public string Status { get; set; }
        public int RetryAttempts { get; set; }
        public int RetryDelaySeconds { get; set; }
        public bool RetryIfFailed { get; set; }
        public int TimeoutSeconds { get; set; }
        public string TimeZoneId { get; set; }
        public int Priority { get; set; }
        public int MaxExecutionMinutes { get; set; }
        public string NotificationEmail { get; set; }

        // Navigation property
        public string EndpointName { get; set; }
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

        public string GetStatusClass()
        {
            return Status switch
            {
                "Completed" => "bg-green-100 text-green-800",
                "Running" => "bg-yellow-100 text-yellow-800",
                "Failed" => "bg-red-100 text-red-800",
                _ => "bg-gray-100 text-gray-800"
            };
        }
    }
}