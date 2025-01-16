using Microsoft.AspNetCore.Mvc;

namespace SchedulingTasks.Controllers
{
    public class TaskExecutionsController : Controller
    {
        public ActionResult Index()
        {
            // Sample data. Replace with actual database call.
            var executions = new List<TaskExecutionViewModel>
        {
            new TaskExecutionViewModel { TaskId = 1, StartedAt = "2025-01-05 01:00:00", CompletedAt = "2025-01-05 01:05:00", Status = "Completed", AttemptNumber = 1, HttpStatusCode = 200, ErrorMessage = null },
            new TaskExecutionViewModel { TaskId = 2, StartedAt = "2025-01-05 00:00:00", CompletedAt = "2025-01-05 00:10:00", Status = "Completed", AttemptNumber = 1, HttpStatusCode = 200, ErrorMessage = null },
            new TaskExecutionViewModel { TaskId = 4, StartedAt = "2025-01-05 02:00:00", CompletedAt = null, Status = "Failed", AttemptNumber = 3, HttpStatusCode = 500, ErrorMessage = "API Timeout" },
        };

            return View(executions);
        }
    }

    public class TaskExecutionViewModel
    {
        public int TaskId { get; set; }
        public string StartedAt { get; set; }
        public string CompletedAt { get; set; }
        public string Status { get; set; }
        public int AttemptNumber { get; set; }
        public int HttpStatusCode { get; set; }
        public string ErrorMessage { get; set; }
    }
}
