using SchedulingTasks.Interfaces;
using SchedulingTasks.Models;

namespace SchedulingTasks.Services
{
    public class ReportHubLibraryService() : IReportHubLibraryService
    {
        public Task GetPendingAttestationsAsync()
        {
            return Task.CompletedTask;
        }

        public Task ProcessRemindersAsync()
        {
            return Task.CompletedTask;
        }

        public Task GenerateScheduledReportsAsync()
        {
            return Task.CompletedTask;
        }
    }
}
