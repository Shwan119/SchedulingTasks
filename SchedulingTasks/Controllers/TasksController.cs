using Microsoft.AspNetCore.Mvc;
using SchedulingTasks.Interfaces;

namespace SchedulingTasks.Controllers
{
    public class TasksController(IScheduledTaskService scheduledTaskService, IEndpointService endpointService, ILogger<ScheduledTasksController> logger) : Controller
    {
        public async Task<IActionResult> IndexAsync()
        {
            var tasks = await scheduledTaskService.GetAllTasksAsync();
            var endpoints = await endpointService.GetAllEndpointsAsync();

            ViewBag.Endpoints = endpoints;
            return View(tasks);
        }
    }
}
