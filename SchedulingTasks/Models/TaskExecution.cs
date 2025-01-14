namespace SchedulingTasks.Models
{
    public class TaskExecution
    {
        public int Id { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string Status { get; set; } // Scheduled, Running, Completed, Failed, Retrying
        public string ErrorMessage { get; set; }
        public int AttemptNumber { get; set; }
        public int HttpStatusCode { get; set; }
        public string ResponseBody { get; set; }
        public long ExecutionTimeMs { get; set; }
        public string CorrelationId { get; set; }


        // Navigation properties
        public int ScheduledTaskId { get; set; }
        public ScheduledTask ScheduledTask { get; set; }
        public ICollection<TaskExecutionLog> Logs { get; set; }
    }
}