using System.Text;

namespace SchedulingTasks.Controllers
{
    public class testing
    {
    }

    private Dictionary<string, UserSubscriptionInfo> CollectNotificationInfo(
            List<SubscriptionUpdateInfo> updatedSubscriptions)
    {
        // Filter out subscriptions with missing email
        var validSubscriptions = updatedSubscriptions
            .Where(s => !string.IsNullOrEmpty(s.UserEmail))
            .ToList();

        // Log any missing emails
        var missingEmails = updatedSubscriptions.Count - validSubscriptions.Count;
        if (missingEmails > 0)
        {
            _logger.Warn($"Found {missingEmails} subscriptions with missing email addresses");
        }

        // Group by email address
        return validSubscriptions
            .GroupBy(s => s.UserEmail)
            .ToDictionary(
                g => g.Key,
                g => new UserSubscriptionInfo
                {
                    UserID = g.First().UserID,
                    UserEmail = g.Key,
                    ManagerEmail = g.First().ManagerEmail,
                    Reports = g.Select(s => s.ReportName).ToList()
                }
            );
    }

    private async Task<int> SendConsolidatedNotifications(Dictionary<string, UserSubscriptionInfo> userReports)
    {
        int emailsSent = 0;

        // Send one email per user with all their reports
        foreach (var entry in userReports)
        {
            var userEmail = entry.Key;
            var info = entry.Value;

            try
            {
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
}
}
