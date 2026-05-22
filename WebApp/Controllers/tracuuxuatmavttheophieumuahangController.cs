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
    public class tracuuxuatmavttheophieumuahangController : BaseController
    {
        #region Database
        NSClient.NSClient ns = new NSClient.NSClient();
        private wqlvattuEntities dbvt = new wqlvattuEntities();
        private wqlkhosoiEntities soi_ = new wqlkhosoiEntities();
        #endregion

        #region Index
        public ActionResult Index()
        {
            var showrooms = dbvt.VATTU2026_SPLOAD_TABLEMAPPING_MAKHO_IDMADV().ToList();
            ViewBag.showrooms = showrooms;
            return View();
        }
        #endregion


        #region Danh sách phiếu gần nhất  1
        [HttpPost]
        public ActionResult GetList1(SearchPhieuMuaHang src)
        {
            var listsr = src.SHOWROOMSEARCH_;
            var sophieu = src.SOPHIEUSEARCH ?? "";

            string masrCsv = (listsr != null && listsr.Any())
                                ? string.Join(",", listsr)
                                : "";

            var list = dbvt.VATTU2026_SPLOAD_XUATNHAP_XBNVH_PHIEUMUAHANG(sophieu, masrCsv)
                           .ToList();

            ViewBag.listPhieu = list;

            return PartialView(list);
        }
        #endregion



        #region  XBNVH
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _FunXuatKho(SearchPhieuMuaHang src)
        {
            if(string.IsNullOrEmpty(src.SOPHIEUSEARCH) || string.IsNullOrEmpty(src.SHOWROOMSEARCH))
            {
                return Json(new { status = -1, title = "", text = "IDMADV hoặc Số Phiếu không được để trống" }, JsonRequestBehavior.AllowGet);
            }
            var ktraloaiphieu = dbvt.VATTU2026_KTRA_LOAIPHIEU_PHIEUMUAHANG(src.SOPHIEUSEARCH, src.SHOWROOMSEARCH, "")?.FirstOrDefault().KQ;
            if (ktraloaiphieu != "1")
            {
                return Json(new { status = -1, title = "", text = "tìm không thấy phiếu, hoặc không đúng loại phiếu xuất kho, vui lòng kiểm tra lại" }, JsonRequestBehavior.AllowGet);
            }

            var createxk = dbvt.sp_AutoCreate_PhieuXuatBanVatTu_INSERT(src.SOPHIEUSEARCH, src.SHOWROOMSEARCH, "XBNVH");

            return Json(new { status = 1, title = "", text = "Cập nhật thành công" }, JsonRequestBehavior.AllowGet);

        }
        #endregion

        #region  NBNVH
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _FunNhapTraBill(SearchPhieuMuaHang src)
        {
            if (string.IsNullOrEmpty(src.SOPHIEUSEARCH) || string.IsNullOrEmpty(src.SHOWROOMSEARCH))
            {
                return Json(new { status = -1, title = "", text = "IDMADV hoặc Số Phiếu không được để trống" }, JsonRequestBehavior.AllowGet);
            }
            var ktraloaiphieu = dbvt.VATTU2026_KTRA_LOAIPHIEU_PHIEUMUAHANG(src.SOPHIEUSEARCH, src.SHOWROOMSEARCH, "")?.FirstOrDefault().KQ;
            if (ktraloaiphieu != "2")
            {
                return Json(new { status = -1, title = "", text = "tìm không thấy phiếu, hoặc không đúng loại phiếu nhập kho trả bill , vui lòng kiểm tra lại" }, JsonRequestBehavior.AllowGet);
            }

            var createxk = dbvt.sp_AutoCreate_PhieuXuatBanVatTu_INSERT_TRABILL(src.SOPHIEUSEARCH, src.SHOWROOMSEARCH, "NBNVH");

            return Json(new { status = 1, title = "", text = "Cập nhật thành công" }, JsonRequestBehavior.AllowGet);

        }
        #endregion

    }
}