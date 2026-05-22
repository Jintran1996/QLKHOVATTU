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
    public class LapDonHangGCSoiController : BaseController
    {
        private wqlvattuEntities vt_ = new wqlvattuEntities();
        private wqlkhosoiEntities ks_ = new wqlkhosoiEntities();
        // GET: LapDonHangGCSoi
        public ActionResult Index()
        {
            return View();
        }
        private string new_DHGC()
        {
            var MaDh = ks_.SP_GC_DONHANGGCSOI_TAODONHANG(User.UserName,DateTime.Now).FirstOrDefault();
            return MaDh;
        }
        #region Load View
        public ActionResult _Getlist(string MADHGCSearch)
        {
            var List = ks_.SP_GC_DONHANGGCSOI().Where(p => p.MADH_GCSOI == MADHGCSearch.Trim() || p.MADH_GCSOI == null || p.MADH_GCSOI == "" || p.MADH_GCSOI.Contains(MADHGCSearch.Trim())).ToList();
            ViewBag.List = List;
            return PartialView();
        }
        #endregion
        #region Load View Chi tiết
        public ActionResult _ChiTietListView(string MADH_GCSOI)
        {
            var List = ks_.DONHANGGCSOI_CT.Where(p => p.MADH_GCSOI == MADH_GCSOI).ToList();
            ViewBag.List = List;
            return PartialView();
        }
        public ActionResult _ChiTietListViewKhoa(string MADH_GCSOI)
        {
            var List = ks_.DONHANGGCSOI_CT.Where(p => p.MADH_GCSOI == MADH_GCSOI).ToList();
            ViewBag.List = List;
            return PartialView();
        }
        #endregion
        #region kiểm tra thêm mới hoặc sửa
        public ActionResult _DetailForEdit(string MADH)
        {
            ViewBag.LoaiDH = ks_.SP_GC_DMLoaiDonHang().ToList();
            ViewBag.MaSoiThanhPham = ks_.SP_GC_MASOITHANHPHAM().ToList();
            ViewBag.DVYC = ks_.SP_GC_LOAD_DONVIYC_ByMANV(User.UserName).ToList();
            ViewBag.DVTH = ks_.SP_GC_LOAD_DONVITH().ToList();
            ViewBag.LoaiGC = ks_.SP_GC_LOAD_LOAIGIACONG().ToList();
            ViewBag.GDDVYC = ks_.SP_GC_LOAD_MANVDONVIYC_ByMANV(User.UserName).ToList();
            if (MADH == null || MADH == string.Empty)
            {
                var maDonHang = new_DHGC();
                ViewData["MaDonHang"] = maDonHang;
                return PartialView("_Insert");
            }
            else
            {
                var data = ks_.DONHANGGCSOIs.Where(p => p.MADH_GCSOI == MADH).FirstOrDefault();
                ViewBag.Data = data;
                var chitiet = ks_.DONHANGGCSOI_CT.Where(p=>p.MADH_GCSOI == MADH).ToList();
                ViewBag.List = chitiet;
                return PartialView("_Update");
            }
        }
        #endregion

        #region Load GD đơn vị thực hiện
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _ListGDDVTH(string IDMADVTH)
        {
            try
            {
                var items = ks_.SP_GC_LOAD_MANVDONVITH_ByMADV(IDMADVTH).ToList();
                return Json(new { status = 1, title = "", text = "", obj = items }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
        #region Load Diễn giải
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _LoadDienGiai(string MASOITHANHPHAM)
        {
            try
            {
                var dienGiai = ks_.SP_GC_MASOITHANHPHAM().Where(p => p.MAVATTU == MASOITHANHPHAM).ToList();
                return Json(new { status = 1, title = "", text = "", obj = dienGiai }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
        #region Chuyển tiếp sang chi tiết phiếu chuyển thông tin
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult ChuyenTiep_click(DONHANGGCSOIModels model)
        {
            if (model.LOAIGIACONG == null)
            {
                return Json(new { status = -1, title = "", text = "Loại gia công không được để trống", obj = "" }, JsonRequestBehavior.AllowGet);
            }
            if (model.IDMADV_YC == null)
            {
                return Json(new { status = -1, title = "", text = "Đơn vị yêu cầu không được để trống", obj = "" }, JsonRequestBehavior.AllowGet);
            }
            if (model.IDMADV_TH == null)
            {
                return Json(new { status = -1, title = "", text = "Đơn vị thực hiện không được để trống", obj = "" }, JsonRequestBehavior.AllowGet);
            }
            if (model.MANV_DONVIYC_XX == null)
            {
                return Json(new { status = -1, title = "", text = "GD đơn vị yêu cầu không được để trống", obj = "" }, JsonRequestBehavior.AllowGet);
            }
            if (model.MANV_DONVITH_XACNHAN == null)
            {
                return Json(new { status = -1, title = "", text = "GD đơn vị thực hiện không được để trống", obj = "" }, JsonRequestBehavior.AllowGet);
            }
            var tb = ks_.DONHANGGCSOIs.FirstOrDefault(p => p.MADH_GCSOI == model.MADH_GCSOI);
            if (tb == null)
            {              
                var addDonHang = ks_.SP_GC_INSERT_DONHANGGCSOI(new_DHGC(), model.LOAIGIACONG, model.IDMADV_YC, model.IDMADV_TH, User.UserName, DateTime.Now, model.MANV_DONVIYC_XX, model.MANV_DONVITH_XACNHAN);
                return Json(new { status = 1, title = "", text = "Thêm thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { status = -1, title = "", text = "Mã đơn hàng gia công đã tồn tại.", obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
        #region Acction Lưu chi tiết
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult Luu_click(DONHANGGCSOI_CTModels model)
        {
            #region Xử lý ngày
            CultureInfo cul = CultureInfo.GetCultureInfo("en-GB");
            var NGAYYEUCAU_ = new DateTime();
            if (!string.IsNullOrEmpty(model.NGAYYEUCAU_))
            {
                try
                {
                    NGAYYEUCAU_ = DateTime.ParseExact(model.NGAYYEUCAU_, "dd/MM/yyyy", cul);
                    if (NGAYYEUCAU_ <= DateTime.Now)
                    {
                        return Json(new { status = -1, title = "", text = "Ngày yêu cầu phải lớn hơn ngày hiện tại.", obj = "" }, JsonRequestBehavior.AllowGet);
                    }
                    model.NGAYYCAU = new DateTime(NGAYYEUCAU_.Year,
                        NGAYYEUCAU_.Month, NGAYYEUCAU_.Day, 0, 0, 0);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
            var NGAYKETTHUC_ = new DateTime();
            if (!string.IsNullOrEmpty(model.NGAYKETTHUC_))
            {
                try
                {
                    NGAYKETTHUC_ = DateTime.ParseExact(model.NGAYKETTHUC_, "dd/MM/yyyy", cul);
                    if (NGAYKETTHUC_ < NGAYYEUCAU_)
                    {
                        return Json(new { status = -1, title = "", text = "Ngày kết thúc phải lớn hơn ngày yêu cầu.", obj = "" }, JsonRequestBehavior.AllowGet);
                    }
                    model.NGAYKETTHUC = new DateTime(NGAYKETTHUC_.Year,
                        NGAYKETTHUC_.Month, NGAYKETTHUC_.Day, 0, 0, 0);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
            #endregion
            if (model.Loaidh == null)
            {
                return Json(new { status = -1, title = "", text = "Loại đơn hàng không được để trống", obj = "" }, JsonRequestBehavior.AllowGet);
            }
            if (model.MASOITHANHPHAM == null)
            {
                return Json(new { status = -1, title = "", text = "Mã sợi thành phẩm không được để trống", obj = "" }, JsonRequestBehavior.AllowGet);
            }
            if (model.SOLUONGYEUCAU == null)
            {
                return Json(new { status = -1, title = "", text = "Số lượng không được để trống", obj = "" }, JsonRequestBehavior.AllowGet);
            }
            try
            {
                var dienGiai = ks_.SP_GC_MASOITHANHPHAM().Where(p => p.MAVATTU == model.MASOITHANHPHAM).FirstOrDefault();
                var ct = ks_.DONHANGGCSOI_CT.Where(p => p.MADH_GCSOI == model.MADH_GCSOI && p.Loaidh == model.Loaidh && p.MASOITHANHPHAM == model.MASOITHANHPHAM && p.SOLUONGYEUCAU == model.SOLUONGYEUCAU ).FirstOrDefault();
                if(ct == null)
                {
                    var model_copy = new DONHANGGCSOI_CT();
                    model_copy.MADH_GCSOI = model.MADH_GCSOI;
                    model_copy.NGAYYCAU = NGAYYEUCAU_;
                    model_copy.NGAYKETTHUC = NGAYKETTHUC_;
                    model_copy.SOLUONGYEUCAU = model.SOLUONGYEUCAU;
                    model_copy.MASOITHANHPHAM = model.MASOITHANHPHAM;
                    model_copy.DIENGIAI = dienGiai.Tenvattu;
                    model_copy.Loaidh = model.Loaidh;
                    model_copy.KHOA_ID = Guid.NewGuid();
                    ks_.DONHANGGCSOI_CT.Add(model_copy);
                    ks_.SaveChanges();
                    return Json(new { status = 1, title = "", text = "Thêm chi tiết đơn hàng thành công", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    ct.Loaidh = model.Loaidh;
                    ct.SOLUONGYEUCAU = model.SOLUONGYEUCAU;
                    ct.MASOITHANHPHAM = model.MASOITHANHPHAM;
                    ks_.Entry(ct).State = EntityState.Modified;
                    ks_.SaveChanges();
                    return Json(new { status = 1, title = "", text = "Cập nhật thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
                }    
                
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
        #region Xóa phiếu mua hàng khi chưa có chi tiết 
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult Xoa_DonHangKhiDongKhongChiTiet_Click(string MADH_GCSOI)
        {
            try
            {
                var dataCheckPhieu = ks_.DONHANGGCSOIs.Where(a => a.MADH_GCSOI == MADH_GCSOI).FirstOrDefault();
                if (dataCheckPhieu != null)
                {
                    var dataCheckDHCT = ks_.DONHANGGCSOI_CT.Where(a => a.MADH_GCSOI == dataCheckPhieu.MADH_GCSOI).ToList();
                    if (dataCheckDHCT.Count() > 0)
                    {
                        return Json(new { status = 1, title = "", text = "Đóng không xoá đơn hàng gia công sợi", obj = "" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var xoaDonHang = ks_.SP_GC_DELETE_DONHANGGCSOI(MADH_GCSOI);
                        return Json(new { status = -1, title = "", text = "Đóng xoá đơn hàng đơn hàng khi chưa có chi tiết", obj = "" }, JsonRequestBehavior.AllowGet);
                    }
                }
                {
                    return Json(new { status = -1, title = "", text = "Đơn hàng đã được xoá", obj = "" }, JsonRequestBehavior.AllowGet);
                }


            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
        #region Action delete phiếu chuyển thông tin
        [HttpPost]
        public JsonResult delete_Click(string MADH_GCSOI)
        {
            try
            {
                if (MADH_GCSOI == String.Empty)
                {
                    return Json(new { status = -1, title = "", text = "Không tìm thấy mã đơn hàng.", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var dataCheckDH = ks_.DONHANGGCSOIs.Where(a => a.MADH_GCSOI == MADH_GCSOI).FirstOrDefault();
                    if (dataCheckDH.MANVCN != User.UserName)
                    {
                        return Json(new { status = -1, title = "", text = "Chỉ nhân viên tạo đơn hàng mới có quyền xóa.", obj = "" }, JsonRequestBehavior.AllowGet);
                    }
                    if (dataCheckDH.HIEULUCXX == true)
                    {
                        return Json(new { status = -1, title = "", text = "Đơn hàng đã xem xét từ đơn vị yêu cầu không thể xóa.", obj = "" }, JsonRequestBehavior.AllowGet);
                    }
                    if (dataCheckDH.HIEULUCXACNHAN == true)
                    {
                        return Json(new { status = -1, title = "", text = "Đơn hàng đã xác nhận từ đơn vị thực hiện không thể xóa.", obj = "" }, JsonRequestBehavior.AllowGet);
                    }
                    if (dataCheckDH == null)
                    {
                        return Json(new { status = -1, title = "", text = "Không tìm thấy đơn hàng gia công sợi.", obj = "" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var dataCheckCTDH = ks_.DONHANGGCSOI_CT.Where(a => a.MADH_GCSOI == MADH_GCSOI).ToList();

                        foreach (var a in dataCheckCTDH)
                        {
                            ks_.DONHANGGCSOI_CT.Remove(a);
                            ks_.SaveChanges();
                        }
                        ks_.DONHANGGCSOIs.Remove(dataCheckDH);
                        ks_.SaveChanges();
                        return Json(new { status = 1, title = "", text = "Xoá đơn hàng thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = -2, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
        #region Action tạo phiếu mới
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult TaoPhieuMoi_Click(string MADH_GCSOI)
        {
            try
            {
                var data = ks_.DONHANGGCSOI_CT.Where(a => a.MADH_GCSOI == MADH_GCSOI).ToList();
                if (data.Count() > 0)// kiểm tra chi tiết đơn hàng đã có dữ liệu chưa
                {
                    return Json(new { status = -1, title = "", text = "Đã có chi tiết phiếu không thể tạo mới được", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                else// tiến hành xoá đơn hàng khi chưa có dữ liệu chi tiết đơn hàng
                {
                    var dataDelete = ks_.DONHANGGCSOIs.Where(a => a.MADH_GCSOI == MADH_GCSOI).FirstOrDefault();
                    ks_.DONHANGGCSOIs.Remove(dataDelete);
                    ks_.SaveChanges();
                    return Json(new { status = 1, title = "", text = "Đã xoá phiếu thông tin đã tạo trước đó. Vì chưa có dữ liệu chi tiết", obj = "" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = -2, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion
        #region Acction Xoá đơn hàng gia công sợi chi tiết
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult Xoa_ChiTietLapPhieu_Click(string IDKHOA)
        {
            try
            {
                var data = ks_.SP_GC_DELETE_DONHANGGCSOI_CT(IDKHOA);
                if (data > 0)
                {
                    return Json(new { status = 1, title = "", text = "Xoá chi tiết phiếu chuyển thành công", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = -1, title = "", text = "Xoá chi tiết phiếu chuyển thất bại", obj = "" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion
        #region Action update chi tiết đơn hàng
        [HttpPost]
        public ActionResult update_Detail_Click(DONHANGGCSOIModels model)
        {
            try
            {
                var list = new List<DONHANGGCSOI>();
                var list_temp = new List<DONHANGGCSOI_CT>();
                var chitiets = new List<DONHANGGCSOI_CT>();
                var dataCheckDH = ks_.DONHANGGCSOIs.Where(p => p.MADH_GCSOI == model.MADH_GCSOI).FirstOrDefault();
                if (dataCheckDH == null)
                {
                    return Json(new { status = -1, title = "", text = "Không thể lưu ở đây.", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                if (dataCheckDH.HIEULUCXX == true)
                {
                    return Json(new { status = -1, title = "", text = "Đơn hàng đã xem xét từ đơn vị yêu cầu không thể sửa.", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                if (dataCheckDH.HIEULUCXACNHAN == true)
                {
                    return Json(new { status = -1, title = "", text = "Đơn hàng đã xác nhận từ đơn vị thực hiện không thể sửa.", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                if (model.Detail.Count <= 0)
                {
                    return Json(new { status = -2, title = "", text = "Chưa có chi tiết không thể sửa được. Vui lòng tạo lại phiếu chuyển thông tin", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                #region Chi tiết
                if (model.Detail.Any())
                {
                    foreach (var items in model.Detail)
                    {
                        if (items.Delete == false)
                        {
                            var dienGiai = ks_.SP_GC_MASOITHANHPHAM().Where(p => p.MAVATTU == items.MASOITHANHPHAM).FirstOrDefault();
                            
                            var chitiet = ks_.DONHANGGCSOI_CT.FirstOrDefault(p => p.KHOA_ID == items.KHOA_ID);
                            if (chitiet == null)
                            {
                                
                                #region kiểm tra trùng trong chi tiết
                                if (ks_.DONHANGGCSOI_CT.FirstOrDefault(p => p.KHOA_ID == items.KHOA_ID) != null)
                                {
                                    return Json(new { status = -2, title = "", text = "Thêm không thành công. Đã có phiếu thông tin này ", obj = "" }, JsonRequestBehavior.AllowGet);
                                }
                                else
                                {                                 
                                    chitiet = new DONHANGGCSOI_CT()
                                    {
                                        MADH_GCSOI = model.MADH_GCSOI,
                                        Loaidh = items.Loaidh,
                                        MASOITHANHPHAM = items.MASOITHANHPHAM,
                                        SOLUONGYEUCAU = items.SOLUONGYEUCAU,
                                        KHOA_ID = Guid.NewGuid(),
                                        DIENGIAI = dienGiai.Tenvattu,
                                        NGAYYCAU = items.NGAYYCAU,
                                        NGAYKETTHUC = items.NGAYKETTHUC                                      
                                    };                                  
                                    ks_.DONHANGGCSOI_CT.Add(chitiet);
                                    ks_.SaveChanges();
                                }
                                #endregion
                            }
                            else
                            {                              
                                chitiet.Loaidh = items.Loaidh;
                                chitiet.MASOITHANHPHAM = items.MASOITHANHPHAM;
                                chitiet.SOLUONGYEUCAU = items.SOLUONGYEUCAU;
                                chitiet.DIENGIAI = dienGiai.Tenvattu;
                                ks_.Entry(chitiet).State = EntityState.Modified;
                            }
                            list_temp.Add(chitiet);
                        }
                        #region Xóa chi tiết
                        if (items.Delete == true && items.KHOA_ID != null)
                        {
                            var DetailCT = ks_.DONHANGGCSOI_CT.FirstOrDefault(p => p.KHOA_ID == items.KHOA_ID);
                            ks_.DONHANGGCSOI_CT.Remove(DetailCT);
                            ks_.SaveChanges();
                        }
                        #endregion
                    }
                }
                #endregion
                ks_.SaveChanges();
                return Json(new { status = 1, title = "", text = "Cập nhật thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { status = -2, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
    }
}