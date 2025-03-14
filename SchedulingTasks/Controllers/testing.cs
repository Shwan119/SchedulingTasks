

using System.Data;

namespace SchedulingTasks.Controllers
{
    public class testing
    {
        public IQueryable<LookupDto> GetLookups(int? orgId)
        {
            var query = from lok in db.DimLookups
                        join org in db.ReportingOrgs on lok.ReportingOrg_ID equals org.ID into orgJoin
                        from org in orgJoin.DefaultIfEmpty()
                        where lok.ReportingOrg_ID == orgId || orgId == null
                        orderby lok.Category, lok.ReportingOrg_ID, lok.SortOrder, lok.Code
                        select new LookupDto
                        {
                            ID = lok.ID,
                            Code = lok.Code,
                            Description = lok.Description,
                            Category = lok.Category,
                            ReportingOrg_ID = lok.ReportingOrg_ID,
                            SortOrder = lok.SortOrder,
                            // Additional fields from DimLookups

                            // This handles the IIF expression
                            ReportingOrg = lok.ReportingOrg_ID == null ? "Global" : org.Name
                        };

            return query;
        }

        public List<LookupDto> GetLookupsSqlQuery(int? orgId)
        {
            string query = @"
        select lok.*
        , ReportingOrg = IIF(ReportingOrg_ID is null, 'Global', org.Name)
        from DimLookups lok
        left join ReportingOrgs org on org.ID = lok.ReportingOrg_ID
        where ReportingOrg_ID = @orgId or @orgId is null
        order by Category, ReportingOrg_ID, SortOrder, Code";

            var parameter = new SqlParameter("@orgId", SqlDbType.Int);
            if (orgId == null)
            {
                parameter.Value = DBNull.Value;
            }
            else
            {
                parameter.Value = orgId;
            }

            var lookups = db.Database.SqlQuery<LookupDto>(query, parameter).ToList();
            return lookups;
        }

        // Simplified version
        public List<LookupDto> GetLookupsSqlQuerySimple(int? orgId)
        {
            string query = @"
        select lok.*
        , ReportingOrg = IIF(ReportingOrg_ID is null, 'Global', org.Name)
        from DimLookups lok
        left join ReportingOrgs org on org.ID = lok.ReportingOrg_ID
        where ReportingOrg_ID = @orgId or @orgId is null
        order by Category, ReportingOrg_ID, SortOrder, Code";

            var lookups = db.Database.SqlQuery<LookupDto>(query,
                new SqlParameter { ParameterName = "@orgId", Value = (object)orgId ?? DBNull.Value }).ToList();
            return lookups;
        }
    }

    public class LookupDto
    {
        // Based on your SQL using lok.*, you'll need all DimLookups properties
        // Plus the calculated ReportingOrg property
        public int ID { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public int? ReportingOrg_ID { get; set; }
        public int SortOrder { get; set; }
        // Additional fields from DimLookups as needed

        // This is the calculated field from your IIF expression
        public string ReportingOrg { get; set; }
    }
}