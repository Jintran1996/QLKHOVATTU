using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ToolsApp.Authentication;
using ToolsApp.EntityFramework.DonHang09;
using ToolsApp.EntityFramework.KhoSoi;
using ToolsApp.EntityFramework.VatTu;
using ToolsApp.Models;

namespace ToolsApp.Controllers
{
    public class PhieuYeuCauXuatKho_SoiController : BaseController
    {
        // GET: PhieuYeuCauXuatKho_Soi

        private wqlvattuEntities dbvt_ = new wqlvattuEntities();
        private wqlkhosoiEntities dbks_ = new wqlkhosoiEntities();
        private QLDonHang09Entities db09_ = new QLDonHang09Entities();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult _GetList(SearchPYCXuatSoi search)
        {
            var listPhieu_ = dbks_.NL_DMPHIEUXN.Where(p => (p.SOCTXN.Contains("XSOI_")) &&
            (search.MaphieuSearch == null || search.MaphieuSearch == "" || p.SOCTXN.Contains(search.MaphieuSearch))).OrderByDescending(p => p.STT).ToList();
            ViewBag.listPhieu = listPhieu_;

            return PartialView();
        }
        public ActionResult _GetList_Insert(string iSOCTXN)
        {
            var List = dbks_.NL_CTXUATNHAP.Where(p => (
               (iSOCTXN == null || iSOCTXN == "" || p.SOCTXN.Contains(iSOCTXN))
           )).OrderByDescending(p => p.NGAYKETOAN).ToList();
            ViewBag.List = List;

            return PartialView();
        }

        #region Load view insert
        public ActionResult _Insert()
        {
            var kiHieu = dbvt_.sp_kyhieudonvi_by_manv(User.UserName).FirstOrDefault().KiHieu;
            ViewBag.maKho = dbks_.SP_PHIEUYEUCAUXUATKHO_LOAD_MAKHO(kiHieu, User.UserName).ToList();
            ViewBag.maHinhThuc = dbks_.SP_LOADMAHINHTHUCYEUCAUXUAT(kiHieu).ToList();
            ViewBag.loaiDH = dbks_.NL_DMloaidonhang.ToList();
            ViewBag.donHang = db09_.GET_SODONHANG().ToList();
            ViewBag.maDonHang = dbks_.LOAD_MADONHANG_ND_XK("").ToList();

            string _soCT = dbks_.SP_Load_MaPhieuYCXuatSoi("X", kiHieu).FirstOrDefault().Sophieu;
            ViewData["soCT"] = _soCT;
            return PartialView();
        }
        #endregion

        #region Onchange LoadMaVT
        public JsonResult _LoadMaVT(string maKho)
        {
            try
            {
                //var item = dbks_.SP_LOAD_DATA_MASOI_XD(maKho).ToList();
                var item = dbks_.SP_VATTU_YC_XUATSOI(maKho).ToList();
                ViewBag.loadMaVT = item;
                return Json(new
                {
                    status = 1,
                    title = "",
                    text = "",
                    obj = item
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { status = -1, title = "", text = e.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Onchange LoadMaDH có XD
        public JsonResult _LoadMADH_XD()
        {
            try
            {
                //var item = dbks_.SP_LOAD_DATA_MASOI_XD(maKho).ToList();
                var item = dbks_.LOAD_MADONHANG_ND_XK("").ToList();
                //ViewBag.loadMaVT = item;
                return Json(new
                {
                    status = 1,
                    title = "",
                    text = "",
                    obj = item
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { status = -1, title = "", text = e.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Onchange LoadMaDH không có XD
        public JsonResult _LoadMADH_No_XD()
        {
            try
            {
                //var item = dbks_.SP_LOAD_DATA_MASOI_XD(maKho).ToList();
                var item = db09_.GET_SODONHANG().ToList();
                //ViewBag.loadMaVT = item;
                return Json(new
                {
                    status = 1,
                    title = "",
                    text = "",
                    obj = item
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { status = -1, title = "", text = e.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Lưu vào danh mục phiếu
        public JsonResult _InsertFun(NL_DMPHIEUXNViewModels model)
        {
            try
            {
                #region check data client
                if (dbks_.NL_DMPHIEUXN.FirstOrDefault(c => c.SOCTXN == model.SOCTXN) != null)
                {
                    return Json(new { status = -1, title = "", text = "Chứng từ đã tồn tại", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                if (model.MAKHO == null)
                {
                    return Json(new { status = -1, title = "", text = "Chọn mã kho", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                if (model.MAHTHUC == null)
                {
                    return Json(new { status = -1, title = "", text = "Chọn mã hình thức", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                #endregion
                //var maBP = dbvt_.DMNHANVIENs.FirstOrDefault(p => p.MANV == User.UserName).MABP;
                var dm_phieu = new EntityFramework.KhoSoi.NL_DMPHIEUXN();
                dm_phieu.SOCTXN = model.SOCTXN;
                dm_phieu.NGAY = DateTime.Now;
                dm_phieu.MAHTHUC = model.MAHTHUC;
                dm_phieu.MaNVYC = User.UserName;
                dm_phieu.MABP = dbvt_.DMNHANVIENs.FirstOrDefault(p => p.MANV == User.UserName).MABP;
                dm_phieu.MAKHO = model.MAKHO;
                dm_phieu.GHICHU = model.GHICHU;
                dm_phieu.NOIDEN = model.NOIDEN;
                dm_phieu.MADH = model.MADH;
                dbks_.NL_DMPHIEUXN.Add(dm_phieu);
                dbks_.SaveChanges();

                return Json(new { status = 1, title = "", text = "Thêm phiếu thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {             
                var messageERR = ex.InnerException.InnerException.Message;
                return Json(new { status = -1, title = "Thêm phiếu thất bại!", text = messageERR, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Lưu chi tiết phiếu
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _SaveCTPhieu(NL_CTXUATNHAPViewModels model)
        {
            try
            {
                if (model.SOCTXN == null)
                {
                    return Json(new { status = -1, title = "", text = "Không tìm thấy số chứng từ", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                if (model.MAVT == null)
                {
                    return Json(new { status = -1, title = "", text = "Chọn mã vật tư", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                var vattu = dbks_.SP_VATTU_YC_XUATSOI(model.MAKHO).FirstOrDefault(p => p.ID == model.MAVT);
                var mavt = vattu == null ? "" : vattu.MAVT;
                var hieu = vattu == null ? "" : vattu.HIEU;
                var lo = vattu == null ? "" : vattu.LO;
                var maphieuPD = vattu == null ? "" : vattu.MAPHIEUPD;
                decimal slTon = vattu == null ? 0 : vattu.SOLUONGTT.Value;

                if (model.SOLUONGYC <= 0)
                {
                    return Json(new { status = -1, title = "", text = "Số lượng yêu cầu phải lớn hơn 0.", obj = "" }, JsonRequestBehavior.AllowGet);
                } else if (model.SOLUONGYC > slTon)
                {
                    return Json(new { status = -1, title = "", text = "Số lượng yêu cầu phải nhỏ hơn hoặc bằng số lượng tồn", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                var tb = dbks_.NL_CTXUATNHAP.Where(a => a.SOCTXN == model.SOCTXN && a.MAVT == model.MAVT && a.HIEU == hieu).FirstOrDefault();
                if (tb == null)
                {
                    string key = model.MAVT + "||" + hieu + "||" + lo + "||" + maphieuPD + "||" + model.MAHTHUC + "||" + Guid.NewGuid().ToString();

                    EntityFramework.KhoSoi.NL_CTXUATNHAP model_copy = new EntityFramework.KhoSoi.NL_CTXUATNHAP();
                    model_copy.SOCTXN = model.SOCTXN;
                    model_copy.MAVT = mavt;
                    model_copy.SOLUONGYC = model.SOLUONGYC;
                    model_copy.MAVTTAM = model_copy.MAVT;
                    model_copy.SOLUONGTT = model.SOLUONGYC;
                    model_copy.GHICHU = model.GHICHU;
                    model_copy.LOAIDONHANG = model.LOAIDONHANG;
                    model_copy.MAPHIEUPD = maphieuPD;
                    model_copy.MADH = model.MADH;
                    model_copy.NK_STATIC = true;
                    model_copy.NGAYKETOAN = DateTime.Now;
                    model_copy.DAXUATKHO = 0;
                    model_copy.LO = lo;
                    model_copy.HIEU = hieu;
                    model_copy.KHOAKEYXN = key;
                    model_copy.KHOATHAMCHIEU = Guid.NewGuid().ToString().ToUpper();
                    model_copy.MODIFIED = DateTime.Now;

                    dbks_.NL_CTXUATNHAP.Add(model_copy);
                    dbks_.SaveChanges();

                    return Json(new { status = 1, title = "", text = "Thêm thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    tb.SOLUONGYC = tb.SOLUONGYC + model.SOLUONGYC;
                    tb.LOAIDONHANG = model.LOAIDONHANG;
                    //tb.MADH = model.MADH;
                    tb.GHICHU = model.GHICHU;
                    dbks_.Entry(tb).State = EntityState.Modified;
                    dbks_.SaveChanges();
                    return Json(new { status = 1, title = "", text = "Cập nhật thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "Thêm thất bại!", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Xóa item trong chi tiết phiếu
        public JsonResult _DeleteCTFun(string KHOAKEYXN)
        {
            try
            {
                var item = dbks_.NL_CTXUATNHAP.FirstOrDefault(p => p.KHOAKEYXN == KHOAKEYXN);
                var MAPHIEU = item.SOCTXN;
                dbks_.NL_CTXUATNHAP.Remove(item);
                dbks_.SaveChanges();
                return Json(new { status = 1, title = "", text = "Xóa thành công.", obj = MAPHIEU }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Xóa item trong danh mục phiếu
        public JsonResult _DeleteFun(string SOCTXN)
        {
           
            try
            {
                var item = dbks_.NL_DMPHIEUXN.FirstOrDefault(p => p.SOCTXN == SOCTXN);
                var ctitem = dbks_.NL_CTXUATNHAP.Where(p => p.SOCTXN == item.SOCTXN).ToList();

                if (item.MANVXUAT == null || item.MANVXUAT == "")
                {
                    if (ctitem.Count > 0)
                    {
                        for (int i = 0; i < ctitem.Count; i++)
                        {
                            dbks_.NL_CTXUATNHAP.Remove(ctitem[i]);
                            dbks_.SaveChanges();
                        }

                    }
                    dbks_.NL_DMPHIEUXN.Remove(item);
                    dbks_.SaveChanges();
                    return Json(new { status = 1, title = "", text = "Xóa thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = -1, title = "", text = "Mã phiếu đã xuất không được xóa", obj = "" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Load Detail
        public ActionResult _Detail(string iSOCTXN)
        {
            var item = dbks_.NL_DMPHIEUXN.FirstOrDefault(p => p.SOCTXN == iSOCTXN);
            if (item == null)
            {
                return Json(new { status = -1, title = "", text = "Không tìm thấy mã phiếu.", obj = "" }, JsonRequestBehavior.AllowGet);
            }
            ViewBag.list_Phieu = item;

            var List = dbks_.NL_CTXUATNHAP.Where(p => (
               (iSOCTXN == null || iSOCTXN == "" || p.SOCTXN.Contains(iSOCTXN))
           )).OrderByDescending(p => p.NGAYKETOAN).ToList();

            ViewBag.List = List;

            return PartialView("_DetailCTPhieu");
        }
        #endregion

        #region Load View Edit CTphieu
        public ActionResult _Editview(string KHOAKEYXN)
        {


            var model = dbks_.NL_CTXUATNHAP.FirstOrDefault(p => p.KHOAKEYXN == KHOAKEYXN);
            //var makho = dbks_.NL_DMPHIEUXN.FirstOrDefault(p => p.SOCTXN == model.SOCTXN).MAKHO;
            var dmphieu = dbks_.NL_DMPHIEUXN.Where(p => p.SOCTXN == model.SOCTXN).FirstOrDefault();
            ViewBag.ctphieu = model;
            ViewBag.dmphieu = dmphieu;

            //load vật tư
            var item = dbks_.SP_VATTU_YC_XUATSOI(dmphieu.MAKHO).ToList();
            ViewBag.loadMaVT_ = item;

            var kiHieu_ = dbvt_.sp_kyhieudonvi_by_manv(User.UserName).FirstOrDefault().KiHieu;
            ViewBag.maHinhThuc_ = dbks_.SP_LOADMAHINHTHUCYEUCAUXUAT(kiHieu_).ToList();
            ViewBag.loaiDH_ = dbks_.NL_DMloaidonhang.ToList();
            ViewBag.donHang_ = db09_.GET_SODONHANG().ToList();

            return PartialView("_UpdateCTPhieu");
        }
        #endregion

        #region Update CT phiếu
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _EditFunCT(NL_CTXUATNHAPViewModels model)
        {
            try
            {
                var item = dbks_.NL_CTXUATNHAP.FirstOrDefault(p => p.KHOAKEYXN == model.KHOAKEYXN && p.SOCTXN == model.SOCTXN);
                if (item == null)
                {
                    return Json(new { status = -1, title = "", text = "Không tìm thấy chi tiết phiếu.", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var check_xuat = dbks_.NL_DMPHIEUXN.FirstOrDefault(a => ((a.MANVXUAT != null && a.SOCTXN == model.SOCTXN) || (a.MANVXUAT != "" && a.SOCTXN == model.SOCTXN)));
                    if (check_xuat != null)
                    {
                        var evattu = dbks_.SP_VATTU_YC_XUATSOI(model.MAKHO).FirstOrDefault(p => p.ID == model.MAVT);
                        var emavt = evattu == null ? "" : evattu.MAVT;
                        var ehieu = evattu == null ? "" : evattu.HIEU;
                        var elo = evattu == null ? "" : evattu.LO;
                        var emaphieuPD = evattu == null ? "" : evattu.MAPHIEUPD;
                        decimal eslTon = evattu == null ? 0 : evattu.SOLUONGTT.Value;
                        if (model.SOLUONGYC <= 0 /*|| model.SOLUONGYC > eslTon*/)
                        {
                            return Json(new { status = -1, title = "", text = "Số lượng yêu cầu lớn hơn không và nhỏ hơn số lượng tồn.", obj = "" }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            var mphieu = item.SOCTXN;
                            item.MAVT = emavt;
                            item.HIEU = ehieu;
                            item.LO = elo;
                            item.MAPHIEUPD = emaphieuPD;
                            item.SOLUONGTT = model.SOLUONGYC;
                            //item.MADH = model.MADH;
                            item.LOAIDONHANG = model.LOAIDONHANG;
                            item.GHICHU = model.GHICHU;
                            item.SOLUONGYC = model.SOLUONGYC;
                            item.MODIFIED = DateTime.Now;
                            dbks_.Entry(item).State = EntityState.Modified;
                            dbks_.SaveChanges();
                            return Json(new { status = 1, title = "", text = "Cập nhật thành công.", obj = mphieu }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        return Json(new { status = -1, title = "", text = "Phiếu đã được xuất.", obj = "" }, JsonRequestBehavior.AllowGet);
                    }
                }

            }
            catch (Exception ex)
            {
                var messageERR = ex.InnerException.InnerException.Message;
                return Json(new { status = -1, title = "", text = messageERR, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

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
                }else
                {
                    ViewBag.List = CTPhieu;
                }
            }
            return PartialView();
        }
        #endregion

        #region Xóa phiếu mua hàng khi chưa có chi tiết 
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult Xoa_DonHangKhiDongKhongChiTiet_Click(string SOCTXN)
        {
            try
            {
                var dataCheckDH = dbks_.NL_DMPHIEUXN.Where(a => a.SOCTXN == SOCTXN).FirstOrDefault();
                if (dataCheckDH != null)
                {
                    var dataCheckDHCT = dbks_.NL_CTXUATNHAP.Where(a => a.SOCTXN == dataCheckDH.SOCTXN).ToList();
                    if (dataCheckDHCT.Count() > 0)
                    {
                        return Json(new { status = 1, title = "", text = "Đóng không xoá chứng từ", obj = "" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        dbks_.NL_DMPHIEUXN.Remove(dataCheckDH);
                        dbks_.SaveChanges();
                        return Json(new { status = -1, title = "", text = "Đóng xoá chứng từ do chưa có chi tiết phiếu", obj = "" }, JsonRequestBehavior.AllowGet);
                    }
                }
                {
                    return Json(new { status = -1, title = "", text = "Chứng từ đã được xoá do chưa có chi tiết phiếu", obj = "" }, JsonRequestBehavior.AllowGet);
                }


            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

    }
}