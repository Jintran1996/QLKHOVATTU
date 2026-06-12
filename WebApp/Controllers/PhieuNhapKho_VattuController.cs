using NSClient;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using ToolsApp.Authentication;
using ToolsApp.com.netsuite.webservices;
using ToolsApp.EntityFramework.NetsuiteTT;
using ToolsApp.EntityFramework.VatTu;
using ToolsApp.EntityFramework.KhoSoi;
using ToolsApp.Models;
using System.Globalization;
using System.Collections.Generic;
using ToolsApp.Utilities;
using ToolsApp.Helper;
using System.Net.Mail;
using SpreadsheetLight;
using System.IO;
using System.Data.Entity.Validation;

namespace ToolsApp.Controllers
{
    [Authorize]
    public class PhieuNhapKho_VattuController : BaseController
    {

        wqlvattuEntities vt_ = new wqlvattuEntities();
        wqlkhosoiEntities soi_ = new wqlkhosoiEntities();
        NetsuiteTTEntities1 nstt_ = new NetsuiteTTEntities1();

        #region Index
        // GET: PhieuNhapKho
        public ActionResult Index()
        {
            return View();
        }
        #endregion

        #region Load View 
        public ActionResult _Getlist(string SOCTXNSearch = null)
        {
            
      
                var username = User.UserName;
            if (User.UserName == "8346")
            {
                username = "7672";
            }
            var List = vt_.VATTU2024_SP_GET_DM_XUATNHAP(username, "N").Where(p =>
            (
                (SOCTXNSearch == null || SOCTXNSearch == "" || p.SOCTXN.Contains(SOCTXNSearch)) &&
                (p.MANVCN == User.UserName || p.MANVCN == username)

            )).ToList();

            ViewBag.List = List;

            return PartialView();
        }
        #endregion

        #region Search vật tư
        public ActionResult SearchVatTu(string search, int page = 1, string loaiXN = "")
        {
            const int pageSize = 20;

            var all = vt_.VATTU2024_SPLOAD_DANHMUCVATTU(loaiXN)
                            .Select(x => new {
                                MAVT = x.MAVT,
                                TenVT = x.TenVT
                            })
                         .ToList();

            var data = all
             .Where(x => x.MAVT.Contains(search) || x.TenVT.Contains(search))
             .Skip((page - 1) * pageSize)
             .Take(pageSize)
             .Select(x => new {
                 id = x.MAVT,
                 text = x.MAVT + " || " + x.TenVT
             })
             .ToList();

            bool more = data.Count == pageSize;

            return Json(new
            {
                items = data,
                more = more
            }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region GET_MAPHIEUPD_BY_RADCOMBOBOXMAVATTU
        public static string GET_MAPHIEUPD_BY_RADCOMBOBOXMAVATTU(string TEXT)
        {
            string kq = String.Empty;
            try
            {
                if (TEXT != String.Empty && TEXT != null)
                {
                    string valuesplit = "||";

                    string[] temp = TEXT.Split(new[] { valuesplit }, StringSplitOptions.None);

                    kq = temp[2].ToString().Trim();
                }

                return kq;
            }
            catch (Exception)
            {
                return String.Empty;
            }
        }
        #endregion

        #region Load View _GetListCTPhieu + LOAIXN
        public ActionResult _GetListCTPhieu(string KHOAKEYXN, string SOCTXN, string LOAIXN)
        {
            // ===== 1. Phiếu =====
            var list_Phieu = vt_.DM_XUATNHAP.FirstOrDefault(p => p.SOCTXN == SOCTXN);
            if (list_Phieu == null)
                return HttpNotFound();

            // ===== 2. Danh sách chi tiết (QUERY 1 LẦN) =====
            var listXN = vt_.XUATNHAPs
                .Where(p => p.SoCTXN == SOCTXN && p.Ngay.HasValue && p.Ngay.Value.Year > 2022)
                .ToList();

            ViewBag.List = listXN;
            ViewBag.list_Phieu = list_Phieu;
            ViewBag.LOAIXN = list_Phieu.LOAIXN;
            ViewBag.MAKHONHAN = list_Phieu.MAKHONHAN;
            ViewBag.MAKHOXUAT = list_Phieu.MAKHOXUAT;
            ViewBag.SOCTXN_nhap = SOCTXN;

            // ===== 3. Dictionary MAVT → TenVT (CHỈ LẤY MAVT ĐANG DÙNG) =====
            var listMAVT = listXN
                .Where(x => !string.IsNullOrEmpty(x.MAVT))
                .Select(x => x.MAVT)
                .Distinct()
                .ToList();

            var dmvtDict = vt_.DANHMUCVATTUs
                .Where(x => listMAVT.Contains(x.MAVT))
                .Select(x => new { x.MAVT, x.TenVT })
                .ToDictionary(x => x.MAVT, x => x.TenVT);

            ViewBag.DMVT_DICT = dmvtDict;

            // ===== 4. Danh mục vật tư theo loại XN =====
            ViewBag.DANHMUCMAVT = vt_.VATTU2024_SPLOAD_DANHMUCVATTU(LOAIXN).ToList();

            // ===== 5. Người xem xét =====
            ViewBag.nguoixx = vt_.SP_LoadXemXetBM03TTKD(User.UserName).ToList();

            // ===== 6. Kế thừa theo LOAIXN =====
            if (LOAIXN == "NNB")
            {
                ViewBag.kethua_soctxn = vt_.VATTU2024_SPLOAD_XVCNB_KETHUA_SOCTXN().Where(X => X.MAKHOXUAT == list_Phieu.MAKHOXUAT).ToList();
            }
      
            if (LOAIXN == "NZZZ")
            {
                ViewBag.kethua_xzzz = vt_.VATTU2024_SPLOAD_SOCT_XZZZ(list_Phieu.IDMADVXUAT).ToList();
            }

            // ===== 7. Map LOAIXN → fLOAIXN (GỌN & RÕ) =====
            var mapLoaiXN = new Dictionary<string, string>
                {
                    { "NNB", "NNB" },
                    { "NZZZ", "NZZZ" },
                    { "NDC", "NDC" },
                    { "NKH", "NHTA" },
                    { "NHTA", "NHTA" },
                    { "NHGA", "NHGA" },
                    { "HKNVL_GCN", "NDC" },
                    { "N_GCN", "NDC" },
                    { "HK", "HK" },
                    { "HKGCGB", "HKGCGB" } 
                };

            var fLOAIXN = mapLoaiXN.ContainsKey(LOAIXN) ? mapLoaiXN[LOAIXN] : "NK";

            if (LOAIXN == "HKGCGB")
            {
                ViewBag.kethua_BANGKE = soi_
                    .VATTU2025_SPLOAD_KHOATHAMCHIEU_BANGKE("")
                    .ToList();
            }

            // ===== 8. Return partial =====
            return PartialView("_GetListCTPhieu_" + fLOAIXN,
                new XUATNHAPViewModels { SoCTXN = SOCTXN });
        }
        #endregion

        #region Load View _GetListCTPhieu_View 
        public ActionResult _GetListCTPhieu_View(string SOCTXN)
        {

            var dmvtDict = vt_.DANHMUCVATTUs.ToDictionary(x => x.MAVT, x => x.TenVT);
            ViewBag.DMVT_DICT = dmvtDict;
            var list_Phieu = vt_.DM_XUATNHAP.FirstOrDefault(p => p.SOCTXN == SOCTXN);
            var List = vt_.XUATNHAPs.Where(p => p.SoCTXN == SOCTXN).ToList();
            ViewBag.List = List;
            ViewBag.list_Phieu = list_Phieu;
            ViewBag.SOCTXN = SOCTXN;
            return PartialView("_GetListCTPhieu_NK", new XUATNHAPViewModels { SoCTXN = SOCTXN });
        }
        #endregion

        #region Load View reloadData_getlist_ctphieu_xzzz 
        public ActionResult _Getlist_ctphieu_xzzz(string SOCTXN_xzzz)
        {
            var List = vt_.TBL_PHIENMAVT_BYMAHANG.Where(p => p.SOCTXUAT_VAI == SOCTXN_xzzz).ToList();
            ViewBag.List = List;
            return PartialView("_Getlist_ctphieu_xzzz", new XUATNHAPViewModels { });
        }
        #endregion

        #region Load View _DienGiaiView
        public ActionResult _DienGiaiView(string SOCTXN, string LOAIXN, string MAVT)
        {
            var List = vt_.XUATNHAPs.Where(p => p.SoCTXN == SOCTXN && p.MAVT == MAVT && p.LoaiXN == LOAIXN).ToList();
            ViewBag.List = List;
            var diengiai = vt_.NHAPDIENGIAIs.Where(p => p.SOCTXN == SOCTXN && p.MAVT == MAVT).ToList();
            ViewBag.diengiai = diengiai;
            var dvt = vt_.tblDMDVTINHs.ToList();
            ViewBag.dvt = dvt;
            return PartialView("_DienGiaiView", new XUATNHAPViewModels { });
        }
        #endregion

        #region Load View Update _UpdateCTPhieu_NK  _GetList_DienGiai
        public ActionResult _GetList_DienGiai(string SOCTXN_DG, string MAVT_DG)
        {
            var List = vt_.NHAPDIENGIAIs.Where(p => p.MAVT == MAVT_DG && p.SOCTXN == SOCTXN_DG).ToList();
            ViewBag.List = List;

            return PartialView("_GetList_DienGiai");
        }
        #endregion

        #region Load View Thêm mới _Insert
        public ActionResult _Insert()
        {
            var kyhieu = vt_.sp_Load_NLKiHieuDV(User.UserName).FirstOrDefault();

            ViewBag.makhoxuat = "";
            var makhonhan = vt_.VATTU2024_SP_LOADKHOQUYENNHAP(User.UserName, "1").ToList();
            ViewBag.makhonhan = makhonhan;
            var loaihinhthuc = vt_.VATTU2024_SPLOAD_DMHINHTHUC_NHAPKHO(User.UserName).ToList();
            var f_hinhthuc = loaihinhthuc.Count < 1 ? "" : loaihinhthuc.FirstOrDefault().LOAIXN;
            ViewBag.loaihinhthuc = loaihinhthuc;
            var SOCTXN = vt_.VATTU2024_SP_TAOSCT_NHAPKHO(kyhieu.ToUpper(), f_hinhthuc).ToList();
            var f_SOCTXN = SOCTXN.Count < 1 ? "" : SOCTXN.FirstOrDefault().SOCTXN;
            ViewBag.SOCTXN = f_SOCTXN.ToUpper();

            var NL_DMNHACCAP = soi_.NL_DMNHACCAP.ToList();
            ViewBag.NL_DMNHACCAP = NL_DMNHACCAP;
            var dmdonvi = vt_.VATTU2024_SP_LOAD_QUYEN_SUDUNG_DMDONVI(User.UserName).ToList();
            var f_iddonvi = dmdonvi.FirstOrDefault().IDMaDV;
            ViewBag.DMDONVI = dmdonvi;
            ViewBag.manvnhan = vt_.DMNHANVIENs.Where(p => p.IDMaDV == f_iddonvi && p.HieuLuc == 1).ToList();

            return PartialView("_Insert", new DM_XUATNHAPViewModels { /*Sophieu = Sophieu.FirstOrDefault().Sophieu*/ });
        }
        #endregion

        #region OnChange _Change_SOCTXN
        public JsonResult _Change_SOCTXN(string LOAIXNid, string IDMADVNHAN)
        {
            try
            {
                string SOCTXN = "";
                if (LOAIXNid == "N")
                {
                    var kyhieu = vt_.DMDONVIs.FirstOrDefault(p => p.IDMaDV == IDMADVNHAN).KiHieu.Trim().ToUpper();
                    SOCTXN = vt_.VATTU2024_SP_TAOSCT_NHAPKHO(kyhieu.ToUpper(), LOAIXNid).FirstOrDefault().SOCTXN;
                }
                else
                {
                    var kyhieu = vt_.sp_Load_NLKiHieuDV(User.UserName).FirstOrDefault();
                    SOCTXN = vt_.VATTU2024_SP_TAOSCT_NHAPKHO(kyhieu.ToUpper(), LOAIXNid).FirstOrDefault().SOCTXN;
                }

                return Json(new { status = 1, title = "", text = "", obj = SOCTXN.ToUpper() }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { status = -1, title = "", text = e.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult _Onchange_Location_MANV(string IDMADVNHAN, string LOAIXN)
        {
            try
            {
                var manvnhan = vt_.DMNHANVIENs.Where(p => p.IDMaDV == IDMADVNHAN && p.HieuLuc == 1).ToList();

                return Json(new { status = 1, title = "", text = "", obj = manvnhan }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { status = -1, title = "", text = e.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult _Change_NCC_KHOXUAT(string LOAIXNid)
        {
            dynamic KHOXUAT = "";
            var kyhieu = vt_.sp_Load_NLKiHieuDV(User.UserName).FirstOrDefault();
            if (LOAIXNid == "NK" || LOAIXNid == "NKCHO")
            {
                KHOXUAT = soi_.NL_DMNHACCAP.ToList();
            }
            else
            {
                KHOXUAT = soi_.NL_DMNHACCAP.ToList();
            }
            return Json(new { status = 1, title = "", text = "", obj = KHOXUAT }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Load View Update _UpdateCTPhieu_NK 
        public ActionResult _UpdateCTPhieu_NK(string KHOAKEYXN, string SOCTXN)
        {
            var ctphieu = vt_.XUATNHAPs.FirstOrDefault(p => p.KHOAKEYXN == KHOAKEYXN && p.SoCTXN == SOCTXN);
            //var makho = dbks_.NL_DMPHIEUXN.FirstOrDefault(p => p.SOCTXN == model.SOCTXN).MAKHO;
            var dmphieu = vt_.DM_XUATNHAP.Where(p => p.SOCTXN == SOCTXN).FirstOrDefault();
            var dvt = vt_.tblDMDVTINHs.ToList();
            ViewBag.dvt = dvt;

            ViewBag.ctphieu = ctphieu;
            ViewBag.dmphieu = dmphieu;
            return PartialView("_UpdateCTPhieu_NK", new XUATNHAPViewModels { KHOAKEYXN = KHOAKEYXN });
        }
        #endregion        

        #region Update CT phiếu NK //ok
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _ActionUpdateCTPhieu_NK(XUATNHAPViewModels model)
        {
            try
            {
                #region Xử lý ngày
                CultureInfo cul = CultureInfo.GetCultureInfo("en-GB");
                var NgayLapTuSearch_ = new DateTime();
                if (!string.IsNullOrEmpty(model.NGAYCT_))
                {
                    try
                    {
                        NgayLapTuSearch_ = DateTime.ParseExact(model.NGAYCT_, "dd/MM/yyyy", cul);

                        model.NgayKeToan = new DateTime(NgayLapTuSearch_.Year,
                            NgayLapTuSearch_.Month, NgayLapTuSearch_.Day, 0, 0, 0);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                }
                #endregion
                var dmxuatnhap = vt_.DM_XUATNHAP.FirstOrDefault(p => p.SOCTXN == model.SoCTXN);
                var f_LOAIXN = dmxuatnhap.LOAIXN;
                var f_SOCTXN = model.SoCTXN;
                var item = vt_.XUATNHAPs.FirstOrDefault(p => p.KHOAKEYXN == model.KHOAKEYXN && p.SoCTXN == model.SoCTXN);
                if (item == null)
                {
                    return Json(new { status = -1, title = "", text = "Không tìm thấy chi tiết phiếu.", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if (f_LOAIXN != "NKH" && dmxuatnhap.GUIAPI != "1")
                    {
                        var SLCOTHENK = vt_.VATTU2025_Sum_SLDaNhapKho_PO(dmxuatnhap.SOPO, item.MaPhieuPD, item.MAVT).FirstOrDefault();
                        if ((SLCOTHENK.SLCOTHENK_PO) < model.SoLuongTT)
                        {
                            return Json(new { status = -1, title = "", text = "Item đã vượt giới hạn phiếu PR SLPD: " + SLCOTHENK.SLPD_PR + ", SL đã nhập từ PO: " + SLCOTHENK.SLNK_PO + ", SL Dung sai: " + SLCOTHENK.Note + ", SL tối đa có thể nhập kho là: " + (SLCOTHENK.SLCOTHENK_PO), obj = "" }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    item.SoCTKeToan = model.SoCTKeToan;
                    item.SoLuongTT = Convert.ToDecimal(model.SoLuongTT);
                    item.SoLuongYC = Convert.ToDecimal(model.SoLuongTT);
                    item.NgayKeToan = dmxuatnhap.NGAY;
                    item.GHICHU = model.GHICHU;
                    item.TenVT_NCC = model.TenVT_NCC;
                    item.DVT_NCC = model.DVT_NCC;
                    item.SOLUONGTT_NCC = Convert.ToDecimal(model.SOLUONGTT_NCC);
                    item.MANVMODIFY = User.UserName;
                    item.NGAYMODIFY = DateTime.Now;
                    vt_.Entry(item).State = EntityState.Modified;
                    vt_.SaveChanges();
                    return Json(new { status = 1, title = "", text = "Cập nhật thành công.", obj = f_SOCTXN, obj2 = f_LOAIXN }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Insert MAVT HÀNG PHIÊN NZZZ
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _InsertFuN_NZZZ(string SOCTXN_xzzz)
        {
            try
            {
                var tb = vt_.VATTU2024_SPLOAD_CTPHIEU_SOCT_XZZZ(SOCTXN_xzzz).ToList();

                if (tb != null)
                {
                    var data = vt_.TBL_PHIENMAVT_BYMAHANG.Where(P => P.SOCTXUAT_VAI == SOCTXN_xzzz).FirstOrDefault();
                    if (data == null)
                    {
                        foreach (var item in tb)
                        {
                            #region Insert DM_XUATNHAP
                            var model_copy = new EntityFramework.VatTu.TBL_PHIENMAVT_BYMAHANG();
                            model_copy.SOCTXUAT_VAI = SOCTXN_xzzz.Trim();
                            model_copy.MAHANGVAI = item.MAMH;
                            model_copy.MACAY = item.MACAY;
                            model_copy.SOLUONG = item.SOLUONG;
                            model_copy.MAVT = item.MAMH;
                            model_copy.SOLUONGTT = item.SOLUONG;
                            model_copy.MANVCN = User.UserName;
                            model_copy.NGAYCN = DateTime.Now;
                            model_copy.ID_PHIEN = Guid.NewGuid();
                            vt_.TBL_PHIENMAVT_BYMAHANG.Add(model_copy);
                            vt_.SaveChanges();
                            #endregion
                        }
                    }

                    return Json(new { status = 1, title = "", text = "Thêm thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = -2, title = "", text = "Thêm không thành công. ", obj = "" }, JsonRequestBehavior.AllowGet);

                }
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }

        }
        #endregion

        #region Insert MAVT _InsertFunCTPhieu_HK

        public JsonResult _InsertFunCTPhieu_HK(string LOAIXN, string SOCTXN, string MAKHONHAN, string MAVT, string NGAYKETOAN, string SoLuongTT, string GHICHU)
        {
            try
            {
                #region Xử lý ngày
                CultureInfo cul = CultureInfo.GetCultureInfo("en-GB");
                var NgayLapTuSearch_ = new DateTime();
                var NGAYCT = new DateTime();
                if (!string.IsNullOrEmpty(NGAYKETOAN))
                {
                    try
                    {
                        NgayLapTuSearch_ = DateTime.ParseExact(NGAYKETOAN, "dd/MM/yyyy", cul);

                        NGAYCT = new DateTime(NgayLapTuSearch_.Year,
                            NgayLapTuSearch_.Month, NgayLapTuSearch_.Day, 0, 0, 0);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                }
                #endregion

                // var tb = vt_.XUATNHAPs.FirstOrDefault(P => P.SoCTXN == model.SoCTXN && P.MAVT == model.MAVT);
                var tb = vt_.XUATNHAPs.FirstOrDefault(P => P.SoCTXN == SOCTXN && P.MAVT == MAVT);
                if (tb == null)
                {
                    #region Insert DM_XUATNHAP
                    var model_copy = new EntityFramework.VatTu.XUATNHAP();
                    model_copy.SoCTXN = SOCTXN.Trim();
                    model_copy.MAVT = MAVT.Trim();
                    model_copy.LoaiXN = LOAIXN.Trim();
                    model_copy.MaPhieuPD = SOCTXN;
                    model_copy.SoLuongYC = Convert.ToDecimal(SoLuongTT);
                    model_copy.SoLuongTT = Convert.ToDecimal(SoLuongTT);
                    model_copy.TongGia = Convert.ToDecimal(0.00);
                    model_copy.MaKhoXuat = "";
                    model_copy.MaKho = MAKHONHAN.Trim();
                    model_copy.GHICHU = (GHICHU == null || GHICHU == "") ? "" : GHICHU;
                    model_copy.MaNVYC = User.UserName;
                    model_copy.MANVCN = User.UserName;
                    model_copy.Ngay = DateTime.Now;
                    model_copy.NGAYCN = DateTime.Now;
                    model_copy.NgayKeToan = NGAYCT;
                    var KeyKhoa = Guid.NewGuid();
                    model_copy.KHOAKEYXN = LOAIXN.Trim() + "_KeyKhoa_" + KeyKhoa;
                    vt_.XUATNHAPs.Add(model_copy);
                    vt_.SaveChanges();
                    #endregion

                    return Json(new { status = 1, title = "", text = "Thêm thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = -2, title = "", text = "Thêm không thành công. ", obj = "" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }

        }
        #endregion

        #region _InsertFunCTPhieu_NDC Action
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _InsertFunCTPhieu_NDC(string LOAIXN, string SOCTXN, string MAKHONHAN, string MAVT_NDC, string NGAYKETOAN_NDC, string SoLuongTT_NDC, string GHICHU_NDC, string Maphieupd_NDC = null)
        {
            try
            {
                #region Xử lý ngày
                CultureInfo cul = CultureInfo.GetCultureInfo("en-GB");
                var NgayLapTuSearch_ = new DateTime();
                var NGAYCT_NDC = new DateTime();
                if (!string.IsNullOrEmpty(NGAYKETOAN_NDC))
                {
                    try
                    {
                        NgayLapTuSearch_ = DateTime.ParseExact(NGAYKETOAN_NDC, "dd/MM/yyyy", cul);

                        NGAYCT_NDC = new DateTime(NgayLapTuSearch_.Year,
                            NgayLapTuSearch_.Month, NgayLapTuSearch_.Day, 0, 0, 0);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                }
                #endregion

                var dmphieu = vt_.DM_XUATNHAP.FirstOrDefault(P => P.SOCTXN == SOCTXN);
                var tb = vt_.XUATNHAPs.FirstOrDefault(P => P.SoCTXN == SOCTXN && P.MAVT == MAVT_NDC);
                var kyhieu = vt_.VATTU2024_SP_LOAD_QUYEN_SUDUNG_DMDONVI(User.UserName).FirstOrDefault(p => p.IDMaDV == dmphieu.IDMADVNHAN)?.KiHieu.Trim().ToUpper() ?? "";
                if (tb == null)
                {
                    #region Insert DM_XUATNHAP
                    var model_copy = new XUATNHAP();
                    model_copy.SoCTXN = SOCTXN.Trim();
                    model_copy.MAVT = MAVT_NDC.Trim();
                    model_copy.LoaiXN = LOAIXN.Trim();
                    model_copy.MaPhieuPD = (Maphieupd_NDC == null || Maphieupd_NDC == "") ? (kyhieu + "_" + SOCTXN.ToUpper().Trim()) : Maphieupd_NDC.Trim().ToUpper();
                    model_copy.SoLuongYC = Convert.ToDecimal(SoLuongTT_NDC);
                    model_copy.SoLuongTT = Convert.ToDecimal(SoLuongTT_NDC);
                    model_copy.TongGia = Convert.ToDecimal(0.00);
                    model_copy.MaKhoXuat = "";
                    model_copy.MaKho = MAKHONHAN.Trim();
                    model_copy.GHICHU = (GHICHU_NDC == null || GHICHU_NDC == "") ? "" : GHICHU_NDC;
                    model_copy.MaNVYC = User.UserName;
                    model_copy.MANVCN = User.UserName;
                    model_copy.Ngay = DateTime.Now;
                    model_copy.NGAYCN = DateTime.Now;
                    model_copy.NgayKeToan = NGAYCT_NDC;
                    var KeyKhoa = Guid.NewGuid();
                    model_copy.KHOAKEYXN = LOAIXN.Trim() + "_KeyKhoa_" + KeyKhoa;
                    vt_.XUATNHAPs.Add(model_copy);
                    vt_.SaveChanges();
                    #endregion

                    return Json(new { status = 1, title = "", text = "Thêm thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = -2, title = "", text = "Thêm không thành công. ", obj = "" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }

        }
        #endregion

        #region _InsertFunCTPhieu_NHTA Action
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _InsertFunCTPhieu_NHTA(string LOAIXN, string SOCTXN, string MAKHONHAN, string MAVT_NHTA , string SoLuongTT_NHTA, string GHICHU_NHTA, string Maphieupd_NHTA = null)
        {
            try
            {         

                var dmphieu = vt_.DM_XUATNHAP.FirstOrDefault(P => P.SOCTXN == SOCTXN);

                if (dmphieu.GUIAPI == "1" )
                {
                    return Json((new { status = -1, title = "", text = string.Format("Phiếu này đã được xem xét không thể thêm mới, vui lòng kiểm tra lại hoặc tạo phiếu khác"), obj = "" }), JsonRequestBehavior.AllowGet);
                }
                var tb = vt_.XUATNHAPs.FirstOrDefault(P => P.SoCTXN == SOCTXN && P.MAVT == MAVT_NHTA);
                var kyhieu = vt_.VATTU2024_SP_LOAD_QUYEN_SUDUNG_DMDONVI(User.UserName).FirstOrDefault(p => p.IDMaDV == dmphieu.IDMADVNHAN)?.KiHieu.Trim().ToUpper() ?? "";
                if (tb == null)
                {
                    #region Insert DM_XUATNHAP
                    var model_copy = new XUATNHAP();
                    model_copy.SoCTXN = SOCTXN.Trim();
                    model_copy.MAVT = MAVT_NHTA.Trim();
                    model_copy.LoaiXN = LOAIXN.Trim();
                    model_copy.MaPhieuPD = (Maphieupd_NHTA == null || Maphieupd_NHTA == "") ? (kyhieu + "_" + SOCTXN.ToUpper().Trim()) : Maphieupd_NHTA.Trim().ToUpper();
                    model_copy.SoLuongYC = Convert.ToDecimal(SoLuongTT_NHTA);
                    model_copy.SoLuongTT = Convert.ToDecimal(SoLuongTT_NHTA);
                    model_copy.TongGia = Convert.ToDecimal(0.00);
                    model_copy.MaKhoXuat = "";
                    model_copy.MaKho = MAKHONHAN.Trim();
                    model_copy.GHICHU = (GHICHU_NHTA == null || GHICHU_NHTA == "") ? "" : GHICHU_NHTA;
                    model_copy.MaNVYC = User.UserName;
                    model_copy.MANVCN = User.UserName;
                    model_copy.Ngay = DateTime.Now;
                    model_copy.NGAYCN = DateTime.Now;
                    model_copy.NgayKeToan = dmphieu.NGAY;
                    var KeyKhoa = Guid.NewGuid();
                    model_copy.KHOAKEYXN = LOAIXN.Trim() + "_KeyKhoa_" + KeyKhoa;
                    vt_.XUATNHAPs.Add(model_copy);
                    vt_.SaveChanges();
                    #endregion

                    return Json(new { status = 1, title = "", text = "Thêm thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = -2, title = "", text = "Thêm không thành công. ", obj = "" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }

        }
        #endregion

        #region _InsertFunCTPhieu_NHGA Action
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _InsertFunCTPhieu_NHGA(string LOAIXN, string SOCTXN, string MAKHONHAN, string MAVT_NHGA, string SoLuongTT_NHGA, string GHICHU_NHGA, string Maphieupd_NHGA = null)
        {
            try
            {
                

                var dmphieu = vt_.DM_XUATNHAP.FirstOrDefault(P => P.SOCTXN == SOCTXN);

                if (dmphieu.GUIAPI == "1")
                {
                    return Json((new { status = -1, title = "", text = string.Format("Phiếu này đã được xem xét không thể thêm mới, vui lòng kiểm tra lại hoặc tạo phiếu khác"), obj = "" }), JsonRequestBehavior.AllowGet);
                }
                var tb = vt_.XUATNHAPs.FirstOrDefault(P => P.SoCTXN == SOCTXN && P.MAVT == MAVT_NHGA);
                var kyhieu = vt_.VATTU2024_SP_LOAD_QUYEN_SUDUNG_DMDONVI(User.UserName).FirstOrDefault(p => p.IDMaDV == dmphieu.IDMADVNHAN)?.KiHieu.Trim().ToUpper() ?? "";
                if (tb == null)
                {
                    #region Insert DM_XUATNHAP
                    var model_copy = new XUATNHAP();
                    model_copy.SoCTXN = SOCTXN.Trim();
                    model_copy.MAVT = MAVT_NHGA.Trim();
                    model_copy.LoaiXN = LOAIXN.Trim();
                    model_copy.MaPhieuPD = (Maphieupd_NHGA == null || Maphieupd_NHGA == "") ? (kyhieu + "_" + SOCTXN.ToUpper().Trim()) : Maphieupd_NHGA.Trim().ToUpper();
                    model_copy.SoLuongYC = Convert.ToDecimal(SoLuongTT_NHGA);
                    model_copy.SoLuongTT = Convert.ToDecimal(SoLuongTT_NHGA);
                    model_copy.TongGia = Convert.ToDecimal(0.00);
                    model_copy.MaKhoXuat = "";
                    model_copy.MaKho = MAKHONHAN.Trim();
                    model_copy.GHICHU = (GHICHU_NHGA == null || GHICHU_NHGA == "") ? "" : GHICHU_NHGA;
                    model_copy.MaNVYC = User.UserName;
                    model_copy.MANVCN = User.UserName;
                    model_copy.Ngay = DateTime.Now;
                    model_copy.NGAYCN = DateTime.Now;
                    model_copy.NgayKeToan = dmphieu.NGAY;
                    var KeyKhoa = Guid.NewGuid();
                    model_copy.KHOAKEYXN = LOAIXN.Trim() + "_KeyKhoa_" + KeyKhoa;
                    vt_.XUATNHAPs.Add(model_copy);
                    vt_.SaveChanges();
                    #endregion

                    return Json(new { status = 1, title = "", text = "Thêm thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = -2, title = "", text = "Thêm không thành công. ", obj = "" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }

        }
        #endregion

        #region _InsertDienGiaiFun Diễn giải
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _InsertDienGiaiFun(NHAPDIENGIAIViewModels models)
        {
            try
            {
                #region Xử lý ngày
                CultureInfo cul = CultureInfo.GetCultureInfo("en-GB");
                var Ngay_ = new DateTime();
                var NGAY_NCC = new DateTime();
                if (!string.IsNullOrEmpty(models.NGAYNCC_))
                {
                    try
                    {
                        Ngay_ = DateTime.ParseExact(models.NGAYNCC_, "dd/MM/yyyy", cul);

                        NGAY_NCC = new DateTime(Ngay_.Year,
                            Ngay_.Month, Ngay_.Day, 0, 0, 0);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                }
                #endregion

                if (models.SOCTKETOAN == null || models.SOCTKETOAN == "")
                {
                    return Json(new { status = -2, title = "", text = "Số hóa đơn (số chứng từ kế toán) không được trống, vui lòng nhập bổ sung. ", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                // var tb = vt_.XUATNHAPs.FirstOrDefault(P => P.SoCTXN == model.SoCTXN && P.MAVT == model.MAVT);
                var tb = vt_.NHAPDIENGIAIs.FirstOrDefault(P => P.SOCTXN == models.SOCTXN && P.MAVT == models.MAVT);

                if (tb == null)
                {

                    #region Insert DM_XUATNHAP
                    var model_copy = new NHAPDIENGIAI();
                    model_copy.SOCTXN = models.SOCTXN.Trim();
                    model_copy.MAVT = models.MAVT.Trim();
                    model_copy.SOCTKETOAN = models.SOCTKETOAN.Trim();
                    model_copy.TENVATTU_NCC = models.TENVATTU_NCC.Trim();
                    model_copy.SOLUONG_NCC = models.SOLUONG_NCC;
                    model_copy.DVT_NCC = models.DVT_NCC;
                    model_copy.MAPHIEUPD = models.MAPHIEUPD;
                    model_copy.NGAY_NCC = NGAY_NCC;
                    var KeyKhoa = Guid.NewGuid();
                    model_copy.KHOAKEYXN = models.KHOAKEYXN;
                    model_copy.KEYNHAP = KeyKhoa.ToString();
                    vt_.NHAPDIENGIAIs.Add(model_copy);
                    vt_.SaveChanges();
                    #endregion

                    return Json(new { status = 1, title = "", text = "Thêm thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = -2, title = "", text = "Thêm không thành công. ", obj = "" }, JsonRequestBehavior.AllowGet);

                }
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }

        }
        #endregion

        #region Insert //ok
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _InsertFun(DM_XUATNHAPViewModels model)
        {
            try
            {
                #region Xử lý ngày
                CultureInfo cul = CultureInfo.GetCultureInfo("en-GB");
                var NgayLapTuSearch_ = new DateTime();
                if (!string.IsNullOrEmpty(model.NGAYCT_))
                {
                    try
                    {
                        NgayLapTuSearch_ = DateTime.ParseExact(model.NGAYCT_, "dd/MM/yyyy", cul);

                        model.NGAYCT = new DateTime(NgayLapTuSearch_.Year,
                            NgayLapTuSearch_.Month, NgayLapTuSearch_.Day, 0, 0, 0);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                }
                #endregion

                #region Kiểm tra khóa sổ
                if (!model.NGAYCT.HasValue)
                {
                    return Json(new { success = false, message = "Ngày chứng từ không được để trống" });
                }
              //  KhoaSoHelper.CheckKhoaSo("", model.NGAYCT.Value, "VATTU");
                #endregion

                var tb = vt_.DM_XUATNHAP.FirstOrDefault(p => p.SOCTXN == model.SOCTXN);

                if (tb == null)
                {
                    #region Check tồn tại
                    if (vt_.DM_XUATNHAP.FirstOrDefault(p => p.SOCTXN == model.SOCTXN) != null)
                    {
                        return Json(new { status = -2, title = "", text = "Thêm không thành công. SOCTXN đã tồn tại. Kiểm tra lại.", obj = model.SOCTXN }, JsonRequestBehavior.AllowGet);
                    }
                    if (vt_.XUATNHAPs.FirstOrDefault(p => p.SoCTXN == model.SOCTXN) != null)
                    {
                        return Json(new { status = -2, title = "", text = "Thêm không thành công. SOCTXN đã tồn tại.LH ITC Kiểm tra lại.", obj = model.SOCTXN }, JsonRequestBehavior.AllowGet);
                    }
                    if (model.LOAIXN == "" || model.LOAIXN == null)
                    {
                        return Json(new { status = -2, title = "", text = "Thêm không thành công. LOAIXN không được trống. Kiểm tra lại.", obj = model.SOCTXN }, JsonRequestBehavior.AllowGet);
                    }
                    if (model.MAKHONHAN == "" || model.MAKHONHAN == null)
                    {
                        return Json(new { status = -2, title = "", text = "Thêm không thành công. MAKHONHAN không được trống. Kiểm tra lại.", obj = model.SOCTXN }, JsonRequestBehavior.AllowGet);
                    }
                    if (model.MAKHOXUAT == "" || model.MAKHOXUAT == null)
                    {
                        return Json(new { status = -2, title = "", text = "Thêm không thành công. MAKHOXUAT không được trống. Kiểm tra lại.", obj = model.SOCTXN }, JsonRequestBehavior.AllowGet);
                    }
                    if (model.SOPO == "" || model.SOPO == null && model.LOAIXN == "N")
                    {
                        return Json(new { status = -2, title = "", text = "Thêm không thành công. SOPO không được trống. Kiểm tra lại.", obj = model.SOCTXN }, JsonRequestBehavior.AllowGet);
                    }
                    #endregion

                    #region Insert DM_XUATNHAP
                    var ncc =((model.KH_NCC == null || model.KH_NCC == "") ? "" : model.KH_NCC);
                    var model_copy = new EntityFramework.VatTu.DM_XUATNHAP();
                    model_copy.SOCTXN = model.SOCTXN.Trim();
                    model_copy.LOAIXN = model.LOAIXN.Trim();
                    model_copy.SOPO = model.SOPO;                    
                    model_copy.NGAY = model.NGAYCT;
                    model_copy.NGUOINHAN = model.NGUOINHAN;
                    model_copy.IDMADVNHAN = model.IDMADVNHAN;
                    model_copy.IDMADVXUAT = model.IDMADVXUAT;
                    model_copy.MAKHOXUAT = model.MAKHOXUAT.Trim();
                    model_copy.MAKHONHAN = model.MAKHONHAN.Trim();
                    model_copy.GHICHU = (model.GHICHU == null || model.KH_NCC == "") ? "" : model.GHICHU + (model.LOAIXN.Trim() == "NKH" ? (" || NCC: " + ncc) : "");
                    model_copy.MaNVYC = User.UserName;
                    model_copy.MANVCN = User.UserName;
                    model_copy.NGAYCN = DateTime.Now;

                    vt_.DM_XUATNHAP.Add(model_copy);
                    vt_.SaveChanges();
                    #endregion

                    if (model.LOAIXN.Trim() == "N" || model.LOAIXN.Trim() == "NGC")
                    {
                        #region Netsuite                    
                        System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                        // In order to enable SOAPscope to work through SSL. Refer to FAQ for more details
                        ServicePointManager.ServerCertificateValidationCallback += delegate (object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
                        {
                            return true;
                        };
                        NSClient.NSClient ns = null;
                        try
                        {
                            ns = new NSClient.NSClient();
                            NSBase.Client = ns;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error while loading the application:" + ex.Message);
                            Console.WriteLine("Press Enter to quit ... ");
                            Console.ReadKey();
                        }

                        #endregion

                        #region Nhập kho mua ngoài

                        //int i = 0;
                        dynamic internalidcreatedfrom = "";
                        // var phieu = tb.FirstOrDefault();

                        #region Get PO
                        var tranId = model.SOPO;
                        TransactionSearchBasic search = new TransactionSearchBasic();
                        search.type = new SearchEnumMultiSelectField();
                        search.type.@operator = SearchEnumMultiSelectFieldOperator.anyOf;
                        search.type.operatorSpecified = true;
                        search.type.searchValue = new string[] { "purchaseOrder" }; // Lọc theo loại giao dịch là Purchase Order
                        search.tranId = new SearchStringField();
                        search.tranId.@operator = SearchStringFieldOperator.@is;
                        search.tranId.operatorSpecified = true;
                        search.tranId.searchValue = tranId; // Đặt giá trị tranId cần tìm


                        SearchPreferences searchPrefs = new SearchPreferences();
                        SearchResult result = ns.Service.search(search); // Thực hiện tìm kiếm
                        #endregion

                        if (result.status.isSuccess && result.recordList != null && result.recordList.Length > 0)
                        {
                            var po = (PurchaseOrder)result.recordList[0];
                            internalidcreatedfrom = po.internalId; // lấy internal PO

                            #region Update Internalid PO
                            var tb_update = vt_.DM_XUATNHAP.FirstOrDefault(p => p.SOCTXN == model.SOCTXN);
                            tb_update.InternalidPO = internalidcreatedfrom;
                            vt_.Entry(tb_update).State = EntityState.Modified;
                            vt_.SaveChanges();
                            #endregion

                            RecordRef purchaseOrderRef = new RecordRef();
                            purchaseOrderRef.internalId = internalidcreatedfrom;
                            purchaseOrderRef.type = RecordType.purchaseOrder;
                            purchaseOrderRef.typeSpecified = true;

                            // Lấy thông tin chi tiết của đơn đặt hàng mua hàng
                            ReadResponse purchaseOrderRecords = ns.Service.get(purchaseOrderRef);

                            if (!purchaseOrderRecords.status.isSuccess)
                            {
                                return Json(new { status = -1, text = "Không tìm thấy PO", obj = "" }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                PurchaseOrder record = (PurchaseOrder)purchaseOrderRecords.record;
                                string maphieuInternalID = record.internalId;
                                var Currency = record.currency.name;
                                PurchaseOrderItemList fields = (PurchaseOrderItemList)record.itemList;// list field trong 1 record

                                for (int k = 0; k < fields.item.Length; k++)
                                {
                                    PurchaseOrderItem itemfields = (PurchaseOrderItem)fields.item[k];
                                    var itemname = itemfields.item.name;
                                    var soluong = itemfields.quantity;
                                    var dvt = itemfields.units.name;
                                    var dondenghimuahang = "";
                                    var rate = itemfields.rate;
                                    var tonggia = itemfields.amount;

                                    CustomFieldRef[] madonhang = itemfields.customFieldList;

                                    for (int j = 0; j < madonhang.Length; j++)
                                    {
                                        CustomFieldRef madonhangfield = madonhang[j];
                                        if (madonhangfield.scriptId == "custcol_btm_tt_purchase_request")
                                        {
                                            if (madonhangfield.GetType() == typeof(SelectCustomFieldRef))
                                            {
                                                SelectCustomFieldRef valField = madonhangfield as SelectCustomFieldRef;
                                                dondenghimuahang = valField.value.name;
                                            }
                                        }
                                    }
                                    try
                                    {
                                        if (vt_.DANHMUCVATTUs.FirstOrDefault(p => p.MAVT == itemname) == null)
                                        {
                                            return Json(new { status = -1, title = "", text = "MAVT không tồn tại", obj = "" }, JsonRequestBehavior.AllowGet);
                                        }
                                        var SLCOTHENK = vt_.VATTU2025_Sum_SLDaNhapKho_PO(model.SOPO, dondenghimuahang, itemname).FirstOrDefault();
                                        double sluongtt = (Double)SLCOTHENK.SLCON_PO;
                                        if (soluong < Convert.ToDouble(SLCOTHENK.SLCON_PO))
                                        {
                                            sluongtt = soluong;
                                        }
                                        var model_item = new EntityFramework.VatTu.XUATNHAP();
                                        model_item.SoCTXN = model.SOCTXN.Trim();
                                        model_item.LoaiXN = "N";
                                        model_item.MaPhieuPD = dondenghimuahang;
                                        model_item.MAVT = itemname;
                                        model_item.MaVTTam = itemname;
                                        model_item.SoLuongYC = Convert.ToDecimal(sluongtt);
                                        model_item.SoLuongTT = Convert.ToDecimal(sluongtt);
                                        model_item.TongGia = Convert.ToDecimal(tonggia);
                                        model_item.RatePO = Convert.ToDecimal(rate);
                                        model_item.DVT_TIEN = Currency.ToString();
                                        model_item.SOLUONGTT_NCC = (Decimal)sluongtt;
                                        model_item.MaKhoXuat = model.MAKHOXUAT.Trim();
                                        model_item.MaKho = model.MAKHONHAN.Trim();
                                        model_item.GHICHU = (model.GHICHU == null || model.GHICHU == "") ? "" : model.GHICHU;
                                        model_item.MaNVYC = User.UserName;
                                        model_item.MANVCN = User.UserName;
                                        model_item.Ngay = DateTime.Now;
                                        model_item.NGAYCN = DateTime.Now;
                                        model_item.NgayKeToan = model.NGAYCT;
                                        var KeyKhoa = Guid.NewGuid();
                                        model_item.KHOAKEYXN = model.LOAIXN.Trim() + "_KeyKhoa_" + KeyKhoa;
                                        vt_.XUATNHAPs.Add(model_item);
                                        vt_.SaveChanges();

                                        var model_ctphieu = vt_.CTPHIEUx.Where(p => p.MAPHIEU == dondenghimuahang && p.MAVT == itemname).FirstOrDefault();
                                        model_ctphieu.Rate = Convert.ToDecimal(rate);
                                        model_ctphieu.Currency = Currency.ToString();
                                        model_ctphieu.SoPONetSuite = model.SOPO;
                                        vt_.Entry(model_ctphieu).State = EntityState.Modified;
                                        vt_.SaveChanges();
                                    }

                                    catch (Exception ex)
                                    {
                                        return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
                                    }

                                }

                            }
                        }
                        else
                        {

                            return Json(new { status = -1, text = "Không tìm thấy PO, Vui lòng kiểm tra số PO trên Netsuite ở mục Purchase Order", obj = "" }, JsonRequestBehavior.AllowGet);
                        }

                        #endregion                         
                    }
                   
                    return Json(new { status = 1, title = "", text = "Thêm thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = -2, title = "", text = "Thêm không thành công. SOCTXN đã tồn tại vui lòng làm mới SOCTXN bằng cách ấn f5 để load lại chương trình ", obj = "" }, JsonRequestBehavior.AllowGet);

                }
            }
            catch (Exception ex)
            {
                var messageERR = ex.InnerException.InnerException.Message;
                return Json(new { status = -1, title = "", text = messageERR, obj = "" }, JsonRequestBehavior.AllowGet);
            }

        }
        #endregion      

        #region Delete
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _DeleteFun(string SOCTXN)
        {
            try
            {  

                #region Check tồn tại
                if (vt_.XUATNHAPs.FirstOrDefault(p => p.SoCTXN == SOCTXN) != null)
                {
                    return Json(new { status = -2, title = "", text = "Xóa không thành công. Số phiếu còn tồn tại trong chi tiết. Vui lòng xóa chi tiết trước!.", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                #endregion

                var item = vt_.DM_XUATNHAP.FirstOrDefault(p => p.SOCTXN == SOCTXN);
                vt_.DM_XUATNHAP.Remove(item);
                vt_.SaveChanges();
                return Json(new { status = 1, title = "", text = "Xóa thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var messageERR = ex.InnerException.InnerException.Message;
                return Json(new { status = -1, title = "", text = messageERR, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Delete ct
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _DeletectFun(string KHOAKEYXN)
        {
            try
            {
                //    var manvhanhdong = db_.sp_update_manvhanhdong_ghilog(User.UserName, "", Khoa);

                var item = vt_.XUATNHAPs.FirstOrDefault(p => p.KHOAKEYXN == KHOAKEYXN);
                vt_.XUATNHAPs.Remove(item);
                vt_.SaveChanges();
                return Json(new { status = 1, title = "", text = "Xóa thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var messageERR = ex.InnerException.InnerException.Message;
                return Json(new { status = -1, title = "", text = messageERR, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Delete Diễn giải
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _DeleteDienGiaiFun(string KEYNHAP)
        {
            try
            {
                var item = vt_.NHAPDIENGIAIs.FirstOrDefault(p => p.KEYNHAP == KEYNHAP);
                vt_.NHAPDIENGIAIs.Remove(item);
                vt_.SaveChanges();
                return Json(new { status = 1, title = "", text = "Xóa thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region _Delete_XZZZ
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _Delete_XZZZ(Guid id_phien)
        {
            try
            {
                var items = vt_.TBL_PHIENMAVT_BYMAHANG.FirstOrDefault(p => p.ID_PHIEN == id_phien);
                vt_.TBL_PHIENMAVT_BYMAHANG.Remove(items);
                vt_.SaveChanges();
                return Json(new { status = 1, title = "", text = "Xóa thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region _UpdateMAVT_PHIEN
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _UpdateMAVT_PHIEN(Guid ID_PHIEN, string mavt)
        {
            try
            {
                var items = vt_.TBL_PHIENMAVT_BYMAHANG.FirstOrDefault(p => p.ID_PHIEN == ID_PHIEN);
                items.MAVT = mavt;
                vt_.Entry(items).State = EntityState.Modified;
                vt_.SaveChanges();
                return Json(new { status = 1, title = "", text = "Cập nhật thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion  

        #region Delete ALL Chi tiết theo số phiếu
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _DeleteALLFun(string Sophieu)
        {
            try
            {
                var item = soi_.NL_CTXUATNHAP.Where(p => p.SOCTXN == Sophieu).ToList();
                foreach (var items in item)
                {
                    soi_.NL_CTXUATNHAP.Remove(items);
                }

                soi_.SaveChanges();
                return Json(new { status = 1, title = "", text = "Xóa tất cả thành công.", obj = Sophieu }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region  _ReportPrint
        public ActionResult _ReportPrint(string SOCTXN, string makho)
        {
            try
            {
                if (SOCTXN == "" || SOCTXN == null)
                {
                    return Json(new { status = -1, title = "", text = "Không tìm thấy mã số chứng từ", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    string PhieuYCXK_DonVi = "";
                    if (makho.Trim() == "KHOHC")
                    {
                        PhieuYCXK_DonVi = "06";
                    }
                    else
                    {
                        PhieuYCXK_DonVi = "03";
                    }
                    var data = vt_.SP_REPYCXUATKHO_DONVI(SOCTXN.Trim(), PhieuYCXK_DonVi).ToList();
                    if (data.Count() == 0)
                    {
                        return Json(new { status = -1, title = "", text = "Không tìm thấy dữ liệu", obj = "" }, JsonRequestBehavior.AllowGet);
                    }
                    ViewBag.data = data;
                }

                return PartialView("_ReportPrint");
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region  _ReportPrint Diễn giải
        public ActionResult _ReportPrint_Diengiai(string SOCTXN)
        {
            try
            {
                if (SOCTXN == "" || SOCTXN == null)
                {
                    return Json(new { status = -1, title = "", text = "Không tìm thấy mã số chứng từ", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var soPO = vt_.DM_XUATNHAP.FirstOrDefault(p => p.SOCTXN == SOCTXN).SOPO;
                    var data = vt_.VATTU2024_LOAD_PRINT_DATA_NHAPDIENGIAI_SOCTXN(SOCTXN).Where(p => p.SOCTXN_A == SOCTXN).ToList();
                    if (data.Count() == 0)
                    {
                        return Json(new { status = -1, title = "", text = "Không tìm thấy dữ liệu", obj = "" }, JsonRequestBehavior.AllowGet);
                    }
                    ViewBag.data = data;
                    ViewBag.SOCTXN = SOCTXN;
                    ViewBag.soPO = soPO;
                }

                return PartialView("_ReportPrint_Diengiai");
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Đồng bộ Netsuite
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _TransAPI(string SOCTXN = null,string MAKHONHAN = null, string LOAIXN = null, string MAKHOXUAT = null)
        {
            if (ModelState.IsValid)
            {
                #region Khai báo data TTG
                var dmxuatnhap = vt_.DM_XUATNHAP.FirstOrDefault(p => p.SOCTXN == SOCTXN);
                var ctxuatnhap = vt_.XUATNHAPs.Where(p => p.SoCTXN == SOCTXN).ToList();
                var xuat3buoc = vt_.DMLOAIXNs.FirstOrDefault(p => p.LOAIXN == dmxuatnhap.LOAIXN);
                var department = nstt_.Donvi_mapping_Department.Where(p => p.idmadv == dmxuatnhap.IDMADVNHAN).ToList();
                if (dmxuatnhap.IDMADVNHAN == "30" || dmxuatnhap.IDMADVNHAN == "92")
                {
                    department = nstt_.Donvi_mapping_Department.Where(p => p.idmacs == dmxuatnhap.IDMADVNHAN).ToList();
                }

                var kNhan = nstt_.Khoes.Where(p => p.makho == dmxuatnhap.MAKHONHAN).ToList();
                var kXuat = nstt_.Khoes.Where(p => p.makho == dmxuatnhap.MAKHOXUAT).ToList();

                if (kNhan.Count < 1)
                {
                    kNhan = nstt_.Khoes.Where(p => p.externalid == dmxuatnhap.MAKHONHAN).ToList();

                }
                if (kXuat.Count < 1)
                {
                    kXuat = nstt_.Khoes.Where(p => p.externalid == dmxuatnhap.MAKHOXUAT).ToList();

                }
                #endregion
                try
                {
                    #region

                    System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    ServicePointManager.ServerCertificateValidationCallback += delegate (object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
                    {
                        return true;
                    };
                    NSClient.NSClient ns = null;
                    try
                    {
                        ns = new NSClient.NSClient();
                        NSBase.Client = ns;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error while loading the application:" + ex.Message);
                        Console.WriteLine("Press Enter to quit ... ");
                        Console.ReadKey();
                    }
                    #endregion

                    if (ns != null)
                    {
                        if (LOAIXN == "N") // (nhập kho mua ngoài)
                        {
                            #region Nhập kho mua ngoài

                            #region Khai báo data
                            var dmphieu = vt_.DM_XUATNHAP.FirstOrDefault(p => p.SOCTXN == SOCTXN);
                            var ctphieu = vt_.XUATNHAPs.Where(p => p.SoCTXN == SOCTXN).ToList();
                            var sumByMavt = vt_.XUATNHAPs
                                            .Where(p => p.SoCTXN == SOCTXN)
                                            .GroupBy(p => p.MAVT)
                                            .Select(g => new
                                            {
                                                MAVT = g.Key,
                                                TongSoLuong = g.Sum(x => x.SoLuongTT)
                                            })
                                            .ToList();

                            var f_ctphieu = ctphieu.FirstOrDefault();

                            if (dmphieu == null)
                            {
                                return Json(new { status = -1, title = "", text = "Đồng bộ không thành công, vui lòng LH ITC", obj = "" }, JsonRequestBehavior.AllowGet);
                            }
                            var eSOCTXN = SOCTXN;
                            var internalidPO = dmphieu.InternalidPO == null ? "" : dmphieu.InternalidPO;
                            #endregion

                            PurchaseOrder record = new PurchaseOrder();
                            PurchaseOrderItemList fields = (PurchaseOrderItemList)record.itemList;// list field trong 1 record

                            ItemReceiptItemList orderItemList = new ItemReceiptItemList();
                            List<ItemReceiptItem> orderItems = new List<ItemReceiptItem>();
                            CustomFieldRef[] cusDetail = new CustomFieldRef[100];

                            InitializeRef initializeRef = new InitializeRef()
                            {
                                internalId = internalidPO,
                                type = InitializeRefType.purchaseOrder,
                                typeSpecified = true,
                            };

                            InitializeRecord initializeRecord = new InitializeRecord() { reference = initializeRef, type = InitializeType.itemReceipt };
                            ReadResponse read = ns.Service.initialize(initializeRecord);
                            ItemReceipt ir = (ItemReceipt)read.record;
                            ir.exchangeRateSpecified = false;
                            ir.tranDate = Convert.ToDateTime(dmxuatnhap.NGAY);
                            ir.tranDateSpecified = true;
                            ir.tranId = eSOCTXN;
                   
                            ir.externalId = eSOCTXN;

                            ir.landedCostPerLine = true; // kế toán yêu cầu 30012026
                            ir.landedCostPerLineSpecified = true;
                            ir.memo = dmphieu.GHICHU;
                            // ir.createdDate = DateTime.Now;
                            ir.createdDateSpecified = false;
                            ir.customForm = new RecordRef() { internalId = "127" }; //id:127 Mua ngoài vattu


                            // Tạo từ điển tạm theo MAVT để quản lý các dòng còn lại
                            // Tạo dictionary
                            var groupedByMavt = sumByMavt
                                .GroupBy(p => p.MAVT.Trim())
                                .ToDictionary(g => g.Key, g => g.ToList());

                            // Chuẩn bị location 1 lần duy nhất
                            var locationRef = new RecordRef
                            {
                                externalId = kNhan.FirstOrDefault()?.externalid
                            };

                            for (int j = 0; j < ir.itemList.item.Length; j++)
                            {
                                var itemLine = ir.itemList.item[j];
                                string mavt = itemLine.itemName?.Trim();

                                itemLine.location = locationRef;
                                itemLine.itemReceiveSpecified = true;

                                if (mavt != null && groupedByMavt.TryGetValue(mavt, out var list))
                                {
                                    double qtyReceive = list.Sum(x => (double)x.TongSoLuong);

                                    if (qtyReceive > 0)
                                    {
                                        itemLine.itemReceive = true;
                                        itemLine.quantity = qtyReceive;
                                    }
                                    else
                                    {
                                        itemLine.itemReceive = false;
                                        itemLine.quantity = 0;
                                    }

                                    // Remove toàn bộ MAVT này để tránh xử lý lại
                                    groupedByMavt.Remove(mavt);
                                }
                                else
                                {
                                    itemLine.itemReceive = false;
                                    itemLine.quantity = 0;
                                }
                            }




                            WriteResponse response = ns.Service.add(ir);

                            #endregion
                            if (response.status.isSuccess == false)
                            {
                                var mess = response.status.statusDetail.FirstOrDefault().message;
                                return Json(new { status = 0, title = "Error", text = mess, obj = "" }, JsonRequestBehavior.AllowGet);
                            }
                        }

                        if (LOAIXN == "NZZZ" || LOAIXN == "N_MAU" || LOAIXN == "NDC" || LOAIXN == "HKNVL_GCN" || LOAIXN == "N_GCN" || LOAIXN == "NKH") // (nhập kho NZZZ phiên mã vải)
                        {
                            #region Nhập điều chỉnh
                            #region Call data                    

                            var customform = "152"; // Điều chỉnh tồn                        
                                                    //var customform = "169"; // Điều chỉnh tồn                        
                                                    // var cls = nstt_.Classes.FirstOrDefault(p => p.externalid == "BAN_PTVT");
                            var tk_iad = vt_.DMLOAIXNs.FirstOrDefault(p => p.LOAIXN == LOAIXN).Account_IAD;
                            var int_iad = nstt_.Chart_of_Accounts.FirstOrDefault(p => p.Number_TT == tk_iad).InternalID_NS;
                            var taikhoanketoan = int_iad; // Account: 1571 - Hàng gửi bán: Vật tư
                                                          // Account: 1578  - Hàng gửi bán: Chờ xử lý

                            var ex_location = kNhan.FirstOrDefault().externalid;
                            #endregion

                            #region InventoryAdjustment
                            InventoryAdjustment InAdj = new InventoryAdjustment();
                            CustomFieldRef[] cusfield = new CustomFieldRef[100];

                            RecordRef cusfRec = new RecordRef();
                            cusfRec.internalId = customform;
                            InAdj.customForm = cusfRec;

                            InAdj.externalId = dmxuatnhap.Externalid_IAD;
                            InAdj.tranId = dmxuatnhap.Externalid_IAD;

                            RecordRef account = new RecordRef();
                            account.internalId = taikhoanketoan;
                            InAdj.account = account;

                            InAdj.tranDateSpecified = true;
                            InAdj.tranDate = Convert.ToDateTime(dmxuatnhap.NGAY);

                            RecordRef khonhan = new RecordRef();
                            // khoxuat.externalId = kNhan.Count < 1 ? "" : kNhan.FirstOrDefault().externalid;
                            khonhan.externalId = ex_location;
                            InAdj.adjLocation = khonhan;
                            InAdj.memo = dmxuatnhap.GHICHU;

                            if (dmxuatnhap.IDMADVNHAN != "30")
                            {
                                RecordRef mabp = new RecordRef();
                                mabp.externalId = department.Count < 1 ? "" : department.FirstOrDefault().externalid;
                                InAdj.department = mabp;
                            }

                            int j = 0;

                            cusfield[j] = CUSTOMRECORD.FUNC_SelectCustomFieldRef("custbody_btm_tt_phan_loai_dieu_chinh",
                                          ENUM_ID.PhanLoaiDieuChinh.DieuChinhTon); j++;
                            cusfield[j] = CUSTOMRECORD.FUNC_SelectCustomFieldRef("custbody_btm_tt_loai_sp", "",
                                          "VATTU"); j++;
                            cusfield[j] = CUSTOMRECORD.FUNC_SelectCustomFieldRef("custbody_btm_tt_nguon_goc",
                                          ENUM_ID.NGUONGOC.MUANGOAIND); j++;
                            cusfield[j] = CUSTOMRECORD.FUNC_SelectCustomFieldRef("custbody_btm_tt_nguon_goc_hang",
                                          ENUM_ID.NGUONGOCHANG.MUANGOAI); j++;

                            InAdj.customFieldList = cusfield;

                            #region Transfer CTPHIEU data

                            InventoryAdjustmentInventoryList orderItemList_IA = new InventoryAdjustmentInventoryList();

                            List<InventoryAdjustmentInventory> orderItems_IA = new List<InventoryAdjustmentInventory>();

                            foreach (var items in ctxuatnhap)
                            {
                                var idmacs = vt_.TBLDMCAPCOSOes.Where(p => p.MABP == items.MaBP).ToList();
                                var f_idmacs = idmacs.Count < 1 ? "" : idmacs.FirstOrDefault().IDMaCS;
                                var checkidmacs = (f_idmacs == "" ? (department.Count < 1 ? "" : department.FirstOrDefault().externalid) : f_idmacs);
                                var IAD_detail_department = nstt_.Donvi_mapping_Department.Where(p => p.idmacs == checkidmacs).ToList();
                                var f_IAD_detail_department = IAD_detail_department.Count < 1 ? "" : IAD_detail_department.FirstOrDefault().externalid;

                                CustomFieldRef[] cusDetail = new CustomFieldRef[100];
                                InventoryAdjustmentInventory line_IA = new InventoryAdjustmentInventory();
                                InventoryAdjustment asd = new InventoryAdjustment();

                                // line_IA.GetType()
                                RecordRef refItem = new RecordRef();
                                refItem.externalId = items.MAVT;
                                line_IA.item = refItem;
                                line_IA.memo = items.GHICHU;

                                RecordRef location_item = new RecordRef();
                                location_item.externalId = kNhan.FirstOrDefault().externalid; // Kho nhận
                                line_IA.location = location_item;

                                line_IA.adjustQtyBySpecified = true;
                                line_IA.adjustQtyBy = Convert.ToDouble(items.SoLuongTT);

                                RecordRef reflocation = new RecordRef();
                                // reflocation.externalId = kNhan.FirstOrDefault().externalid;
                                reflocation.externalId = ex_location;
                                line_IA.location = reflocation;

                                RecordRef refdepartment = new RecordRef();
                                // reflocation.externalId = kNhan.FirstOrDefault().externalid;
                                refdepartment.externalId = f_IAD_detail_department;
                                line_IA.department = refdepartment;
                                orderItems_IA.Add(line_IA);
                            }

                            #endregion

                            orderItemList_IA.inventory = orderItems_IA.ToArray();
                            InAdj.inventoryList = orderItemList_IA;
                            WriteResponse response_IA = ns.Service.upsert(InAdj);

                            #endregion end InventoryAdjustment

                            if (response_IA.status.isSuccess == false)
                            {
                                var messerror = ((ToolsApp.com.netsuite.webservices.Status)response_IA.status).statusDetail;
                                var messdetail = messerror.FirstOrDefault().message;
                                return Json(new { status = 0, title = "Error", text = messdetail, obj = "" }, JsonRequestBehavior.AllowGet);
                            }
                            #endregion end Xuất sử dụng
                        }

                        if (LOAIXN == "HK") // (Hồi kho từ kho SX về kho chính (A2.03 >> A2.01)) <=> XNB
                        {
                            #region Hồi kho NVL
                            #region Gọi thông tin data đã mapping

                            #endregion

                            #region Xử lý add vào Netsuite 423
                            //var manetsuite = db_.SP_API_Load_MaNetSuiteYC().FirstOrDefault().MaNetSuiteYC;
                            InventoryTransfer IT_PhieuXkho = new InventoryTransfer();
                            //Date
                            IT_PhieuXkho.tranDate = Convert.ToDateTime(dmxuatnhap.NGAY);
                            IT_PhieuXkho.tranDateSpecified = true;

                            //Số phiếu// Ref.No
                            IT_PhieuXkho.tranId = dmxuatnhap.SOCTXN; //-- bị chặn trên netsuite
                            IT_PhieuXkho.externalId = dmxuatnhap.SOCTXN;

                            //From location

                            RecordRef FromLocation = new RecordRef();
                            // FromLocation.externalId = "NS-A1.00";
                            FromLocation.externalId = kXuat.Count < 1 ? "" : kXuat.FirstOrDefault().externalid;
                            IT_PhieuXkho.location = FromLocation;

                            //To TransLocation 

                            RecordRef ToLocation = new RecordRef();
                            // ToLocation.externalId = "NS-A1.03";
                            ToLocation.externalId = kNhan.Count < 1 ? "" : kNhan.FirstOrDefault().externalid;
                            IT_PhieuXkho.transferLocation = ToLocation;

                            //Memo / Ghi chú
                            IT_PhieuXkho.memo = dmxuatnhap.GHICHU == null ? "" : dmxuatnhap.GHICHU;

                            // đơn vị nhận// Department

                            RecordRef PYC_Department = new RecordRef();
                            PYC_Department.externalId = department.Count < 1 ? "" : department.FirstOrDefault().externalid;
                            IT_PhieuXkho.department = PYC_Department;

                            // Phân loại phiếu
                            CustomFieldRef[] Cus_PhieuXkho = new CustomFieldRef[2];

                            //var phanloai = nstt_.Table_LoaiPhieuVanChuyenNoiBo
                            SelectCustomFieldRef select_cus_phanloaiphieu = new SelectCustomFieldRef();
                            ListOrRecordRef List_phanloaiphieu = new ListOrRecordRef();
                            List_phanloaiphieu.internalId = "2";
                            select_cus_phanloaiphieu.scriptId = "custbody_btm_tt_loai_phieu_van_chuyen";
                            select_cus_phanloaiphieu.value = List_phanloaiphieu;
                            Cus_PhieuXkho[0] = select_cus_phanloaiphieu;

                            //StringCustomFieldRef REF_SOCTXN = new StringCustomFieldRef();
                            //REF_SOCTXN.scriptId = "custbody_btm_tt_ma_don_yeu_cau";
                            //REF_SOCTXN.value = dmxuatnhap.SOCTXN.ToString();
                            //Cus_PhieuXkho[1] = REF_SOCTXN;

                            //// CHI TIẾT PHIẾU YC

                            //var CTphieu = db_.XUATNHAPs.Where(p => p.SoCTXN == SOCTXN && p.DaXuatKho == 1).ToList();
                            InventoryTransferInventoryList orderItemList = new InventoryTransferInventoryList();
                            List<InventoryTransferInventory> orderItems = new List<InventoryTransferInventory>();

                            foreach (var ctitem in ctxuatnhap)
                            {

                                InventoryTransferInventory item_PhieuXkho = new InventoryTransferInventory();
                                //Item // Mặt hàng// MAVT
                                RecordRef Item_rec = new RecordRef();
                                Item_rec.externalId = ctitem.MAVT;
                                item_PhieuXkho.item = Item_rec;
                                item_PhieuXkho.adjustQtyBy = Convert.ToDouble(ctitem.SoLuongTT);
                                item_PhieuXkho.adjustQtyBySpecified = true;

                                orderItems.Add(item_PhieuXkho);
                            }

                            orderItemList.inventory = orderItems.ToArray();

                            IT_PhieuXkho.customFieldList = Cus_PhieuXkho;
                            IT_PhieuXkho.inventoryList = orderItemList;

                            WriteResponse response = ns.Service.upsert(IT_PhieuXkho);

                            #endregion

                            if (response.status.isSuccess == false)
                            {
                                var messerror = ((ToolsApp.com.netsuite.webservices.Status)response.status).statusDetail;
                                var messdetail = messerror.FirstOrDefault().message;
                                return Json(new { status = 1, title = "Error", text = messdetail, obj = "" }, JsonRequestBehavior.AllowGet);
                            }
                            #endregion
                        }

                        if (LOAIXN == "HKGCGB")
                        {
                            #region Tạo đơn hàng yc cấu sản xuất

                            #region Call data
                            var MADINHMUC = ctxuatnhap.FirstOrDefault().MaPhieuPD;
                            var info_bangke = soi_.VATTU2024_SPLOAD_GC_DMPHIEU_BANGKENHAPSOI(MADINHMUC, "").FirstOrDefault();//không null 
                            var bophansx_hkgc = nstt_.Donvi_mapping_Department.Where(p => p.idmadv == info_bangke.MADV).ToList();
                            var f_bophansx_hkgc = bophansx_hkgc.Count < 1 ? "" : bophansx_hkgc.FirstOrDefault().externalid;
                            var IDMADV_TH = nstt_.Donvi_mapping_Department.Where(p => p.idmadv == info_bangke.MADV).ToList();
                            var IDMADV_YC = nstt_.Donvi_mapping_Department.Where(p => p.idmadv == info_bangke.MAKH).ToList();
                            var f_IDMADV_TH = IDMADV_TH.Count < 1 ? "" : IDMADV_TH.FirstOrDefault().externalid;
                            var f_IDMADV_YC = IDMADV_YC.Count < 1 ? "" : IDMADV_YC.FirstOrDefault().externalid;
                            var nhamaysx_location_hkgc = nstt_.Khoes.Where(p => p.makho == dmxuatnhap.MAKHOXUAT).ToList();
                            var f_nhamaysx_location_hkgc = nhamaysx_location_hkgc.Count < 1 ? "" : nhamaysx_location_hkgc.FirstOrDefault().externalid;
                            var internalid_SoiGC = (info_bangke.LoaiYCSX == null || info_bangke.LoaiYCSX == "") ? UtilsNetsuite.LoaiYeuCauSanXuat.GCSoi : info_bangke.LoaiYCSX;
                            var sumsl = soi_.VATTU2024_SPLOAD_GC_CTPHIEUXUATGC_SUMSOLUONG(MADINHMUC, "").ToList();
                            #endregion

                            #region Check tồn định mức
                            foreach (var chk in ctxuatnhap)
                            {
                                var dinhmuccp_chk = soi_.VATTU2024_DINHMUC_SOIGC_TONGSLVATTUCAN(chk.MAVT, chk.MaPhieuPD, chk.SoLuongTT).ToList();
                                if (dinhmuccp_chk.Count < 1)
                                {
                                    return Json(new { status = -1, title = "", text = "Đồng bộ thất bại, kiểm tra lại trạng thái xem xét định mức", obj = chk.MaPhieuPD }, JsonRequestBehavior.AllowGet);
                                }

                                //foreach (var chkitem in dinhmuccp_chk)
                                //{
                                //    var itemcheck = CUSTOMSEARCH.SEARCH_ITEM_INTERNALID(chkitem.MAVT, f_nhamaysx_location_hkgc);
                                //    if ((double)itemcheck.quantityonhand < (double)chkitem.TONGSLNVLCAN)
                                //    {
                                //        return Json(new { status = -1, title = "", text = "Đồng bộ thất bại, kiểm tra lại số lượng định mức cần của mavt: " + chkitem.MAVT + " Số lượng tồn: " + (double)itemcheck.quantityonhand + "Số lượng định mức: " + (double)chkitem.TONGSLNVLCAN, obj = chk.MADINHMUC }, JsonRequestBehavior.AllowGet);
                                //    }
                                //}

                            }
                            #endregion
                            CustomRecord customRec = new CustomRecord();

                            RecordRef RecordRef = new RecordRef();
                            RecordRef.internalId = UtilsNetsuite.CustomRecord.DonYeuCauSanXuat;
                            customRec.recType = RecordRef; // Chọn CustomRecord 
                            customRec.name = info_bangke.SOPHIEU + "_DH_GCGIAYBONG"; // gán mã đơn hàng 
                            customRec.externalId = info_bangke.SOPHIEU + "_DH_GCGIAYBONG"; // gán ext cho đơn hàng
                            customRec.isInactive = false; // ...bật hiệu lực cho đơn hàng : True: tắt hiệu lực

                            CustomFieldRef[] fields = new CustomFieldRef[99];

                            SelectCustomFieldRef f_donyc_loai = new SelectCustomFieldRef()
                            {
                                scriptId = UtilsNetsuite.CustomFieldRef.custrecord_btm_tt_don_yc_loai,
                                value = new ListOrRecordRef()
                                {
                                    internalId = internalid_SoiGC,
                                }
                            };
                            SelectCustomFieldRef f_BoPhanyc = new SelectCustomFieldRef()
                            {
                                scriptId = UtilsNetsuite.CustomFieldRef.custrecord_btm_tt_don_yc_bo_phan,
                                value = new ListOrRecordRef()
                                {
                                    externalId = f_IDMADV_YC,
                                }
                            };
                            StringCustomFieldRef f_MANVYC = new StringCustomFieldRef()
                            {
                                scriptId = UtilsNetsuite.CustomFieldRef.custrecord_btm_tt_don_yc_nguoi,
                                value = info_bangke.MANV // người yêu cầu, chưa biết gán tạm người lập phiếu
                            };
                            DateCustomFieldRef f_Ngay = new DateCustomFieldRef()
                            {
                                scriptId = UtilsNetsuite.CustomFieldRef.custrecord_btm_tt_don_yc_ngay,
                                value = (DateTime)info_bangke.NGAYLAP,
                            };
                            SelectCustomFieldRef f_Loaisx = new SelectCustomFieldRef()
                            {
                                scriptId = UtilsNetsuite.CustomFieldRef.custrecord_btm_tt_don_yc_loai_sx,
                                value = new ListOrRecordRef()
                                {
                                    internalId = info_bangke.NHAPKHO == "HK_XGIAO_GC" ? UtilsNetsuite.LoaiSanXuat.GiaoGiaCong :
                                    UtilsNetsuite.LoaiSanXuat.SanXuat,
                                }
                            };
                            SelectCustomFieldRef f_Thitruong = new SelectCustomFieldRef()
                            {
                                scriptId = UtilsNetsuite.CustomFieldRef.custrecord_btm_tt_don_yc_thi_truong,
                                value = new ListOrRecordRef()
                                {
                                    internalId = UtilsNetsuite.ThiTruong.ND
                                }
                            };
                            SelectCustomFieldRef f_Trangthai = new SelectCustomFieldRef()
                            {
                                scriptId = UtilsNetsuite.CustomFieldRef.custrecord_btm_tt_don_yc_tt_don_hang,
                                value = new ListOrRecordRef()
                                {
                                    internalId = UtilsNetsuite.TrangThaiDonYeuCauSX.BatDau
                                }
                            };
                            DateCustomFieldRef f_date_start = new DateCustomFieldRef()
                            {
                                scriptId = UtilsNetsuite.CustomFieldRef.custrecord_btm_tt_don_yc_date_start,
                                value = (DateTime)info_bangke.NGAYLAP,
                            };
                            DateCustomFieldRef f_date_end = new DateCustomFieldRef()
                            {
                                scriptId = UtilsNetsuite.CustomFieldRef.custrecord_btm_tt_don_yc_date_end,
                                value = (DateTime)DateTime.Now.AddDays(30),
                            };


                            fields[1] = f_donyc_loai;
                            fields[2] = f_BoPhanyc;
                            fields[3] = f_Ngay;
                            fields[4] = f_Loaisx;
                            fields[5] = f_Thitruong;
                            fields[6] = f_Trangthai;
                            fields[7] = f_date_start;
                            fields[8] = f_date_end;
                            fields[9] = f_MANVYC;


                            customRec.customFieldList = fields;
                            WriteResponse response = ns.Service.upsert(customRec);

                            if (response.status.isSuccess == false)
                            {
                                return Json(new { status = -1, title = "", text = "API Netsuite thất bại", obj = response }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                var parent = ((ToolsApp.com.netsuite.webservices.CustomRecordRef)response.baseRef).internalId;


                                foreach (var items in sumsl)
                                {
                                    #region customRec_pageDinhmuc -- 509  // add form định mức
                                    var item_dinhmuc = items.SOPHIEU;
                                    CustomRecord customRec_pageDinhmuc = new CustomRecord();
                                    RecordRef DataRec = new RecordRef();
                                    DataRec.internalId = "318"; //  định mức
                                    customRec_pageDinhmuc.recType = DataRec;
                                    customRec_pageDinhmuc.name = item_dinhmuc;
                                    customRec_pageDinhmuc.externalId = item_dinhmuc;
                                    WriteResponse response_pageDinhmuc = ns.Service.upsert(customRec_pageDinhmuc);
                                    #endregion


                                    #region Add  CT
                                    var item_externalid = items.SOPHIEU + "_" + items.MAVT;
                                    CustomRecord customRec_item = new CustomRecord();

                                    RecordRef RecordRef_item = new RecordRef();
                                    RecordRef_item.internalId = "207"; // yêu cầu gia công sợi
                                    customRec_item.recType = RecordRef_item; // Chọn RecordRef_item                                     
                                    customRec_item.externalId = item_externalid; // gán ext cho đơn hàng

                                    CustomFieldRef[] fields_items = new CustomFieldRef[99];

                                    SelectCustomFieldRef fi_parent = new SelectCustomFieldRef()
                                    {
                                        scriptId = UtilsNetsuite.CustomFieldRef.custrecord_btm_tt_dyc_gcs_parent,
                                        value = new ListOrRecordRef()
                                        {
                                            internalId = parent
                                        }
                                    };
                                    SelectCustomFieldRef fi_Loaidh = new SelectCustomFieldRef()
                                    {
                                        scriptId = UtilsNetsuite.CustomFieldRef.custrecord_btm_tt_dyc_gcs_loai_dh,
                                        value = new ListOrRecordRef()
                                        {
                                            internalId = UtilsNetsuite.LoaiDonHang.INKH
                                        }
                                    };
                                    SelectCustomFieldRef fi_mahangthanhpham = new SelectCustomFieldRef()
                                    {
                                        scriptId = UtilsNetsuite.CustomFieldRef.custrecord_btm_tt_dyc_gcs_mhtp,
                                        value = new ListOrRecordRef()
                                        {
                                            externalId = items.MAVT
                                        }
                                    };
                                    SelectCustomFieldRef fi_dinhmucsoi = new SelectCustomFieldRef()
                                    {
                                        scriptId = UtilsNetsuite.CustomFieldRef.custrecord_btm_tt_dyc_gcs_dinh_muc,
                                        value = new ListOrRecordRef()
                                        {
                                            externalId = item_dinhmuc
                                        }
                                    };
                                    StringCustomFieldRef fi_SLYC = new StringCustomFieldRef()
                                    {
                                        scriptId = UtilsNetsuite.CustomFieldRef.custrecord_btm_tt_dyc_gcs_sl_yc,
                                        value = items.SOLUONGXuat == null ? "0" : Convert.ToString(items.SOLUONGXuat)
                                    };
                                    StringCustomFieldRef fi_MANVYC = new StringCustomFieldRef()
                                    {
                                        scriptId = UtilsNetsuite.CustomFieldRef.custrecord_btm_tt_dyc_gcs_ma_nhan_vien,
                                        value = info_bangke.MANV == null ? "" : Convert.ToString(info_bangke.MANV)
                                    };
                                    DateCustomFieldRef fi_NgayYC = new DateCustomFieldRef()
                                    {
                                        scriptId = UtilsNetsuite.CustomFieldRef.custrecord_btm_tt_dyc_gcs_ngay_cap_nhat,
                                        value = (DateTime)info_bangke.NGAYLAP,
                                    };

                                    var ishieulucbangke = soi_.GC_DMPHIEU_BANGKENHAPSOI.FirstOrDefault(p => p.SOPHIEU == item_dinhmuc).HIEULUC_XX;
                                    if (ishieulucbangke != true)
                                    {
                                        return Json(new { status = -1, title = "", text = "Số phiếu bảng kê:" + item_dinhmuc + " GĐ đơn vị chưa xem xét, vui lòng kiểm tra lại", obj = "" }, JsonRequestBehavior.AllowGet);
                                    };

                                    StringCustomFieldRef fi_MANVXX = new StringCustomFieldRef()
                                    {
                                        scriptId = UtilsNetsuite.CustomFieldRef.custrecord_btm_tt_dyc_gcs_m_nv_ycsx,
                                        value = info_bangke.MANV_XX == null ? "" : Convert.ToString(info_bangke.MANV_XX)
                                    };

                                    DateCustomFieldRef fi_NgayXX = new DateCustomFieldRef()
                                    {
                                        scriptId = UtilsNetsuite.CustomFieldRef.custrecord_btm_tt_dyc_gcs_ngay_xem_xet,
                                        value = (DateTime)info_bangke.NGAY_XX,
                                    };

                                    StringCustomFieldRef fi_HLXX = new StringCustomFieldRef()
                                    {
                                        scriptId = UtilsNetsuite.CustomFieldRef.custrecord_btm_tt_dyc_gcs_hieu_luc_xem_x,
                                        value = info_bangke.HIEULUC_XX == null ? "F" : "T"
                                    };

                                    StringCustomFieldRef fi_MANVXACNHAN = new StringCustomFieldRef()
                                    {
                                        scriptId = UtilsNetsuite.CustomFieldRef.custrecord_btm_tt_dyc_gcs_mnvdv_thuc_hie,
                                        value = info_bangke.MANV_XX == null ? "" : Convert.ToString(info_bangke.MANV_XX)
                                    };

                                    DateCustomFieldRef fi_NgayXACNHAN = new DateCustomFieldRef()
                                    {
                                        scriptId = UtilsNetsuite.CustomFieldRef.custrecord_btm_tt_dyc_gcs_ngay_xac_nhan,
                                        value = (DateTime)info_bangke.NGAY_XX,
                                    };

                                    StringCustomFieldRef fi_HLXACNHAN = new StringCustomFieldRef()
                                    {
                                        scriptId = UtilsNetsuite.CustomFieldRef.custrecord_btm_tt_dyc_gcs_hieu_luc_xn,
                                        value = info_bangke.HIEULUC_XX == null ? "F" : "T"
                                    };

                                    SelectCustomFieldRef fi_nhamaysx_dhgcsoi = new SelectCustomFieldRef()
                                    {
                                        scriptId = UtilsNetsuite.CustomFieldRef.custrecordcustrecord_btm_tt_don_yc_gcsoi,
                                        value = new ListOrRecordRef()
                                        {
                                            externalId = f_nhamaysx_location_hkgc
                                        }
                                    };

                                    fields_items[0] = fi_parent;
                                    fields_items[1] = fi_Loaidh;
                                    fields_items[2] = fi_mahangthanhpham;
                                    fields_items[3] = fi_SLYC;
                                    fields_items[4] = fi_MANVYC;
                                    fields_items[5] = fi_NgayYC;
                                    fields_items[6] = fi_MANVXX;
                                    fields_items[7] = fi_NgayXX;
                                    fields_items[8] = fi_HLXX;
                                    fields_items[9] = fi_MANVXACNHAN;
                                    fields_items[10] = fi_NgayXACNHAN;
                                    fields_items[11] = fi_HLXACNHAN;
                                    fields_items[12] = fi_dinhmucsoi;
                                    fields_items[13] = fi_nhamaysx_dhgcsoi;
                                    customRec_item.customFieldList = fields_items;
                                    WriteResponse response_item = ns.Service.upsert(customRec_item);
                                    #endregion

                                    #region response_item

                                    if (response_item.status.isSuccess == false)
                                    {
                                        var messerror = ((ToolsApp.com.netsuite.webservices.Status)response_item.status).statusDetail;
                                        var messdetail = messerror.FirstOrDefault().message;
                                        return Json(new { status = -1, title = "", text = messdetail, obj = messdetail }, JsonRequestBehavior.AllowGet);
                                    }
                                    #endregion
                                }

                            }
                            #endregion

                            #region Nhập BOM
                            foreach (var items in ctxuatnhap)
                            {
                                #region Call data
                                var bom_externalid = items.MAVT + "_" + items.MaPhieuPD + "_BOMV1";
                                var dinhmuccp = soi_.DINHMUC_SOIGC(items.MAVT, items.MaPhieuPD).ToList();
                                if (dinhmuccp.Count < 1)
                                {
                                    return Json(new { status = -1, title = "", text = "Đồng bộ thất bại, kiểm tra lại trạng thái xem xét định mức", obj = items.MaPhieuPD }, JsonRequestBehavior.AllowGet);
                                }

                                #endregion

                                #region Check tồn tại BOM
                                RecordRef bomRefChk = new RecordRef();

                                bomRefChk.externalId = bom_externalid;// Hoặc externalId = "";
                                bomRefChk.type = RecordType.bom;
                                bomRefChk.typeSpecified = true;
                                ReadResponse responseBOMchk = ns.Service.get(bomRefChk);
                                #endregion
                                if (responseBOMchk.record == null)
                                {
                                    #region ADD BOM
                                    #region Add định mức 509
                                    Bom bomRec = new Bom();
                                    bomRec.name = bom_externalid;
                                    // bomRec.externalId = model.KeyHHSearch;
                                    bomRec.externalId = bom_externalid;
                                    bomRec.isInactive = true;
                                    bomRec.availableForAllAssemblies = false;
                                    bomRec.availableForAllAssembliesSpecified = true;
                                    bomRec.availableForAllLocations = true;
                                    bomRec.memo = items.GHICHU != null ? items.GHICHU : bom_externalid;

                                    RecordRef[] assembliesItemList = new RecordRef[2];
                                    RecordRef assembliesItem = new RecordRef()
                                    { externalId = items.MAVT };
                                    assembliesItemList[0] = assembliesItem;
                                    bomRec.restrictToAssembliesList = assembliesItemList;

                                    WriteResponse response_BOM = ns.Service.upsert(bomRec);
                                    var statusx = response_BOM.status.isSuccess;
                                    #endregion

                                    if (statusx == false)
                                    {
                                        return Json(new { status = -1, title = "", text = "Đồng bộ thất bại . ERROR response_BOM.", obj = response_BOM }, JsonRequestBehavior.AllowGet);
                                    }
                                    else
                                    {
                                        var bomRev_externalid = items.MAVT + "_" + items.MaPhieuPD + "_Rev";

                                        #region BOM REVISION

                                        RecordRef bomRef = new RecordRef() { externalId = bom_externalid };
                                        BomRevisionComponentList compList = new BomRevisionComponentList();
                                        BomRevisionComponent bomComp = new BomRevisionComponent();
                                        BomRevisionComponent[] bomRevisionComponents = new BomRevisionComponent[199];
                                        int i = 0;
                                        foreach (var itemdm in dinhmuccp)
                                        {
                                            RecordRef itemRec = new RecordRef() { externalId = itemdm.MAVT }; /// KIỂM TRA LẠI
                                            bomComp = new BomRevisionComponent()
                                            {
                                                item = itemRec,
                                                bomQuantity = (double)itemdm.dinhmuc,
                                                bomQuantitySpecified = true,
                                            };
                                            int stt = 0;
                                            CustomFieldRef[] a = new CustomFieldRef[99];

                                            StringCustomFieldRef dm_KeyHH = new StringCustomFieldRef()
                                            {
                                                scriptId = UtilsNetsuite.CustomFieldRef.custrecord_btm_tt_ma_khoa,
                                                value = itemdm.SOPHIEU
                                            };
                                            SelectCustomFieldRef dm_LoaiKhoa = new SelectCustomFieldRef()
                                            {
                                                scriptId = UtilsNetsuite.CustomFieldRef.custrecord_btm_tt_loai_khoa,
                                                value = new ListOrRecordRef()
                                                {
                                                    internalId = UtilsNetsuite.LoaiKhoaCuaThaiTuan.KeyHH
                                                }
                                            };
                                            a[stt] = dm_KeyHH; stt++;
                                            a[stt] = dm_LoaiKhoa; stt++;
                                            bomComp.customFieldList = a;
                                            bomRevisionComponents[i] = bomComp;
                                            i++;
                                        }

                                        compList.bomRevisionComponent = bomRevisionComponents;

                                        BomRevision bomRev = new BomRevision();
                                        bomRev.name = bomRev_externalid;
                                        bomRev.externalId = bomRev_externalid;
                                        bomRev.memo = dinhmuccp.FirstOrDefault().SOPHIEU;
                                        bomRev.billOfMaterials = bomRef;
                                        bomRev.componentList = compList;
                                        bomRev.effectiveStartDate = Convert.ToDateTime(DateTime.Now.AddMonths(-3));
                                        // bomRev.effectiveStartDate = (DateTime)info_bangke.NGAYLAP;
                                        bomRev.effectiveStartDateSpecified = true;

                                        WriteResponse responsebomrev = ns.Service.upsert(bomRev);
                                        if (responsebomrev.status.isSuccess == false)
                                        {
                                            return Json(new { status = -1, title = "", text = responsebomrev.status.statusDetail.FirstOrDefault().message, obj = responsebomrev }, JsonRequestBehavior.AllowGet);
                                        }
                                        else
                                        {
                                            var bom_rev_internalid = ((ToolsApp.com.netsuite.webservices.RecordRef)responsebomrev.baseRef).internalId;
                                            var bom_internalid = ((ToolsApp.com.netsuite.webservices.RecordRef)response_BOM.baseRef).internalId;
                                            var i_idchiphi = dinhmuccp.FirstOrDefault().SOPHIEU;
                                            var mapping = nstt_.Table_Mapping_DinhMuc_KeyHH_MOC.Where(p => p.KeyHH == bomRev_externalid).ToList();
                                            var update_mapping = nstt_.Table_Mapping_DinhMuc_KeyHH_MOC.Where(p => p.KeyHH == bomRev_externalid).FirstOrDefault();
                                            if (mapping.Count < 1)
                                            {
                                                Table_Mapping_DinhMuc_KeyHH_MOC a = new Table_Mapping_DinhMuc_KeyHH_MOC();
                                                a.Internalid_BOM_Rev = bom_rev_internalid;
                                                a.Internalid_BOM = bom_internalid;
                                                a.KeyHH = bomRev_externalid;
                                                a.IDChiPhi = bomRev_externalid + "_1";
                                                nstt_.Table_Mapping_DinhMuc_KeyHH_MOC.Add(a);
                                                nstt_.SaveChanges();
                                            }
                                            else
                                            {
                                                update_mapping.Internalid_BOM_Rev = bom_rev_internalid;
                                                update_mapping.Internalid_BOM = bom_internalid;
                                                update_mapping.KeyHH = bomRev_externalid;
                                                update_mapping.IDChiPhi = bomRev_externalid + "_1";
                                                nstt_.Entry(update_mapping).State = EntityState.Modified;
                                                nstt_.SaveChanges();
                                            }
                                        }
                                        #endregion

                                        #region Assemblies                                 
                                        LotNumberedAssemblyItem assemblyItem = new LotNumberedAssemblyItem();
                                        assemblyItem.externalId = dinhmuccp.FirstOrDefault().Masoi;
                                        LotNumberedAssemblyItemBillOfMaterialsList lotNumberedAssemblyItemBillOfMaterialsList = new LotNumberedAssemblyItemBillOfMaterialsList();
                                        LotNumberedAssemblyItemBillOfMaterials assemblyItemBillOfMaterials = new LotNumberedAssemblyItemBillOfMaterials();
                                        assemblyItemBillOfMaterials.billOfMaterials = new RecordRef()
                                        {
                                            type = RecordType.bom,
                                            externalId = bom_externalid
                                        };
                                        lotNumberedAssemblyItemBillOfMaterialsList.lotNumberedAssemblyItemBillOfMaterials = new LotNumberedAssemblyItemBillOfMaterials[] { assemblyItemBillOfMaterials };
                                        assemblyItem.billOfMaterialsList = lotNumberedAssemblyItemBillOfMaterialsList;
                                        WriteResponse responseBOM_assemblyItem = ns.Service.upsert(assemblyItem);
                                        #endregion
                                    }
                                    #endregion
                                }

                                #region Nhập thành phẩm

                                #region Call data
                                var keyhh_idcp = nstt_.Table_Mapping_DinhMuc_KeyHH_MOC.Where(p => p.KeyHH == items.MAVT + "_" + items.MaPhieuPD + "_Rev").OrderByDescending(p => p.SoLanTinh_KeyHH).ToList();
                                var i_bom_rev_internalid = keyhh_idcp.Count < 1 ? "" : keyhh_idcp.FirstOrDefault().Internalid_BOM_Rev;
                                var i_bom_internalid = keyhh_idcp.Count < 1 ? "" : keyhh_idcp.FirstOrDefault().Internalid_BOM;

                                #endregion

                                CustomRecord customRec_TPSX = new CustomRecord();
                                RecordRef RecordRef_TPSX = new RecordRef();
                                RecordRef_TPSX.internalId = UtilsNetsuite.CustomRecord.ThanhPhamSanXuat;
                                customRec_TPSX.recType = RecordRef_TPSX; // Chọn CustomRecord
                                customRec_TPSX.name = items.MAVT; // gán mã đơn hàng 
                                customRec_TPSX.externalId = items.KHOAKEYXN.ToString(); // gán ext cho đơn hàng

                                CustomFieldRef[] fields_TPSX = new CustomFieldRef[99];

                                SelectCustomFieldRef f_dh_yc_sx = new SelectCustomFieldRef()
                                {
                                    scriptId = UtilsNetsuite.CustomFieldRef.custrecord_btm_tt_m_dh_yc_sx,
                                    value = new ListOrRecordRef()
                                    {
                                        externalId = items.MADH_CN + "_DH_GCGIAYBONG"
                                    }
                                };

                                SelectCustomFieldRef f_mahang_tp = new SelectCustomFieldRef()
                                {
                                    scriptId = UtilsNetsuite.CustomFieldRef.custrecord_btm_tt_mh_tp,
                                    value = new ListOrRecordRef()
                                    {
                                        externalId = items.MAVT
                                    }
                                };

                                //StringCustomFieldRef fi_macay_LOT = new StringCustomFieldRef()
                                //{
                                //    scriptId = UtilsNetsuite.CustomFieldRef.custrecord_btm_tt_lot_serial_number,
                                //    value = items.HIEU + "-" + items.LO
                                //};

                                StringCustomFieldRef fi_SOCT_NHAP = new StringCustomFieldRef()
                                {
                                    scriptId = UtilsNetsuite.CustomFieldRef.custrecord_btm_tt_so_phieu_nhap,
                                    value = items.SoCTXN
                                };
                                DateCustomFieldRef f_Ngaysx = new DateCustomFieldRef()
                                {
                                    scriptId = UtilsNetsuite.CustomFieldRef.custrecord_btm_tt_ngay_sx,
                                    value = (DateTime)items.NgayKeToan,
                                };
                                StringCustomFieldRef fi_soluongthanhpham = new StringCustomFieldRef()
                                {
                                    scriptId = UtilsNetsuite.CustomFieldRef.custrecord_btm_tt_sl_thanh_pham,
                                    value = items.SoLuongTT < 0 ? "0" : items.SoLuongTT.ToString()
                                };
                                SelectCustomFieldRef fi_bophansx = new SelectCustomFieldRef()
                                {
                                    scriptId = UtilsNetsuite.CustomFieldRef.custrecord_btm_tt_bpsx,
                                    value = new ListOrRecordRef()
                                    {
                                        externalId = f_bophansx_hkgc
                                    }
                                };
                                SelectCustomFieldRef fi_nhamaysx = new SelectCustomFieldRef()
                                {
                                    scriptId = UtilsNetsuite.CustomFieldRef.custrecord_btm_tt_mnsx,
                                    value = new ListOrRecordRef()
                                    {
                                        externalId = f_nhamaysx_location_hkgc
                                    }
                                };
                                SelectCustomFieldRef fi_status = new SelectCustomFieldRef()
                                {
                                    scriptId = UtilsNetsuite.CustomFieldRef.custrecord_btm_tt_status_manu,
                                    value = new ListOrRecordRef()
                                    {
                                        internalId = UtilsNetsuite.TrangThaiDonYeuCauSX.BatDau
                                    }
                                };
                                SelectCustomFieldRef fi_BOM = new SelectCustomFieldRef()
                                {
                                    scriptId = UtilsNetsuite.CustomFieldRef.custrecord_btm_tt_bill_of_materials,
                                    value = new ListOrRecordRef()
                                    {
                                        internalId = i_bom_internalid
                                    }
                                };
                                SelectCustomFieldRef fi_BOM_rev = new SelectCustomFieldRef()
                                {
                                    scriptId = UtilsNetsuite.CustomFieldRef.custrecord_btm_tt_bill_of_materials_revi,
                                    value = new ListOrRecordRef()
                                    {
                                        internalId = i_bom_rev_internalid
                                    }
                                };
                                StringCustomFieldRef fi_cothaydoiBOM = new StringCustomFieldRef()
                                {
                                    scriptId = "custrecord_btm_tt_thay_doi_bom",
                                    value = "T"
                                };

                                StringCustomFieldRef fi_cothaydoiBOM_rev = new StringCustomFieldRef()
                                {
                                    scriptId = "custrecord_btm_tt_thay_doi_revision",
                                    value = "T"
                                };


                                fields_TPSX[1] = f_dh_yc_sx;
                                fields_TPSX[2] = f_mahang_tp;
                                // fields_TPSX[3] = fi_macay_LOT;
                                fields_TPSX[5] = f_Ngaysx;
                                fields_TPSX[6] = fi_soluongthanhpham;
                                fields_TPSX[7] = fi_bophansx;
                                fields_TPSX[8] = fi_nhamaysx;
                                fields_TPSX[9] = fi_status;
                                fields_TPSX[10] = fi_BOM;
                                fields_TPSX[11] = fi_BOM_rev;
                                fields_TPSX[12] = fi_SOCT_NHAP;
                                fields_TPSX[13] = fi_cothaydoiBOM;
                                fields_TPSX[14] = fi_cothaydoiBOM_rev;

                                customRec_TPSX.customFieldList = fields_TPSX;
                                WriteResponse response_TPSX = ns.Service.upsert(customRec_TPSX);


                                #endregion
                            }
                            #endregion
                        }

                        #region
                        dmxuatnhap.GUIAPI = "1";
                        dmxuatnhap.NGAYGUIAPI = DateTime.Now;
                        vt_.Entry(dmxuatnhap).State = EntityState.Modified;
                        vt_.SaveChanges();

                        #endregion

                        return Json(new { status = 1, title = "", text = "Thêm thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { status = -1, title = "", text = "Đồng bộ không thành công", obj = "" }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new { status = -2, title = "", text = "Lưu không thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Kế thừa CT phiếu xuất _InsertFunCTTheoSOCTXuat
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _InsertFunCTTheoSOCTXuat(string SOCTXN_Xuat = null, string SOCTXN_Nhap = null)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var dm_nhap = vt_.DM_XUATNHAP.FirstOrDefault(c => c.SOCTXN == SOCTXN_Nhap);
                    var dm_xuat = vt_.DM_XUATNHAP.FirstOrDefault(c => c.SOCTXN == SOCTXN_Xuat);
                    var ct_xuat = vt_.XUATNHAPs.Where(p => p.SoCTXN == SOCTXN_Xuat).ToList();
                    var ct_nhap = vt_.XUATNHAPs.Where(p => p.SoCTXN == SOCTXN_Nhap).ToList();

                    #region Check tồn tại
                    if (ct_nhap.FirstOrDefault(p => p.SoCTXN == SOCTXN_Nhap) != null)
                    {
                        return Json(new { status = -2, title = "", text = "Thêm không thành công. chi tiết đã tồn tại, không thể kế thừa phiếu khác. Kiểm tra lại.", obj = "" }, JsonRequestBehavior.AllowGet);
                    }
                    #endregion
                    if (dm_nhap != null)
                    {
                        foreach (var model in ct_xuat)
                        {
                            #region Check tồn tại
                            if (ct_nhap.FirstOrDefault(p => p.MAVT == model.MAVT && p.SoCTXN == SOCTXN_Nhap) != null)
                            {
                                return Json(new { status = -2, title = "", text = "Thêm không thành công. Mã hàng đã tồn tại. Kiểm tra lại.", obj = "" }, JsonRequestBehavior.AllowGet);
                            }
                            #endregion
                            var kyhieu = vt_.VATTU2024_SP_LOAD_QUYEN_SUDUNG_DMDONVI(User.UserName).FirstOrDefault(p => p.IDMaDV == dm_nhap.IDMADVNHAN)?.KiHieu.Trim().ToUpper() ?? "";
                            var dmnhanvien = vt_.DMNHANVIENs.Where(p => p.MANV == User.UserName).FirstOrDefault();
                            var model_item = new EntityFramework.VatTu.XUATNHAP();
                            model_item.SoCTXN = SOCTXN_Nhap.Trim();
                            model_item.LoaiXN = dm_nhap.LOAIXN;
                            model_item.DonViYeuCau = dmnhanvien.MADV;
                            model_item.MaBP = dmnhanvien.MABP;
                            model_item.MaPhieuPD = kyhieu + "_" + model.MaPhieuPD;
                            model_item.MAVT = model.MAVT;
                            model_item.MaVTTam = model.MAVT;
                            model_item.SoLuongYC = Convert.ToDecimal(model.SoLuongYC);
                            model_item.SoLuongTT = Convert.ToDecimal(model.SoLuongTT);
                            model_item.TongGia = Convert.ToDecimal(0.00);
                            model_item.MaKhoXuat = dm_nhap.MAKHOXUAT.Trim();
                            model_item.MaKho = dm_nhap.MAKHONHAN.Trim();
                            model_item.GHICHU = (model.GHICHU == null || model.GHICHU == "") ? "Kế thừa từ " + SOCTXN_Xuat : model.GHICHU;
                            model_item.MaNVYC = User.UserName;
                            model_item.MANVCN = User.UserName;
                            model_item.Ngay = DateTime.Now;
                            model_item.NgayKeToan = dm_nhap.NGAY;
                            model_item.NGAYCN = DateTime.Now;
                            var KeyKhoa = Guid.NewGuid();
                            model_item.KHOAKEYXN = dm_nhap.LOAIXN.Trim() + "_KeyKhoa_" + KeyKhoa;
                            vt_.XUATNHAPs.Add(model_item);
                            vt_.SaveChanges();
                        }
                    }
                    else
                    {
                        return Json(new { status = -2, title = "", text = "Thêm không thành công. Dữ liệu không tồn có.  Kiểm tra lại.", obj = "" }, JsonRequestBehavior.AllowGet);
                    }
                    vt_.VATTU2024_UPDATE_STATUS_DM_XUATNHAP(SOCTXN_Xuat, SOCTXN_Nhap);
                    return Json(new { status = 1, title = "", text = "Thêm thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new { status = -2, title = "", text = "Lưu không thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Kế thừa CT phiếu xuất _InsertFunCTTheoSOCTXuat NZZZ
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _Insert_ctphieu_nzzz(string SOCTXN_xzzz = null, string SOCTXN_nzzz = null)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var dm_nhap = vt_.DM_XUATNHAP.FirstOrDefault(c => c.SOCTXN == SOCTXN_nzzz);
                    var CT_NHAP = vt_.XUATNHAPs.Where(c => c.SoCTXN == SOCTXN_nzzz).ToList();
                    var ct_nhap_xzzz = vt_.VATTU2024_LOAD_SUM_PHIENMAVT_XZZZ(SOCTXN_xzzz).ToList();
                    if (dm_nhap != null)
                    {
                        foreach (var model in ct_nhap_xzzz)
                        {
                            #region Check tồn tại
                            if (CT_NHAP.FirstOrDefault(p => p.MAVT == model.MAVT && p.SoCTXN == SOCTXN_nzzz) != null)
                            {
                                return Json(new { status = -2, title = "", text = "Thêm không thành công. Mã hàng đã tồn tại. Kiểm tra lại.", obj = "" }, JsonRequestBehavior.AllowGet);
                            }
                            #endregion
                            var dmnhanvien = vt_.DMNHANVIENs.Where(p => p.MANV == User.UserName).FirstOrDefault();
                            var model_item = new EntityFramework.VatTu.XUATNHAP();
                            model_item.SoCTXN = SOCTXN_nzzz.Trim();
                            model_item.LoaiXN = dm_nhap.LOAIXN;
                            model_item.DonViYeuCau = dmnhanvien.MADV;
                            model_item.MaBP = dmnhanvien.MABP;
                            model_item.MaPhieuPD = SOCTXN_nzzz.Trim();
                            model_item.MAVT = model.MAVT;
                            model_item.MaVTTam = model.MAVT;
                            model_item.SoLuongYC = Convert.ToDecimal(model.SOLUONGTT);
                            model_item.SoLuongTT = Convert.ToDecimal(model.SOLUONGTT);
                            model_item.TongGia = Convert.ToDecimal(0.00);
                            model_item.MaKhoXuat = dm_nhap.MAKHOXUAT.Trim();
                            model_item.MaKho = dm_nhap.MAKHONHAN.Trim();
                            model_item.GHICHU = "Kế thừa từ " + SOCTXN_xzzz;
                            model_item.MaNVYC = User.UserName;
                            model_item.MANVCN = User.UserName;
                            model_item.Ngay = DateTime.Now;
                            model_item.NgayKeToan = dm_nhap.NGAY;
                            model_item.NGAYCN = DateTime.Now;
                            var KeyKhoa = Guid.NewGuid();
                            model_item.KHOAKEYXN = dm_nhap.LOAIXN.Trim() + "_KeyKhoa_" + KeyKhoa;
                            vt_.XUATNHAPs.Add(model_item);
                            vt_.SaveChanges();
                        }
                    }
                    else
                    {
                        return Json(new { status = -2, title = "", text = "Thêm không thành công. Dữ liệu không tồn có.  Kiểm tra lại.", obj = "" }, JsonRequestBehavior.AllowGet);
                    }
                    vt_.VATTU2024_UPDATE_STATUS_DM_XUATNHAP(SOCTXN_xzzz, SOCTXN_nzzz);
                    return Json(new { status = 1, title = "", text = "Thêm thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new { status = -2, title = "", text = "Lưu không thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Kế thừa CT phiếu xuất _InsertFunCTTheo_BANGKE
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _Insert_ctphieu_HKGC(string MAKHONHAN_HKGC = null, string SOCTXN_HKGC = null, string SOPHIEU_HKGC = null)
        {
            if (ModelState.IsValid)
            {
                var dm_nhap = vt_.DM_XUATNHAP.FirstOrDefault(c => c.SOCTXN == SOCTXN_HKGC);
                var ct_bangke = soi_.VATTU2025_SP_LOAD_DATA_SOIGIACONG_CHUAHOIKHO_GIAYBONG(dm_nhap.MAKHONHAN, SOPHIEU_HKGC).ToList();
                var ct_nhap = vt_.XUATNHAPs.Where(p => p.SoCTXN == SOCTXN_HKGC).ToList();
                if (dm_nhap != null)
                {
                    #region Check tồn tại
                    if (ct_nhap.Count > 0)
                    {
                        return Json(new { status = -2, title = "", text = "Thêm không thành công, chi tiết phiếu đã tồn tại. Kiểm tra lại.", obj = "" }, JsonRequestBehavior.AllowGet);
                    }
                    #endregion
                    foreach (var model in ct_bangke)
                    {
                        var dmnhanvien = vt_.DMNHANVIENs.Where(p => p.MANV == User.UserName).FirstOrDefault();
                        var model_item = new EntityFramework.VatTu.XUATNHAP();
                        model_item.SoCTXN = SOCTXN_HKGC.Trim();
                        model_item.MAVT = model.MAVT;
                        model_item.Ngay = DateTime.Now;
                        model_item.NGAYCN = DateTime.Now;
                        model_item.NgayKeToan = DateTime.Now;
                        model_item.MaKho = dm_nhap.MAKHONHAN;
                        model_item.MaKhoXuat = dm_nhap.MAKHOXUAT;
                        model_item.LoaiXN = dm_nhap.LOAIXN;
                        model_item.MaVTTam = model.MAVT;
                        model_item.MADH_CN = SOPHIEU_HKGC;
                        model_item.MANVMODIFY = User.UserName;
                        model_item.NGAYMODIFY = DateTime.Now;
                        model_item.SoLuongYC = Convert.ToDecimal(model.SOLUONGTT);
                        model_item.SoLuongTT = Convert.ToDecimal(model.SOLUONGTT);
                        model_item.MaPhieuPD = model.MAPHIEUPD;
                        model_item.KHOATHAMCHIEU = model.KHOATHAMCHIEU;
                        model_item.SoCTKeToan = "";
                        model_item.DaXuatKho = 0;
                        model_item.GHICHU = "Kế thừa từ số phiếu bảng kê: " + SOPHIEU_HKGC;
                        model_item.NgayKeToan = (DateTime)dm_nhap.NGAY;
                        var KeyKhoa = Guid.NewGuid();
                        model_item.KHOAKEYXN = dm_nhap.LOAIXN.Trim() + "_KeyKhoa_" + KeyKhoa;
                        vt_.XUATNHAPs.Add(model_item);
                        vt_.SaveChanges();
                    }

                    return Json(new { status = 1, title = "", text = "Thêm thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = -2, title = "", text = "Thêm không thành công. Dữ liệu không tồn có.  Kiểm tra lại.", obj = "" }, JsonRequestBehavior.AllowGet);
                }

            }
            else
            {
                return Json(new { status = -2, title = "", text = "Lưu không thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Gửi mail yêu cầu xem xét
        [HttpPost]
        public string openOutlookemailbox(string SOCTXN, string MANVXX)
        {
            #region Lấy địa chỉ mail của người pd

            var Email = vt_.DMNHANVIENs.FirstOrDefault(p => p.MANV == MANVXX).EMail;
            #endregion

            MailMessage mail = new MailMessage();
            mail.To.Add(new MailAddress(Email));
            mail.IsBodyHtml = true;
            mail.Subject = SOCTXN;
            mail.Body = SOCTXN;
            var outlookmail = "mailto:" + mail.To.ToString()
                + "?subject=" + "FW: Trình duyệt mã phiếu: " + mail.Subject.ToString() + " từ chương trình QLVT "
                + "&body=" + "Kính gửi : Ban GĐ. %0D%0A" 
                + "Trình duyệt Loại phiếu:" + " Tăng giảm tồn kho" + "(" + mail.Body.ToString() + ")%0D%0A" 
                + "Đường dẫn: https://thaituangarment.com.vn/QLVatTuKhoSoi/XemXetPhieuTangGiamKho" + "%0D%0A" 
                + "Trân trọng!";
            return outlookmail;
        }
        #endregion

        #region UPLOAD EXCEL TĂNG GIẢM TỒN
        public ActionResult _UploadNHTGA(string SOCTXN =null)
        {
            var dmxuatnhap = vt_.DM_XUATNHAP.Where(p => p.SOCTXN == SOCTXN)?.FirstOrDefault();
            ViewBag.LOAIXN = dmxuatnhap.LOAIXN;
            ViewBag.SOCTXN = SOCTXN;
            return PartialView();
        }

        #region Import Excel NHTGA
        [HttpPost]
        [ValidateInput(false)]
        public JsonResult _FunImportNHTGA(string SOCTXN)
        {
            try
            {
                #region Upload
                string fileName = "";
                if (Request.Files != null && Request.Files.Count > 0)
                {
                    var postedFile = Request.Files[0];
                    string fileExtension = postedFile.FileName.Substring(postedFile.FileName.LastIndexOf("."));
                    string newName = Guid.NewGuid().ToString();
                    fileName = postedFile.FileName.Replace(fileExtension, "") + "_" + DateTime.Now.ToString("ddMMyyyy") + newName + fileExtension;
                    postedFile.SaveAs(Server.MapPath("~/UserFiles/Upload/") + Path.GetFileName(fileName));

                    string fileIn = Request.PhysicalApplicationPath + @"UserFiles\Upload\" + fileName;

                    SLDocument sl = new SLDocument(fileIn);

                    sl.SelectWorksheet(sl.GetSheetNames()[0]);

                    SLWorksheetStatistics stats = sl.GetWorksheetStatistics();

                    var isHaveData = false;

                    var numRecord = 1;

                    var list_XUATNHAP = new List<XUATNHAP>();
                    var dmxuatnhap = vt_.DM_XUATNHAP.Where(p => p.SOCTXN == SOCTXN)?.FirstOrDefault();
                    var kyhieu = vt_.VATTU2024_SP_LOAD_QUYEN_SUDUNG_DMDONVI(User.UserName).FirstOrDefault(p => p.IDMaDV == dmxuatnhap.IDMADVNHAN)?.KiHieu.Trim().ToUpper() ?? "";
                    #region Read
                    for (int i = 2; i <= stats.EndRowIndex; i++)
                    {
                        var fileSOCTXN = sl.GetCellValueAsString(i, 1);
                        var fileMAVT = sl.GetCellValueAsString(i, 2);
                        var fileSoluong = sl.GetCellValueAsString(i, 3).ToUpper().Trim();
                        var fileMAPHIEUPD = sl.GetCellValueAsString(i, 4).ToUpper().Trim();
                        var fileGHICHU = sl.GetCellValueAsString(i, 5).ToUpper().Trim();
         

                    //    var f_COA = COA.FirstOrDefault();
                        if (!string.IsNullOrEmpty(fileMAVT))
                        {
                            #region Kiểm tra dữ liệu
                            
                            if (string.IsNullOrEmpty(fileSOCTXN))
                            {
                                return Json((new { status = -1, title = "", text = string.Format("SOCTXN dòng {0} : " + fileSOCTXN + " không được bỏ trống.", numRecord), obj = "" }), JsonRequestBehavior.AllowGet);
                            }
                            if (string.IsNullOrEmpty(fileMAVT))
                            {
                                return Json((new { status = -1, title = "", text = string.Format("Mã vật tư dòng {0} : " + fileMAVT + " không được bỏ trống.", numRecord), obj = "" }), JsonRequestBehavior.AllowGet);
                            }
                            if (string.IsNullOrEmpty(fileSoluong))
                            {
                                return Json((new { status = -1, title = "", text = string.Format("Loại vật tư dòng {0} : " + fileSoluong + " không được bỏ trống.", numRecord), obj = "" }), JsonRequestBehavior.AllowGet);
                            }
                            if (dmxuatnhap.LOAIXN == "NHTA" && Convert.ToDecimal(fileSoluong) < 0)
                            {
                                return Json((new { status = -1, title = "", text = string.Format("Số lượng phiếu NHTA dòng {0} : " + fileSoluong + " không được  nhỏ hơn 0, (nhập số dương).", numRecord), obj = "" }), JsonRequestBehavior.AllowGet);
                            }
                            if (dmxuatnhap.LOAIXN == "NHGA" && Convert.ToDecimal(fileSoluong) > 0)
                            {
                                return Json((new { status = -1, title = "", text = string.Format("Số lượng phiếu NHGA dòng {0} : " + fileSoluong + " không được  lớn hơn 0, (nhập số âm).", numRecord), obj = "" }), JsonRequestBehavior.AllowGet);
                            }
                            if (dmxuatnhap.GUIAPI == "1" )
                            {
                                return Json((new { status = -1, title = "", text = string.Format("Phiếu này đã được xem xét đồng bộ netsuite vui lòng kiểm tra lại", numRecord), obj = "" }), JsonRequestBehavior.AllowGet);
                            }
                            if (fileSOCTXN != SOCTXN)
                            {
                                return Json((new { status = -1, title = "", text = string.Format("SOCTXN file Excel khác với SOCTXN phiếu hiện tại. Kiểm tra lại", numRecord), obj = "" }), JsonRequestBehavior.AllowGet);
                            }
                            #endregion

                            #region Kiểm tra trùng trong excel
                            if (list_XUATNHAP.FirstOrDefault(p => (p.MAVT == fileMAVT)) != null)
                            {
                                return Json((new { status = -1, title = "", text = string.Format(" MAVT dòng {0} : " + fileMAVT + " trùng trong file. Vui lòng kiểm tra lại.", numRecord), obj = "" }), JsonRequestBehavior.AllowGet);
                            }
                            #endregion
                            #region Kiểm tra cập nhật hay insert

                            if (vt_.XUATNHAPs.FirstOrDefault(c => c.MAVT == fileMAVT && c.SoCTXN == SOCTXN) == null)
                            {
                                var model_copy = new XUATNHAP();
                                {
                                    #region Insert XUATNHAP                                  
                                    model_copy.SoCTXN = SOCTXN.Trim();
                                    model_copy.MAVT = fileMAVT.Trim();
                                    model_copy.LoaiXN = dmxuatnhap.LOAIXN.Trim();
                                    model_copy.MaPhieuPD = (fileMAPHIEUPD == null || fileMAPHIEUPD == "") ? (kyhieu + "_" + SOCTXN.ToUpper().Trim()) : fileMAPHIEUPD.Trim().ToUpper();
                                    model_copy.SoLuongYC = Convert.ToDecimal(fileSoluong);
                                    model_copy.SoLuongTT = Convert.ToDecimal(fileSoluong);
                                    model_copy.TongGia = Convert.ToDecimal(0.00);
                                    model_copy.MaKhoXuat = "";
                                    model_copy.MaKho = dmxuatnhap.MAKHONHAN.Trim();
                                    model_copy.GHICHU = (fileGHICHU == null || fileGHICHU == "") ? "" : fileGHICHU;
                                    model_copy.MaNVYC = User.UserName;
                                    model_copy.MANVCN = User.UserName;
                                    model_copy.Ngay = DateTime.Now;
                                    model_copy.NGAYCN = DateTime.Now;
                                    model_copy.NgayKeToan = dmxuatnhap.NGAY;
                                    var KeyKhoa = Guid.NewGuid();
                                    model_copy.KHOAKEYXN = dmxuatnhap.LOAIXN.Trim() + "_KeyKhoa_" + KeyKhoa;
                           
                                    #endregion
                                };
                                list_XUATNHAP.Add(model_copy);
                            }
                            #endregion
                            isHaveData = true;
                            numRecord++;
                        }
                    }
                    if (isHaveData)
                    {
                        vt_.XUATNHAPs.AddRange(list_XUATNHAP);
                        vt_.SaveChanges();
                        return Json((new { status = 1, title = "", text = string.Format("Import thành công {0} dòng.", numRecord - 1), obj = "" }), JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json((new { status = -1, title = "", text = "File excel không có dữ liệu.", obj = "" }), JsonRequestBehavior.AllowGet);
                    }
                    #endregion
                }
                else
                {
                    return Json((new { status = -1, title = "", text = "Vui lòng chọn file để import.", obj = "" }), JsonRequestBehavior.AllowGet);
                }
                #endregion
            }
            catch (DbEntityValidationException e)
            {
                string T = "";

                foreach (var eve in e.EntityValidationErrors)
                {
                    foreach (var ve in eve.ValidationErrors)
                    {
                        T += ve.PropertyName + "|" + ve.ErrorMessage;
                    }
                }

                return Json(new { status = -1, title = "", text = "Lỗi: " + T, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #endregion
    }
}