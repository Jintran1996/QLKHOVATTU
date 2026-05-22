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
    public class TonghopxuatnhapBM05Controller : BaseController
    {
        private wqlvattuEntities vt_ = new wqlvattuEntities();
        private QUANLYKHOTP2023Entities tp_ = new QUANLYKHOTP2023Entities();
        private NetsuiteTTEntities1 ns_ = new NetsuiteTTEntities1();
        // GET: XemXetAllVatTu
        public ActionResult Index()
        {
            ViewBag.MAKHO = ns_.SPLOAD_TONKHOALL_TP_2024().ToList();
            ViewBag.NAM = vt_.DMNAMs.Where(p => p.HieuLuc == 1).ToList();
            ViewBag.THANG = vt_.DMTHANGs.Where(p => p.HieuLuc == true).ToList();
            ViewBag.MADVI = vt_.VATTU2024_SPLOAD_DANHMUCBOPHAN().ToList();
            ViewBag.GETYEAR = DateTime.Now.Year.ToString();
            ViewBag.GETMONTH = DateTime.Now.ToString("MM");
            return View();
        }


        public ActionResult _GetList(int NAM, string THANG)
        {
            var boolThang = (THANG == "ALL") ? true : false;
            int THANG_ = THANG == "ALL" ? 01 : Convert.ToInt16(THANG);


            var list = vt_.VATTU2024_SP_VATTU_BAOCAOTONGHOPBM05_ALLTHANG(true, boolThang, NAM, THANG_,"","").ToList();
            ViewBag.List = list;      
            
            return PartialView();
        }


        #region Xuất excel

        [HttpPost]
        [ValidateInput(false)]
        public JsonResult _ExportExcelFun(int NAM, string THANG)
        {
            var boolThang = (THANG == "ALL") ? true : false;
            int THANG_ = THANG == "ALL" ? 01 : Convert.ToInt16(THANG);

      
            if(boolThang == true)
            {
                var models = vt_.VATTU2024_SP_VATTU_BAOCAOTONGHOPBM05_ALLTHANG(true, boolThang, NAM, THANG_, "", "").ToList();
                if (models.Count > 0)
                {
                    string fileIn = Request.PhysicalApplicationPath + @"UserFiles\Template\" + "BAOCAOTONGHOPBM05_ALLTHANG.xlsx";

                    string fileOutName = "BAOCAOTONGHOPBM05_ALLTHANG" + Guid.NewGuid().ToString() + ".xlsx";

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
                        sl.SetCellValue("A" + row.ToString(), stt);
                        sl.SetCellValue("B" + row.ToString(), item.MAVT);
                        sl.SetCellValue("C" + row.ToString(), item.TENVT);
                        sl.SetCellValue("D" + row.ToString(), item.DVT);
                        sl.SetCellValue("E" + row.ToString(), item.MABP);
                        sl.SetCellValue("F" + row.ToString(), item.TENBP);
                        sl.SetCellValue("G" + row.ToString(), item.MADV);
                        sl.SetCellValue("H" + row.ToString(), item.NAM.ToString());
                        sl.SetCellValue("I" + row.ToString(), (Double)item.January);
                        sl.SetCellValue("J" + row.ToString(), (Double)item.February);
                        sl.SetCellValue("K" + row.ToString(), (Double)item.March);
                        sl.SetCellValue("L" + row.ToString(), (Double)item.April);
                        sl.SetCellValue("M" + row.ToString(), (Double)item.May);
                        sl.SetCellValue("N" + row.ToString(), (Double)item.June);
                        sl.SetCellValue("O" + row.ToString(), (Double)item.July);
                        sl.SetCellValue("P" + row.ToString(), (Double)item.August);
                        sl.SetCellValue("Q" + row.ToString(), (Double)item.September);
                        sl.SetCellValue("R" + row.ToString(), (Double)item.October);
                        sl.SetCellValue("S" + row.ToString(), (Double)item.November);
                        sl.SetCellValue("T" + row.ToString(), (Double)item.December);
                   

                        row++;

                        stt++;
                    }

                    sl.SetCellStyle("A2", "T" + (row - 1), style);

                    sl.SaveAs(Request.PhysicalApplicationPath + @"UserFiles\Download\" + fileOutName);

                    return Json((new { status = 1, title = "", text = "Exported.", obj = ToolsApp.Utilities.AppParameters.FolderDownload + fileOutName }), JsonRequestBehavior.DenyGet);
                }
                else
                {
                    return Json((new { status = -1, title = "", text = "Nothing to export.", obj = "" }), JsonRequestBehavior.DenyGet);
                }
            }   
            else
            {
                var models2 = vt_.VATTU2024_SP_VATTU_BAOCAOTONGHOPBM05_THEOTHANG(true, boolThang, NAM, THANG_, "", "").ToList();
                if (models2.Count > 0)
                {

                    string fileIn = Request.PhysicalApplicationPath + @"UserFiles\Template\" + "BAOCAOTONGHOPBM05_THEOTHANG.xlsx";

                    string fileOutName = "BAOCAOTONGHOPBM05_THEOTHANG_" + Guid.NewGuid().ToString() + ".xlsx";

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

                    foreach (var item in models2)
                    {
                        sl.SetCellValue("A" + row.ToString(), stt);
                        sl.SetCellValue("B" + row.ToString(), item.MAPHIEU);
                        sl.SetCellValue("C" + row.ToString(), item.MAPHIEUPD);
                        sl.SetCellValue("D" + row.ToString(), item.MAVT);                     
                        sl.SetCellValue("E" + row.ToString(), (Decimal)item.SLPD);
                        sl.SetCellValue("F" + row.ToString(), item.GHICHU);
                        sl.SetCellValue("G" + row.ToString(), item.MABP);
                        sl.SetCellValue("H" + row.ToString(), item.TENBP);
                        sl.SetCellValue("I" + row.ToString(), item.MADV);
                        sl.SetCellValue("J" + row.ToString(), item.KEHOACHTHANG.Value.ToString("dd/MM/yyyy"));
                        sl.SetCellValue("K" + row.ToString(), item.TENVT);
                        sl.SetCellValue("L" + row.ToString(), item.DVT);

                        row++;

                        stt++;
                    }

                    sl.SetCellStyle("A2", "L" + (row - 1), style);

                    sl.SaveAs(Request.PhysicalApplicationPath + @"UserFiles\Download\" + fileOutName);

                    return Json((new { status = 1, title = "", text = "Exported.", obj = ToolsApp.Utilities.AppParameters.FolderDownload + fileOutName }), JsonRequestBehavior.DenyGet);
                }
                else
                {
                    return Json((new { status = -1, title = "", text = "Nothing to export.", obj = "" }), JsonRequestBehavior.DenyGet);
                }
            }
        
        }
        #endregion

    }
}