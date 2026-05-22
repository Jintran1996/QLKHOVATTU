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
using System.Globalization;

namespace ToolsApp.Controllers
{
    public class tracuuxuatnhaptontheokyController : BaseController
    {
        #region Database
        NSClient.NSClient ns = new NSClient.NSClient();
        private wqlvattuEntities dbvt = new wqlvattuEntities();
        private wqlkhosoiEntities soi_ = new wqlkhosoiEntities();
        #endregion

        #region Index
        public ActionResult Index()
        {
            var locations = dbvt.VATTU2024_SP_LOADKHOQUYENNHAP("","1").ToList();
            ViewBag.locations = locations;
            return View();
        }
        #endregion
    

        #region Danh sách phiếu gần nhất  1
        [HttpPost]
        public ActionResult GetList(SearchTraCuuXNKho src)
        {

            #region Xử lý ngày
            CultureInfo cul = CultureInfo.GetCultureInfo("en-GB");
            var Tungayz = new DateTime();
            var Denngayz = new DateTime();
            Tungayz = DateTime.ParseExact(src.TUNGAY, "dd/MM/yyyy", cul);
            Denngayz = DateTime.ParseExact(src.DENNGAY, "dd/MM/yyyy", cul);
 
            src.TUNGAY_ = new DateTime(Tungayz.Year,Tungayz.Month, Tungayz.Day, 0, 0, 0);
            src.DENNGAY_ = new DateTime(Denngayz.Year, Denngayz.Month, Denngayz.Day, 23, 59, 59);
            #endregion


            var listKho = src.MAKHOSEARCH_;          

            string maKhoCsv = (listKho != null && listKho.Any())
                                ? string.Join(",", listKho)
                                : "";

            var list = dbvt.VATTU2026_SP_RPT_XUATNHAPTON_THEOKHO(src.TUNGAY_, src.DENNGAY_, maKhoCsv)
                           .ToList();

            ViewBag.listPhieu = list;

            return PartialView(list);
        }
        #endregion

        #region Danh sách phiếu gần nhất  1
        [HttpPost]
        public ActionResult GetList2(SearchTraCuuXNKho src)
        {

            #region Xử lý ngày
            CultureInfo cul = CultureInfo.GetCultureInfo("en-GB");
            var Tungayz = new DateTime();
            var Denngayz = new DateTime();
            Tungayz = DateTime.ParseExact(src.TUNGAY, "dd/MM/yyyy", cul);
            Denngayz = DateTime.ParseExact(src.DENNGAY, "dd/MM/yyyy", cul);

            src.TUNGAY_ = new DateTime(Tungayz.Year, Tungayz.Month, Tungayz.Day, 0, 0, 0);
            src.DENNGAY_ = new DateTime(Denngayz.Year, Denngayz.Month, Denngayz.Day, 23, 59, 59);
            #endregion


            var listKho = src.MAKHOSEARCH_;

            string maKhoCsv = (listKho != null && listKho.Any())
                                ? string.Join(",", listKho)
                                : "";

            var list = dbvt.VATTU2026_SP_RPT_XUATNHAPTON_THEONHIEUKHO(src.TUNGAY_, src.DENNGAY_, maKhoCsv)
                           .ToList();

            ViewBag.listPhieu = list;

            return PartialView(list);
        }
        #endregion

        #region Danh sách phiếu gần nhất  3
        [HttpPost]
        public ActionResult GetList3(SearchTraCuuXNKho src)
        {

            #region Xử lý ngày
            CultureInfo cul = CultureInfo.GetCultureInfo("en-GB");
            var Tungayz = new DateTime();
            var Denngayz = new DateTime();
            Tungayz = DateTime.ParseExact(src.TUNGAY, "dd/MM/yyyy", cul);
            Denngayz = DateTime.ParseExact(src.DENNGAY, "dd/MM/yyyy", cul);

            src.TUNGAY_ = new DateTime(Tungayz.Year, Tungayz.Month, Tungayz.Day, 0, 0, 0);
            src.DENNGAY_ = new DateTime(Denngayz.Year, Denngayz.Month, Denngayz.Day, 23, 59, 59);
            #endregion


            var listKho = src.MAKHOSEARCH_;

            string maKhoCsv = (listKho != null && listKho.Any())
                                ? string.Join(",", listKho)
                                : "";

            var list = dbvt.VATTU2026_SP_VATTU_LOADVATTUNHAPKHO_THUKHO_DIENGIAI_XUATNHAPKHO(src.TUNGAY_, src.DENNGAY_, maKhoCsv)
                           .ToList();

            ViewBag.listPhieu = list;

            return PartialView(list);
        }
        #endregion





    }
}