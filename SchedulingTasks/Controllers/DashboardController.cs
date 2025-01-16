using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using SchedulingTasks.Dto;
using SchedulingTasks.Interfaces;

namespace SchedulingTasks.Controllers
{
    public class DashboardController(ITaskExecutionService taskExecutionService) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var recentExecutions = await taskExecutionService.GetRecentExecutionsAsync();

            var stats = new DashboardStats
            {
                TotalTasks = recentExecutions.Count(),
                ActiveTasks = 8,
                SuccessRate = 95.5,
                FailedTasks = 2,
                NextScheduled = "15:00"
            };


            var distribution = new StatusDistribution
            {
                Completed = 75,
                Failed = 15,
                Pending = 10
            };

            var dashboardViewModel = new DashboardViewModel
            {
                Stats = stats,
                RecentExecutions = recentExecutions,
                Distribution = distribution
            };

            return View(dashboardViewModel);
        }
    }

    // View Model Classes
    public class DashboardViewModel
    {
        public DashboardStats Stats { get; set; }
        public IEnumerable<TaskExecutionResponseDto> RecentExecutions { get; set; }
        public StatusDistribution Distribution { get; set; }
    }

    public class DashboardStats
    {
        public int TotalTasks { get; set; }
        public int ActiveTasks { get; set; }
        public double SuccessRate { get; set; }
        public int FailedTasks { get; set; }
        public string NextScheduled { get; set; }
    }

    public class RecentExecution
    {
        public int Id { get; set; }
        public string TaskName { get; set; }
        public string StartedAt { get; set; }
        public string Duration { get; set; }
        public string Status { get; set; }
    }

    public class StatusDistribution
    {
        public int Completed { get; set; }
        public int Failed { get; set; }
        public int Pending { get; set; }
    }

    public static class HtmlHelpers
    {
        public static IHtmlContent RenderStatusBar(string label, int percentage, string barColor)
        {
            return new HtmlString($@"
            <div class='flex-1'>
                <div class='flex justify-between mb-1'>
                    <span class='text-sm font-medium text-gray-600'>{label}</span>
                    <span class='text-sm font-medium text-gray-600'>{percentage}%</span>
                </div>
                <div class='w-full bg-gray-200 rounded-full h-2'>
                    <div class='{barColor} h-2 rounded-full' style='width: {percentage}%;'></div>
                </div>
            </div>
        ");
        }
    }

}
