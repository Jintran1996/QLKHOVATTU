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
    public class tracuutontheomavtController : BaseController
    {
        #region Database
        NSClient.NSClient ns = new NSClient.NSClient();
        private wqlvattuEntities dbvt = new wqlvattuEntities();
        private wqlkhosoiEntities soi_ = new wqlkhosoiEntities();
        #endregion

        #region Index
        public ActionResult Index()
        {
            var locations = dbvt.VATTU2024_DMKHO.ToList();
            ViewBag.locations = locations;
            return View();
        }
        #endregion

      

        #region Danh sách phiếu gần nhất 2
        [HttpPost]
        public ActionResult GetList2(SearchTraCuuXNKho src)
        {
            var listKho = src.MAKHOSEARCH_;
            var MAVT = src.MAVTSearch ?? "";

            string maKhoCsv = (listKho != null && listKho.Any())
                                ? string.Join(",", listKho)
                                : "";

            var list = dbvt.VATTU2025_SPLOAD_SOLUONGTON_THEOMAVT(MAVT, maKhoCsv)   
                .ToList();


            ViewBag.listPhieu = list;

            return PartialView(list);
        }
        #endregion
    

        #region Danh sách phiếu gần nhất  1
        [HttpPost]
        public ActionResult GetList1(SearchTraCuuXNKho src)
        {
            var listKho = src.MAKHOSEARCH_;
            var MAVT = src.MAVTSearch ?? "";

            string maKhoCsv = (listKho != null && listKho.Any())
                                ? string.Join(",", listKho)
                                : "";

            var list = dbvt.VATTU2026_SPLOAD_TRACUUNHANH_SOLUONGTON_THEOKHO(MAVT, maKhoCsv)
                           .ToList();

            ViewBag.listPhieu = list;

            return PartialView(list);
        }
        #endregion



        #region Đồng bộ (nhập kho)
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _SEARCHNETSUITEMAVT(SearchTraCuuXNKho src)
        {
            var listKho = src.MAKHOSEARCH_;
            var MAVT = src.MAVTSearch ?? "";

            string maKhoCsv = (listKho != null && listKho.Any())
                                ? string.Join(",", listKho)
                                : "";

            var itemcheck = CUSTOMSEARCH.SEARCH_ITEM_INTERNALID(MAVT, maKhoCsv);           

            return Json(new { status = 1, title = "", text = "THÀNH CÔNG"}, JsonRequestBehavior.AllowGet);

        }
        #endregion

    }
}