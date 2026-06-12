using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using ToolsApp.Authentication;
using ToolsApp.EntityFramework.KhoSoi;
using ToolsApp.EntityFramework.VatTu;
using ToolsApp.EntityFramework.NetsuiteTT;
using ToolsApp.EntityFramework.DonHang09;
using System.Net;
using NSClient;
using ToolsApp.com.netsuite.webservices;
using ToolsApp.Models;
using System.Data.Entity;
using System.Data;
using System.Globalization;
using ToolsApp.Utilities;
using DocumentFormat.OpenXml.Spreadsheet;
using SpreadsheetLight;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml;
using System.IO;
using Rotativa;
using Rotativa.Options;
using System.Text;


namespace ToolsApp.Controllers
{
    public class InphieuchatluongdonviController : BaseController
    {        
        private wqlvattuEntities vt_ = new wqlvattuEntities();
        // GET: KiemTraChatLuongVTSOI
        public ActionResult Index()
        {       
            return View();
        }
        #region Load View Vật Tư
        public ActionResult _GetList(string SOCTVT)
        {
            try
            {             
               var kihieu = vt_.SP_LOAD_KIHIEUDV_BY_MANV(User.UserName).FirstOrDefault().KiHieu;
               var data = vt_.SP_INPHIEUCHATLUONG_LOAD(kihieu, SOCTVT).ToList();
               ViewBag.data = data;
               return PartialView();
            }
            catch
            {
               string errorMessage = "Đã xảy ra lỗi khi xử lý yêu cầu.";
               return Content(errorMessage);
            }
        }

        public ActionResult _GetListHD(string SoHoaDon)
        {
            try
            {
                var kihieu = vt_.SP_LOAD_KIHIEUDV_BY_MANV(User.UserName).FirstOrDefault().KiHieu;
                var data = vt_.VATTU2025_SP_INPHIEUCHATLUONGDONVI_THEOHOADON_LOAD(kihieu, SoHoaDon).ToList();
                ViewBag.data = data;
                
                return PartialView();
            }
            catch
            {
                string errorMessage = "Đã xảy ra lỗi khi xử lý yêu cầu.";
                return Content(errorMessage);
            }

        }

        #endregion
        

        #region BGĐ xác nhận
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult BGDXacNhanFun(string SOCTXN)
        {
            try
            {
                // 1. Kiểm tra quyền xem xét
                var nhanVien = vt_.DMNHANVIENs
                    .FirstOrDefault(p => p.MANV == User.UserName);

                if (nhanVien == null || nhanVien.XemXet != 1)
                {
                    return Json(new
                    {
                        status = -1,
                        title = "",
                        text = "BẠN KHÔNG ĐƯỢC PHÉP XÁC NHẬN. CHỈ BAN GIÁM ĐỐC ĐƠN VỊ MỚI ĐƯỢC PHÉP XÁC NHẬN CHẤT LƯỢNG.",
                        obj = ""
                    }, JsonRequestBehavior.AllowGet);
                }

                // 2. Kiểm tra phiếu chất lượng
                var item = vt_.CHATLUONGs
                    .FirstOrDefault(p => p.SOCT == SOCTXN);

                if (item == null)
                {
                    return Json(new
                    {
                        status = -1,
                        title = "",
                        text = "KHÔNG TÌM THẤY PHIẾU CHẤT LƯỢNG.",
                        obj = ""
                    }, JsonRequestBehavior.AllowGet);
                }

                item.DVXN = true;
                item.GDDV = User.UserName;
                vt_.Entry(item).State = EntityState.Modified;
                vt_.SaveChanges();
                return Json(new { status = 1, title = "", text = "Xác nhận thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Print 

        #region in báo cáo
        public ActionResult _PrintReport(string SOCTVT)
        {
            try
            {
                if(string.IsNullOrEmpty(SOCTVT))
                {
                    return Json(new { status = -1, title = "", text = "Vui lòng chọn Số CT", obj = "" }, JsonRequestBehavior.AllowGet);
                }    
                var ks = vt_.CHATLUONGs.Where(p => p.SOCT == SOCTVT).FirstOrDefault();
                if (ks.DVKT == false)
                {
                    return Json(new { status = -1, title = "", text = "Chưa kiểm tra không thể in", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                string PCName = GetComputerName();
                string IPPC = GetIPClient();
                string log = "Đã in phiếu kiểm tra chất lượng: " + SOCTVT.Trim();
                string log1 = "Đã xác nhận đơn vị kiểm tra chất lượng: " + SOCTVT.Trim();
                ViewData["URL_IMAGE"] = Utils.WebConfigKey.Domain;
                var baoCao = vt_.SP_CRCHATLUONG_NEW(SOCTVT).ToList();
                ViewBag.inVT = baoCao;
                var first = baoCao.FirstOrDefault();
                if (first != null)
                {
                    ViewBag.Base64ImageGDDV = ToBase64Image(first.CHUKY_GDDV);
                    ViewBag.Base64ImageThuKho = ToBase64Image(first.CHUKY_THUKHO);
                }

                var ghiLog = vt_.SP_GHIFILELOG(User.UserName, PCName, IPPC, log, DateTime.Now, "");
                var ghiLog1 = vt_.SP_GHIFILELOG(User.UserName, PCName, IPPC, log1, DateTime.Now, "");

                #region Update đã in
                ks.DaIn = true;
                vt_.Entry(ks).State = EntityState.Modified;
                vt_.SaveChanges();
                #endregion


                return PartialView();
            }

            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult _PrintReportSoHD(string SOHD = null, string Makho = null)
        {
            try
            {
                if (string.IsNullOrEmpty(SOHD))
                {
                    return Json(new { status = -1, title = "", text = "Vui lòng chọn số hóa đơn", obj = "" }, JsonRequestBehavior.AllowGet);
                }
         

                ViewData["URL_IMAGE"] = Utils.WebConfigKey.Domain;
                var baoCao = vt_.SP_CRCHATLUONG_THEOHOADON(SOHD, Makho).ToList();
                ViewBag.inVT = baoCao;
                var first = baoCao.FirstOrDefault();
                if (first != null)
                {
                    ViewBag.Base64ImageGDDV = ToBase64Image(first.CHUKY_GDDV);
                    ViewBag.Base64ImageThuKho = ToBase64Image(first.CHUKY_THUKHO);
                }             

                return PartialView();
            }

            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }

        private string ToBase64Image(byte[] bytes)
        {
            if (bytes == null || bytes.Length < 10)
                return null;

            // PNG: 89 50 4E 47
            if (bytes[0] == 0x89 && bytes[1] == 0x50 &&
                bytes[2] == 0x4E && bytes[3] == 0x47)
            {
                return "data:image/png;base64," + Convert.ToBase64String(bytes);
            }

            // JPG: FF D8
            if (bytes[0] == 0xFF && bytes[1] == 0xD8)
            {
                return "data:image/jpeg;base64," + Convert.ToBase64String(bytes);
            }

            // ❌ Không phải ảnh web → bỏ
            return null;
        }

        #endregion


        public string GetComputerName()
        {
         
            try
            {
                string clientMachineName;
                clientMachineName = (Dns.GetHostEntry(Request.ServerVariables["remote_addr"]).HostName);

                return clientMachineName;
            }
            catch (Exception )
            {
                return string.Empty;
            }
        }

        public string GetIPClient()
        {
            try
            {
                string IP;
                IP = Request.UserHostAddress;

                return IP;
            }
            catch (Exception )
            {
                return "";
            }
        }
        #endregion

        #region Print PDF
        public ActionResult ExportPdf(string soctvt)
        {
            if (string.IsNullOrEmpty(soctvt))
                return Content("Thiếu số chứng từ");

            var baoCao = vt_.SP_CRCHATLUONG_NEW(soctvt).ToList();

            if (!baoCao.Any())
                return Content("Không có dữ liệu");

            ViewData["URL_IMAGE"] = Utils.WebConfigKey.Domain;
            ViewBag.inVT = baoCao;

            var first = baoCao.FirstOrDefault();
            if (first != null)
            {
                ViewBag.Base64ImageGDDV = ToBase64Image(first.CHUKY_GDDV);
                ViewBag.Base64ImageThuKho = ToBase64Image(first.CHUKY_THUKHO);
            }

            return new Rotativa.ViewAsPdf("_PrintReport")
            {
                FileName = $"PhieuChatLuong_{soctvt}.pdf",
             //   PageSize = Rotativa.Options.Size.A4,
              //  PageOrientation = Rotativa.Options.Orientation.Portrait,
                CustomSwitches = "--disable-smart-shrinking"
            };
        }
        #endregion

        #region Print EXcel
        public ActionResult ExportExcel(string soctvt)
        {
            if (string.IsNullOrEmpty(soctvt))
                return Content("Thiếu số chứng từ");

            var data = vt_.SP_CRCHATLUONG_NEW(soctvt).ToList();
            if (!data.Any())
                return Content("Không có dữ liệu");

            using (var sl = new SLDocument())
            {
                int row = 1;

                // Header
                sl.SetCellValue(row, 1, "STT");
                sl.SetCellValue(row, 2, "Mã SP");
                sl.SetCellValue(row, 3, "Tên SP");
                sl.SetCellValue(row, 4, "ĐVT");
                sl.SetCellValue(row, 5, "SL Thực");
                sl.SetCellValue(row, 6, "Kho kiểm tra");
                sl.SetCellValue(row, 7, "Dạng lỗi");
                sl.SetCellValue(row, 8, "Kết quả");
                sl.SetCellValue(row, 9, "Nhận / Loại");
                sl.SetCellValue(row, 10, "NCC");
                sl.SetCellValue(row, 11, "SL NCC");

                row++;

                int stt = 1;
                foreach (var item in data)
                {
                    sl.SetCellValue(row, 1, stt++);
                    sl.SetCellValue(row, 2, item.MAVT);
                    sl.SetCellValue(row, 3, item.TENVT);
                    sl.SetCellValue(row, 4, item.DVT);
                    sl.SetCellValue(row, 5, item.THUC);
                    sl.SetCellValue(row, 6, item.KHOKIEMTRA);
                    sl.SetCellValue(row, 7, item.DANGLOI);
                    sl.SetCellValue(row, 8, item.LOAI);
                    sl.SetCellValue(row, 9, item.HOTENNGUOIKT);
                    sl.SetCellValue(row, 10, item.TENVATTU_NCC);
                    sl.SetCellValue(row, 11, item.SOLUONGNCC == null ? 0 :(decimal)item.SOLUONGNCC);
                    row++;
                }

                using (var ms = new MemoryStream())
                {
                    sl.SaveAs(ms);
                    return File(
                        ms.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"PhieuChatLuong_{soctvt}.xlsx"
                    );
                }
            }
        }
        #endregion
        #region Word
        public ActionResult ExportWord(string soctvt)
        {
            if (string.IsNullOrEmpty(soctvt))
                return Content("Thiếu số chứng từ");

            var baoCao = vt_.SP_CRCHATLUONG_NEW(soctvt).ToList();
            if (!baoCao.Any())
                return Content("Không có dữ liệu");

            ViewData["URL_IMAGE"] = Utils.WebConfigKey.Domain;
            ViewBag.inVT = baoCao;

            var first = baoCao.FirstOrDefault();
            if (first != null)
            {
                ViewBag.Base64ImageGDDV = ToBase64Image(first.CHUKY_GDDV);
                ViewBag.Base64ImageThuKho = ToBase64Image(first.CHUKY_THUKHO);
            }

            string html = RenderViewToString("_PrintReport");

            byte[] bytes = Encoding.UTF8.GetBytes(html);

            return File(
                bytes,
                "application/msword",
                $"PhieuChatLuong_{soctvt}.doc"
            );
        }


        private string RenderViewToString(string viewName, object model = null)
        {
            if (model != null)
                ViewData.Model = model;

            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                if (viewResult.View == null)
                    throw new FileNotFoundException("Không tìm thấy view: " + viewName);

                var viewContext = new ViewContext(
                    ControllerContext,
                    viewResult.View,
                    ViewData,
                    TempData,
                    sw
                );

                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);

                return sw.ToString();
            }
        }
        #endregion
    }
}