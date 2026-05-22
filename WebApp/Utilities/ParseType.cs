using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace ToolsApp.Utilities
{
    public static class ParseType
    {
        public static int TryParseInt(string val)
        {
            try
            {
                int result;
                int.TryParse(val, out result);
                return result;
            }
            catch
            {
                return 0;
            }
        }

        public static long TryParseLong(string val)
        {
            long result;
            long.TryParse(val, out result);
            return result;
        }

        public static float TryParseFloat(string val)
        {
            float result;
            float.TryParse(val, out result);
            return result;
        }

        public static DateTime TryParseDateTime(string val)
        {
            DateTime result;
            DateTime.TryParse(val, out result);
            return result;
        }

        public static double TryParseDouble(string val)
        {
            double result;
            double.TryParse(val, out result);
            return result;
        }

        public static bool TryParseBolean(string val)
        {
            bool result;
            bool.TryParse(val, out result);
            return result;
        }

        public static DateTime TryParseDatetimeDMY(string val)
        {
            #region Xử lý ngày
            CultureInfo cul = CultureInfo.GetCultureInfo("en-GB");
            var result = new DateTime();
            if (!string.IsNullOrEmpty(val))
            {
                try
                {
                    result = DateTime.ParseExact(val, "dd/MM/yyyy", cul);

                    result = new DateTime(result.Year,
                        result.Month, result.Day, 0, 0, 0);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
            #endregion

            return result;
        }
    }
}