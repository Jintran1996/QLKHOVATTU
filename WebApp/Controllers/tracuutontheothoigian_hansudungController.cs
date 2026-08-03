using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ToolsApp.Authentication;
using ToolsApp.com.netsuite.webservices;
using ToolsApp.EntityFramework.KhoSoi;
using ToolsApp.EntityFramework.VatTu;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using System.IO;
using System.Drawing;
using static System.Collections.Specialized.BitVector32;
using System.Web.UI.WebControls;
using ToolsApp.Models;
using System.Collections;
using System.Drawing.Imaging;
using Image = System.Drawing.Image;
using System.Configuration;
using ToolsApp.Helper;
using SpreadsheetLight;
using DocumentFormat.OpenXml.Spreadsheet;


namespace ToolsApp.Controllers
{
    public class tracuutontheothoigian_hansudungController : BaseController
    {
        #region Database

        private wqlvattuEntities dbvt = new wqlvattuEntities();
     
        #endregion

        #region Index
        public ActionResult Index()
        {
            var locations = dbvt.VATTU2024_DMKHO.ToList();
            ViewBag.locations = locations;
            return View();
        }
        #endregion
    

        #region Danh sách phiếu gần nhất  1
        [HttpPost]
        public ActionResult GetList1(SearchTraCuuTonTheoThoiGian_HanSuDung src)
        {
            var Makho = src.MAKHOSEARCH_ ?? "";         
            src.DENNGAY_ = Helper.DateHelper.ParseNgay(src.DENNGAY);
            var MAVT = src.MAVTSearch ?? "";

            var list = dbvt.VATTU2026_SP_BAOCAOTONKHOTHEOTHOIGIAN_HANSUDUNG(Makho, src.DENNGAY_, MAVT)
                           .ToList();

            ViewBag.listPhieu = list;

            return PartialView(list);
        }
        #endregion





    }
}