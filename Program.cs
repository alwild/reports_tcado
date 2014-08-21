using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PetaPoco;

namespace reports_tcado
{
    class Program
    {
        static void Main(string[] args)
        {
            #region CSR Reports
            //this year
            var startdate = DateTime.Today.AddMonths(1).AddYears(-1).AddDays(1-DateTime.Today.Day);
            var enddate = DateTime.Today.AddMonths(1);
            var this_year = CSRReports.GetMonthly(startdate, enddate);

            //this week
            startdate = DateTime.Today.AddDays(0 - (int)DateTime.Today.DayOfWeek);
            enddate = startdate.AddDays(7);
            var this_week = CSRReports.GetSummary(startdate, enddate);

            //today
            startdate = DateTime.Today;
            enddate = DateTime.Today.AddDays(1);
            var today = CSRReports.GetSummary(startdate, enddate);

            //this month
            startdate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            enddate = startdate.AddMonths(1);
            var this_month = CSRReports.GetSummary(startdate, enddate);
            #endregion

            #region LO Reports
            //previous month
            startdate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-1);
            enddate = startdate.AddMonths(1);
            var lo_last_month = LOReports.GetSummary(startdate, enddate);

            //this month
            startdate = enddate.AddMonths(1);
            enddate = enddate.AddMonths(1);
            var lo_this_month = LOReports.GetSummary(startdate, enddate);

            //yesterday
            startdate = DateTime.Today.AddDays(-1);
            enddate = DateTime.Today;
            var lo_today = LOReports.GetSummary(startdate, enddate);
            #endregion

        }
    }
}
