using NLog.Internal;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolsApp.Utilities
{
    public static class Utils
    {
        public static class WebConfigKey
        {
            public static string SupperPassword = ConfigurationManager.AppSettings["AdminPass"].ToString();
            public static String Domain { get { return ConfigurationManager.AppSettings["Domain"].ToString(); } }
        }
        public static class Enums
        {

        }
    }
}
