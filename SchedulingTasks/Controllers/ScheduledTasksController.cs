using Microsoft.AspNetCore.Mvc;
using SchedulingTasks.Interfaces;

namespace SchedulingTasks.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScheduledTasksController(IScheduledTaskService scheduledTaskService, ILogger<ScheduledTasksController> logger) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAllScheduledTasks()
        {
            var scheduledTasks = await scheduledTaskService.GetAllTasksAsync();

            logger.LogInformation("Fetched all scheduled tasks successfully.");

            return Ok(scheduledTasks);
        }
    }
}
