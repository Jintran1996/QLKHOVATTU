
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using ToolsApp.Authentication;
using ToolsApp.EntityFramework.KhoSoi;
using ToolsApp.EntityFramework.VatTu;
using System.Net;
using NSClient;
using ToolsApp.com.netsuite.webservices;

using ToolsApp.EntityFramework.NhuomAH;
using ToolsApp.Utilities;
using System.Data.Entity;
using ToolsApp.Helper;

namespace ToolsApp.Controllers
{
    [Authorize]
    public class YeuCauHoaChatCongDoanController : BaseController
    {
        private wqlkhosoiEntities db_ = new wqlkhosoiEntities();
        private wqlvattuEntities dbvt = new wqlvattuEntities();
        private QLSX_Nhuom_AnhHongEntities nhuomah = new QLSX_Nhuom_AnhHongEntities();
        // GET: XemXet_BM01
        public ActionResult Index()
        {
            #region công đoạn
            ViewBag.congdoan = nhuomah.Spload_tblCongDoan().ToList();
            ViewBag.noilayhc = nhuomah.NOILAYHOACHATs.ToList();
            ViewBag.mahang = nhuomah.XuatHCTN_theotruc
                           .Where(p => (p.QCxacnhan ?? false) == false)
                           .GroupBy(p => p.MaMH)
                           .Select(g => g.Key)
                           .ToList();
            #endregion


            return View();
        }
        public ActionResult _GetList(string congdoan, string mahang)
        {
           
            var list = nhuomah.SPLOAD_XUATHCTN_THEOTRUC(congdoan, mahang, "NO1").ToList();
            ViewBag.List = list;
            return PartialView();
        }

        public ActionResult _GetList2(string congdoan, string mahang)
        {

            var list2 = nhuomah.SPLOAD_XUATHCTN_THEOTRUC(congdoan, mahang, "NO2").ToList();
            ViewBag.List2 = list2;
            return PartialView();
        }






    }
}