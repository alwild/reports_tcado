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
    }
}
