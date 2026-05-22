using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ToolsApp.App_Start;
using ToolsApp.Authentication;
using ToolsApp.EntityFramework.VatTu;
using ToolsApp.Models;
using ToolsApp.Utilities;

namespace ToolsApp.Controllers
{
    [Authorize]
    public class HomeController : BaseController
    {
        wqlvattuEntities db_ = new wqlvattuEntities();
        // GET: Home
        public ActionResult Index(string Id = "")
        {         
            return View();
        }
    }
}