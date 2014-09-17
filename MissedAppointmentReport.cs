using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PetaPoco;

namespace reports_tcado
{
    public class MissedAppointmentReport
    {
        public string BorrowerFirstName { get; set; }
        public string BorrowerLastName { get; set; }
        public string BorrowerPhone { get; set; }
        public DateTime AppointmentSetDate { get; set; }
        public string LoanOfficerFirstName { get; set; }
        public string LoanOfficerLastName { get; set; }
        public string SetterFirstName { get; set; }
        public string SetterLastName { get; set; }
        public DateTime LastModified { get; set; }
        public string Note { get; set; }

        public static IEnumerable<MissedAppointmentReport> GetReport()
        {
            using (var db = new PetaPoco.Database("LOXPressDatabase"))
            {
                db.EnableAutoSelect = false;
                return db.Fetch<MissedAppointmentReport>("exec usp_MissedAppointmentReport");
            }
        }

        public static IEnumerable<MissedAppointmentReport> GetBeckyReport(DateTime startdate, DateTime enddate)
        {
            using (var db = new PetaPoco.Database("LOXPressDatabase"))
            {
                var sql = @"
select 
b.FirstName as BorrowerFirstName, 
b.LastName as BorrowerLastName, 
b.Phone as BorrowerPhone, 
h.HistoryDate as AppointmentSetDate,
l.FirstName as LoanOfficerFirstName,
l.LastName as LoanOfficerLastName,
u.FirstName as SetterFirstName,
u.LastName as SetterLastName,
b.LastModified
from History h
inner join [User] u on u.ID=h.UserID and u.ID=878
inner join [File] f on f.ID=h.TargetID
inner join [User] l on l.ID=f.LoanOfficerID
inner join [Borrower] b on b.ID=f.BorrowerID
left join History h2 on h2.TargetID=h.TargetID and h2.NewValue=278 and h2.HistoryCategoryID=6 and h2.HistoryDate<=DateAdd(dd, 3, h.HistoryDate)
where h.HistoryCategoryID=6 and h.NewValue=278 and h.HistoryDate >= @0 and h.HistoryDate < @1
";
                return db.Fetch<MissedAppointmentReport>(sql, startdate, enddate);
            }
        }
    }
}
