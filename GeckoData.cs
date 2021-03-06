﻿using System;
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
            percentage = "hide";
        }
        public string percentage { get; set; }
        public IList<GeckoFunnelItem> item { get; set; }
    }

    public class GeckoList
    {
        public GeckoList()
        {
            data = new List<GeckoListItem>();
        }
        public string api_key { get; set; }
        public IList<GeckoListItem> data { get; set; }
    }

    public class GeckoListTitle
    {
        public string text { get; set; }
    }

    public class GeckoListItem
    {
        public GeckoListTitle title { get; set; }
        public string description { get; set; }
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
