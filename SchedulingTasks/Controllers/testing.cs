

namespace SchedulingTasks.Controllers
{
    public class testing
    {
        public List<LobDto> GetLobDataEfCore6(int? divId)
        {
            FormattableString sql = $@"
        select div.Code as DivisionCode, div.Name as DivisionName
        , grp.ID as GroupID, grp.Cd as GroupCd, grp.Name as GroupName  
        , lob.ID as LOBID, lob.Cd as LOBCd, lob.Name as LOBName
        , ex.ExecutiveNBK, ex.ExecutiveName
        from Divisions div
        left join DimLOBGroups grp on grp.Division_ID = div.ID and grp.ActiveFlag = 1
        left join DimLOBs lob on lob.LOBGroup_ID = grp.ID and lob.Division_ID = grp.Division_ID and lob.ActiveFlag = 1
        left join vwDimLOBExecutives ex on ex.LOBID = lob.ID
        where grp.Division_ID = {divId} or {divId} is null
        order by DivisionName, GroupName, LOBName";

            return _context.Database.SqlQuery<LobDto>(sql).ToList();
        }
        public List<LobDto> GetLobData(int? divId)
        {
            string sql = @"
        select div.Code as DivisionCode, div.Name as DivisionName
        , grp.ID as GroupID, grp.Cd as GroupCd, grp.Name as GroupName  
        , lob.ID as LOBID, lob.Cd as LOBCd, lob.Name as LOBName
        , ex.ExecutiveNBK, ex.ExecutiveName
        from Divisions div
        left join DimLOBGroups grp on grp.Division_ID = div.ID and grp.ActiveFlag = 1
        left join DimLOBs lob on lob.LOBGroup_ID = grp.ID and lob.Division_ID = grp.Division_ID and lob.ActiveFlag = 1
        left join vwDimLOBExecutives ex on ex.LOBID = lob.ID
        where grp.Division_ID = @divId or @divId is null
        order by DivisionName, GroupName, LOBName";

            // Option 1: Using FromSqlRaw with DbSet<LobDto> (EF Core 3.0+)
            // You would need to have a DbSet<LobDto> in your context for this to work
            var result = _context.Set<LobDto>().FromSqlRaw(sql, new SqlParameter("@divId", divId ?? (object)DBNull.Value)).ToList();

            // Option 2: Using raw ADO.NET connection
            var result = new List<LobDto>();
            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = sql;
                var parameter = command.CreateParameter();
                parameter.ParameterName = "@divId";
                parameter.Value = divId ?? (object)DBNull.Value;
                command.Parameters.Add(parameter);

                _context.Database.OpenConnection();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new LobDto
                        {
                            DivisionCode = reader["DivisionCode"].ToString(),
                            DivisionName = reader["DivisionName"].ToString(),
                            GroupID = Convert.ToInt32(reader["GroupID"]),
                            GroupCd = reader["GroupCd"].ToString(),
                            GroupName = reader["GroupName"].ToString(),
                            LOBID = Convert.ToInt32(reader["LOBID"]),
                            LOBCd = reader["LOBCd"].ToString(),
                            LOBName = reader["LOBName"].ToString(),
                            ExecutiveNBK = reader["ExecutiveNBK"] != DBNull.Value ? reader["ExecutiveNBK"].ToString() : null,
                            ExecutiveName = reader["ExecutiveName"] != DBNull.Value ? reader["ExecutiveName"].ToString() : null
                        });
                    }
                }
            }

            return result;
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