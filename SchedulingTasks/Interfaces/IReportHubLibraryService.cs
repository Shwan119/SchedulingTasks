using SchedulingTasks.Models;

namespace SchedulingTasks.Interfaces
{
    public interface IReportHubLibraryService
    {
        Task GetPendingAttestationsAsync();
        Task ProcessRemindersAsync();
        Task GenerateScheduledReportsAsync();
    }
}
