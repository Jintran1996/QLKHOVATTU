using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ToolsApp.Utilities
{
    public static class Enum
    {
        public static List<SelectListItem> ListYearFrom2020(bool All = false)
        {
            var list = new List<SelectListItem>();

            #region All
            if (All)
            {
                list.Add(new SelectListItem { Value = "0", Text = "--All--" });
            }
            #endregion

            for (int i = 2020; i <= DateTime.Now.Year; i++)
            {
                list.Add(new SelectListItem { Value = i.ToString(), Text = i.ToString() });
            }

            return list;
        }
        public static List<SelectListItem> ListMonth(bool All = false)
        {
            var list = new List<SelectListItem>();

            #region All
            if (All)
            {
                list.Add(new SelectListItem { Value = "0", Text = "--All--" });
            }
            #endregion

            for (int i = 1; i <= 12; i++)
            {
                list.Add(new SelectListItem { Value = i.ToString(), Text = i.ToString() });
            }

            return list;
        }
        public static string[] ListYearFrom2008()
        {
            var len = DateTime.Now.Year - 2008;
            string[] returtData = new string[len + 1];
            var j = 0;
            for (int i = DateTime.Now.Year; i >= 2008; i--)
            {
                returtData[j] = i.ToString();
                j++;
            }
            return returtData;
        }
        public static List<SelectListItem> ListGender(bool All = false)
        {
            var list = new List<SelectListItem>();

            #region All
            if (All)
            {
                list.Add(new SelectListItem { Value = "", Text = "--All--" });
            }
            #endregion

            list.Add(new SelectListItem { Value = "True", Text = "Male" });
            list.Add(new SelectListItem { Value = "False", Text = "Female" });

            return list;
        }
        public static List<SelectListItem> ListStringActiveCode(bool All = false)
        {
            var list = new List<SelectListItem>();

            #region All
            if (All)
            {
                list.Add(new SelectListItem { Value = "", Text = "--All--" });
            }
            #endregion

            list.Add(new SelectListItem { Value = "A", Text = "Active" });
            list.Add(new SelectListItem { Value = "D", Text = "Delete" });

            return list;
        }
    }
}
