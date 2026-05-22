using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ToolsApp.Authentication;
using ToolsApp.EntityFramework.DonHang09;
using ToolsApp.EntityFramework.VatTu;
using ToolsApp.Models;

namespace ToolsApp.Controllers
{
    public class PhieuYeuCauXuatKhoController : BaseController
    {
        private wqlvattuEntities db_ = new wqlvattuEntities();
        private QLDonHang09Entities db09_ = new QLDonHang09Entities();
        public string SoChungTu(string kihieu)
        {
            var db = db_.SP_GETINDEX(kihieu).FirstOrDefault();
            if (db == null)
            {
                db = 0;
            }
            string kitucuoi = Convert.ToString(db + 1);
            string SoCT;
            DateTime date = DateTime.Now;
            string day = String.Format("{0:D2}", date.Day);
            string month = String.Format("{0:D2}", date.Month);
            string year = String.Format("{0:D4}", date.Year);
            SoCT = kihieu.ToLower() + day + month + year + "_" + kitucuoi;
            return SoCT;
        }
        // GET: PhieuYeuCauXuatKho
        public ActionResult Index()
        {

            return View();
        }
        public JsonResult PhanQuyen()
        {
            bool isCheck = false;
            var data = db_.sp_GetUserMaBP(User.UserName).FirstOrDefault();
            if (data.MABP == "HCTH" || data.MABP == "DPH" || data.MABP == "PB28" || data.MABP == "CN.HNO" || data.MABP == "KDBL.MIENBAC")
            {
                isCheck = true;
            }
            return Json(new { status = 1, title = "", text = "Trưởng BP điều phối và HCTH mới được yêu cầu xuất kho.", obj = isCheck }, JsonRequestBehavior.AllowGet);
        }



        public JsonResult Load_MaVT_DV(string MaKho,string MaVT)
        {
            var kihieudonvi = db_.sp_kyhieudonvi_by_manv(User.UserName).FirstOrDefault();
            var data = db_.SP_LOADVTYEUCAUXUATKHO_DONVI(MaKho, kihieudonvi.KiHieu, (MaVT == null || MaVT == "") ? "" : MaVT).ToList();
            if(data.Count>0)
            {
                return Json(new { status = 1, title = "", text = "", obj = data }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { status = -1, title = "", text = "", obj = "" }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult Load_MaVT_GCN(string MaKho, string MaVT)
        {
            var kihieudonvi = db_.sp_kyhieudonvi_by_manv(User.UserName).FirstOrDefault();
            var data = db_.SP_LOADVTYEUCAUXUATKHO_GCNhuom(MaKho, kihieudonvi.KiHieu, (MaVT == null || MaVT == "") ? "" : MaVT).ToList();
            if (data.Count > 0)
            {
                return Json(new { status = 1, title = "", text = "", obj = data }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { status = -1, title = "", text = "", obj = "" }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult Load_MaVT_TTKD(string MaKho)
        {
            var mabp = db_.sp_GetUserMaBP(User.UserName).FirstOrDefault();
            var kihieudonvi = db_.sp_kyhieudonvi_by_manv(User.UserName).FirstOrDefault();
            var data = db_.SP_LOADVTYEUCAUXUATCHOTUNGDV_TBPKD(MaKho, kihieudonvi.KiHieu, "%", mabp.MABP).ToList();
            if (data.Count > 0)
            {
                return Json(new { status = 1, title = "", text = "", obj = data }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { status = -1, title = "", text = "", obj = "" }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult Load_SoCTXN_PhieuPD_GCN(string MaKho, string MaVT)
        {
            var kihieudonvi = db_.sp_kyhieudonvi_by_manv(User.UserName).FirstOrDefault();
            var data = db_.SP_LOADVTYEUCAUXUATKHO_GCNhuom(MaKho, kihieudonvi.KiHieu, MaVT).FirstOrDefault();
            if (data !=null)
            {
                return Json(new { status = 1, title = "", text = "", obj = data }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { status = -1, title = "", text = "", obj = "" }, JsonRequestBehavior.AllowGet);
        }


        //-----------====== Gia công nhuộm ======----------/////////
        public ActionResult _GetList_GiaCongNhuom(string SOCTXN_GCN)
        {
            var data_GCN = db_.SP_LOADCTPHIEUYCXUATKHO_DONVI(SOCTXN_GCN).ToList();
            ViewBag.data_GCN = data_GCN;
            return PartialView();
        }
        public ActionResult _insert_GiaCongNhuom()
        {
            var kihieudonvi = db_.sp_kyhieudonvi_by_manv(User.UserName).FirstOrDefault();
            var data_SoChungTu = SoChungTu(kihieudonvi.KiHieu);
            ViewData["data_SoChungTu"] = data_SoChungTu;
            var data_MaKho_GCN = db_.SP_KTQKForWebNew(User.UserName, kihieudonvi.KiHieu, "L").ToList();
            ViewBag.data_MaKho_GCN = data_MaKho_GCN;
            var data_LoaiCapPhat_GCN = db_.SP_LOAIXUATNHAP().ToList();
            ViewBag.data_LoaiCapPhat_GCN = data_LoaiCapPhat_GCN;
            var data_LoaiXuatNhap_GCN = db_.sp_LoadLoaiXN_GCNhuom().ToList();
            ViewBag.data_LoaiXuatNhap_GCN = data_LoaiXuatNhap_GCN;
            return PartialView();
        }
        [HttpPost]
        public JsonResult Insert_GCN_(xuatkho_GCN_model model)
        {
            if(model.MaKho_GCN=="false")
            {
                return Json(new { status = -1, title = "", text = "Mã kho không tồn tại!!", obj = "" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                if(model.MaVT_GCN ==""|| model.MaVT_GCN == null || model.MaVT_GCN == "false")
                {
                    return Json(new { status = -1, title = "", text = "Mã vật tư không tồn tại!!", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if(model.Soluong_GCN==0)
                    {
                        return Json(new { status = -1, title = "", text = "Số lượng phải khác 0!!", obj = "" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        if(Convert.ToDecimal(model.Soluong_GCN) >Convert.ToDecimal(model.SL_PheDuyet_GCN))
                        {
                            return Json(new { status = -1, title = "", text = "Số lượng yêu cầu phải bé hơn số lượng phê duyệt", obj = "" }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            var check = db_.SP_KIEMTRA_VTYCXK_DONVI_GCNhuom_EXITS(model.LoaiXuatNhap_GCN, model.SOCTXN_GCN, model.MaVT_GCN).ToList();
                            if(check.Count()>0)
                            {
                                return Json(new { status = -1, title = "", text = "Mã vật tư yêu cầu này đã có trong phiếu yêu cầu xuất kho : " + model.SOCTXN_GCN.Trim(), obj = "" }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                var data = db_.SP_PHIEUYCXK_GCNhuom_INSERT(model.SOCTXN_GCN, User.UserName, model.MaKho_GCN.Trim(), model.MaVT_GCN.Trim(), model.SOCTXN_GCN.Substring(0, 3), model.Soluong_GCN, model.LoaiCapPhat_GCN.Trim(), model.GhiChu_GCN, String.Empty, model.SoCTXN_PhieuPD_GCN, model.LoaiXuatNhap_GCN);
                                if (data == 1)
                                {
                                    return Json(new { status = -1, title = "", text = "Có lỗi xãy ra vui lòng kiễm tra lại dữ liệu .............  !!! ", obj = "" }, JsonRequestBehavior.AllowGet);
                                }
                                return Json(new { status = 1, title = "", text = "Thêm thành công ", obj = "" }, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                }
            }
        }
        [HttpPost]
        public JsonResult update_GCN_(xuatkho_GCN_model model)
        {
            try
            {
                if (model.Soluong_GCN == 0)
                {
                    return Json(new { status = -1, title = "", text = "Số lượng yêu cầu xuất kho phải lớn hơn 0 !!! ", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                var data = db_.SP_PHIEUYCXK_GCNhuom_UPDATE(model.KhoaKeyXN_GCN,model.Soluong_GCN,model.GhiChu_GCN);
                if (data <= 0)
                {
                    return Json(new { status = -1, title = "", text = "Có lỗi xãy ra vui lòng kiễm tra lại dữ liệu .............  !!! ", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { status = 1, title = "", text = "Cập nhật thành công ", obj = "" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public JsonResult Delete_GCN(string KHOAKEYXN,string SOCTXN_GCN)
        {
            try
            {
                var data = db_.SP_PHIEUYCXK_GCNhuom_Delete(KHOAKEYXN, SOCTXN_GCN);
                if (data == 1)
                {
                    return Json(new { status = -1, title = "", text = "Xoá Thất bại !!", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { status = 1, title = "", text = "Xóa thành công mã vật tư", obj = "" }, JsonRequestBehavior.AllowGet);
            
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }


        //----------=======kho đơn vị =====------------//////////
        public ActionResult _GetList_KhoDonVi(string SOCTXN_DV)
        {
            var data_DV = db_.SP_LOADCTPHIEUYCXUATKHO_DONVI(SOCTXN_DV).ToList();
            ViewBag.data_DV = data_DV;
            return PartialView();
        }
        public ActionResult _insert_khoDonVi()
        {
            var kihieudonvi = db_.sp_kyhieudonvi_by_manv(User.UserName).FirstOrDefault();
            var data_SoChungTu = SoChungTu(kihieudonvi.KiHieu);
            ViewData["data_SoChungTu"] = data_SoChungTu;
            var data_MaKho_DV = db_.SP_KTQKForWebNew(User.UserName, kihieudonvi.KiHieu, "L").ToList();
            ViewBag.data_MaKho_DV = data_MaKho_DV;
            var data_LoaiXuatNhap_DV = db_.SP_LOAIXUATNHAP().ToList();
            ViewBag.data_LoaiXuatNhap_DV = data_LoaiXuatNhap_DV;
            
            return PartialView();
        }
        [HttpPost]
        public JsonResult Insert_DV_(xuatkho_DV_model model)
        {
            if (model.MaVT_DV == null || model.MaVT_DV == "")
            {
                return Json(new { status = -1, title = "", text = "Bạn chưa chọn mã vật tư xuất kho !!!", obj = "" }, JsonRequestBehavior.AllowGet);
            }
            else
            {


                var checkData = db_.SP_KIEMTRA_VTYCXK_DONVI_EXITS_VER1(model.SOCTXN_DV, model.MaVT_DV).ToList();
                if (checkData.Count() > 0)
                {
                    return Json(new { status = -1, title = "", text = "Mã vật tư yêu cầu này đã có trong phiếu yêu cầu xuất kho : " + model.SOCTXN_DV.Trim(), obj = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    try
                    {
                        if (model.Soluong_DV == 0)
                        {
                            return Json(new { status = -1, title = "", text = "Số lượng yêu cầu xuất kho phải lớn hơn 0 !!! ", obj = "" }, JsonRequestBehavior.AllowGet);
                        }
                        var data = db_.SP_PHIEUYCXK_DONVI_INSERT(model.SOCTXN_DV, User.UserName, model.MaKho_DV.Trim(), model.MaVT_DV, model.SOCTXN_DV.Substring(0, 3), model.Soluong_DV, model.LoaiXuatNhap_DV.Trim(), model.GhiChu_DV, string.Empty);
                        if (data == 1)
                        {
                            return Json(new { status = -1, title = "", text = "Có lỗi xãy ra vui lòng kiễm tra lại dữ liệu .............  !!! ", obj = "" }, JsonRequestBehavior.AllowGet);
                        }
                        return Json(new { status = 1, title = "", text = "Thêm thành công ", obj = "" }, JsonRequestBehavior.AllowGet);
                    }
                    catch (Exception ex)
                    {
                        return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
           
        }
        [HttpPost]
        public JsonResult update_DV_(xuatkho_DV_model model)
        {
            try
            {
                if (model.Soluong_DV == 0)
                {
                    return Json(new { status = -1, title = "", text = "Số lượng yêu cầu xuất kho phải lớn hơn 0 !!! ", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                var data = db_.SP_PHIEUYCXK_DONVI_UPDATE(model.SOCTXN_DV, User.UserName, model.MaKho_DV, model.MaVT_DV, model.SOCTXN_DV.Substring(0, 3), model.Soluong_DV, model.Soluong_DV, model.LoaiXuatNhap_DV.Trim(), model.GhiChu_DV, model.KhoaKeyXN_DV).ToList();
                if (data.Count() <= 0)
                {
                    return Json(new { status = -1, title = "", text = "Có lỗi xãy ra vui lòng kiễm tra lại dữ liệu .............  !!! ", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { status = 1, title = "", text = "Cập nhật thành công ", obj = "" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public JsonResult Delete_DV(string KHOAKEYXN)
        {
            try
            {
                var check = db_.SP_XUATNHAP_BYKHOAKEY_REVEICE(KHOAKEYXN).FirstOrDefault();

                if (check != null)
                {

                    var data = db_.SP_PHIEUYCXK_DONVI_DELETE_KHONGMABP(check.SOCTXN, check.MAVT, User.UserName, "X", check.MAKHO);
                    if (data == 1)
                    {
                        return Json(new { status = -1, title = "", text = "Xoá Thất bại !!", obj = "" }, JsonRequestBehavior.AllowGet);
                    }
                    return Json(new { status = 1, title = "", text = "Xóa thành công mã vật tư: " + check.MAVT, obj = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = -1, title = "", text = "Xóa thất bại..!", obj = "" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }


        //-----------====== Gia công nhuộm ======----------/////////
        public ActionResult _GetList_TrungTamKinhDoanh(string SOCTXN_TTKD)
        {
            var data_TTKD = db_.SP_LOADCTPHIEUYCXUATKHO_DONVI(SOCTXN_TTKD).ToList();
            ViewBag.data_TTKD = data_TTKD;
            return PartialView();
        }
        public ActionResult _insert_TrungTamKinhDoanh()
        {
            var kihieudonvi = db_.sp_kyhieudonvi_by_manv(User.UserName).FirstOrDefault();
            var data_SoChungTu = SoChungTu(kihieudonvi.KiHieu);
            ViewData["data_SoChungTu"] = data_SoChungTu;
            var data_MaKho_TTKD = db_.SP_KTQKFORWEB( kihieudonvi.KiHieu, "L").ToList();
            ViewBag.data_MaKho_TTKD = data_MaKho_TTKD;
            var data_LoaiXuatNhap_TTKD = db_.SP_LOAIXUATNHAP().ToList();
            ViewBag.data_LoaiXuatNhap_TTKD = data_LoaiXuatNhap_TTKD;
            var data_XuatCho_TTKD = db_.sp_load_capcoso().ToList();
            ViewBag.data_XuatCho_TTKD = data_XuatCho_TTKD;
            return PartialView();
        }
        [HttpPost]
        public JsonResult Insert_TTKD_(xuatkho_TTKD_model model)
        {
            if (model.MaVT_TTKD == null || model.MaVT_TTKD == "")
            {
                return Json(new { status = -1, title = "", text = "Bạn chưa chọn mã vật tư xuất kho !!!", obj = "" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                    try
                    {
                    var mabp = db_.sp_GetUserMaBP(User.UserName).FirstOrDefault();
                    if (model.Soluong_TTKD == 0)
                        {
                            return Json(new { status = -1, title = "", text = "Số lượng yêu cầu xuất kho phải lớn hơn 0 !!! ", obj = "" }, JsonRequestBehavior.AllowGet);
                        }
                        var data = db_.SP_PHIEUYCXK_DONVI_INSERT_KINHDOANH(model.SOCTXN_TTKD, User.UserName, model.MaKho_TTKD, model.MaVT_TTKD, model.SOCTXN_TTKD.Substring(0, 3), model.Soluong_TTKD, model.LoaiXuatNhap_TTKD, model.GhiChu_TTKD, String.Empty, mabp.MABP).ToList();
                        if (data.Count() == 0)
                        {
                            return Json(new { status = -1, title = "", text = "Có lỗi xãy ra vui lòng kiễm tra lại dữ liệu .............  !!! ", obj = "" }, JsonRequestBehavior.AllowGet);
                        }
                        return Json(new { status = 1, title = "", text = "Thêm thành công ", obj = "" }, JsonRequestBehavior.AllowGet);
                    }
                    catch (Exception ex)
                    {
                        return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
                    }
                
            }

        }
        [HttpPost]
        public JsonResult Delete_TTKD(xuatkho_TTKD_model model)
        {
            try
            {
                var mabp = db_.sp_GetUserMaBP(User.UserName).FirstOrDefault();
                var data = db_.SP_PHIEUYCXK_DONVI_DELETE(model.SOCTXN_TTKD, model.MaVT_TTKD, User.UserName, "X", model.MaKho_TTKD, mabp.MABP);
                    if (data == 1)
                    {
                        return Json(new { status = -1, title = "", text = "Xoá Thất bại !!", obj = "" }, JsonRequestBehavior.AllowGet);
                    }
                    return Json(new { status = 1, title = "", text = "Xóa thành công mã vật tư: " + model.MaVT_TTKD, obj = "" }, JsonRequestBehavior.AllowGet);
        
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }


    }
}