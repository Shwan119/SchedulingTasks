using SchedulingTasks.Controllers.YourNamespace.Common;
using SchedulingTasks.Controllers.YourNamespace.JobService;
using System.Text;

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

        public class UserSubscriptionsGroup
        {
            public int UserID { get; set; }
            public string UserEmail { get; set; }
            public string ManagerEmail { get; set; }
            public List<int> SubscriptionIDs { get; set; }
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
                    var queryTimeoutSeconds = int.Parse(ConfigurationManager.AppSettings["QueryTimeoutSeconds"] ?? "60");

                    _logger.Info($"Fetching inactive subscriptions with threshold {unusedDays} days for org {orgCode}");

                    // Create the command with timeout
                    var sql = @"
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
                    ORDER BY usage.LastDateUsed";

                    var parameters = new[] {
                    new SqlParameter("@unusedDays", unusedDays),
                    new SqlParameter("@orgCode", orgCode)
                };

                    // Create and configure the command
                    var command = _context.Database.Connection.CreateCommand();
                    command.CommandText = sql;
                    command.CommandTimeout = queryTimeoutSeconds;
                    foreach (var parameter in parameters)
                    {
                        command.Parameters.Add(parameter);
                    }

                    // Execute with timeout
                    _context.Database.Connection.Open();
                    try
                    {
                        var results = new List<InactiveSubscriptionWithManager>();
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                results.Add(new InactiveSubscriptionWithManager
                                {
                                    SubscriptionID = reader.GetInt32(reader.GetOrdinal("SubscriptionID")),
                                    UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                                    ReportID = reader.GetInt32(reader.GetOrdinal("ReportID")),
                                    ReportName = reader.GetString(reader.GetOrdinal("ReportName")),
                                    Code = reader.GetString(reader.GetOrdinal("Code")),
                                    LastDateUsed = reader.GetDateTime(reader.GetOrdinal("LastDateUsed")),
                                    UserEmail = reader.IsDBNull(reader.GetOrdinal("UserEmail")) ? null : reader.GetString(reader.GetOrdinal("UserEmail")),
                                    ManagerEmail = reader.IsDBNull(reader.GetOrdinal("ManagerEmail")) ? null : reader.GetString(reader.GetOrdinal("ManagerEmail")),
                                    ManagerID = reader.IsDBNull(reader.GetOrdinal("ManagerID")) ? null : (int?)reader.GetInt32(reader.GetOrdinal("ManagerID"))
                                });
                            }
                        }

                        _logger.Info($"Found {results.Count} inactive subscriptions");
                        return results;
                    }
                    finally
                    {
                        _context.Database.Connection.Close();
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error retrieving inactive subscriptions: {ex.Message}", ex);
                    throw;
                }
            }

            /// <summary>
            /// Updates subscriptions for a user
            /// </summary>
            public async Task<int> UpdateUserSubscriptions(List<int> subscriptionIds)
            {
                if (subscriptionIds == null || !subscriptionIds.Any())
                {
                    return 0;
                }

                try
                {
                    _logger.Debug($"Updating {subscriptionIds.Count} subscriptions");

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
                    _logger.Debug($"Updated {rowsAffected} subscriptions");
                    return rowsAffected;
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error updating subscriptions: {ex.Message}", ex);
                    throw;
                }
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

            public UnsubscribeJob(YourDbContext context, IEmailService emailService, ILogger logger)
            {
                _reportService = new ReportCleanupService(context, logger);
                _emailService = emailService;
                _logger = logger;
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

                    // Step 2: Group subscriptions by user
                    var userSubscriptions = GroupSubscriptionsByUser(inactiveSubscriptions);
                    _logger.Info($"Found inactive subscriptions for {userSubscriptions.Count} users");

                    // Step 3: Update subscriptions by user and send notifications
                    int totalUpdated = 0;
                    int totalNotified = 0;

                    foreach (var userGroup in userSubscriptions)
                    {
                        var userEmail = userGroup.Key;
                        var userInfo = userGroup.Value;

                        try
                        {
                            // Update this user's subscriptions
                            var updatedCount = await _reportService.UpdateUserSubscriptions(userInfo.SubscriptionIDs);
                            totalUpdated += updatedCount;

                            // If successfully updated, send notification
                            if (updatedCount > 0)
                            {
                                await SendUserNotification(userInfo);
                                totalNotified++;
                                _logger.Info($"Updated and notified user {userEmail} for {userInfo.Reports.Count} reports");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.Error($"Error processing user {userEmail}: {ex.Message}", ex);
                            // Continue to next user even if this one failed
                        }
                    }

                    _logger.Info($"Job completed: updated {totalUpdated} subscriptions and notified {totalNotified} users");
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

            private Dictionary<string, UserSubscriptionsGroup> GroupSubscriptionsByUser(
                List<InactiveSubscriptionWithManager> subscriptions)
            {
                // Filter out subscriptions with missing email
                var validSubscriptions = subscriptions
                    .Where(s => !string.IsNullOrEmpty(s.UserEmail))
                    .ToList();

                // Log any missing emails
                var missingEmails = subscriptions.Count - validSubscriptions.Count;
                if (missingEmails > 0)
                {
                    _logger.Warn($"Found {missingEmails} subscriptions with missing email addresses");
                }

                // Group by email address
                return validSubscriptions
                    .GroupBy(s => s.UserEmail)
                    .ToDictionary(
                        g => g.Key,
                        g => new UserSubscriptionsGroup
                        {
                            UserID = g.First().UserID,
                            UserEmail = g.Key,
                            ManagerEmail = g.First().ManagerEmail,
                            SubscriptionIDs = g.Select(s => s.SubscriptionID).ToList(),
                            Reports = g.Select(s => s.ReportName).Distinct().ToList()
                        }
                    );
            }

            private async Task SendUserNotification(UserSubscriptionsGroup userInfo)
            {
                try
                {
                    var emailBody = BuildEmailBody(userInfo.Reports);

                    var ccList = new List<string>();
                    if (!string.IsNullOrEmpty(userInfo.ManagerEmail))
                    {
                        ccList.Add(userInfo.ManagerEmail);
                    }

                    await _emailService.SendEmailAsync(
                        userInfo.UserEmail,
                        "Your Report Subscriptions Have Been Auto-Cancelled",
                        emailBody,
                        ccList
                    );

                    _logger.Info($"Sent notification to {userInfo.UserEmail} for {userInfo.Reports.Count} reports");
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error sending email to {userInfo.UserEmail}: {ex.Message}", ex);
                    throw;
                }
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
