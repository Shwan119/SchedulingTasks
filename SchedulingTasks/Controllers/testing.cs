namespace SchedulingTasks.Controllers
{
    public class testing
    {
    }

    public List<ReportAccess> GetSubscriptionsToUnsubscribe(int batchSize, int? lastProcessedId, DateTime cutoffDate)
    {
        var query = @"
        SELECT ra.ID, ra.Report_ID, ra.Requestor_ID
        FROM [ROVER].[dbo].[ReportInventories] ri
        INNER JOIN [ROVER].[dbo].[ReportAccesses] ra ON ra.Report_ID = ri.ID
        WHERE ri.ReportingOrg_ID = 19
        AND ra.Requestor_ID != ri.PrimaryAnalyst
        AND (@lastProcessedId IS NULL OR ra.ID > @lastProcessedId)
        AND NOT EXISTS (
            SELECT 1 
            FROM [dbo].[vwReportUsage] ru
            WHERE ru.ReportID = ri.ID 
            AND ru.UserID = ra.Requestor_ID
            AND ru.LogDate > @cutoffDate
        )
        ORDER BY ra.ID
        OFFSET 0 ROWS
        FETCH NEXT @batchSize ROWS ONLY";

        return Database.SqlQuery<ReportAccess>(query,
            new SqlParameter("@lastProcessedId", (object)lastProcessedId ?? DBNull.Value),
            new SqlParameter("@cutoffDate", cutoffDate),
            new SqlParameter("@batchSize", batchSize))
            .ToList();
    }

    public async Task UnsubscribeInactiveUsersWithCursorAsync(int batchSize = 1000)
    {
        var cutoffDate = DateTime.Now.AddDays(-90);
        int? lastProcessedId = null;

        while (true)
        {
            var subscriptionsBatch = GetSubscriptionsToUnsubscribe(batchSize, lastProcessedId, cutoffDate);

            if (!subscriptionsBatch.Any())
                break;

            _context.ReportAccesses.RemoveRange(subscriptionsBatch);
            await _context.SaveChangesAsync();

            lastProcessedId = subscriptionsBatch.Max(ra => ra.ID);
        }
    }

    var subscriptionsBatch = await _context.ReportInventories
    .Where(r => r.ReportingOrg_ID == 19)
    .SelectMany(r => r.ReportAccesses
        .Where(ra =>
            (lastProcessedId == null || ra.ID > lastProcessedId) &&
            ra.Requestor_ID != r.PrimaryAnalyst &&  // Add check for primary analyst
            !recentUsage.Any(ru =>
                ru.ReportID == r.ID &&
                ru.UserID == ra.Requestor_ID)))
    .OrderBy(ra => ra.ID)
    .Take(batchSize)
    .ToListAsync();

    public List<VwReportUsage> GetRecentUsage(DateTime cutoffDate)
    {
        return Database.SqlQuery<VwReportUsage>(
            @"SELECT DISTINCT 
            ReportID, 
            UserID
        FROM vwReportUsage 
        WHERE LogDate > @cutoffDate",
            new SqlParameter("@cutoffDate", cutoffDate))
            .ToList();
    }

    public class YourDbContext : DbContext
    {
        // Instead of using IQueryable, we'll use a method
        public List<VwReportUsage> GetRecentUsage(DateTime cutoffDate)
        {
            return Database.SqlQuery<VwReportUsage>(
                "SELECT ReportID, UserID, LogDate FROM vwReportUsage WHERE LogDate > @cutoffDate",
                new SqlParameter("@cutoffDate", cutoffDate))
                .ToList();
        }
    }

    public async Task UnsubscribeInactiveUsersWithCursorAsync(int batchSize = 1000)
    {
        var cutoffDate = DateTime.Now.AddDays(-90);
        int? lastProcessedId = null;

        // Get recent usage using the method
        var recentUsage = _context.GetRecentUsage(cutoffDate);

        while (true)
        {
            var subscriptionsBatch = await _context.ReportInventories
                .Where(r => r.ReportingOrg_ID == 19)
                .SelectMany(r => r.ReportAccesses
                    .Where(ra =>
                        (lastProcessedId == null || ra.ID > lastProcessedId) &&
                        !recentUsage.Any(ru =>
                            ru.ReportID == r.ID &&
                            ru.UserID == ra.Requestor_ID)))
                .OrderBy(ra => ra.ID)
                .Take(batchSize)
                .ToListAsync();

            if (!subscriptionsBatch.Any())
                break;

            _context.ReportAccesses.RemoveRange(subscriptionsBatch);
            await _context.SaveChangesAsync();

            lastProcessedId = subscriptionsBatch.Max(ra => ra.ID);
        }
    }
}
