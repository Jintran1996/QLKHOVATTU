using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using ToolsApp.EntityFramework.KhoSoi;
using ToolsApp.EntityFramework.NetsuiteTT;
using ToolsApp.EntityFramework.VatTu;

namespace ToolsApp.Helper
{
    public static class HtmlHelperExtentions
    {
        #region Active link
        public static string IsActive(this HtmlHelper htmlHelper, string action, string controller)
        {
            var routeData = htmlHelper.ViewContext.RouteData;
            var routeAction = routeData.Values["action"].ToString();
            var routeController = routeData.Values["controller"].ToString();

            #region Controllers
            var returnControllersActive = false;
            //var actions = action.Split(',').ToArray();
            var controllers = controller.Split(',').ToArray();
            if (controllers.Contains(routeController))
            {
                returnControllersActive = true;
            }
            #endregion

            #region Actions
            var returnActionsActive = false;
            var actions = action.Split(',').ToArray();
            if (actions.Contains(routeAction))
            {
                returnActionsActive = true;
            }
            #endregion

            return (returnControllersActive && returnActionsActive) ? "active" : "";
        }
        public static string IsOpen(this HtmlHelper htmlHelper, string action, string controller)
        {
            var routeData = htmlHelper.ViewContext.RouteData;
            var routeAction = routeData.Values["action"].ToString();
            var routeController = routeData.Values["controller"].ToString();

            #region Controllers
            var returnControllersActive = false;
            //var actions = action.Split(',').ToArray();
            var controllers = controller.Split(',').ToArray();
            if (controllers.Contains(routeController))
            {
                returnControllersActive = true;
            }
            #endregion

            #region Actions
            var returnActionsActive = false;
            var actions = action.Split(',').ToArray();
            if (actions.Contains(routeAction))
            {
                returnActionsActive = true;
            }
            #endregion

            return (returnControllersActive && returnActionsActive) ? "menu-open" : "";
        }
        #endregion

        public static IHtmlString reCaptcha(this HtmlHelper helper)
        {
            StringBuilder sb = new StringBuilder();
            string publickey = WebConfigurationManager.AppSettings["Recaptcha_SiteKey"];
            sb.AppendLine("<script type=\"text/javascript\" src='https://www.google.com/recaptcha/api.js'></script>");
            sb.AppendLine("");
            sb.AppendLine("<div class=\"g-recaptcha\" data-sitekey=\"" + publickey + "\"></div>");
            return MvcHtmlString.Create(sb.ToString());

        }
    }

    public static class DateHelper
    {
        public static DateTime? ParseNgay(string ngayString)
        {
            if (string.IsNullOrEmpty(ngayString))
                return null;

            try
            {
                CultureInfo cul = CultureInfo.GetCultureInfo("en-GB");
                DateTime ngay = DateTime.ParseExact(ngayString, "dd/MM/yyyy", cul);
                return new DateTime(ngay.Year, ngay.Month, ngay.Day, 0, 0, 0);
            }
            catch
            {
                return null;
            }
        }
    }

  
}