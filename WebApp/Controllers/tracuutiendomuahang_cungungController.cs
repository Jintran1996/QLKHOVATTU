using DocumentFormat.OpenXml.Spreadsheet;
using Newtonsoft.Json;
using SpreadsheetLight;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using ToolsApp.Authentication;
using ToolsApp.com.netsuite.webservices;
using ToolsApp.EntityFramework.KhoSoi;
using ToolsApp.EntityFramework.VatTu;
using ToolsApp.Helper;
using ToolsApp.Models;
using static System.Collections.Specialized.BitVector32;
using static ToolsApp.Utilities.UtilsNetsuite.CustomForm;
using Image = System.Drawing.Image;

namespace ToolsApp.Controllers
{
    public class tracuutiendomuahang_cungungController : BaseController
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

        #region Load View _Details 
        public ActionResult _Details(string maphieu)
        {
            var List = dbvt.VATTU2025_LOAD_CTPHIEU_PR_(maphieu).ToList();
            ViewBag.List = List;
            return PartialView();
        }
        #endregion

        #region In phiếu 
        public ActionResult _ReportPrintCT(string maphieu)
        {
            if (maphieu == null)
            {
                return Json(new { status = -1, text = "", obj = "" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var CTPhieu = dbvt.SP_REPHIEUMUAHANGTHEOBM(maphieu).ToList();

                var chuKyBytes = CTPhieu.FirstOrDefault()?.CHUKYPD;
                if (chuKyBytes != null)
                {
                    string base64Image = Convert.ToBase64String(chuKyBytes);
                    ViewBag.Base64Image = "data:image/png;base64," + base64Image;
                }

                ViewBag.List = CTPhieu;
            }
            return PartialView();
        }
        #endregion


        #region In phiếu 
        public ActionResult GetListMAVTTD(string MAPHIEUPD, string MAVT)
        {
            if (MAPHIEUPD == null || MAVT == null)
            {
                return Json(new { status = -1, text = "Mã phiếu phê duyệt hoặc mavt không nhận dữ liệu , liên hệ ITC để kiểm tra.", obj = "" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                ViewBag.MAVTTD = dbvt.VATTU2026_CTPHIEU_MAVTTUONGDUONG.Where(P => P.MAPHIEU == MAPHIEUPD && P.MAVT == MAVT).ToList();
            }
            return PartialView();
        }
        #endregion

        #region Danh sách phiếu gần nhất   
        [HttpPost]
        public ActionResult GetList(SearchTraCuuTheoDoiTienDoMuaHang src)
        {

            var tuNgayStr = src.TUNGAYs;
            var denNgayStr = src.DENNGAYs;
            byte DK1 = 0, DK2 = 0, DK3 = 0, DK4 = 0, DK5 = 0;
            var maphieupd = src.MAPHIEUPDSearch ?? "";
            var mavt = src.MAVTSearch ?? "";
            switch (src.searchType)
            {
                case "DK1":
                    DK1 = 1;
                    break;
                case "DK2":
                    DK2 = 1;
                    break;
                case "DK3":
                    DK3 = 1;
                    break;
                case "DK4":
                    DK4 = 1;
                    break;
                case "DK5":
                    DK5 = 1;
                    break;
            }
            if (DK5 == 1)
            {
                if (src.weekValue == null)
                {
                    return Json((new { status = -1, title = "", text = "Vui lòng chọn tuần.", obj = "" }), JsonRequestBehavior.AllowGet);
                }

                var (tuNgay, denNgay) = GetDateRangeFromWeek(src.weekValue);
                tuNgayStr = tuNgay.ToString("dd/MM/yyyy");
                denNgayStr = denNgay.ToString("dd/MM/yyyy");
                DK3 = 1;
            }
            var list = dbvt.VATTU2025_SP_THEODOIPHIEUPD_CUGC(mavt, maphieupd, maphieupd, tuNgayStr, denNgayStr, DK1, DK2, DK3, DK4)
                           .ToList();
            ViewBag.listPhieu = list;
            ViewBag.DMTinhTrangNCC = dbvt.DM_PHTinhTrangNCC.ToList();
            ViewBag.DMTTKythuat = dbvt.DM_TinhTrangKyThuatCUGC.ToList();
            ViewBag.DMTTTToan = dbvt.DM_Tinhtrangthanhtoan.ToList();
            ViewBag.DK5 = DK5;

            // return PartialView("GetList");
            return PartialView(list);
        }
        #endregion

        #region Danh sách phiếu gần nhất   
        [HttpPost]
        public ActionResult GetListWeek(SearchTraCuuTheoDoiTienDoMuaHang src)
        {

            var tuNgayStr = src.TUNGAY_?.ToString("dd/MM/yyyy");
            var denNgayStr = src.DENNGAY_?.ToString("dd/MM/yyyy");
            byte DK1 = 0, DK2 = 0, DK3 = 0, DK4 = 0, DK5 = 0;
            var maphieupd = src.MAPHIEUPDSearch ?? "";
            var mavt = src.MAVTSearch ?? "";
            switch (src.searchType)
            {
                case "DK1":
                    DK1 = 1;
                    break;
                case "DK2":
                    DK2 = 1;
                    break;
                case "DK3":
                    DK3 = 1;
                    break;
                case "DK4":
                    DK4 = 1;
                    break;
                case "DK5":
                    DK5 = 1;
                    break;
            }
            if (DK5 == 1)
            {
                if (src.weekValue == null)
                {
                    return Json((new { status = -1, title = "", text = "Vui lòng chọn tuần.", obj = "" }), JsonRequestBehavior.AllowGet);
                }
                var (tuNgay, denNgay) = GetDateRangeFromWeek(src.weekValue);
                tuNgayStr = tuNgay.ToString("dd/MM/yyyy");
                denNgayStr = denNgay.ToString("dd/MM/yyyy");
                DK3 = 1;
            }
            var list = dbvt.VATTU2025_SP_THEODOIPHIEUPD_CUGC(mavt, maphieupd, maphieupd, tuNgayStr, denNgayStr, DK1, DK2, DK3, DK4)
                           .ToList();
            ViewBag.listPhieu = list;
            ViewBag.DMTinhTrangNCC = dbvt.DM_PHTinhTrangNCC.ToList();
            ViewBag.DMTTKythuat = dbvt.DM_TinhTrangKyThuatCUGC.ToList();


            // return PartialView("GetList");
            return PartialView(list);
        }
        #endregion

        #region Convert Week
        public static (DateTime fromDate, DateTime toDate) GetDateRangeFromWeek(string weekValue)
        {
            // Ví dụ weekValue = "2025-W03"

            var parts = weekValue.Split(new[] { "-W" }, StringSplitOptions.None);

            int year = int.Parse(parts[0]); // 2025
            int week = int.Parse(parts[1]); // 03

            // ISO 8601: tuần bắt đầu từ Thứ 2
            DateTime jan4 = new DateTime(year, 1, 4);
            int dayOfWeek = (int)jan4.DayOfWeek;
            if (dayOfWeek == 0) dayOfWeek = 7;

            DateTime firstWeekStart = jan4.AddDays(1 - dayOfWeek);
            DateTime fromDate = firstWeekStart.AddDays((week - 1) * 7);
            DateTime toDate = fromDate.AddDays(6);

            return (fromDate, toDate);
        }
        #endregion

        #region UpdateTgNhaMay
        [HttpPost]
        public ActionResult UpdateTgNhaMay(string maPhieu, string maVattu, string tgNhaMayStr)
        {
            DateTime? ngay_ = null;
            if (!string.IsNullOrEmpty(tgNhaMayStr))
            {
                ngay_ = DateTime.ParseExact(
                    tgNhaMayStr,
                    "dd/MM/yyyy",
                    CultureInfo.InvariantCulture
                );
            }

            var data = dbvt.CTPHIEUx.FirstOrDefault(p => p.MAPHIEU == maPhieu && p.MAVT == maVattu);
            data.TgianNhaMayPHCU = ngay_;
            dbvt.Entry(data).State = EntityState.Modified;
            dbvt.SaveChanges();


            //  return Json(new { status = 1, text = "C", obj = "" }, JsonRequestBehavior.AllowGet);
            return Json(new { success = true });
        }
        #endregion

        #region Update Tình trạng kỹ thuật
        [HttpPost]

        public ActionResult UpdateTinhTrangKyThuat(string maPhieu, string maVattu, string tinhTrangkythuat)
        {

            var data = dbvt.CTPHIEUx.FirstOrDefault(p => p.MAPHIEU == maPhieu && p.MAVT == maVattu);
            data.PHTinhTrangKyThuatCU = tinhTrangkythuat;
            dbvt.Entry(data).State = EntityState.Modified;
            dbvt.SaveChanges();

            return Json(new { success = true });
        }
        #endregion

        #region Update Tình trạng NCC
        [HttpPost]

        public ActionResult UpdateTinhTrangNCC(string maPhieu, string maVattu, string tinhTrangNCC)
        {

            var data = dbvt.CTPHIEUx.FirstOrDefault(p => p.MAPHIEU == maPhieu && p.MAVT == maVattu);
            data.PHTinhTrangNCC = tinhTrangNCC;
            dbvt.Entry(data).State = EntityState.Modified;
            dbvt.SaveChanges();
            return Json(new { success = true });
        }
        #endregion

        #region Update Tình trạng thanh toán
        [HttpPost]

        public ActionResult UpdateTinhTrangThanhToan(string maPhieu, string maVattu, string tinhTrangTtoan)
        {

            var data = dbvt.CTPHIEUx.FirstOrDefault(p => p.MAPHIEU == maPhieu && p.MAVT == maVattu);
            data.PHTinhTrangThanhtoan = tinhTrangTtoan;
            dbvt.Entry(data).State = EntityState.Modified;
            dbvt.SaveChanges();

            return Json(new { success = true });
        }
        #endregion

        #region Update Hạn thanh toán
        [HttpPost]
        public ActionResult UpdateHanthanhtoan(string maPhieu, string maVattu, string hanthanhtoan)
        {
            DateTime? ngay_ = null;
            if (!string.IsNullOrEmpty(hanthanhtoan))
            {
                ngay_ = DateTime.ParseExact(
                    hanthanhtoan,
                    "dd/MM/yyyy",
                    CultureInfo.InvariantCulture
                );
            }
            var data = dbvt.CTPHIEUx.FirstOrDefault(p => p.MAPHIEU == maPhieu && p.MAVT == maVattu);
            data.PHHanThanhToan = ngay_;
            dbvt.Entry(data).State = EntityState.Modified;
            dbvt.SaveChanges();

            return Json(new { success = true });
        }
        #endregion

        #region Update Phản hồi đáp ứng
        [HttpPost]
        public ActionResult UpdatePhanHoiDapUng(string maPhieu, string maVattu, string ngay)
        {
            DateTime? ngay_ = null;
            if (!string.IsNullOrEmpty(ngay))
            {
                ngay_ = DateTime.ParseExact(
                    ngay,
                    "dd/MM/yyyy",
                    CultureInfo.InvariantCulture
                );
            }
            var data = dbvt.CTPHIEUx.FirstOrDefault(p => p.MAPHIEU == maPhieu && p.MAVT == maVattu);
            data.PHDapUngCU = ngay_;
            dbvt.Entry(data).State = EntityState.Modified;
            dbvt.SaveChanges();

            return Json(new { success = true });
        }
        #endregion

        #region Update Tiến độ đáp ứng thực tế
        [HttpPost]
        public ActionResult UpdateTiendoDapUngThucte(string maPhieu, string maVattu, string ngay)
        {
            DateTime? ngay_ = null;
            if (!string.IsNullOrEmpty(ngay))
            {
                ngay_ = DateTime.ParseExact(
                    ngay,
                    "dd/MM/yyyy",
                    CultureInfo.InvariantCulture
                );
            }
            var data = dbvt.CTPHIEUx.FirstOrDefault(p => p.MAPHIEU == maPhieu && p.MAVT == maVattu);
            data.PHTienDoDapUngThucTeCU = ngay_;
            dbvt.Entry(data).State = EntityState.Modified;
            dbvt.SaveChanges();

            return Json(new { success = true });
        }
        #endregion

        #region Update Lý do trễ
        [HttpPost]

        public ActionResult UpdateLydoTre(string maPhieu, string maVattu, string lydotrePCU)
        {

            var data = dbvt.CTPHIEUx.FirstOrDefault(p => p.MAPHIEU == maPhieu && p.MAVT == maVattu);
            data.LyDoTreHanCU = lydotrePCU;
            dbvt.Entry(data).State = EntityState.Modified;
            dbvt.SaveChanges();

            return Json(new { success = true });
        }
        #endregion

        #region Update MANV mua hàng
        [HttpPost]

        public ActionResult UpdateMANVMuahang(string maPhieu, string maVattu, string manvmuahangPCU)
        {

            var data = dbvt.CTPHIEUx.FirstOrDefault(p => p.MAPHIEU == maPhieu && p.MAVT == maVattu);
            data.MANVMuaHang = manvmuahangPCU;
            dbvt.Entry(data).State = EntityState.Modified;
            dbvt.SaveChanges();

            return Json(new { success = true });
        }
        #endregion

        #region Update Ghi chú PCU
        [HttpPost]

        public ActionResult UpdateGhichuPCU(string maPhieu, string maVattu, string ghichuPCU)
        {

            var data = dbvt.CTPHIEUx.FirstOrDefault(p => p.MAPHIEU == maPhieu && p.MAVT == maVattu);
            data.GhiChuCUng = ghichuPCU;
            dbvt.Entry(data).State = EntityState.Modified;
            dbvt.SaveChanges();

            return Json(new { success = true });
        }
        #endregion

        #region Update mã vật tư tương đương
        [HttpPost]

        public ActionResult UpdateMAVTTD(int keyid, string GIANHAPGANNHAT)
        {

            try
            {
                decimal gianhapgannhat_ = (GIANHAPGANNHAT == null) ? 0 : Convert.ToDecimal(GIANHAPGANNHAT);
                var item = dbvt.VATTU2026_CTPHIEU_MAVTTUONGDUONG.FirstOrDefault(p => p.KEYID == keyid);
                if (item == null)
                {
                    return Json(new { status = -1, title = "", text = "Không tìm thấy chi tiết phiếu.", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var mphieu = item.MAPHIEU;
                    item.GIANHAPGANNHAT = Convert.ToDecimal(gianhapgannhat_);
                    item.UpdatedBy = User.UserName;
                    item.UpdatedAt = DateTime.Now;
                    dbvt.Entry(item).State = EntityState.Modified;
                    dbvt.SaveChanges();
                    return Json(new { success = true });

                }
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Xuất Excel
        public ActionResult ExportTienDoMuaHangExcel(SearchTraCuuTheoDoiTienDoMuaHang src)
        {
            using (var db = new wqlvattuEntities())
            {

                //  var tuNgayStr = src.TUNGAY_?.ToString("dd/MM/yyyy");
                var tuNgayStr = src.TUNGAYs;
                //  var denNgayStr = src.DENNGAY_?.ToString("dd/MM/yyyy");
                var denNgayStr = src.DENNGAYs;
                byte DK1 = 0, DK2 = 0, DK3 = 0, DK4 = 0, DK5 = 0;
                var maphieupd = src.MAPHIEUPDSearch ?? "";
                var mavt = src.MAVTSearch ?? "";
                switch (src.searchType)
                {
                    case "DK1":
                        DK1 = 1;
                        break;
                    case "DK2":
                        DK2 = 1;
                        break;
                    case "DK3":
                        DK3 = 1;
                        break;
                    case "DK4":
                        DK4 = 1;
                        break;
                    case "DK5":
                        DK5 = 1;
                        break;
                }
                if (DK5 == 1)
                {
                    if (src.weekValue == null)
                    {
                        return Json((new { status = -1, title = "", text = "Vui lòng chọn tuần.", obj = "" }), JsonRequestBehavior.AllowGet);
                    }
                    var (tuNgay, denNgay) = GetDateRangeFromWeek(src.weekValue);
                    tuNgayStr = tuNgay.ToString("dd/MM/yyyy");
                    denNgayStr = denNgay.ToString("dd/MM/yyyy");
                    DK3 = 1;
                }
                var list = dbvt.VATTU2025_SP_THEODOIPHIEUPD_CUGC(mavt, maphieupd, maphieupd, tuNgayStr, denNgayStr, DK1, DK2, DK3, DK4)
                               .ToList();

                var fileBytes = ExcelTienDoMuaHangHelper.Export(list);

                return File(
                    fileBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "TienDoMuaHangCUGC.xlsx"
                );
            }
        }
        #endregion

        #region File Hướng hẫn Docx
        public ActionResult DownloadHuongDanTienDo()
        {
            var filePath = Server.MapPath("~/Content/HuongDan/HuongDan_TienDoMuaHang.docx");

            if (!System.IO.File.Exists(filePath))
                return HttpNotFound();

            return File(
                filePath,
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "HuongDan_TienDoMuaHang.docx"
            );
        }
        #endregion
    }
}