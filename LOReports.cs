using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PetaPoco;

namespace reports_tcado
{
    public class LOSummary
    {
    }

    public static class LOReports
    {
        public static IEnumerable<LOSummary> GetSummary(DateTime startdate, DateTime enddate)
        {
            using (var db = new PetaPoco.Database(CSRReports.connection_name))
            {
                var sql = @"
select u.FirstName + ' ' + u.LastName as LoanOfficer, COUNT(*) 
from History h
inner join [User] u on u.ID=h.UserID
where HistoryCategoryID=6 and h.NewValue=278 and HistoryDate >= @0 and HistoryDate < @1 
group by u.FirstName + ' ' + u.LastName
";
                return db.Fetch<LOSummary>(sql, startdate, enddate);
            }
        }
    }
}
