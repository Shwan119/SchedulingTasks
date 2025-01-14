namespace SchedulingTasks.Models
{
    public class TaskExecutionLog
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string Level { get; set; } // Info, Warning, Error
        public string Message { get; set; }
        public string Data { get; set; } // JSON data
        public string CorrelationId { get; set; }


        // Navigation properties
        public int TaskExecutionId { get; set; }
        public TaskExecution TaskExecution { get; set; }
    }
}