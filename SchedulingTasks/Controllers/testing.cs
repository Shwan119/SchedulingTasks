

using System.Data;

namespace SchedulingTasks.Controllers
{
    public class testing
    {
        public IQueryable<ReportAccessDto> GetReportAccesses(string requestedByNBK, string reviewedByNBK)
        {
            // Apply the special handling logic for empty strings and "(All)"
            if (string.IsNullOrEmpty(requestedByNBK) || requestedByNBK == "(All)")
            {
                requestedByNBK = null;
            }

            if (string.IsNullOrEmpty(reviewedByNBK) || reviewedByNBK == "(All)")
            {
                reviewedByNBK = null;
            }

            var query = from rac in db.ReportAccesses
                        join ri in db.ReportInventories on rac.Report_ID equals ri.ID
                        join rqstr in db.AppUsers on rac.Requestor_ID equals rqstr.ID
                        join rvwr in db.AppUsers on rac.Reviewer_ID equals rvwr.ID into reviewerJoin
                        from rvwr in reviewerJoin.DefaultIfEmpty()
                        join rvkr in db.AppUsers on rac.Revoker_ID equals rvkr.ID into revokerJoin
                        from rvkr in revokerJoin.DefaultIfEmpty()
                        where (requestedByNBK == null || rqstr.NBK == requestedByNBK)
                        && (reviewedByNBK == null || rvwr.NBK == reviewedByNBK)
                        select new ReportAccessDto
                        {
                            RequestID = rac.ID,
                            RequestDt = rac.RequestDt,
                            RequestStatus = rac.RequestStatus,
                            ReportID = ri.ID,
                            ReportName = ri.Name,
                            SecurityScope = ri.SecurityScope,
                            RequestorNBK = rqstr.NBK,
                            RequestorUserName = rqstr.Username,
                            RequestorEmail = rqstr.Email,
                            Justification = rac.Justification,
                            ReviewedByNBK = rvwr != null ? rvwr.NBK : null,
                            ReviewedByUserName = rvwr != null ? rvwr.Username : null,
                            ReviewedByEmail = rvwr != null ? rvwr.Email : null,
                            ReviewComment = rac.ReviewComment,
                            RevokedByNBK = rvkr != null ? rvkr.NBK : null,
                            RevokedByUserName = rvkr != null ? rvkr.Username : null,
                            RevokedByEmail = rvkr != null ? rvkr.Email : null,
                            RevocationComment = rac.RevocationComment
                        };

            return query;
        }

        public List<ReportAccessDto> GetReportAccessesSqlQuery(string requestedByNBK, string reviewedByNBK)
        {
            // First apply the transformation logic as in the original SQL
            string sqlSetParams = @"
        declare @requestedByNBK varchar(20) = @reqByNBKParam
        declare @reviewedByNBK varchar(20) = @revByNBKParam
        
        set @requestedByNBK = (select iif(@requestedByNBK = '' or @requestedByNBK = '(All)', null, @requestedByNBK))
        set @reviewedByNBK = (select iif(@reviewedByNBK = '' or @reviewedByNBK = '(All)', null, @reviewedByNBK))";

            string sqlQuery = @"
        select RequestID = rac.ID, rac.RequestDt, rac.RequestStatus
        , ReportID = ri.ID, ReportName = ri.Name, ri.SecurityScope
        , RequestorNBK = rqstr.NBK, RequestorUserName = rqstr.Username, RequestorEmail = rqstr.Email, rac.Justification
        , ReviewedByNBK = rvwr.NBK, ReviewedByUserName = rvwr.Username, ReviewedByEmail = rvwr.Email, rac.ReviewComment
        , RevokedByNBK = rvkr.NBK, RevokedByUserName = rvkr.Username, RevokedByEmail = rvkr.Email, rac.RevocationComment
        from ReportAccesses rac
        inner join ReportInventories ri on ri.ID = rac.Report_ID
        inner join AppUsers rqstr on rqstr.ID = rac.Requestor_ID
        left join AppUsers rvwr on rvwr.ID = rac.Reviewer_ID
        left join AppUsers rvkr on rvkr.ID = rac.Revoker_ID
        where (rqstr.NBK = @requestedByNBK) or (@requestedByNBK is null)
        and ((rvwr.NBK = @reviewedByNBK) or (@reviewedByNBK is null))";

            // In EF6 and earlier, you might need to construct your SQL command manually 
            // because of the variable declarations
            string fullSql = sqlSetParams + sqlQuery;

            var parameters = new List<SqlParameter>
    {
        new SqlParameter("@reqByNBKParam", SqlDbType.VarChar, 20) { Value = (object)requestedByNBK ?? DBNull.Value },
        new SqlParameter("@revByNBKParam", SqlDbType.VarChar, 20) { Value = (object)reviewedByNBK ?? DBNull.Value }
    };

            // Unfortunately, EF6's SqlQuery doesn't handle T-SQL variable declarations well
            // So there are a few approaches: 

            // Option 1: If your database supports stored procedures, convert this to a stored proc

            // Option 2: Simplify the query by doing the parameter logic in C# instead:
            string simplifiedQuery = @"
        select RequestID = rac.ID, rac.RequestDt, rac.RequestStatus
        , ReportID = ri.ID, ReportName = ri.Name, ri.SecurityScope
        , RequestorNBK = rqstr.NBK, RequestorUserName = rqstr.Username, RequestorEmail = rqstr.Email, rac.Justification
        , ReviewedByNBK = rvwr.NBK, ReviewedByUserName = rvwr.Username, ReviewedByEmail = rvwr.Email, rac.ReviewComment
        , RevokedByNBK = rvkr.NBK, RevokedByUserName = rvkr.Username, RevokedByEmail = rvkr.Email, rac.RevocationComment
        from ReportAccesses rac
        inner join ReportInventories ri on ri.ID = rac.Report_ID
        inner join AppUsers rqstr on rqstr.ID = rac.Requestor_ID
        left join AppUsers rvwr on rvwr.ID = rac.Reviewer_ID
        left join AppUsers rvkr on rvkr.ID = rac.Revoker_ID
        where (rqstr.NBK = @requestedByNBK) or (@requestedByNBK is null)
        and ((rvwr.NBK = @reviewedByNBK) or (@reviewedByNBK is null))";

            // Process parameters in C# code
            if (string.IsNullOrEmpty(requestedByNBK) || requestedByNBK == "(All)")
            {
                requestedByNBK = null;
            }

            if (string.IsNullOrEmpty(reviewedByNBK) || reviewedByNBK == "(All)")
            {
                reviewedByNBK = null;
            }

            var simplifiedParams = new List<SqlParameter>
    {
        new SqlParameter("@requestedByNBK", SqlDbType.VarChar, 20) { Value = (object)requestedByNBK ?? DBNull.Value },
        new SqlParameter("@reviewedByNBK", SqlDbType.VarChar, 20) { Value = (object)reviewedByNBK ?? DBNull.Value }
    };

            var result = db.Database.SqlQuery<ReportAccessDto>(
                simplifiedQuery,
                simplifiedParams.ToArray()).ToList();

            return result;
        }
    }

    public class ReportAccessDto
    {
        public int RequestID { get; set; }
        public DateTime RequestDt { get; set; }
        public string RequestStatus { get; set; }
        public int ReportID { get; set; }
        public string ReportName { get; set; }
        public string SecurityScope { get; set; }
        public string RequestorNBK { get; set; }
        public string RequestorUserName { get; set; }
        public string RequestorEmail { get; set; }
        public string Justification { get; set; }
        public string ReviewedByNBK { get; set; }
        public string ReviewedByUserName { get; set; }
        public string ReviewedByEmail { get; set; }
        public string ReviewComment { get; set; }
        public string RevokedByNBK { get; set; }
        public string RevokedByUserName { get; set; }
        public string RevokedByEmail { get; set; }
        public string RevocationComment { get; set; }
    }
}