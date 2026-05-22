using SpreadsheetLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ToolsApp.Utilities
{
    public static class UtilsExcel
    {
        public static string GetDataType(string formatCode)
        {
            if (formatCode.Contains("h:mm") || formatCode.Contains("mm:ss"))
            {
                return "Time";
            }
            else if (formatCode.Contains("[$-409]") || formatCode.Contains("[$-F800]") || formatCode.Contains("m/d"))
            {
                return "Date";
            }
            else if (formatCode.Contains("#,##0.0"))
            {
                return "Currency";
            }
            else if (formatCode.Last() == '%')
            {
                return "Percentage";
            }
            else if (formatCode.IndexOf("0") == 0)
            {
                return "Numeric";
            }
            else
            {
                return "String";
            }
        }
        public static string GetValueByDataType(string formatCode, SLDocument sl, int row, int col)
        {
            var dataType = GetDataType(formatCode);

            if (dataType == "Date")
            {
                var model = sl.GetCellValueAsDateTime(row, col).ToString("dd-MM-yyyy");
                return model.Contains("01-01-1900") ? sl.GetCellValueAsString(row, col) : model;
            }
            else if (dataType == "Time")
            {
                return sl.GetCellValueAsDateTime(row, col).ToString("dd-MM-yyyy HH:mm");
            }
            else if (dataType == "Currency")
            {
                return sl.GetCellValueAsDecimal(row, col).ToString();
            }
            else if (dataType == "Percentage")
            {
                return sl.GetCellValueAsDecimal(row, col).ToString();
            }
            else if (dataType == "Numeric")
            {
                return sl.GetCellValueAsDecimal(row, col).ToString();
            }
            else
            {
                return sl.GetCellValueAsString(row, col).ToString();
            }
        }
    }
}