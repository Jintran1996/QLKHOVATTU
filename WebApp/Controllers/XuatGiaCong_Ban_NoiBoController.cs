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
    public class XuatGiaCong_Ban_NoiBoController : BaseController
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
        public static string CREATE_KHOAKEYXN(string TEXT)
        {
            string KHOAKEYXN = String.Empty;
            ThuVien.Xulychuoi xlchuoi = new ThuVien.Xulychuoi();
            KHOAKEYXN = xlchuoi.RandomString(20, false);

            return TEXT + "_" + KHOAKEYXN;
        }
        // GET: XuatGiaCong_Ban_NoiBo
        public ActionResult Index()
        {
            return View();
        }
        public JsonResult PhanQuyen()
        {
            bool isCheck = false;
            var data = db_.SP_LOADKHOQUYENXUAT_VER1(User.UserName).FirstOrDefault();
            if (data != null)
            {
                isCheck = true;
                return Json(new { status = 1, title = "", text = "", obj = isCheck }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { status = -1, title = "", text = "Những nhân viên phòng cung ứng mới được sử dụng được chức năng này.", obj = isCheck }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult Load_MaVT(string MaKho)
        {
            var kihieudonvi = db_.sp_kyhieudonvi_by_manv(User.UserName).FirstOrDefault();
            var data = db_.XUATNHAPNOIBO_LOADCHITIETVATTU_XUATKHO_BY_KYHIEUDONVI_MAKHO( kihieudonvi.KiHieu, MaKho,"",User.UserName).ToList();
            if (data != null)
            {
                return Json(new { status = 1, title = "", text = "", obj = data }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { status = -1, title = "", text = "", obj = "" }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult _GetList(string SOCTXN)
        {
            var data = db_.SP_LOADCTPHIEUYCXUATKHO_DONVI(SOCTXN).ToList();
            ViewBag.data = data;
            return PartialView();
        }
        public ActionResult _Insert()
        {
            var kihieudonvi = db_.sp_kyhieudonvi_by_manv(User.UserName).FirstOrDefault();
            var data_SoChungTu = SoChungTu(kihieudonvi.KiHieu);
            ViewData["data_SoChungTu"] = data_SoChungTu;
            var data_Kho = db_.SP_KTQKFORWEB(kihieudonvi.KiHieu, "L").ToList();
            ViewBag.data_Kho = data_Kho;
            var data_loaixn = db_.DMLOAIXNs.Where(p => p.LOAIXN == "XGGC" || p.LOAIXN == "XBAN" || p.LOAIXN == "XVCNB").ToList();
            ViewBag.data_loaixn = data_loaixn;
            var data_KhoNhan = db_.DMKHONHANs.ToList();
            ViewBag.data_KhoNhan = data_KhoNhan;
            var data_NVYCXN = db_.Load_MANVYC_XNKHO().ToList();
            ViewBag.data_NVYCXN = data_NVYCXN;
            var data_CN_DONHANG = db09_.CN_DONHANG.ToList();
            ViewBag.data_CN_DONHANG = data_CN_DONHANG;
            return PartialView();
        }

        [HttpPost]
        public JsonResult Insert_GCN_(xuatkho_GGC_model model)
        {
           if(model.SOCTXN==""|| model.SOCTXN==null)
            {
                return Json(new { status = -1, title = "", text = "Không tạo được số chứng từ xuất nhập!!", obj = "" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                if(model.MaVT==""||model.MaVT==null||model.MaVT== "false")
                {
                    return Json(new { status = -1, title = "", text = "Vui lòng chọn mã vật tư!!", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if(model.MaNVYC==""||model.MaNVYC==null||model.MaNVYC== "false")
                    {
                        return Json(new { status = -1, title = "", text = "Vui lòng chọn mã nhân viên yêu cầu!!", obj = "" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var NhanVien = db_.DMNHANVIENs.Where(a=>a.MANV== User.UserName).FirstOrDefault();
                        var data = db_.XUATNHAPNOIBO_LOADCHITIETVATTU_XUATKHO_BY_KYHIEUDONVI_MAKHO(model.SOCTXN.Substring(0,3), model.MaKho, model.MaVT, User.UserName).FirstOrDefault();
                        if (data == null)
                        {
                            return Json(new { status = -1, title = "", text = "Không tìm thấy chi tiết vật tư", obj = "" }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            if (data.MAVT == string.Empty || data.MAPHIEUPD == string.Empty || data.MAKHO == string.Empty)
                            {
                                return Json(new { status = -1, title = "", text = "Dữ liệu không đầy đủ vui lòng chọn lại mã vật tư yêu cầu xuất", obj = "" }, JsonRequestBehavior.AllowGet);
                            }

                            if (Convert.ToDecimal(model.SoLuongXuat) > data.SOLUONGTON)
                            {
                                return Json(new { status = -1, title = "", text = "Số lượng yêu cầu xuất không được lớn hơn số lượng tồn kho "+data.SOLUONGTON, obj = "" }, JsonRequestBehavior.AllowGet);
                            }
                            var insert = db_.SP_XUATNHAP_INSERT_NEW_VER2(
                                                    model.SOCTXN.Trim()
                                                  , model.LoaiXuatNhap
                                                  , model.MaNVYC
                                                  , User.UserName
                                                  , NhanVien.MANV
                                                  , NhanVien.MABP
                                                  , data.MAKHO
                                                  , data.MAVT
                                                  , Convert.ToDecimal(model.SoLuongXuat)
                                                  , Convert.ToDecimal(model.SoLuongXuat)
                                                  , data.MAPHIEUPD
                                                  , model.GhiChu
                                                  , "XUATNB"
                                                  , model.SOCTKT
                                                  , DateTime.Now
                                                  , data.MAVT
                                                  , string.Empty
                                                  , 0
                                                  , 1
                                                  , CREATE_KHOAKEYXN("")
                                                  , string.Empty
                                                  , 0
                                                  , new DateTime(1900, 01, 01)
                                                  , string.Empty
                                                  , string.Empty
                                                  , 0
                                                  , 0
                                                  , string.Empty
                                                  , string.Empty
                                                  , string.Empty
                                                  , model.KhoNhan
                                                  , (model.DonHangCongNghiep=="false"?string.Empty: model.DonHangCongNghiep));
                            if (insert > 0)
                            {
                                return Json(new { status = 1, title = "", text = "Thêm thành công!!", obj = "" }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                return Json(new { status = -1, title = "", text = "Thêm thất bại!!", obj = "" }, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                }
            }
        }


        [HttpPost]
        public JsonResult Delete_Action(string KHOAKEYXN)
        {
            try
            {
                if (KHOAKEYXN == string.Empty)
                {
                    return Json(new { status = -1, title = "", text = "Không tìm thấy khoá!!", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var data = db_.XUATNHAPs.Where(a => a.KHOAKEYXN == KHOAKEYXN).FirstOrDefault();
                    if(data ==null)
                    {
                        return Json(new { status = -1, title = "", text = "Không tìm thấy chi tiết xuất nhập!!", obj = "" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        db_.XUATNHAPs.Remove(data);
                        db_.SaveChanges();
                        return Json(new { status = 1, title = "", text = "Xoá thành công!!", obj = "" }, JsonRequestBehavior.AllowGet);
                    }
                }

            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}