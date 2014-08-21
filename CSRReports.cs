using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace reports_tcado
{
    public class CSRMonthly
    {
        public string SetterName { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int AppointmentsMade { get; set; }
        public int AppointmentsKept { get; set; }
        public decimal Ratio
        {
            get 
            { 
                if (AppointmentsMade > 0) return AppointmentsKept / AppointmentsMade; 
                return 0; 
            }
        }            
    }

    public class CSRDaily
    {
        public string SetterName { get; set; }
        public DateTime Date { get; set; }
        public int AppointmentsMade { get; set; }
        public int AppointmentsKept { get; set; }
        public decimal Ratio
        {
            get
            {
                if (AppointmentsMade > 0) return AppointmentsKept / AppointmentsMade;
                return 0;
            }
        }
    }

    public class CSRSummary
    {
        public string SetterName { get; set; }
        public int AppointmentsMade { get; set; }
        public int AppointmentsKept { get; set; }
        public decimal Ratio
        {
            get
            {
                if (AppointmentsMade > 0) return AppointmentsKept / AppointmentsMade;
                return 0;
            }
        }
    }

    public static class CSRReports
    {
        internal static readonly string connection_name = "LOXPressDatabase";
        public static IEnumerable<CSRMonthly> GetMonthly(DateTime startdate, DateTime enddate) 
        {
            using (var db = new PetaPoco.Database(connection_name))
            {
                var sql = @"
select u.FirstName + ' ' + u.LastName as SetterName, Year(h.HistoryDate) as [Year],
Month(h.HistoryDate) as [Month],
Count(*) as AppointmentsMade, COUNT(h2.NewValue) as AppointmentsKept
from History h
inner join [User] u on u.ID=h.UserID
Left join History h2 on h2.TargetID=h.TargetID and h2.NewValue=278 and h2.HistoryCategoryID=6 and h2.HistoryDate<=DateAdd(dd, 3, h.HistoryDate)
where h.HistoryCategoryID=6 and h.NewValue=277 and h.HistoryDate >= @0 and h.HistoryDate < @1
GROUP BY u.FirstName + ' ' + u.LastName, Year(h.HistoryDate), Month(h.HistoryDate)
";
                return db.Fetch<CSRMonthly>(sql, startdate, enddate);
            }
        }

        public static IEnumerable<CSRDaily> GetDaily(DateTime startdate, DateTime enddate)
        {
            using (var db = new PetaPoco.Database(connection_name))
            {
                var sql = @"
select u.FirstName + ' ' + u.LastName as SetterName, CAST(h.HistoryDate as Date) as [Date],
Count(*) as AppointmentsMade, COUNT(h2.NewValue) as AppointmentsKept
from History h
inner join [User] u on u.ID=h.UserID
Left join History h2 on h2.TargetID=h.TargetID and h2.NewValue=278 and h2.HistoryCategoryID=6 and h2.HistoryDate<=DateAdd(dd, 3, h.HistoryDate)
where h.HistoryCategoryID=6 and h.NewValue=277 and h.HistoryDate >= @0 and h.HistoryDate < @1
GROUP BY u.FirstName + ' ' + u.LastName, CAST(h.HistoryDate as Date)
";
                return db.Fetch<CSRDaily>(sql, startdate, enddate);
            }
        }

        public static IEnumerable<CSRSummary> GetSummary(DateTime startdate, DateTime enddate)
        {
            using (var db = new PetaPoco.Database(connection_name))
            {
                var sql = @"
select u.FirstName + ' ' + u.LastName as SetterName,
Count(*) as AppointmentsMade, COUNT(h2.NewValue) as AppointmentsKept
from History h
inner join [User] u on u.ID=h.UserID
Left join History h2 on h2.TargetID=h.TargetID and h2.NewValue=278 and h2.HistoryCategoryID=6 and h2.HistoryDate<=DateAdd(dd, 3, h.HistoryDate)
where h.HistoryCategoryID=6 and h.NewValue=277 and h.HistoryDate >= @0 and h.HistoryDate < @1
GROUP BY u.FirstName + ' ' + u.LastName
";
                return db.Fetch<CSRSummary>(sql, startdate, enddate);
            }
        }
    }
}
