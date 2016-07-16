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
            var reports_folder = ConfigurationManager.AppSettings["reports_folder"];
            var missedappointments = MissedAppointmentReport.GetReport();
            var missedappointments_file = String.Format("missedappointments_{0:yyyyMMdd}.csv", DateTime.Now);
            missedappointments_file = Path.Combine(reports_folder, missedappointments_file);
            using (var writer = new CsvWriter(File.CreateText(missedappointments_file)))
            {
                writer.WriteRecords(missedappointments);
            }

            var beckyreport = MissedAppointmentReport.GetBeckyReport(DateTime.Today.AddDays(-30), DateTime.Today.AddDays(1));
            var beckyreport_file = String.Format("beckyreport_{0:yyyyMMdd}.csv", DateTime.Now);
            beckyreport_file = Path.Combine(reports_folder, beckyreport_file);
            using (var writer = new CsvWriter(File.CreateText(beckyreport_file)))
            {
                writer.WriteRecords(beckyreport);
            }

            var campaignsummary = CampaignSummary.GetSummary(DateTime.Today.AddYears(-1), DateTime.Today.AddDays(1), 6);
            var campaignsummary_file = String.Format("campaignsummary_{0:yyyyMMdd}.csv", DateTime.Now);
            campaignsummary_file = Path.Combine(reports_folder, campaignsummary_file);
            using (var writer = new CsvWriter(File.CreateText(campaignsummary_file)))
            {
                writer.WriteRecords(campaignsummary);
            }

            var campaigndetails = CampaignDetails.GetDetails(DateTime.Today.AddYears(-1), DateTime.Today.AddDays(1), 6);
            var campaigndetails_file = String.Format("campaigndetails_{0:yyyyMMdd}.csv", DateTime.Now);
            campaigndetails_file = Path.Combine(reports_folder, campaigndetails_file);
            using (var writer = new CsvWriter(File.CreateText(campaigndetails_file)))
            {
                writer.WriteRecords(campaignsummary);
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

            var recent_offercodes = GetRecentOfferCodes();
            var offers = CSRReports.GetOfferCodeSummary(DateTime.Today, DateTime.Today.AddDays(1));
            var recent_offers = offers.Where(o => recent_offercodes.Contains(o.OfferCode));
            PushList(recent_offers, ConfigurationManager.AppSettings["offercode_list"]);

            #endregion
            


            #region LO Reports

            #endregion
        }

        private static string[] GetRecentOfferCodes()
        {
            var url = "https://docs.google.com/spreadsheets/d/1njoKbNQOR1Z0rKQUmCcrPnIpI3gWQTe5lmGmYfqnAcY/pub?gid=0&single=true&output=csv";
            var wc = new WebClient();
            var csvdata = wc.DownloadString(url);
            CsvReader reader = new CsvReader(new StringReader(csvdata));
            var result = new List<string>();
            result.Add("Internet");
            //let's go back 2 weeks
            var start_date = DateTime.Today.AddDays(-15);
            while (reader.Read())
            {
                var sent = reader["sent"];
                if (!String.IsNullOrEmpty(sent))
                {
                    var sent_date = Convert.ToDateTime(sent);
                    if (sent_date >= start_date)
                    {
                        result.Add(reader["offer code"]);
                    }
                }
            }
            return result.ToArray();
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

        private static void PushList(IEnumerable<OfferCodeSummary> summary, string widget_key)
        {
            var gecko = new GeckoList()
            {
                api_key = ConfigurationManager.AppSettings["api_key"]
            };

            foreach (var item in summary)
            {
                if (!String.IsNullOrEmpty(item.OfferCode))
                {
                    var listitem = new GeckoListItem();
                    listitem.title = new GeckoListTitle()
                    {
                        text = item.OfferCode
                    };
                    listitem.description = Convert.ToString(item.OfferCount);
                    gecko.data.Add(listitem);
                }
            }
            var json = JsonConvert.SerializeObject(gecko);
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
