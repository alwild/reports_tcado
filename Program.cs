using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PetaPoco;
using Newtonsoft.Json;
using System.Net;
using System.Configuration;
using CsvHelper;
using System.IO;
namespace reports_tcado
{
    class Program
    {
        private static string widget_base = ConfigurationManager.AppSettings["widget_url"];
        static void Main(string[] args)
        {
            if (args[0] == "gecko")
            {
                GeckoBoard();
            }

            if (args[0] == "reports")
            {
                CSVReports();
            }
        }

        static void CSVReports()
        {
            var missedappointments = MissedAppointmentReport.GetReport();
            var missedappointments_file = String.Format("missedappointments_{0:yyyyMMdd}.csv", DateTime.Now);
            missedappointments_file = Path.Combine(ConfigurationManager.AppSettings["reports_folder"], missedappointments_file);
            using (var writer = new CsvWriter(File.CreateText(missedappointments_file)))
            {
                writer.WriteRecords(missedappointments);
            }
        }

        static void GeckoBoard()
        {
            #region CSR Reports

            //today
            var startdate = DateTime.Today;
            var enddate = DateTime.Today.AddDays(1);
            var today = CSRReports.GetSummary(startdate, enddate, 277);
            var yesterday = CSRReports.GetSummary(startdate.AddDays(-1), enddate.AddDays(-1), 278);
            PushSummary(today, ConfigurationManager.AppSettings["today_widget"]);
            PushSummary(yesterday, ConfigurationManager.AppSettings["yesterday_widget"]);

            //this month
            startdate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            enddate = startdate.AddMonths(1);
            var this_month = CSRReports.GetSummary(startdate, enddate, 277);
            PushSummary(this_month, ConfigurationManager.AppSettings["monthly_widget"]);
            this_month = CSRReports.GetSummary(startdate, enddate, 278);
            PushSummary(this_month, ConfigurationManager.AppSettings["lo_monthly_funnel"]);
            var location_month = CSRReports.GetStateAppointments(startdate, enddate, 278);
            PushMap(location_month, ConfigurationManager.AppSettings["location_monthly"]);
            var campaign_source = CSRReports.GetCampaignSource(startdate, enddate, 6);
            PushSummary(campaign_source, ConfigurationManager.AppSettings["campaign_source"]);

            var min_appointments = CSRReports.GetMinAppointmentsDaily(startdate, enddate, 277);
            var max_appointments = CSRReports.GetMaxAppointmentsDaily(startdate, DateTime.Today.AddDays(-1), 277);
            var today_appointments = CSRReports.GetMaxAppointmentsDaily(DateTime.Today, DateTime.Today.AddDays(1), 277);
            PushGauge(min_appointments, max_appointments, today_appointments, ConfigurationManager.AppSettings["daily_gauge"]);

            min_appointments = CSRReports.GetMinAppointmentsDaily(startdate, enddate, 278);
            max_appointments = CSRReports.GetMaxAppointmentsDaily(startdate, DateTime.Today.AddDays(-1), 278);
            today_appointments = CSRReports.GetMaxAppointmentsDaily(DateTime.Today, DateTime.Today.AddDays(1), 278);
            PushGauge(min_appointments, max_appointments, today_appointments, ConfigurationManager.AppSettings["lo_daily_gauge"]);

            startdate = new DateTime(DateTime.Today.Year, 1, 1);
            enddate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            min_appointments = CSRReports.GetMinAppointmentsMonthly(startdate, enddate, 277);
            max_appointments = CSRReports.GetMaxAppointmentsMonthly(startdate, enddate, 277);
            var month_appointments = CSRReports.GetMaxAppointmentsMonthly(new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1), DateTime.Today, 277);
            PushGauge(min_appointments, max_appointments, month_appointments, ConfigurationManager.AppSettings["monthly_gauge"]);

            min_appointments = CSRReports.GetMinAppointmentsMonthly(startdate, enddate, 278);
            max_appointments = CSRReports.GetMaxAppointmentsMonthly(startdate, enddate, 278);
            month_appointments = CSRReports.GetMaxAppointmentsMonthly(new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1), DateTime.Today, 278);
            PushGauge(min_appointments, max_appointments, month_appointments, ConfigurationManager.AppSettings["lo_monthly_gauge"]);

            #endregion



            #region LO Reports

            #endregion
        }

        private static void PushSummary(IEnumerable<CSRSummary> summary, string widget_key, bool showtotal=false, int maxitems=8)
        {
            var gecko = new GeckoData();
            var items = summary.Take(maxitems);
            if (showtotal)
            {
                gecko.percentage = "";
                items = summary.Take(maxitems - 1);
                gecko.item.Add(new GeckoFunnelItem()
                {
                    label = "Total",
                    value = Convert.ToString(items.Sum(i => i.AppointmentsMade)),
                });
            }
            foreach (var csr in items)
            {
                gecko.item.Add(new GeckoFunnelItem()
                {
                    label = csr.SetterName,
                    value = Convert.ToString(csr.AppointmentsMade)
                });
            }
            var push_data = new GeckAPI() {
                api_key = ConfigurationManager.AppSettings["api_key"],
                data = gecko
            };
            var json = JsonConvert.SerializeObject(push_data);
            PushWidget(widget_key, json);
        }

        private static void PushMap(IEnumerable<CSRState> summary, string widget_key)
        {
            var gecko = new GeckoPoints();
            foreach (var csr in summary)
            {
                var city = new GeckoCity()
                {
                    region_code = csr.StateName,
                    country_code = "US",
                    city_name = csr.HomeCity
                };
                gecko.point.Add(new GeckoPoint()
                {
                    city = city,
                    size = csr.Appointments
                });
            }
            var push_data = new
            {
                api_key = ConfigurationManager.AppSettings["api_key"],
                data = new
                {
                    points = gecko
                }
            };
            var json = JsonConvert.SerializeObject(push_data);
            PushWidget(widget_key, json);
        }

        private static void PushGauge(int min, int max, int current, string widget_key)
        {
            var gecko = new
            {
                api_key = ConfigurationManager.AppSettings["api_key"],
                data = new
                {
                    item = current,
                    min = new
                    {
                        value = min
                    },
                    max = new
                    {
                        value = max
                    }
                }
            };
            var json = JsonConvert.SerializeObject(gecko);
            PushWidget(widget_key, json);
        }

        private static void PushWidget(string widget_key, string json)
        {
            var wc = new WebClient();
            wc.Headers.Add("Content-Type", "application/json");
            try
            {
                foreach (var key in widget_key.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var result = wc.UploadString(widget_base + key.Trim(), "POST", json);
                }
            }
            catch (System.Net.WebException ex)
            {
                using (var reader = new System.IO.StreamReader(ex.Response.GetResponseStream()))
                {
                    var msg = reader.ReadToEnd();
                    Console.WriteLine(msg);
                }
            }
        }
    }
}
