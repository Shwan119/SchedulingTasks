using System.Configuration;

namespace SchedulingTasks.Controllers
{
    public class testing
    {
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
}
