using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ToolsApp.Authentication;
using ToolsApp.EntityFramework.VatTu;
using ToolsApp.EntityFramework.KhoSoi;
using ToolsApp.Models;

namespace ToolsApp.Controllers
{
    [Authorize]
    public class LapPhieuYeuCauXuatKhoTuMaPhieuPheDuyetController : BaseController
    {
        private wqlvattuEntities db_ = new wqlvattuEntities();
        private wqlkhosoiEntities dbks_ = new wqlkhosoiEntities();
        // GET: LapPhieuYeuCauXuatKhoTuMaPhieuPheDuyet
        public ActionResult Index()
        {
            return View();
        }
        public string SoChungTu(string kihieu)
        {
            var db = db_.SP_GETINDEX(kihieu).FirstOrDefault();
            if(db== null)
            {
                db = 0;
            }
            string kitucuoi = Convert.ToString(db+1);
            string SoCT;
            DateTime date = DateTime.Now;
            string day = String.Format("{0:D2}", date.Day);
            string month = String.Format("{0:D2}", date.Month);
            string year = String.Format("{0:D4}", date.Year);
            SoCT = kihieu + day + month + year+"_"+ kitucuoi;
            return SoCT;
        }
        public JsonResult Load_MaPhieu_Onchage_MaKho(string maKho)
        {
            var kihieudonvi = db_.sp_kyhieudonvi_by_manv(User.UserName).FirstOrDefault();
            var data_PhieuPD = db_.SP_LOAD_MAPHIEUPD(kihieudonvi.KiHieu, maKho).ToList();
            if (data_PhieuPD.Count() > 0)
            { 
                 return Json(new { status = 1, title = "", text = "", obj = data_PhieuPD }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { status = -1, title = "", text = "", obj = "" }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult _Phieu_Action()
        {
            var kihieudonvi = db_.sp_kyhieudonvi_by_manv(User.UserName).FirstOrDefault();
            var data_SoChungTu = SoChungTu(kihieudonvi.KiHieu);
            ViewData["data_SoChungTu"] = data_SoChungTu;
            var data_MaKho = db_.SP_LOAD_MAKHO_PHIEUNHAP().ToList();
            ViewBag.data_MaKho = data_MaKho;
            var data_PhieuPD = db_.SP_LOAD_MAPHIEUPD(kihieudonvi.KiHieu, "").ToList();
            ViewBag.data_PhieuPD = data_PhieuPD;
            var data_LoaiXuatNhap = db_.SP_LOAIXUATNHAP_KHONGTHU().ToList();
            ViewBag.data_LoaiXuatNhap = data_LoaiXuatNhap;
            return PartialView();
        }
        public ActionResult _GetList_MaVatTu(string MaPhieuPD,string MaKho,string SoCtXN)
        {
            var kihieudonvi = db_.sp_kyhieudonvi_by_manv(User.UserName).FirstOrDefault();
            var data = db_.SP_LOAD_CTTONKHOTHEOMAPHIEUPD(kihieudonvi.KiHieu, MaPhieuPD, MaKho, SoCtXN).ToList();
            ViewBag.data = data;
            return PartialView();
        }

        #region In phiếu 
        public ActionResult _ReportPrintCT(string SOCTXN)
        {
            var ct_Phieu = dbks_.NL_CTXUATNHAP.FirstOrDefault(c => c.SOCTXN == SOCTXN);
            if (ct_Phieu == null)
            {
                return Json(new { status = -1, text = "Phiếu " + SOCTXN + " không có chi tiết phiếu.", obj = "" }, JsonRequestBehavior.AllowGet);
            }

            if (SOCTXN == null)
            {
                return Json(new { status = -1, text = "", obj = "" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var CTPhieu = dbks_.SP_RPT_NL_PYC_XUATVT(SOCTXN).ToList();
                if (CTPhieu == null)
                {
                    return Json(new { status = -1, text = "Không có chi tiết phiếu. Kiểm tra lại!", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    ViewBag.List = CTPhieu;
                }
            }
            return PartialView();
        }
        #endregion
        public ActionResult _GetList_MaVatTu_DaChon(string SoCtXN)
        {
            var data = db_.SP_LOADCTPHIEUYCXUATKHO_DONVI( SoCtXN).ToList();
            ViewBag.data = data;
            return PartialView();
        }
        [HttpPost]
        public JsonResult _btn_Next_Click(Phieu_YC_Duyet_model data)
        {
            try
            {

                if (data.SLXK > data.SLTONTHEOMAPHIEUPB)
                {
                    return Json(new { status = -1, title = "", text = "Số lượng xuất kho phải bé hơn số lượng tồn kho", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if(data.MaPhieuPD == "false" || data.MaPhieuPD == "")
                    {
                        return Json(new { status = -1, title = "", text = "Vui lòng chọn mã phiếu phê duyệt", obj = "" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        if(data.MaVt!=null)
                        {
                            var kihieudonvi = db_.sp_kyhieudonvi_by_manv(User.UserName).FirstOrDefault();
                            int ex = db_.SP_PHIEUYCXK_DONVI_INSERT(data.SoChungTu, User.UserName, data.MaKho.Trim(), data.MaVt, kihieudonvi.KiHieu, data.SLXK, data.LoaiXuatNhap.Trim(), (data.GhiChu==null?"": data.GhiChu), String.Empty);
                            if (ex == 1)
                            {
                                return Json(new { status = -1, title = "", text = "Có lỗi xãy ra vui lòng kiểm tra lại dữ liệu", obj = "" }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                return Json(new { status = 1, title = "", text = "Thêm thành công", obj = "" }, JsonRequestBehavior.AllowGet);
                            }
                          
                        }
                        else
                        {
                            return Json(new { status = -1, title = "", text = "Không tìm thấy mã vật tư", obj = "" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                
            }
            catch ( Exception ex)
            {
                return Json(new { status = -3, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public JsonResult _btn_back_Click(Phieu_YC_Duyet_model data)
        {
            try
            {

                object kq = null;
                kq = db_.SP_PHIEUYCXK_DONVI_DELETE_KHONGMABP(data.SoChungTu, data.MaVt, data.MANVYC, "X", data.MaKho);
                if (kq != null)
                {
                    return Json(new { status = 1, title = "", text = "Xóa thành công", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = -1, title = "", text = "Xóa thành công", obj = "" }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                return Json(new { status = -3, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult print_Phieu(string SoChungTu,string makho)
        {
            try
            {
                if(SoChungTu==""||SoChungTu==null)
                {
                    return Json(new { status = -1, title = "", text = "Không tìm thấy mã số chứng từ", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    string PhieuYCXK_DonVi = "";
                    if (makho.Trim()== "KHOHC")
                    {
                        PhieuYCXK_DonVi = "06";
                    }
                    else
                    {
                        PhieuYCXK_DonVi = "03";
                    }
                    var data = db_.SP_REPYCXUATKHO_DONVI(SoChungTu.Trim(), PhieuYCXK_DonVi).ToList();
                    if(data.Count()==0)
                    {
                        return Json(new { status = -1, title = "", text = "Không tìm thấy dữ liệu", obj = "" }, JsonRequestBehavior.AllowGet);
                    }
                    ViewBag.data = data;
                }
               
                return PartialView();
            }
            catch(Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }


    }



}