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

    public class CSRState
    {
        public string HomeCity { get; set; }
        public string StateName { get; set; }
        public string HomeCountry { get; set; }
        public int Appointments { get; set; }
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

        public static IEnumerable<CSRSummary> GetSummary(DateTime startdate, DateTime enddate, int statusid)
        {
            using (var db = new PetaPoco.Database(connection_name))
            {
                var sql = @"
select u.FirstName + ' ' + u.LastName as SetterName,
Count(*) as AppointmentsMade, COUNT(h2.NewValue) as AppointmentsKept
from History h
inner join [User] u on u.ID=h.UserID
Left join History h2 on h2.TargetID=h.TargetID and h2.NewValue=278 and h2.HistoryCategoryID=6 and h2.HistoryDate<=DateAdd(dd, 3, h.HistoryDate)
where h.HistoryCategoryID=6 and h.NewValue=@2 and h.HistoryDate >= @0 and h.HistoryDate < @1
GROUP BY u.FirstName + ' ' + u.LastName ORDER BY 2 DESC
";
                return db.Fetch<CSRSummary>(sql, startdate, enddate, statusid);
            }
        }

        public static IEnumerable<CSRSummary> GetCampaignSource(DateTime startdate, DateTime enddate, int categoryid)
        {
            using (var db = new PetaPoco.Database(connection_name))
            {
                var sql = @"
select CompanyName as SetterName, Count(*) as AppointmentsMade
from Borrower b
inner join [File] f on f.BorrowerID=b.ID and f.DateCreated > @0 and f.DateCreated < @1
inner join Campaign c on c.ID=b.CampaignID
inner join CampaignSource s on s.ID=c.SourceID
where s.CategoryID=@2
group by CompanyName
order by 2 desc
";
                return db.Fetch<CSRSummary>(sql, startdate, enddate, categoryid);
            }
        }

        public static IEnumerable<CSRState> GetStateAppointments(DateTime startdate, DateTime enddate, int statusid)
        {
            using (var db = new PetaPoco.Database(connection_name))
            {
                var sql = @"
select b.HomeCity, s.StateName, b.HomeCounty,
Count(*) as Appointments
from History h
inner join [File] f on f.ID=h.TargetID
inner join [Borrower] b on b.ID=f.BorrowerID
inner join [State] s on s.StateID=b.HomeStateID
where h.HistoryCategoryID=6 and h.NewValue=@2 and h.HistoryDate >= @0 and h.HistoryDate < @1
GROUP BY b.HomeCity, s.StateName, b.HomeCounty ORDER BY 2 DESC
";
                return db.Fetch<CSRState>(sql, startdate, enddate, statusid);
            }
        }


        public static int GetMaxAppointmentsDaily(DateTime startdate, DateTime enddate, int statusid)
        {
            using (var db = new PetaPoco.Database(connection_name))
            {
                var sql = @"
SELECT TOP 1 COUNT(*) FROM History h  WHERE HistoryCategoryID=6 and NewValue=@2 and HistoryDate >= @0 and HistoryDate < @1
GROUP BY CAST(HistoryDate as Date)
ORDER BY 1 DESC
";
                return db.SingleOrDefault<int>(sql, startdate, enddate, statusid);
            }
        }

        public static int GetMinAppointmentsDaily(DateTime startdate, DateTime enddate, int statusid)
        {
            using (var db = new PetaPoco.Database(connection_name))
            {
                var sql = @"
SELECT TOP 1 COUNT(*) FROM History h  WHERE HistoryCategoryID=6 and NewValue=@2 and HistoryDate >= @0 and HistoryDate < @1
GROUP BY CAST(HistoryDate as Date)
ORDER BY 1 ASC
";
                return db.SingleOrDefault<int>(sql, startdate, enddate, statusid);
            }
        }

        public static int GetMaxAppointmentsMonthly(DateTime startdate, DateTime enddate, int statusid)
        {
            using (var db = new PetaPoco.Database(connection_name))
            {
                var sql = @"
SELECT TOP 1 COUNT(*) FROM History h  WHERE HistoryCategoryID=6 and NewValue=@2 and HistoryDate >= @0 and HistoryDate < @1
GROUP BY Year(HistoryDate), Month(HistoryDate)
ORDER BY 1 DESC
";
                return db.SingleOrDefault<int>(sql, startdate, enddate, statusid);
            }
        }

        public static int GetMinAppointmentsMonthly(DateTime startdate, DateTime enddate, int statusid)
        {
            using (var db = new PetaPoco.Database(connection_name))
            {
                var sql = @"
SELECT TOP 1 COUNT(*) FROM History h  WHERE HistoryCategoryID=6 and NewValue=@2 and HistoryDate >= @0 and HistoryDate < @1
GROUP BY Year(HistoryDate), Month(HistoryDate)
ORDER BY 1 ASC
";
                return db.SingleOrDefault<int>(sql, startdate, enddate, statusid);
            }
        }
    }
}
