using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ToolsApp.Utilities
{
    public static class Extensions
    {
        public static string Left(this string input, int count)
        {
            return input.Substring(0, Math.Min(input.Length, count));
        }
        public static string Right(this string input, int count)
        {
            return input.Substring(Math.Max(input.Length - count, 0), Math.Min(count, input.Length));
        }
        public static string Mid(this string input, int start)
        {
            return input.Substring(Math.Min(start, input.Length));
        }
        public static string Mid(this string input, int start, int count)
        {
            return input.Substring(Math.Min(start, input.Length), Math.Min(count, Math.Max(input.Length - start, 0)));
        }
    }
}