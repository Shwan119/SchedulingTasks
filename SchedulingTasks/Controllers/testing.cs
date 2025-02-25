namespace SchedulingTasks.Controllers
{
    public class testing
    {
    }

    <appSettings>
  <add key = "UnusedReportDaysThreshold" value="90"/>
  <add key = "UnsubscribeJobRunDays" value="1,2,3,4,5,6"/>
  <add key = "UnsubscribeJobRunHours" value="2,14"/>
  <add key = "UnsubscribeOrgFilter" value="ECOE"/>
</appSettings>

    using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace YourNamespace
    {
        public class InactiveSubscription
        {
            public int SubscriptionID { get; set; }
            public int UserID { get; set; }
            public int ReportID { get; set; }
            public string ReportName { get; set; }
            public string Code { get; set; }
            public DateTime LastDateUsed { get; set; }
        }

        public interface ILogger
        {
            void Info(string message);
            void Error(string message, Exception ex = null);
        }

        public class ReportCleanupService
        {
            private readonly YourDbContext _context;
            private readonly ILogger _logger;

            public ReportCleanupService(YourDbContext context, ILogger logger)
            {
                _context = context;
                _logger = logger;
            }

            public async Task UpdateInactiveSubscriptions()
            {
                try
                {
                    // Step 1: Fetch inactive subscriptions
                    var inactiveSubscriptions = GetInactiveSubscriptions();
                    if (!inactiveSubscriptions.Any())
                    {
                        _logger.Info("No inactive subscriptions found to update");
                        return;
                    }

                    _logger.Info($"Found {inactiveSubscriptions.Count} inactive subscriptions to auto-cancel");

                    // Step 2: Batch update them
                    const int batchSize = 500;
                    int totalUpdated = 0;

                    for (int i = 0; i < inactiveSubscriptions.Count; i += batchSize)
                    {
                        var batch = inactiveSubscriptions.Skip(i).Take(batchSize).ToList();
                        var subscriptionIds = batch.Select(s => s.SubscriptionID).ToList();

                        // Fetch actual entities for update
                        var subscriptionsToUpdate = await _context.ReportAccesses
                            .Where(ra => subscriptionIds.Contains(ra.ID))
                            .ToListAsync();

                        foreach (var subscription in subscriptionsToUpdate)
                        {
                            subscription.RequestStatus = "AutoCancelled";
                            subscription.ModifiedDate = DateTime.Now;
                            subscription.CancelDate = DateTime.Now;
                        }

                        var rowsAffected = await _context.SaveChangesAsync();
                        totalUpdated += rowsAffected;

                        _logger.Info($"Updated batch of {rowsAffected} inactive subscriptions");
                    }

                    _logger.Info($"Total inactive subscriptions auto-cancelled: {totalUpdated}");
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error cancelling inactive subscriptions: {ex.Message}", ex);
                    throw;
                }
            }

            public List<InactiveSubscription> GetInactiveSubscriptions()
            {
                var unusedDays = int.Parse(ConfigurationManager.AppSettings["UnusedReportDaysThreshold"]);
                var orgCode = ConfigurationManager.AppSettings["UnsubscribeOrgFilter"];

                return _context.Database
                    .SqlQuery<InactiveSubscription>(@"
                    SELECT ra.ID as SubscriptionID, ra.Requestor_ID as UserID, ra.Report_ID as ReportID, ri.Name as ReportName, org.Code
                    , usage.LastDateUsed
                    FROM [dbo].ReportInventories ri (nolock)
                        inner join dbo.ReportingOrgs org (nolock) on org.ID = ri.ReportingOrg_ID
                        inner join [dbo].[ReportAccesses] as ra on ra.Report_ID = ri.ID
                        INNER JOIN [dbo].[vwUserPermission] up ON up.UserID = ra.Requestor_ID and up.Division_ID = org.Division_ID
                    OUTER APPLY (
                        SELECT max(LogDate) as LastDateUsed 
                        FROM [dbo].[vwReportUsage] usg
                        WHERE usg.RID = ri.ID AND usg.UserID = ra.Requestor_ID
                    ) usage
                    WHERE org.Code = @orgCode
                    AND ra.RequestStatus = 'Approved'
                    AND up.IsAnalyst = 0
                    AND up.IsAdmin = 0
                    AND usage.LastDateUsed < GETDATE() - @unusedDays
                    ORDER BY usage.LastDateUsed",
                        new SqlParameter("@unusedDays", unusedDays),
                        new SqlParameter("@orgCode", orgCode))
                    .ToList();
            }
        }

        public class JobScheduler
        {
            private readonly ReportCleanupService _cleanupService;
            private readonly ILogger _logger;

            public JobScheduler(ReportCleanupService cleanupService, ILogger logger)
            {
                _cleanupService = cleanupService;
                _logger = logger;
            }

            public async Task ExecuteScheduledJobs()
            {
                try
                {
                    if (IsScheduledTime())
                    {
                        _logger.Info("Starting auto-unsubscribe job for inactive reports");
                        await _cleanupService.UpdateInactiveSubscriptions();
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error("Error executing scheduled job", ex);
                }
            }

            private bool IsScheduledTime()
            {
                try
                {
                    var scheduleDaysConfig = ConfigurationManager.AppSettings["UnsubscribeJobRunDays"];
                    var scheduleHoursConfig = ConfigurationManager.AppSettings["UnsubscribeJobRunHours"];

                    if (string.IsNullOrEmpty(scheduleDaysConfig) || string.IsNullOrEmpty(scheduleHoursConfig))
                    {
                        _logger.Error("Missing schedule configuration for auto-unsubscribe job");
                        return false;
                    }

                    var scheduleDays = scheduleDaysConfig
                        .Split(',')
                        .Select(int.Parse)
                        .ToList();

                    var scheduleHours = scheduleHoursConfig
                        .Split(',')
                        .Select(int.Parse)
                        .ToList();

                    var now = DateTime.Now;
                    return scheduleDays.Contains((int)now.DayOfWeek) &&
                           scheduleHours.Contains(now.Hour);
                }
                catch (Exception ex)
                {
                    _logger.Error("Error checking scheduled time", ex);
                    return false;
                }
            }
        }

        // Example main class for integration with your existing scheduler
        public class ScheduledTaskManager
        {
            private readonly YourDbContext _context;
            private readonly ILogger _logger;

            public ScheduledTaskManager(YourDbContext context, ILogger logger)
            {
                _context = context;
                _logger = logger;
            }

            // Method to be called by your existing minute-by-minute scheduler
            public async Task CheckAndRunTasks()
            {
                var cleanupService = new ReportCleanupService(_context, _logger);
                var scheduler = new JobScheduler(cleanupService, _logger);

                await scheduler.ExecuteScheduledJobs();

                // Other scheduled tasks can be added here
            }
        }
    }
}
