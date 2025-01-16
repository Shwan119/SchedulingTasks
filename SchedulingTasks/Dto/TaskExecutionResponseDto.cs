using SchedulingTasks.Models;

namespace SchedulingTasks.Dto
{
    public class TaskExecutionResponseDto
    {
        public int Id { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string Status { get; set; }
        public string ErrorMessage { get; set; }
        public int AttemptNumber { get; set; }
        public int HttpStatusCode { get; set; }
        public long ExecutionTimeMs { get; set; }

        public int ScheduledTaskId { get; set; }
        public string ScheduledTaskName { get; set; }
    }
}
