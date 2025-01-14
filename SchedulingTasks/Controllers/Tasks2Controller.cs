using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SchedulingTasks.Interfaces;
using SchedulingTasks.Services;

namespace SchedulingTasks.Controllers
{
    public class Tasks2Controller(IScheduledTaskService scheduledTaskService, IEndpointService endpointService, ILogger<ScheduledTasksController> logger) : Controller
    {
        public async Task<IActionResult> IndexAsync()
        {
            var scheduledTasks = await scheduledTaskService.GetAllTasksAsync();
            logger.LogInformation("Fetched all scheduled tasks successfully.");

            var endpoints = await endpointService.GetAllEndpointsAsync();
            logger.LogInformation("Fetched all endpoints successfully.");

            ViewBag.Endpoints = endpoints;

            return View(scheduledTasks);
        }
    }
}
