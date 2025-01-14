using Microsoft.AspNetCore.Mvc;
using SchedulingTasks.Models;
using System.Diagnostics;

namespace SchedulingTasks.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var scheduledTask = new List<ScheduledTask>()
            {
                new() {
                    Id = 1,
                    CronExpression = "* * * * *",
                    EndpointId = 1,
                    IsEnabled = true,
                    Name = "Test",
                    Status = ""
                }
            };

            return View(scheduledTask);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
