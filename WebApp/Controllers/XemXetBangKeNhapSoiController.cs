using SpreadsheetLight;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using ToolsApp.App_Start;
using ToolsApp.Authentication;
using ToolsApp.EntityFramework.VatTu;
using ToolsApp.EntityFramework.KhoSoi;
using ToolsApp.Models;
using System.Net.Mail;
using System.Web.Script.Serialization;
using System.Globalization;
namespace ToolsApp.Controllers
{
    public class XemXetBangKeNhapSoiController : BaseController
    {
        private wqlvattuEntities vt_ = new wqlvattuEntities();
        private wqlkhosoiEntities ks_ = new wqlkhosoiEntities();
        // GET: XemXetBangKeNhapSoi
        public string donVi()
        {
            var maDonVi = vt_.DMNHANVIENs.Where(p => p.MANV == User.UserName).FirstOrDefault();
            return maDonVi.IDMaDV;
        }
        public ActionResult Index()
        {
            ViewBag.HinhThuc = ks_.SP_BANGKENHAPSOI_MAHINHTHUC_LOAD_MAHTHUC_MADV(donVi()).ToList();
            ViewBag.SoPhieuXX = ks_.SP_GC_GC_DMPHIEU_BANGKENHAPSOI_BY_MADV_HLXX(donVi()).ToList();
            ViewBag.MaDV = ks_.SP_LOAD_DMKHO_BY_IDMADV(donVi()).ToList();
            return View();
        }
        public ActionResult _GetList(string SOPHIEU)
        {
            var List = ks_.GC_CTPHIEUXUATGC.Where(p => p.SOPHIEU == SOPHIEU).ToList();
            ViewBag.List = List;
            return PartialView();
        }
        public ActionResult _GetListXX(string SOPHIEU)
        {
            var List = ks_.GC_CTPHIEUXUATGC.Where(p => p.SOPHIEU == SOPHIEU).ToList();
            ViewBag.List = List;
            return PartialView();
        }
        public JsonResult _LoadSoPhieu(string MALOAICONGDOAN)
        {           
            var soPhieu = ks_.SP_GC_GC_DMPHIEU_BANGKENHAPSOI_BY_MADV(donVi(),MALOAICONGDOAN).ToList();        
            return Json(new { status = 1, text = "", obj = soPhieu }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult _LoadKTXX(string MAHINHTHUC)
        {
            var data = ks_.SP_GC_BANGKENHAPSOI_XEMXET_LOAD_MANVKETOAN_BY_MAHINHTHUC(MAHINHTHUC, donVi()).ToList();
            return Json(new { status = 1, text = "", obj = data }, JsonRequestBehavior.AllowGet);
        }
        #region Lưu Xem Xét
        public ActionResult Luu_click(string SOPHIEU, string MANV_KT)
        {
            try
            {
                var data = ks_.GC_DMPHIEU_BANGKENHAPSOI.Where(p => p.SOPHIEU == SOPHIEU).FirstOrDefault();
                var madvi = vt_.sp_kyhieudonvi_by_manv(User.UserName).FirstOrDefault();
                var nguoix = vt_.DMNHANVIENs.Where(p => p.IDMaDV == madvi.IDMaDV && p.XemXet == 1 && p.MANV == data.MANV_XX).FirstOrDefault();
                if (nguoix != null)
                {
                    var luutt = ks_.SP_GC_GC_DMPHIEU_BANGKENHAPSOI_DVXX_BySOPHIEU(DateTime.Now, SOPHIEU,MANV_KT);
                    return Json(new { status = 1, text = "Đã xem xét thành công", obj = luutt }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = -1, text = "Không có quyền xem xét", obj = "" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, text = "", obj = ex }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
        #region Bỏ Xem Xét
        public ActionResult Luu_BoXX(string SOPHIEU)
        {
            try
            {
                var data = ks_.GC_DMPHIEU_BANGKENHAPSOI.Where(p => p.SOPHIEU == SOPHIEU).FirstOrDefault();
                var madvi = vt_.sp_kyhieudonvi_by_manv(User.UserName).FirstOrDefault();
                var nguoix = vt_.DMNHANVIENs.Where(p => p.IDMaDV == madvi.IDMaDV && p.XemXet == 1 && p.MANV == data.MANV_XX).FirstOrDefault();
                if (nguoix != null)
                {
                    var luutt = ks_.SP_GC_UPDATE_BOXX_DMPHIEU_BANGKENHAPSOI(SOPHIEU,DateTime.Now);
                    return Json(new { status = 1, text = "Đã bỏ xem xét thành công", obj = luutt }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = -1, text = "Không có quyền bỏ xem xét", obj = "" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, text = "", obj = ex }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
        #region Gửi mail 
        [HttpPost]
        public string openOutlookemailbox(string SOPHIEU)
        {
            
            var data = ks_.GC_DMPHIEU_BANGKENHAPSOI.Where(p => p.SOPHIEU == SOPHIEU).FirstOrDefault();
            if (data != null)
            {
                string bp = HttpContext.Request.Cookies["BOPHAN"]?.Value;
                #region Lấy địa chỉ mail của người xx
                var manv = vt_.DMNHANVIENs.FirstOrDefault(p => p.MANV == User.UserName);
                var nguoiNhan = vt_.DMNHANVIENs.FirstOrDefault(p => p.MANV == data.MANV_KT).EMail;
                #endregion
                string ngay = DateTime.Now.ToString("dd/MM/yyyy");
                MailMessage mail = new MailMessage();
                mail.To.Add(new MailAddress("bichlien@thaituan.com.vn"));
                mail.CC.Add(new MailAddress(nguoiNhan));
                mail.IsBodyHtml = true;
                mail.Subject = "KE TOAN KIEM TRA BANG KE NHAP SOI TU CHUONG TRINH QUAN LY KHO SOI : " + SOPHIEU ;
                mail.Body = "Gui P.TCKT " + "%0D%0A" + "DA XEM XET BANG KE TU CHUONG TRINH QUAN LY KHO SOI SO: " + SOPHIEU + " VOI HINH THUC " + data.NHAPKHO + "%0D%0A" + "%0D%0AĐường dẫn: %0D%0A"; 
                var outlookmail = "mailto:" + mail.To.ToString()
                    +"mailcc:" + mail.CC.ToString()
                    + "?subject=" + mail.Subject.ToString()
                    + "&body=" + mail.Body.ToString() + "" + "%0D%0ATrân trọng! %0D%0A" + manv.MANV + " - " + manv.HOTEN;
                return outlookmail;
            }    
            else
            {
                return string.Empty;
            }    
        }
        #endregion
    }
}