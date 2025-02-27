namespace SchedulingTasks.Controllers
{
    public class testing
    {
    }

    using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourNamespace.Common
    {
        public class InactiveSubscriptionWithManager
        {
            public int SubscriptionID { get; set; }
            public int UserID { get; set; }
            public int ReportID { get; set; }
            public string ReportName { get; set; }
            public string Code { get; set; }
            public DateTime LastDateUsed { get; set; }
            public string UserEmail { get; set; }
            public string ManagerEmail { get; set; }
            public int? ManagerID { get; set; }
        }

        public class UserSubscriptionInfo
        {
            public int UserID { get; set; }
            public string UserEmail { get; set; }
            public string ManagerEmail { get; set; }
            public List<string> Reports { get; set; }
        }

        public interface ILogger
        {
            void Info(string message);
            void Error(string message, Exception ex = null);
            void Debug(string message);
            void Warn(string message);
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

            /// <summary>
            /// Gets all inactive report subscriptions that need to be auto-cancelled
            /// </summary>
            public List<InactiveSubscriptionWithManager> GetInactiveSubscriptionsWithManagers()
            {
                try
                {
                    var unusedDays = int.Parse(ConfigurationManager.AppSettings["UnusedReportDaysThreshold"]);
                    var orgCode = ConfigurationManager.AppSettings["UnsubscribeOrgFilter"];

                    _logger.Info($"Fetching inactive subscriptions with threshold {unusedDays} days for org {orgCode}");

                    var results = _context.Database
                        .SqlQuery<InactiveSubscriptionWithManager>(@"
                        SELECT 
                            ra.ID as SubscriptionID, 
                            ra.Requestor_ID as UserID, 
                            ra.Report_ID as ReportID, 
                            ri.Name as ReportName, 
                            org.Code,
                            usage.LastDateUsed,
                            u.Email as UserEmail,
                            m.Email as ManagerEmail,
                            m.ID as ManagerID
                        FROM [dbo].ReportInventories ri (nolock)
                            inner join dbo.ReportingOrgs org (nolock) on org.ID = ri.ReportingOrg_ID
                            inner join [dbo].[ReportAccesses] as ra on ra.Report_ID = ri.ID
                            INNER JOIN [dbo].[vwUserPermission] up ON up.UserID = ra.Requestor_ID and up.Division_ID = org.Division_ID
                            INNER JOIN [dbo].[Users] u ON u.ID = ra.Requestor_ID
                            LEFT JOIN [dbo].[Users] m ON m.ID = u.ManagerID
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

                    _logger.Info($"Found {results.Count} inactive subscriptions");
                    return results;
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error retrieving inactive subscriptions: {ex.Message}", ex);
                    throw;
                }
            }

            /// <summary>
            /// Updates a specific batch of subscriptions and returns notification info
            /// </summary>
            public async Task<Dictionary<int, UserSubscriptionInfo>> UpdateInactiveSubscriptionsBatch(
                List<InactiveSubscriptionWithManager> subscriptionBatch)
            {
                if (subscriptionBatch == null || !subscriptionBatch.Any())
                {
                    return new Dictionary<int, UserSubscriptionInfo>();
                }

                // Track affected users for notification
                var affectedUsers = new Dictionary<int, UserSubscriptionInfo>();

                try
                {
                    var subscriptionIds = subscriptionBatch.Select(s => s.SubscriptionID).ToList();

                    _logger.Debug($"Updating batch of {subscriptionIds.Count} subscriptions");

                    var subscriptionsToUpdate = await _context.ReportAccesses
                        .Where(ra => subscriptionIds.Contains(ra.ID))
                        .ToListAsync();

                    foreach (var subscription in subscriptionsToUpdate)
                    {
                        subscription.RequestStatus = "AutoCancelled";
                        subscription.ModifiedDate = DateTime.Now;
                        subscription.CancelDate = DateTime.Now;

                        // Track for notification
                        var reportInfo = subscriptionBatch.FirstOrDefault(b => b.SubscriptionID == subscription.ID);
                        if (reportInfo != null)
                        {
                            if (!affectedUsers.ContainsKey(subscription.Requestor_ID))
                            {
                                affectedUsers[subscription.Requestor_ID] = new UserSubscriptionInfo
                                {
                                    UserID = reportInfo.UserID,
                                    UserEmail = reportInfo.UserEmail,
                                    ManagerEmail = reportInfo.ManagerEmail,
                                    Reports = new List<string>()
                                };
                            }
                            affectedUsers[subscription.Requestor_ID].Reports.Add(reportInfo.ReportName);
                        }
                    }

                    var rowsAffected = await _context.SaveChangesAsync();
                    _logger.Info($"Updated batch of {rowsAffected} inactive subscriptions");
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error updating subscription batch: {ex.Message}", ex);
                    throw; // Re-throw to let caller handle the error
                }

                return affectedUsers;
            }
        }
    }

    namespace YourNamespace.JobService
    {
        public interface IEmailService
        {
            Task SendEmailAsync(string to, string subject, string body, List<string> cc = null);
        }

        public class UnsubscribeJob
        {
            private readonly ReportCleanupService _reportService;
            private readonly IEmailService _emailService;
            private readonly ILogger _logger;
            private readonly Dictionary<string, UserEmailInfo> _userReports;

            public UnsubscribeJob(YourDbContext context, IEmailService emailService, ILogger logger)
            {
                _reportService = new ReportCleanupService(context, logger);
                _emailService = emailService;
                _logger = logger;
                _userReports = new Dictionary<string, UserEmailInfo>();
            }

            public async Task Run()
            {
                try
                {
                    _logger.Info("Starting inactive report subscription cleanup job");

                    // Check if it's scheduled to run now
                    if (!IsScheduledTime())
                    {
                        _logger.Debug("Not scheduled to run at this time");
                        return;
                    }

                    // Step 1: Get inactive subscriptions
                    var inactiveSubscriptions = _reportService.GetInactiveSubscriptionsWithManagers();
                    if (!inactiveSubscriptions.Any())
                    {
                        _logger.Info("No inactive subscriptions found");
                        return;
                    }

                    // Step 2: Process each batch separately
                    const int batchSize = 500;
                    int totalProcessed = 0;

                    for (int i = 0; i < inactiveSubscriptions.Count; i += batchSize)
                    {
                        try
                        {
                            var batch = inactiveSubscriptions.Skip(i).Take(batchSize).ToList();

                            // Update this batch and get notification info
                            var batchNotificationInfo = await _reportService.UpdateInactiveSubscriptionsBatch(batch);

                            // Collect notification info (don't send yet)
                            CollectNotificationInfo(batchNotificationInfo);

                            totalProcessed += batch.Count;
                        }
                        catch (Exception ex)
                        {
                            _logger.Error($"Error processing batch: {ex.Message}", ex);
                            // Continue to next batch even if this one failed
                        }
                    }

                    // Step 3: Send consolidated emails
                    var emailsSent = await SendConsolidatedNotifications();

                    _logger.Info($"Job completed: processed {totalProcessed} subscriptions, sent {emailsSent} notifications");
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error in UnsubscribeJob: {ex.Message}", ex);
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
                        _logger.Warn("Missing schedule configuration for auto-unsubscribe job");
                        return true; // Default to run if config is missing
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
                    return true; // Default to run if there's an error
                }
            }

            private void CollectNotificationInfo(Dictionary<int, UserSubscriptionInfo> batchNotificationInfo)
            {
                foreach (var userEntry in batchNotificationInfo)
                {
                    var userInfo = userEntry.Value;

                    if (string.IsNullOrEmpty(userInfo.UserEmail))
                    {
                        _logger.Error($"Cannot collect notification - email address is missing for user {userInfo.UserID}");
                        continue;
                    }

                    // Use email as the key to ensure unique entries per email address
                    if (!_userReports.ContainsKey(userInfo.UserEmail))
                    {
                        _userReports[userInfo.UserEmail] = new UserEmailInfo
                        {
                            UserID = userInfo.UserID,
                            UserEmail = userInfo.UserEmail,
                            ManagerEmail = userInfo.ManagerEmail,
                            Reports = new List<string>()
                        };
                    }

                    // Add all reports for this user
                    _userReports[userInfo.UserEmail].Reports.AddRange(userInfo.Reports);
                }
            }

            private async Task<int> SendConsolidatedNotifications()
            {
                int emailsSent = 0;

                // Send one email per user with all their reports
                foreach (var userEmail in _userReports.Keys)
                {
                    try
                    {
                        var info = _userReports[userEmail];

                        // Remove any duplicate report names
                        var uniqueReports = info.Reports.Distinct().ToList();

                        var emailBody = BuildEmailBody(uniqueReports);

                        var ccList = new List<string>();
                        if (!string.IsNullOrEmpty(info.ManagerEmail))
                        {
                            ccList.Add(info.ManagerEmail);
                        }

                        await _emailService.SendEmailAsync(
                            info.UserEmail,
                            "Your Report Subscriptions Have Been Auto-Cancelled",
                            emailBody,
                            ccList
                        );

                        emailsSent++;
                        _logger.Info($"Sent notification to {info.UserEmail} for {uniqueReports.Count} reports");
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Error sending email to {userEmail}: {ex.Message}", ex);
                    }
                }

                return emailsSent;
            }

            private string BuildEmailBody(List<string> reportNames)
            {
                var sb = new StringBuilder();
                sb.AppendLine("Dear User,");
                sb.AppendLine();
                sb.AppendLine("The following report subscriptions have been automatically cancelled due to inactivity (no usage in the past 90 days):");
                sb.AppendLine();

                foreach (var reportName in reportNames)
                {
                    sb.AppendLine($"- {reportName}");
                }

                sb.AppendLine();
                sb.AppendLine("If you would like to re-subscribe to any of these reports, please visit the report portal.");
                sb.AppendLine();
                sb.AppendLine("Thank you,");
                sb.AppendLine("The Reporting Team");

                return sb.ToString();
            }

            // Helper class for email grouping
            private class UserEmailInfo
            {
                public int UserID { get; set; }
                public string UserEmail { get; set; }
                public string ManagerEmail { get; set; }
                public List<string> Reports { get; set; }
            }
        }
    }

    namespace YourNamespace.SchedulerIntegration
    {
        public class ScheduledTaskManager
        {
            private readonly YourDbContext _context;
            private readonly IEmailService _emailService;
            private readonly ILogger _logger;

            public ScheduledTaskManager(YourDbContext context, IEmailService emailService, ILogger logger)
            {
                _context = context;
                _emailService = emailService;
                _logger = logger;
            }

            // Method to be called by your existing minute-by-minute scheduler
            public async Task CheckAndRunTasks()
            {
                try
                {
                    var unsubscribeJob = new UnsubscribeJob(_context, _emailService, _logger);
                    await unsubscribeJob.Run();

                    // Other scheduled tasks can be added here
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error in scheduled tasks: {ex.Message}", ex);
                }
            }
        }
    }
}
