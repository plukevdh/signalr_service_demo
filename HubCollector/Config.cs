using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubCollector
{
    public class Config 
    {

        public static string Hostname
        {
            get { return ConfigurationManager.AppSettings["Statsd.Hostname"]; }
        }

        public static int Port
        {
            get { return Convert.ToInt32(ConfigurationManager.AppSettings["Statsd.Port"]); }
        }

        public static List<string> Hubs
        {
            get { return ConfigurationManager.AppSettings["SignalR.Hubs"].Split(',').ToList(); }
        }

        public static string NameFromUrl(string url)
        {
            return url.Split(".-".ToCharArray())[1];
        }
    }
}
