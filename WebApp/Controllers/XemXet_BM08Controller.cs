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
using ToolsApp.EntityFramework.NetsuiteTT;
using System.Data.Entity;


namespace ToolsApp.Controllers
{
    public class XemXet_BM08Controller : BaseController
    {       
        private wqlvattuEntities dbvt = new wqlvattuEntities();      
        // GET: XemXet_BM08
        public ActionResult Index()
        {
            #region mã phiếu
            ViewBag.maphieu = dbvt.CTPHIEU05_LOADMAPHIEUXEMXET(User.UserName).Where(p => p.MaNV == null || p.MaNV == "").ToList();
            #endregion

            return View();
        }
        #region Load chi tiết mã phiếu
        public ActionResult _GetList(string maphieu)
        {

            if (maphieu != null && maphieu != "")
            {
                var userid = User.UserName;
                var list = dbvt.CTPHIEU05_LOADCHITIETPHIEU_XEMXET(maphieu, userid).ToList();
                ViewBag.List = list;
                var dmphieu = dbvt.DMPHIEUx.Where(p => p.MAPHIEU == maphieu).FirstOrDefault();
                var nam = int.Parse(maphieu.Substring(5, 4));
                var madvi = dbvt.sp_kyhieudonvi_by_manv(User.UserName).FirstOrDefault();
                var dinhmucvppham = dbvt.SP_GET_CHITIET_DINHMUCVANPHONGPHAM(nam, madvi.IDMaDV, dmphieu.LyDoSD);
                ViewBag.dinhmucvp = dinhmucvppham;
                var danhSachDaSuDung = dbvt.SP_GET_DANHSACH_VPP_NYP_DASUDUNG(maphieu).Where(p => p.MADONVI == madvi.MaDonVi).ToList();
                ViewBag.dssudung = danhSachDaSuDung;
            }
            return PartialView();
        }
        #endregion
        #region onchange 
        public JsonResult LoadLydosd(string maphieu)
        {
            var lydo = dbvt.SP_LoadDMPhieu_2023(maphieu).ToList();
            return Json(new { status = 1, text = "", obj = lydo }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region Lưu Xem Xét
        public ActionResult Luu_click(string maphieu)
        {

            try
            {
                var madvi = dbvt.sp_kyhieudonvi_by_manv(User.UserName).FirstOrDefault();
                var nguoix = dbvt.DMNHANVIENs.Where(p => p.IDMaDV == madvi.IDMaDV && p.XemXet == 1 && p.MANV == madvi.MANV).FirstOrDefault();
                if (nguoix != null)
                {

                    var luutt = dbvt.SP_UPDATECTBM05DV_VER2(madvi.KiHieu, maphieu, User.UserName);
                    //string log = "Đã kiểm tra phiếu: " + maphieu.Trim() + " - " + madvi.KiHieu.Trim();
                    //string PCName = GetComputerName();
                    //string IPPC = GetIPClient();

                    //var ghilog = dbvt.SP_GHIFILELOG(User.UserName, PCName, IPPC, log, DateTime.Now, "");
                    dbvt.SaveChanges();
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
        #region Lấy thông tin máy
        public string GetComputerName()
        {
            System.Web.HttpContext context = System.Web.HttpContext.Current;
            if (context != null)
            {
                string computerName = context.Server.MachineName;
                return computerName;
            }
            else
            {
                return "Không có HttpContext";
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
        #region Update
        public ActionResult _Upload(string maphieu, string MAVT, string MaBP)
        {
            var userid = User.UserName;
            var list = dbvt.CTPHIEU05_LOADCHITIETPHIEU_XEMXET(maphieu, userid).FirstOrDefault();
            ViewBag.list = list;
            return PartialView();

        } 
        
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _Edit(string maphieu, string MAVT, string MaBP,decimal SLXXET)
        {
            try
            {
                var item = dbvt.CTPHIEU05.Where(a => a.MAPHIEU == maphieu && a.MAVT == MAVT && a.MaBP == MaBP).FirstOrDefault();
                if (item == null)
                {
                    return Json(new { status = -1, title = "", text = "Không tìm thấy chi tiết phiếu.", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {                   
                    if (item.SLYCAU < 0)
                    {
                        return Json(new { status = -1, title = "", text = "Số lượng yêu cầu phải lớn hơn 0.", obj = "" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var suaslxx = dbvt.SP_Upload_SLXX_CTPHIEU05(maphieu, MAVT, MaBP, SLXXET);
                        dbvt.SaveChanges();
                        return Json(new { status = 1, title = "", text = "Cập nhật thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
                    }                  
                }
            }            
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
        #region Gửi mail yêu cầu phê duyệt
        [HttpPost]
        public string openOutlookemailbox(string maphieu)
        {
            if (maphieu == null || maphieu == "")
            {
                return null;
            }
            else
            {
                #region Lấy địa chỉ mail của người pd
                var Email = dbvt.DMNHANVIENs.FirstOrDefault(p => p.MADV == "P.HCQT" && p.ChucVu == "Quản lý VPP").EMail;
                #endregion
                var dmphieu = dbvt.DMPHIEUx.Where(p => p.MAPHIEU == maphieu).FirstOrDefault();
                var madvi = dbvt.sp_kyhieudonvi_by_manv(User.UserName).FirstOrDefault();
                MailMessage mail = new MailMessage();
                mail.To.Add(new MailAddress(Email));
                mail.IsBodyHtml = true;
                mail.Subject = madvi.KiHieu.ToUpper() + " Đã kiểm tra VPP-NYP" + " thang " + maphieu.Trim().Substring(3, 2) + " - " + maphieu.Trim().Substring(5, 4);
                mail.Body = " Kính gởi: GD.PHCQT %0A" + "Tôi đã kiểm tra phiếu: " + maphieu.Trim()
                  + "%0D%0ALoại phiếu: " + dmphieu.LyDoSD.Trim();
                var outlookmail = "mailto:" + mail.To.ToString()
                    + "?subject=" + mail.Subject.ToString()
                    + "&body=" + mail.Body.ToString() + "" + "%0D%0ATrân trọng!%0D%0A" + "" + User.FullName;
                return outlookmail;
            }                
        }
        #endregion       
    }
}