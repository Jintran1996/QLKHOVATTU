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
using ToolsApp.EntityFramework.NetsuiteTT;
using ToolsApp.Models;
using System.Net.Mail;
using System.Web.Script.Serialization;
using System.Globalization;

namespace ToolsApp.Controllers
{
    public class LapPhieuBangKeNhapSoiController : BaseController
    {
        private wqlvattuEntities vt_ = new wqlvattuEntities();
        private wqlkhosoiEntities ks_ = new wqlkhosoiEntities();
        private NetsuiteTTEntities1 ns_ = new NetsuiteTTEntities1();
        // GET: LapPhieuBangKeNhapSoi
        public ActionResult Index()
        {
            return View();
        }
        private string new_Phieu()
        {
            var donVi = vt_.DMNHANVIENs.Where(p => p.MANV == User.UserName).FirstOrDefault();
            var MaDh = ks_.SP_GC_TAOPHIEUBANGKE_GC_DMPHIEU_BANGKENHAPSOI(DateTime.Now, donVi.IDMaDV).FirstOrDefault();
            return MaDh;
        }
        #region Load Getlist
        public ActionResult _Getlist(string SoPhieuSearch)
        {
            var donVi = vt_.DMNHANVIENs.Where(p => p.MANV == User.UserName).FirstOrDefault();
            var List = ks_.SP_GC_LOAD_GC_DMPHIEU_BANGKENHAPSOI(donVi.IDMaDV).Where(p => p.SOPHIEU == SoPhieuSearch.Trim() || p.SOPHIEU == null || p.SOPHIEU == "" || p.SOPHIEU.Contains(SoPhieuSearch.Trim())).ToList();
            ViewBag.List = List;
            return PartialView();
        }
        #endregion
        #region Load Chi Tiết Phiếu
        public ActionResult _ChiTietListView(string SOPHIEU)
        {
            var List = ks_.GC_CTPHIEUXUATGC.Where(p => p.SOPHIEU == SOPHIEU).ToList();
            ViewBag.List = List;
            return PartialView();
        }
        public ActionResult _ChiTietListViewKhoa(string SOPHIEU)
        {
            var List = ks_.GC_CTPHIEUXUATGC.Where(p => p.SOPHIEU == SOPHIEU).ToList();
            ViewBag.List = List;
            return PartialView();
        }
        #endregion
        #region kiểm tra thêm mới hoặc sửa
        public ActionResult _DetailForEdit(string SOPHIEU)
        {
            var donVi = vt_.DMNHANVIENs.Where(p => p.MANV == User.UserName).FirstOrDefault();
            ViewBag.donVi = ks_.SP_LOAD_DMKHO_BY_IDMADV(donVi.IDMaDV).ToList();
            ViewBag.hinhThuc = ks_.SP_BANGKENHAPSOI_MAHINHTHUC_LOAD_MAHTHUC_MADV(donVi.IDMaDV).ToList();
            ViewBag.khachHang = ks_.SP_LDD_DMNOIDEN_LOAD_BY_MADV_MANV(donVi.IDMaDV, User.UserName).ToList();
            ViewBag.nguoiXX = ks_.SP_GC_MACONGTHUC_LOAD_NVXX_BY_MADV(donVi.IDMaDV).ToList();
            ViewBag.MaSoi = ks_.SP_GC_DMNGUYENLIEU_LOAD_COMBOBOX("").ToList();
            ViewBag.MaSoiHoantat = ks_.VATTU2024_SP_GC_DMNGUYENLIEU_SOIHOANTAT("").ToList();
            ViewBag.Hieu = ks_.NL_DMNHACCAP.Where(p=>p.HIEULUC == true).ToList();
            ViewBag.DVT = ks_.SP_GC_DONVITINH_LOAD().ToList();
            ViewBag.LoaiYCSX = ns_.LoaiYeuCauSanXuats.Where(p => p.GC_SOI == true).ToList();
            ViewBag.MACONGDOAN = ks_.SP_GC_DMMACONGDOAN_LOAD_COMBOBOX("").ToList();
            ViewBag.DANGCONGDOAN = ks_.SP_GC_DMDANGCONGDOAN_LOAD_COMBOBOX("").ToList();
            ViewBag.MADH_GCS = ks_.SP_GC_LOAD_MADH_GCSOI().ToList();
            if (SOPHIEU == null || SOPHIEU == string.Empty)
            {
                var maSoPhieu = new_Phieu();
                ViewData["SoPhieu"] = maSoPhieu;
                ViewBag.KhoaDinhMuc = ks_.SP_CTPHIEUXUATGC_LOAD_MACONGTHUC_BY_MADV_MAKH("", donVi.IDMaDV, "").ToList();
                
                return PartialView("_Insert");
            }
            else
            {
                ViewBag.Data = ks_.GC_DMPHIEU_BANGKENHAPSOI.Where(p => p.SOPHIEU == SOPHIEU).FirstOrDefault();
                ViewBag.List = ks_.GC_CTPHIEUXUATGC.Where(p => p.SOPHIEU == SOPHIEU).ToList();
                return PartialView("_Update");
            }
        }
        #endregion
        #region Load số định mức
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _Loaddinhmuc(string KHOACTHUC)
        {
            try
            {
                var items = ks_.SP_GC_CHITIETCONGTHUC_LOAD_SLPD_BY_KHOA_211223(KHOACTHUC).ToList();
                return Json(new { status = 1, title = "", text = "", obj = items }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
        #region Load Lô
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _LoadLo(string HIEU)
        {
            try
            {
                var items = ks_.SP_LOAD_NL_DMLO_BY_HIEU_GC_CTPHIEUXUATNHAP(HIEU).ToList();
                return Json(new { status = 1, title = "", text = "", obj = items }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
        #region Load Mã sợi hoàn tất
        //[ValidateInput(false)]
        //[HttpPost]
        //public JsonResult _LoadMaSoiHoanTat(string MADH_GCS)
        //{
        //    try
        //    {
        //        var items = ks_.SP_GC_LOAD_MASOITHANHPHAM_BY_MADHGCSOI(MADH_GCS).ToList();
        //        return Json(new { status = 1, title = "", text = "", obj = items }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
        //    }
        //}
        #endregion
        #region Chuyển tiếp sang chi tiết bảng kê gia công sợi
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult ChuyenTiep_click(GC_DMPHIEU_BANGKENHAPSOIModels model, string THANGNAM)
        {
            #region xử lý ngày
            CultureInfo cul = CultureInfo.GetCultureInfo("en-GB");
            var THANGNAM_ = new DateTime();
            if (!string.IsNullOrEmpty(THANGNAM))
            {
                try
                {
                    THANGNAM_ = DateTime.ParseExact(THANGNAM, "MM/yyyy", cul);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
            else
            {
                return Json(new { status = -1, title = "Vui lòng chọn tháng.", text = "", obj = "" }, JsonRequestBehavior.AllowGet);
            }
            #endregion
            var thang = THANGNAM_.ToString("MM");
            var nam = THANGNAM_.ToString("yyyy");
            var tb = ks_.GC_DMPHIEU_BANGKENHAPSOI.FirstOrDefault(p => p.SOPHIEU == model.SOPHIEU);
            if (tb == null)
            {
                var addDonHang = ks_.SP_DMPHIEU_BANGKENHAPSOI_INSERT_211223(new_Phieu(), model.MADV, Convert.ToInt32(nam), Convert.ToInt32(thang), model.MAKH, model.NHAPKHO, model.LYDOSANXUAT, model.MALOAICONGDOAN, model.CONGDOAN, User.UserName, model.MANV_XX,model.LoaiYCSX);
                return Json(new { status = 1, title = "", text = "Thêm thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { status = -1, title = "", text = "Số phiếu đã tồn tại.", obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
        #region Acction Lưu chi tiết
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult Luu_click(GC_CTPHIEUXUATGCModels model)
        {          
            try
            {
                //if (model.MADH_GCSOI == null)
                //{
                //    return Json(new { status = -1, title = "", text = "Mã đơn hàng gia công không được để trống.", obj = "" }, JsonRequestBehavior.AllowGet);
                //}
                if (model.MASOI == null)
                {
                    return Json(new { status = -1, title = "", text = "Mã sợi không được để trống.", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                if (model.KHOACTHUC == null)
                {
                    return Json(new { status = -1, title = "", text = "Khóa định mức không được để trống.", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                var ct = ks_.GC_CTPHIEUXUATGC.Where(p => p.MADH_GCSOI == model.MADH_GCSOI && p.SOPHIEU == model.SOPHIEU && p.MASOIHOANTAT == model.MASOIHOANTAT && p.MASOI == model.MASOI && p.KHOACTHUC == model.KHOACTHUC && p.SOCANH == model.SOCANH).FirstOrDefault();
                if (ct == null)
                {
                    var luuCT = ks_.SP_GC_CTPHIEUXUATGIACONG_INSERT_22122023(model.KHOACTHUC, "", model.MASOI, "", model.HIEU, model.LO, model.SOLUONGNhap, model.SOLUONGXuat,
                        model.SLHaoHutDM, model.DVT, Convert.ToInt32(model.SOTRUC), model.SOCANH, model.KHOTRUC, model.CHIEUDAI, model.GHICHU, model.SOPHIEU, model.MACONGDOAN, model.DANGCONGDOAN, model.MADH_GCSOI, model.MASOIHOANTAT
                        );
                    return Json(new { status = 1, title = "", text = "Thêm chi tiết bảng kê sợi thành công", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    ct.KHOACTHUC = model.KHOACTHUC;
                    ct.MASOI = model.MASOI;
                    ct.HIEU = model.HIEU;
                    ct.LO = model.LO;          
                    ct.SOLUONGNhap = model.SOLUONGNhap;
                    ct.SOLUONGXuat = model.SOLUONGXuat;
                    ct.SLHaoHutDM = model.SLHaoHutDM;
                    ct.DVT = model.DVT;
                    ct.SOTRUC = Convert.ToInt32(model.SOTRUC);
                    ct.SOCANH = model.SOCANH;
                    ct.KHOTRUC = model.KHOTRUC;
                    ct.CHIEUDAI = model.CHIEUDAI;
                    ct.GHICHU = model.GHICHU;
                    ct.SOPHIEU = model.SOPHIEU;
                    ct.MACONGDOAN = model.MACONGDOAN;
                    ct.DANGCONGDOAN = model.DANGCONGDOAN;
                   // ct.MADH_GCSOI = model.MADH_GCSOI;
                    ct.MASOIHOANTAT = model.MASOIHOANTAT;
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
        #region Xóa phiếu bảng kê sợi khi chưa có chi tiết 
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult Xoa_DonHangKhiDongKhongChiTiet_Click(string SOPHIEU)
        {
            try
            {
                var dataCheckPhieu = ks_.GC_DMPHIEU_BANGKENHAPSOI.Where(a => a.SOPHIEU == SOPHIEU).FirstOrDefault();
                if (dataCheckPhieu != null)
                {
                    var dataCheckDHCT = ks_.GC_CTPHIEUXUATGC.Where(a => a.SOPHIEU == dataCheckPhieu.SOPHIEU).ToList();
                    if (dataCheckDHCT.Count() > 0)
                    {
                        return Json(new { status = 1, title = "", text = "Đóng không xoá phiếu bảng kê sợi", obj = "" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        ks_.GC_DMPHIEU_BANGKENHAPSOI.Remove(dataCheckPhieu);
                        return Json(new { status = -1, title = "", text = "Đóng xoá phiếu bảng kê sợi khi chưa có chi tiết", obj = "" }, JsonRequestBehavior.AllowGet);
                    }
                }
                {
                    return Json(new { status = -1, title = "", text = "Phiếu bảng kê sợi đã được xoá", obj = "" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
        #region Action delete phiếu chuyển bảng kê sợi
        [HttpPost]
        public JsonResult delete_Click(string SOPHIEU)
        {
            try
            {
                if (SOPHIEU == String.Empty)
                {
                    return Json(new { status = -1, title = "", text = "Không tìm thấy mã số phiếu.", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var dataCheckDH = ks_.GC_DMPHIEU_BANGKENHAPSOI.Where(a => a.SOPHIEU == SOPHIEU).FirstOrDefault();
                    if (dataCheckDH.MANV != User.UserName)
                    {
                        return Json(new { status = -1, title = "", text = "Chỉ nhân viên tạo phiếu mới có quyền xóa.", obj = "" }, JsonRequestBehavior.AllowGet);
                    }
                    if (dataCheckDH.HIEULUC_XX == true)
                    {
                        return Json(new { status = -1, title = "", text = "Phiếu bảng kê sợi đã xem xét từ đơn vị không thể xóa.", obj = "" }, JsonRequestBehavior.AllowGet);
                    }
                    
                    if (dataCheckDH == null)
                    {
                        return Json(new { status = -1, title = "", text = "Không tìm thấy phiếu bảng kê sợi.", obj = "" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var dataCheckCTDH = ks_.GC_CTPHIEUXUATGC.Where(a => a.SOPHIEU == SOPHIEU).ToList();
                        var ghilog = ks_.SP_GC_CTPHIEUXUATGIACONG_BK_DELETE(SOPHIEU);
                        foreach (var a in dataCheckCTDH)
                        {
                            ks_.GC_CTPHIEUXUATGC.Remove(a);
                            ks_.SaveChanges();
                        }
                        ks_.GC_DMPHIEU_BANGKENHAPSOI.Remove(dataCheckDH);
                        ks_.SaveChanges();
                        return Json(new { status = 1, title = "", text = "Xoá phiếu bảng kê sợi thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
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
        public JsonResult TaoPhieuMoi_Click(string SOPHIEU)
        {
            try
            {
                var data = ks_.GC_CTPHIEUXUATGC.Where(a => a.SOPHIEU == SOPHIEU).ToList();
                if (data.Count() > 0)// kiểm tra chi tiết đơn hàng đã có dữ liệu chưa
                {
                    return Json(new { status = -1, title = "", text = "Đã có chi tiết phiếu không thể tạo mới được", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                else// tiến hành xoá đơn hàng khi chưa có dữ liệu chi tiết đơn hàng
                {
                    var dataDelete = ks_.GC_DMPHIEU_BANGKENHAPSOI.Where(a => a.SOPHIEU == SOPHIEU).FirstOrDefault();
                    ks_.GC_DMPHIEU_BANGKENHAPSOI.Remove(dataDelete);
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
        #region Acction Xoá phiếu bảng kê sợi chi tiết
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult Xoa_ChiTietLapPhieu_Click(string IDKHOA)
        {
            try
            {
                var data = ks_.SP_GC_DELETE_CTPHIEUXUATGIACONG_BYKHOA(IDKHOA);
                if (data > 0)
                {
                    return Json(new { status = 1, title = "", text = "Xoá chi tiết phiếu bảng kê sợi thành công", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = -1, title = "", text = "Xoá chi tiết phiếu bảng kê sợi thất bại", obj = "" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion
        #region Action update chi tiết bảng kê sợi
        [HttpPost]
        public ActionResult update_Detail_Click(GC_DMPHIEU_BANGKENHAPSOIModels model)
        {
            try
            {
                var list = new List<GC_DMPHIEU_BANGKENHAPSOI>();
                var list_temp = new List<GC_CTPHIEUXUATGC>();
                var chitiets = new List<GC_CTPHIEUXUATGC>();
                var dataCheckDH = ks_.GC_DMPHIEU_BANGKENHAPSOI.Where(p => p.SOPHIEU == model.SOPHIEU).FirstOrDefault();
                if (dataCheckDH == null)
                {
                    return Json(new { status = -1, title = "", text = "Không thể lưu ở đây.", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                if (dataCheckDH.HIEULUC_XX == true)
                {
                    return Json(new { status = -1, title = "", text = "Phiếu bảng kê sợi đã xem xét từ đơn vị không thể sửa.", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                if (dataCheckDH.HIEULUC_KT == true)
                {
                    return Json(new { status = -1, title = "", text = "Phiếu bảng kê sợi đã kiểm tra xong không thể sửa.", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                if (model.Detail.Count <= 0)
                {
                    return Json(new { status = -2, title = "", text = "Chưa có chi tiết không thể sửa được. Vui lòng tạo lại phiếu bảng kê sợi", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                #region Chi tiết
                if (model.Detail.Any())
                {
                    foreach (var items in model.Detail)
                    {
                        if (items.Delete == false)
                        {
                            var phieuBangKe = ks_.GC_DMPHIEU_BANGKENHAPSOI.Where(p => p.SOPHIEU == model.SOPHIEU).FirstOrDefault();
                            var ct = ks_.GC_CTPHIEUXUATGC.FirstOrDefault(p => p.Khoa_CTphieuGC == items.Khoa_CTphieuGC);
                            if (ct == null)
                            {
                                #region kiểm tra trùng trong chi tiết
                                if (ks_.GC_CTPHIEUXUATGC.FirstOrDefault(p => p.Khoa_CTphieuGC == items.Khoa_CTphieuGC) != null)
                                {
                                    return Json(new { status = -2, title = "", text = "Thêm không thành công. Đã có phiếu thông tin này ", obj = "" }, JsonRequestBehavior.AllowGet);
                                }
                                #endregion
                            }
                            else
                            {
                                ct.KHOACTHUC = items.KHOACTHUC;
                                ct.MASOI = items.MASOI;
                                ct.HIEU = items.HIEU;
                                ct.LO = items.LO;
                                ct.LO = items.LO;
                                ct.SOLUONGNhap = items.SOLUONGNhap;
                                ct.SOLUONGXuat = items.SOLUONGXuat;
                                ct.SLHaoHutDM = items.SLHaoHutDM;
                                ct.DVT = items.DVT;
                                ct.SOTRUC = Convert.ToInt32(items.SOTRUC);
                                ct.SOCANH = items.SOCANH;
                                ct.KHOTRUC = items.KHOTRUC;
                                ct.CHIEUDAI = items.CHIEUDAI;
                                ct.GHICHU = items.GHICHU;
                                ct.SOPHIEU = items.SOPHIEU;
                                ct.MACONGDOAN = items.MACONGDOAN;
                                ct.DANGCONGDOAN = items.DANGCONGDOAN;
                                ct.MADH_GCSOI = items.MADH_GCSOI;
                                ct.MASOIHOANTAT = items.MASOIHOANTAT;
                                ks_.Entry(ct).State = EntityState.Modified;
                            }
                            phieuBangKe.NHAPKHO = model.NHAPKHO;
                            phieuBangKe.MAKH = model.MAKH;
                            phieuBangKe.MALOAICONGDOAN = model.MALOAICONGDOAN;
                            ks_.Entry(phieuBangKe).State = EntityState.Modified;
                            list.Add(phieuBangKe);
                            list_temp.Add(ct);
                        }
                        #region Xóa chi tiết
                        if (items.Delete == true && items.Khoa_CTphieuGC != null)
                        {
                            var DetailCT = ks_.GC_CTPHIEUXUATGC.FirstOrDefault(p => p.Khoa_CTphieuGC == items.Khoa_CTphieuGC);
                            ks_.GC_CTPHIEUXUATGC.Remove(DetailCT);
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


        #region Print bảng kê

        public ActionResult _print_Bangke(string SOPHIEU)
        {
            try
            {
                if (SOPHIEU == "" || SOPHIEU == null)
                {
                    return Json(new { status = -1, title = "", text = "Không tìm thấy số phiếu, vui lòng kiểm tra lại", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
               
                    var bangke = ks_.VATTU2024_SP_GC_DMPHIEU_BANGKENHAPSOI_BY_SOPHIEU_REPORT(SOPHIEU).Where(p => p.SOPHIEU == SOPHIEU).ToList();
                    var ctbangke = ks_.VATTU2024_SP_GC_DMPHIEU_CHITIETPHIEUXUAT_SUB_REPORT(SOPHIEU).Where(p => p.SOPHIEU == SOPHIEU ).ToList();
                    if (bangke.Count() == 0)
                    {
                        return Json(new { status = -1, title = "", text = "Không tìm thấy dữ liệu", obj = "" }, JsonRequestBehavior.AllowGet);
                    }
                    ViewBag.bangke = bangke;
                    ViewBag.ctbangke = ctbangke;
                }

                return PartialView();
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Gửi mail yêu cầu xem xét
        [HttpPost]
        public string openOutlookemailbox(string SOPHIEU)
        {

            #region Lấy địa chỉ mail của người xx
            var item = ks_.GC_DMPHIEU_BANGKENHAPSOI.FirstOrDefault(p => p.SOPHIEU == SOPHIEU);
            var manvkt = vt_.DMNHANVIENs.FirstOrDefault(p => p.MANV == item.MANV_XX);
            #endregion

            MailMessage mail = new MailMessage();
            mail.To.Add(new MailAddress(manvkt.EMail));
            mail.IsBodyHtml = true;
            mail.Subject = item.SOPHIEU;
            mail.Body = "Xem xét bảng kê nhập sợi TP gia công (" + item.SOPHIEU + ") trong chương trình Quản lý kho sợi %0D%0A";
            var outlookmail = "mailto:" + mail.To.ToString()
                + "?subject=" + "XEM XÉT SỐ PHIẾU BẢNG KÊ SỢI THÀNH PHẨM GIA CÔNG-" + mail.Subject.ToString()
                + "&body=" + "Kính gửi: BGĐ %0D%0A" + "" + mail.Body.ToString() + "" + "Trân trọng!";
            return outlookmail;
        }
        #endregion
    }
}