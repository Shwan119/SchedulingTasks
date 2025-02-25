using System.Configuration;

namespace SchedulingTasks.Controllers
{
    public class testing
    {
    }

    public class ScheduledTasks
    {
        private readonly YourDbContext _context;
        private readonly ILogger _logger;

        public ScheduledTasks(YourDbContext context, ILogger logger)
        {
            _context = context;
            _logger = logger;
        }

        // Call this method from your scheduled job that runs every minute
        public async Task RunScheduledTasksAsync()
        {
            try
            {
                var config = ReportCleanupConfig.Load();

                if (config.IsScheduledTime())
                {
                    _logger.Info("Starting inactive subscription cleanup job");
                    var cleanupService = new ReportCleanupService(_context, config, _logger);
                    await cleanupService.FindAndUpdateInactiveSubscriptionsAsync();
                }
            }
            catch (ConfigurationErrorsException ex)
            {
                _logger.Error($"Configuration error in scheduled tasks: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error in scheduled tasks: {ex.Message}", ex);
            }
        }
    }
    public class ReportCleanupService
    {
        private readonly YourDbContext _context;
        private readonly ReportCleanupConfig _config;
        private readonly ILogger _logger;

        public ReportCleanupService(YourDbContext context, ReportCleanupConfig config, ILogger logger)
        {
            _context = context;
            _config = config;
            _logger = logger;
        }

        public async Task FindAndUpdateInactiveSubscriptionsAsync()
        {
            try
            {
                var cutoffDate = DateTime.Now.AddDays(-_config.RetentionDays);

                // SQL Command to update all inactive subscriptions
                var updateCommand = @"
                UPDATE ra
                SET ra.RequestStatus = 'AutoCancelled',
                    ra.ModifiedDate = GETDATE(),
                    ra.CancelDate = GETDATE()
                FROM [ROVER].[dbo].[ReportAccesses] ra
                INNER JOIN [dbo].[ReportInventories] ri ON ra.Report_ID = ri.ID
                INNER JOIN [dbo].[ReportingOrgs] org ON org.ID = ri.ReportingOrg_ID
                INNER JOIN [dbo].[vwUserPermission] up ON up.UserID = ra.Requestor_ID AND up.Division_ID = org.Division_ID
                OUTER APPLY (
                    SELECT MAX(LogDate) AS LastDateUsed 
                    FROM [dbo].[vwReportUsage] usg
                    WHERE usg.RID = ri.ID AND usg.UserID = ra.Requestor_ID
                ) usage
                WHERE org.Code = @orgCode
                AND ra.RequestStatus = 'Approved'
                AND up.IsAnalyst = 0
                AND up.IsAdmin = 0
                AND usage.LastDateUsed < @cutoffDate";

                var rowsAffected = await _context.Database.ExecuteSqlCommandAsync(
                    updateCommand,
                    new SqlParameter("@cutoffDate", cutoffDate),
                    new SqlParameter("@orgCode", _config.OrganizationCode));

                _logger.Info($"Auto-cancelled {rowsAffected} subscriptions that were inactive for {_config.RetentionDays} days");

                return rowsAffected;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error cancelling inactive subscriptions: {ex.Message}", ex);
                throw;
            }
        }

        // For finding inactive subscriptions without updating them (for testing/reporting)
        public List<InactiveSubscription> GetInactiveSubscriptions()
        {
            var cutoffDate = DateTime.Now.AddDays(-_config.RetentionDays);

            var inactiveSubscriptions = _context.Database
                .SqlQuery<InactiveSubscription>(@"
                SELECT 
                    ra.ID as SubscriptionID, 
                    ra.Requestor_ID as UserID, 
                    ra.Report_ID as ReportID, 
                    ri.Name as ReportName, 
                    org.Code as OrganizationCode,
                    usage.LastDateUsed
                FROM [dbo].[ReportInventories] ri
                INNER JOIN [dbo].[ReportingOrgs] org ON org.ID = ri.ReportingOrg_ID
                INNER JOIN [dbo].[ReportAccesses] ra ON ra.Report_ID = ri.ID
                INNER JOIN [dbo].[vwUserPermission] up ON up.UserID = ra.Requestor_ID AND up.Division_ID = org.Division_ID
                OUTER APPLY (
                    SELECT MAX(LogDate) AS LastDateUsed 
                    FROM [dbo].[vwReportUsage] usg
                    WHERE usg.RID = ri.ID AND usg.UserID = ra.Requestor_ID
                ) usage
                WHERE org.Code = @orgCode
                AND ra.RequestStatus = 'Approved'
                AND up.IsAnalyst = 0
                AND up.IsAdmin = 0
                AND usage.LastDateUsed < @cutoffDate
                ORDER BY usage.LastDateUsed",
                    new SqlParameter("@cutoffDate", cutoffDate),
                    new SqlParameter("@orgCode", _config.OrganizationCode))
                .ToList();

            return inactiveSubscriptions;
        }
    }

    public class InactiveSubscription
    {
        public int SubscriptionID { get; set; }
        public int UserID { get; set; }
        public int ReportID { get; set; }
        public string ReportName { get; set; }
        public string OrganizationCode { get; set; }
        public DateTime LastDateUsed { get; set; }
    }
    public class ReportCleanupConfig
    {
        public int RetentionDays { get; set; }
        public List<int> ScheduleDays { get; set; }
        public List<int> ScheduleHours { get; set; }
        public string OrganizationCode { get; set; }

        public static ReportCleanupConfig Load()
        {
            var config = new ReportCleanupConfig
            {
                RetentionDays = int.Parse(ConfigurationManager.AppSettings["ReportRetentionDays"]),
                ScheduleDays = ConfigurationManager.AppSettings["ReportCleanupScheduleDays"]
                    .Split(',')
                    .Select(int.Parse)
                    .ToList(),
                ScheduleHours = ConfigurationManager.AppSettings["ReportCleanupScheduleHours"]
                    .Split(',')
                    .Select(int.Parse)
                    .ToList(),
                OrganizationCode = ConfigurationManager.AppSettings["ReportOrganizationCode"]
            };

            config.Validate();
            return config;
        }

        public bool IsScheduledTime()
        {
            var now = DateTime.Now;
            return ScheduleDays.Contains((int)now.DayOfWeek) &&
                   ScheduleHours.Contains(now.Hour);
        }

        public void Validate()
        {
            if (RetentionDays <= 0)
                throw new ConfigurationErrorsException("ReportRetentionDays must be greater than 0");

            if (!ScheduleDays.Any())
                throw new ConfigurationErrorsException("ReportCleanupScheduleDays must contain at least one day");

            if (ScheduleDays.Any(d => d < 0 || d > 6))
                throw new ConfigurationErrorsException("ReportCleanupScheduleDays must contain values between 0 and 6");

            if (!ScheduleHours.Any())
                throw new ConfigurationErrorsException("ReportCleanupScheduleHours must contain at least one hour");

            if (ScheduleHours.Any(h => h < 0 || h > 23))
                throw new ConfigurationErrorsException("ReportCleanupScheduleHours must contain values between 0 and 23");

            if (string.IsNullOrWhiteSpace(OrganizationCode))
                throw new ConfigurationErrorsException("ReportOrganizationCode cannot be empty");
        }
    }
}
