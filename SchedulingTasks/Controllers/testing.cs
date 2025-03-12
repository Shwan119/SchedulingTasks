

namespace SchedulingTasks.Controllers
{
    public class testing
    {
        public IQueryable<LobDto> GetLobData(int? divId)
        {
            var query = from div in _context.Divisions
                        join grp in _context.DimLOBGroups on div.ID equals grp.Division_ID
                        join lob in _context.DimLOBs on grp.ID equals lob.LOBGroup_ID
                        join ex in _context.vwDimLOBExecutives on lob.ID equals ex.LOBID into executivesJoin
                        from ex in executivesJoin.DefaultIfEmpty()
                        where (divId == null || grp.Division_ID == divId)
                              && grp.ActiveFlag == 1
                              && lob.ActiveFlag == 1
                              && lob.Division_ID == grp.Division_ID
                        orderby div.Name, grp.Name, lob.Name
                        select new LobDto
                        {
                            DivisionCode = div.Code,
                            DivisionName = div.Name,
                            GroupID = grp.ID,
                            GroupCd = grp.Cd,
                            GroupName = grp.Name,
                            LOBID = lob.ID,
                            LOBCd = lob.Cd,
                            LOBName = lob.Name,
                            ExecutiveNBK = ex != null ? ex.ExecutiveNBK : null,
                            ExecutiveName = ex != null ? ex.ExecutiveName : null
                        };

            return query;
        }
    }

    public class LobDto
    {
        public string DivisionCode { get; set; }
        public string DivisionName { get; set; }
        public int GroupID { get; set; }
        public string GroupCd { get; set; }
        public string GroupName { get; set; }
        public int LOBID { get; set; }
        public string LOBCd { get; set; }
        public string LOBName { get; set; }
        public string ExecutiveNBK { get; set; }
        public string ExecutiveName { get; set; }
    }
}