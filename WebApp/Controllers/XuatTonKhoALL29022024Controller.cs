using NSClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using ToolsApp.Authentication;
using ToolsApp.com.netsuite.webservices;
using ToolsApp.EntityFramework.VatTu;
using ToolsApp.EntityFramework.NetsuiteTT;
using ToolsApp.EntityFramework.KhoTP2023;
using ToolsApp.Utilities;
using System.Data.Entity;
using SpreadsheetLight;
using DocumentFormat.OpenXml.Spreadsheet;

namespace ToolsApp.Controllers
{
    [Authorize]
    public class XuatTonKhoALL29022024Controller : BaseController
    {
        private wqlvattuEntities vt_ = new wqlvattuEntities();
        private QUANLYKHOTP2023Entities tp_ = new QUANLYKHOTP2023Entities();
        private NetsuiteTTEntities1 ns_ = new NetsuiteTTEntities1();
  
        // GET: XemXetAllVatTu
        public ActionResult Index()
        {
            ViewBag.MAKHO = ns_.SPLOAD_TONKHOALL_TP_2024().ToList();
            return View();
        }


        public ActionResult _GetList_IA(string MAKHO , string LoaiTon, string Thang)
        {
            var list = tp_.TONKHO290224_InventoryAdjustment(MAKHO, LoaiTon, Thang).ToList();
            ViewBag.List = list;
            ViewBag.MAKHO = MAKHO;
            return PartialView();
        }

        public ActionResult _GetList_IAA(string MAKHO, string LoaiTon, string Thang)
        {
            var list = tp_.TONKHO290224_IAA_Adjustments(MAKHO, LoaiTon, Thang).ToList();
            ViewBag.List = list;
            ViewBag.MAKHO = MAKHO;
            return PartialView();
        }

        public ActionResult _GetList_IAA_ID(string MAKHO, string LoaiTon, string Thang)
        {
            var list = tp_.TONKHO290224_IAA_InventoryDetail(MAKHO, LoaiTon, Thang).ToList();
            ViewBag.List = list;
            ViewBag.MAKHO = MAKHO;
            return PartialView();
        }


        #region Xuất excel IA

        [HttpPost]
        [ValidateInput(false)]
        public JsonResult _ExportExcelFun_IA(string MAKHO,string LoaiTon, string Thang)
        {
            var models = tp_.TONKHO290224_InventoryAdjustment(MAKHO, LoaiTon, Thang).ToList();

            if (models.Count > 0)
            {
                string fileIn = Request.PhysicalApplicationPath + @"UserFiles\Template\" + "TONKHO2024_InventoryAdjustment.xlsx";

                string fileOutName = "01_InventoryAdjustment_" + MAKHO + "_" + Guid.NewGuid().ToString() + ".xlsx";

                SLDocument sl = new SLDocument(fileIn);

                sl.SelectWorksheet(sl.GetSheetNames()[0]);

                SLWorksheetStatistics stats = sl.GetWorksheetStatistics();

                var Month = DateTime.Now.Month.ToString();
                var Year = DateTime.Now.Year.ToString();

                var row = 2;

                var stt = 1;

                #region style
                SLStyle style = sl.CreateStyle();
                style.Border.TopBorder.BorderStyle = BorderStyleValues.Thin;
                style.Border.BottomBorder.BorderStyle = BorderStyleValues.Thin;
                style.Border.LeftBorder.BorderStyle = BorderStyleValues.Thin;
                style.Border.RightBorder.BorderStyle = BorderStyleValues.Thin;
                style.Border.TopBorder.Color = System.Drawing.Color.Black;
                style.Border.BottomBorder.Color = System.Drawing.Color.Black;
                style.Border.LeftBorder.Color = System.Drawing.Color.Black;
                style.Border.RightBorder.Color = System.Drawing.Color.Black;
                #endregion

                foreach (var item in models)
                {
                    //sl.SetCellValue("A" + row.ToString(), stt);
                    sl.SetCellValue("A" + row.ToString(), item.External_ID);
                    sl.SetCellValue("B" + row.ToString(), item.REFERENCE);
                    sl.SetCellValue("C" + row.ToString(), item.ADJUSTMENT_ACCOUNT);
                    sl.SetCellValue("D" + row.ToString(), "29/02/2024");
                    sl.SetCellValue("E" + row.ToString(), item.ADJUSTMENT_LOCATION);


                    row++;

                    stt++;
                }

                sl.SetCellStyle("A2", "E" + (row - 1), style);

                sl.SaveAs(Request.PhysicalApplicationPath + @"UserFiles\Download\" + fileOutName);

                return Json((new { status = 1, title = "", text = "Exported.", obj = "/UserFiles/Download/" + fileOutName }), JsonRequestBehavior.DenyGet);
            }
            else
            {
                return Json((new { status = -1, title = "", text = "Nothing to export.", obj = "" }), JsonRequestBehavior.DenyGet);
            }
        }
        #endregion

        #region Xuất excel IAA

        [HttpPost]
        [ValidateInput(false)]
        public JsonResult _ExportExcelFun_IAA(string MAKHO, string LoaiTon, string Thang)
        {
            var models = tp_.TONKHO290224_IAA_Adjustments(MAKHO, LoaiTon, Thang).ToList();

            if (models.Count > 0)
            {
                string fileIn = Request.PhysicalApplicationPath + @"UserFiles\Template\" + "TONKHO2024_InventoryAdjustment_IAA.xlsx";

                string fileOutName = "02_InventoryAdjustment_IAA_" + MAKHO + "_" + Guid.NewGuid().ToString() + ".xlsx";

                SLDocument sl = new SLDocument(fileIn);

                sl.SelectWorksheet(sl.GetSheetNames()[0]);

                SLWorksheetStatistics stats = sl.GetWorksheetStatistics();

                var row = 2;

                var stt = 1;

                #region style
                SLStyle style = sl.CreateStyle();
                style.Border.TopBorder.BorderStyle = BorderStyleValues.Thin;
                style.Border.BottomBorder.BorderStyle = BorderStyleValues.Thin;
                style.Border.LeftBorder.BorderStyle = BorderStyleValues.Thin;
                style.Border.RightBorder.BorderStyle = BorderStyleValues.Thin;
                style.Border.TopBorder.Color = System.Drawing.Color.Black;
                style.Border.BottomBorder.Color = System.Drawing.Color.Black;
                style.Border.LeftBorder.Color = System.Drawing.Color.Black;
                style.Border.RightBorder.Color = System.Drawing.Color.Black;
                #endregion

                foreach (var item in models)
                {
                    // sl.SetCellValue("" + row.ToString(), stt);
                    sl.SetCellValue("A" + row.ToString(), item.External_ID);
                    sl.SetCellValue("B" + row.ToString(), item.REFERENCE_);
                    sl.SetCellValue("C" + row.ToString(), item.ITEM);
                    sl.SetCellValue("D" + row.ToString(), item.LOCATION);
                    sl.SetCellValue("E" + row.ToString(), (Decimal)item.ADJUST_QTY_BY);
                    sl.SetCellValue("F" + row.ToString(), item.UNIT.ToString());
                    sl.SetCellValue("G" + row.ToString(), Convert.ToDecimal(item.EST_UNIT_COST));
                    //sl.SetCellValue("H" + row.ToString(), item.CHATLUONG);



                    row++;

                    stt++;
                }

                sl.SetCellStyle("A2", "G" + (row - 1), style);

                sl.SaveAs(Request.PhysicalApplicationPath + @"UserFiles\Download\" + fileOutName);

                return Json((new { status = 1, title = "", text = "Exported.", obj = "/UserFiles/Download/" + fileOutName }), JsonRequestBehavior.DenyGet);
            }
            else
            {
                return Json((new { status = -1, title = "", text = "Nothing to export.", obj = "" }), JsonRequestBehavior.DenyGet);
            }
        }
        #endregion

        #region Xuất excel IAA_ID

        [HttpPost]
        [ValidateInput(false)]
        public JsonResult _ExportExcelFun_IAA_ID(string MAKHO, string LoaiTon, string Thang)
        {
            var models = tp_.TONKHO290224_IAA_InventoryDetail(MAKHO,LoaiTon, Thang).ToList();

            if (models.Count > 0)
            {
                string fileIn = Request.PhysicalApplicationPath + @"UserFiles\Template\" + "TONKHO2024_InventoryAdjustment_IAA_ID.xlsx";

                string fileOutName = "03_InventoryAdjustment_IAA_ID_" + MAKHO + "_" + Guid.NewGuid().ToString() + ".xlsx";

                SLDocument sl = new SLDocument(fileIn);

                sl.SelectWorksheet(sl.GetSheetNames()[0]);

                SLWorksheetStatistics stats = sl.GetWorksheetStatistics();

                var row = 2;

                var stt = 1;

                #region style
                SLStyle style = sl.CreateStyle();
                style.Border.TopBorder.BorderStyle = BorderStyleValues.Thin;
                style.Border.BottomBorder.BorderStyle = BorderStyleValues.Thin;
                style.Border.LeftBorder.BorderStyle = BorderStyleValues.Thin;
                style.Border.RightBorder.BorderStyle = BorderStyleValues.Thin;
                style.Border.TopBorder.Color = System.Drawing.Color.Black;
                style.Border.BottomBorder.Color = System.Drawing.Color.Black;
                style.Border.LeftBorder.Color = System.Drawing.Color.Black;
                style.Border.RightBorder.Color = System.Drawing.Color.Black;
                #endregion

                foreach (var item in models)
                {
                    //sl.SetCellValue("A" + row.ToString(), stt);
                    sl.SetCellValue("A" + row.ToString(), item.REFERENCE_);
                    sl.SetCellValue("B" + row.ToString(), item.ITEM);
                    sl.SetCellValue("C" + row.ToString(), item.SERIAL_LOT_NUMBER);
                    sl.SetCellValue("D" + row.ToString(), item.EXPIRATION_DATE);
                    sl.SetCellValue("E" + row.ToString(), (Decimal)item.QUANTITY);

                    row++;

                    stt++;
                }

                sl.SetCellStyle("A2", "E" + (row - 1), style);

                sl.SaveAs(Request.PhysicalApplicationPath + @"UserFiles\Download\" + fileOutName);

                return Json((new { status = 1, title = "", text = "Exported.", obj = "/UserFiles/Download/" + fileOutName }), JsonRequestBehavior.DenyGet);
            }
            else
            {
                return Json((new { status = -1, title = "", text = "Nothing to export.", obj = "" }), JsonRequestBehavior.DenyGet);
            }
        }
        #endregion

    }
}