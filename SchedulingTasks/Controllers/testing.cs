namespace SchedulingTasks.Controllers
{
    public class testing
    {
        public List<ReportAccess> GetSubscriptionsToUnsubscribe(int batchSize, int? lastProcessedId, DateTime cutoffDate, int primaryDivisionId)
        {
            var query = @"
        SELECT TOP (@batchSize) ra.ID, ra.Report_ID, ra.Requestor_ID
        FROM [ROVER].[dbo].[ReportInventories] ri
        INNER JOIN [ROVER].[dbo].[ReportAccesses] ra ON ra.Report_ID = ri.ID
        INNER JOIN [dbo].[vwUserPermission] up ON up.UserId = ra.Requestor_ID
        WHERE up.PrimaryDivistionId = @primaryDivisionId
        AND up.isAnalyst = 0 
        AND up.isAdmin = 0
        AND ra.Requestor_ID != ri.PrimaryAnalyst
        AND (@lastProcessedId IS NULL OR ra.ID > @lastProcessedId)
        AND NOT EXISTS (
            SELECT 1 
            FROM [dbo].[vwReportUsage] ru
            WHERE ru.ReportID = ri.ID 
            AND ru.UserID = ra.Requestor_ID
            AND ru.LogDate > @cutoffDate
        )
        ORDER BY ra.ID";

            return Database.SqlQuery<ReportAccess>(query,
                new SqlParameter("@lastProcessedId", (object)lastProcessedId ?? DBNull.Value),
                new SqlParameter("@cutoffDate", cutoffDate),
                new SqlParameter("@batchSize", batchSize),
                new SqlParameter("@primaryDivisionId", primaryDivisionId))
                .ToList();
        }

        // And then update the calling method:
        public async Task RemoveInactiveSubscriptionsBatchAsync(int batchSize = 1000)
        {
            var cutoffDate = DateTime.Now.AddDays(-90);
            int? lastProcessedId = null;
            int primaryDivisionId = 19; // Or pass this as a parameter

            while (true)
            {
                var subscriptionsBatch = GetSubscriptionsToUnsubscribe(batchSize, lastProcessedId, cutoffDate, primaryDivisionId);

                if (!subscriptionsBatch.Any())
                    break;

                _context.ReportAccesses.RemoveRange(subscriptionsBatch);
                await _context.SaveChangesAsync();

                lastProcessedId = subscriptionsBatch.Max(ra => ra.ID);
            }
        }

        public async Task RemoveInactiveSubscriptionsBatchAsync(int batchSize = 1000)
        {
            var cutoffDate = DateTime.Now.AddDays(-90);
            int? lastProcessedId = null;
            int primaryDivisionId = 19;

            // Get recent usage once
            var recentUsage = _context.Database
                .SqlQuery<VwReportUsage>(
                    @"SELECT DISTINCT ReportID, UserID
            FROM [dbo].[vwReportUsage] 
            WHERE LogDate > @cutoffDate",
                    new SqlParameter("@cutoffDate", cutoffDate))
                .ToList();

            // Get user permissions once
            var userPermissions = _context.Database
                .SqlQuery<VwUserPermission>(
                    @"SELECT UserId, PrimaryDivistionId, isAnalyst, isAdmin
            FROM [dbo].[vwUserPermission] 
            WHERE PrimaryDivistionId = @primaryDivisionId
            AND isAnalyst = 0 
            AND isAdmin = 0",
                    new SqlParameter("@primaryDivisionId", primaryDivisionId))
                .ToList();

            while (true)
            {
                var subscriptionsBatch = await _context.ReportInventories
                    .Where(r => r.ReportingOrg_ID == primaryDivisionId)
                    .SelectMany(r => r.ReportAccesses
                        .Where(ra =>
                            userPermissions.Any(up => up.UserId == ra.Requestor_ID) &&
                            ra.Requestor_ID != r.PrimaryAnalyst &&
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
}
