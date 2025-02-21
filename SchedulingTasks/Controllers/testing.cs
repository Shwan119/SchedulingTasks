namespace SchedulingTasks.Controllers
{
    public class testing
    {
        -- Step 1: Check ReportInventories and ReportAccesses join
SELECT ra.ID AS requestAccessID, 
       ra.Report_ID, 
       ra.Requestor_ID,
       COUNT(*) as DuplicateCount
FROM[ROVER].[dbo].[ReportInventories]
        ri
INNER JOIN[ROVER].[dbo].[ReportAccesses] ra ON ra.Report_ID = ri.ID
GROUP BY ra.ID, ra.Report_ID, ra.Requestor_ID
HAVING COUNT(*) > 1;

-- Step 2: Check what vwUserPermission join adds
SELECT ra.ID AS requestAccessID, 
       ra.Report_ID, 
       ra.Requestor_ID,
       up.UserID,
       COUNT(*) as DuplicateCount
FROM[ROVER].[dbo].[ReportInventories]
        ri
INNER JOIN[ROVER].[dbo].[ReportAccesses] ra ON ra.Report_ID = ri.ID
INNER JOIN[dbo].[vwUserPermission] up ON up.UserID = ra.Requestor_ID
GROUP BY ra.ID, ra.Report_ID, ra.Requestor_ID, up.UserID
HAVING COUNT(*) > 1;

-- Step 3: Look at all columns from vwUserPermission for one specific record
SELECT ra.ID AS requestAccessID, 
       ra.Report_ID, 
       ra.Requestor_ID,
       up.*
FROM[ROVER].[dbo].[ReportInventories]
        ri
INNER JOIN[ROVER].[dbo].[ReportAccesses] ra ON ra.Report_ID = ri.ID
INNER JOIN[dbo].[vwUserPermission] up ON up.UserID = ra.Requestor_ID
WHERE ra.ID = /* pick an ID that showed 14 duplicates */;
    }
}
