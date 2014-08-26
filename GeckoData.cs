using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace reports_tcado
{
    public class GeckAPI
    {
        public string api_key { get; set; }
        public GeckoData data { get; set; }
    }
    public class GeckoData
    {
        public GeckoData()
        {
            item = new List<GeckoFunnelItem>();
        }
        public string percentage
        {
            get { return "hide"; }
        }
        public IList<GeckoFunnelItem> item { get; set; }
    }

    public class GeckoFunnelItem
    {
        public string value { get; set; }
        public string label { get; set; }
    }

    public class GeckoCity
    {
        public string city_name { get; set; }
        public string country_code { get; set; }
        public string region_code { get; set; }
    }

    public class GeckoPoint
    {
        public GeckoCity city { get; set; }
        public int size { get; set; }
    }

    public class GeckoPoints
    {
        public GeckoPoints()
        {
            point = new List<GeckoPoint>();
        }
        public IList<GeckoPoint> point { get; set; }
    }
}
