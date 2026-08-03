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

namespace ToolsApp.Controllers
{
    [Authorize]
    public class PhieuNhapKhoController : BaseController
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
            var List = soi_.SP_GETLIST_NL_DMPHIEU().Where(p =>
           (
               (SOCTXNSearch == null || SOCTXNSearch == "" || p.SOCTXN.Contains(SOCTXNSearch))
               && p.MaNVYC == User.UserName

           )).OrderByDescending(p => p.NGAY).OrderByDescending(c => c.STT).ToList();

            ViewBag.List = List;

            return PartialView();
        }
        #endregion

        #region Load View _DienGiaiView
        public ActionResult _DienGiaiView(string SOCTXN, string KHOAKEYXN, string MAVT)
        {
            var List = soi_.NL_CTXUATNHAP.Where(p => p.SOCTXN == SOCTXN && p.KHOAKEYXN == KHOAKEYXN).ToList();
            ViewBag.List = List;
            var diengiai = vt_.NHAPDIENGIAIs.Where(p => p.SOCTXN == SOCTXN && p.MAVT == MAVT).ToList();
            ViewBag.diengiai = diengiai;
            var dvt = vt_.tblDMDVTINHs.ToList();
            ViewBag.dvt = dvt;
            return PartialView("_DienGiaiView", new XUATNHAPViewModels { });
        }
        #endregion

        #region Load View Update _UpdateCTPhieu_NK  _GetList_DienGiai
        public ActionResult _GetList_DienGiai(string SOCTXN_DG, string MAVT_DG, string KHOAKEYXN_DG)
        {
            var List = soi_.NHAPDIENGIAI_SOI.Where(p => p.MAVT == MAVT_DG && p.SOCTXN == SOCTXN_DG && p.KHOAKEYXN == KHOAKEYXN_DG).ToList();
            ViewBag.List = List;

            return PartialView("_GetList_DienGiai");
        }
        #endregion

        #region Load View CTphieu _
        public ActionResult _GetListCTPhieu_View(string SOCTXN, string KEY)
        {
            var dmphieu = soi_.NL_DMPHIEUXN.Where(p => p.SOCTXN == SOCTXN).ToList();

            var List = soi_.NL_CTXUATNHAP.Where(p => p.SOCTXN == SOCTXN).ToList();

            var listMAVT = List
            .Where(x => !string.IsNullOrEmpty(x.MAVT))
            .Select(x => x.MAVT)
            .Distinct()
            .ToList();
            var dmvtDict = soi_.TBLDMNGUYENLIEUx
       .Where(x => listMAVT.Contains(x.MAVATTU))
       .Select(x => new { x.MAVATTU, x.Tenvattu })
       .ToDictionary(x => x.MAVATTU, x => x.Tenvattu);

            ViewBag.DMVT_DICT = dmvtDict;

            ViewBag.List = List;
            ViewBag.dmphieu = dmphieu;
            //ViewBag.MAHTHUC = dmphieu.FirstOrDefault().MAHTHUC;

            return PartialView("_GetListCTPhieu_View", new NL_CTXUATNHAPViewModels { SOCTXN = SOCTXN });
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
                var item = soi_.NL_CTXUATNHAP.FirstOrDefault(P => P.SOCTXN == models.SOCTXN && P.MAVT == models.MAVT && P.KHOAKEYXN == models.KHOAKEYXN);
                var tb = soi_.NHAPDIENGIAI_SOI.FirstOrDefault(P => P.SOCTXN == models.SOCTXN && P.MAVT == models.MAVT && P.KHOAKEYXN == models.KHOAKEYXN);
                if (tb == null)
                {
                    #region Insert DM_XUATNHAP
                    var model_copy = new NHAPDIENGIAI_SOI();
                    model_copy.SOCTXN = models.SOCTXN.Trim();
                    model_copy.MAVT = models.MAVT.Trim();
                    model_copy.HIEU = item.HIEU.Trim();
                    model_copy.LO = item.LO.Trim();
                    model_copy.SOCTKETOAN = models.SOCTKETOAN.Trim();
                    model_copy.TENVATTU_NCC = models.TENVATTU_NCC.Trim();
                    model_copy.SOLUONG_NCC = models.SOLUONG_NCC;
                    model_copy.DVT_NCC = models.DVT_NCC;
                    model_copy.MAPHIEUPD = models.MAPHIEUPD;
                    model_copy.NGAY_NCC = NGAY_NCC;
                    var KeyKhoa = Guid.NewGuid();
                    model_copy.KHOAKEYXN = models.KHOAKEYXN;
                    model_copy.KEYNHAP = KeyKhoa.ToString();
                    soi_.NHAPDIENGIAI_SOI.Add(model_copy);
                    soi_.SaveChanges();
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

        #region GET_MAPHIEUPD_BY_RADCOMBOBOXMAVATTU Combobox
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

        #region Load View _GetListCTPhieu
        public ActionResult _GetListCTPhieu(string SOCTXN, string MAHTHUC)
        {

            var APIs = soi_.NL_DMPHIEUXN.Where(c => c.SOCTXN == SOCTXN).ToList();
            var CTPHIEU = soi_.NL_CTXUATNHAP.Where(c => c.SOCTXN == SOCTXN).ToList();
            ViewBag.APIs = APIs;
            var dmphieuxn = APIs.FirstOrDefault();
            var masoi = soi_.LOAD_DATA_MASOI_NHAPKHO_NKNB_XBAN_XGC_v1(dmphieuxn.MAKHONHAN, MAHTHUC).ToList();
            ViewBag.masoi = masoi;
            var kethua_soctxn = soi_.SOI2024_SPLOAD_XVCNB_KETHUA_SOCTXN(dmphieuxn.MAKHOXUAT).ToList();
            ViewBag.kethua_soctxn = kethua_soctxn;
            ViewBag.SOCTXN_nhap = SOCTXN;
            ViewBag.MAKHONHAN = dmphieuxn.MAKHONHAN == null ? dmphieuxn.MAKHO : dmphieuxn.MAKHONHAN;
            ViewBag.MAKHOXUAT = dmphieuxn.MAKHOXUAT == null ? dmphieuxn.MAKHO : dmphieuxn.MAKHOXUAT;
            var DMLO = soi_.SP_LOAD_NL_DMLO_BY_HIEU("").ToList();
            ViewBag.DMLO = DMLO;
            var NL_DMNHACCAP = soi_.NL_DMNHACCAP.ToList();
            ViewBag.NL_DMNHACCAP = NL_DMNHACCAP;
            var xuat3buoc = soi_.NL_DMHINHTHUC.FirstOrDefault(p => p.MAHTHUC == MAHTHUC).XUAT3BUOC;

    


            dynamic MAHINHTHUC = MAHTHUC;
            if (MAHTHUC == "HK_XCHUYEN" || MAHTHUC == "HK_XBAN" || MAHTHUC == "HKGC")
            {
                MAHINHTHUC = "HKGC";
                var SOPHIEU_HKGC = soi_.VATTU2024_SP_LOAD_SOPHIEU_SOIGIACONG_CHUAHOIKHO(dmphieuxn.MAKHONHAN, "").ToList();
                if (CTPHIEU.Count > 0)
                {
                    SOPHIEU_HKGC = soi_.VATTU2024_SP_LOAD_SOPHIEU_SOIGIACONG_CHUAHOIKHO("", "").ToList();
                }
                ViewBag.SOPHIEU_HKGC = SOPHIEU_HKGC;
            }
            if (MAHTHUC == "HK")
            {
                MAHINHTHUC = "HK";
                var Masoi_hknvl = soi_.VATTU2024_SPLOAD_DMNGUYENLIEUSOI().ToList();
                ViewBag.Masoi_hknvl = Masoi_hknvl;
            }

            if (xuat3buoc == "0")
            {
                MAHINHTHUC = "NDC";
                var Masoi_hknvl = soi_.VATTU2024_SPLOAD_DMNGUYENLIEUSOI().ToList();
                ViewBag.Masoi_hknvl = Masoi_hknvl;
                ViewBag.nguoixx = vt_.SP_LoadXemXetBM03TTKD(User.UserName).ToList();
            }

            ViewBag.MAHTHUC = MAHINHTHUC;
            return PartialView("_GetListCTPhieu_" + MAHINHTHUC, new NL_CTXUATNHAPViewModels { SOCTXN = SOCTXN });
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
                    var dm_nhap = soi_.NL_DMPHIEUXN.FirstOrDefault(c => c.SOCTXN == SOCTXN_Nhap);
                    var dm_xuat = soi_.NL_DMPHIEUXN.FirstOrDefault(c => c.SOCTXN == SOCTXN_Xuat);
                    var ct_xuat = soi_.NL_CTXUATNHAP.Where(p => p.SOCTXN == SOCTXN_Xuat).ToList();
                    var ct_nhap = soi_.NL_CTXUATNHAP.Where(p => p.SOCTXN == SOCTXN_Nhap).ToList();

                    if (dm_nhap != null)
                    {
                        foreach (var model in ct_xuat)
                        {

                            #region Check tồn tại
                            if (ct_nhap.FirstOrDefault(p => p.MAVT == model.MAVT && p.SOCTXN == SOCTXN_Nhap) != null)
                            {
                                return Json(new { status = -2, title = "", text = "Thêm không thành công. Mã VT đã tồn tại. Kiểm tra lại.", obj = "" }, JsonRequestBehavior.AllowGet);
                            }
                            #endregion
                            var dmnhanvien = vt_.DMNHANVIENs.Where(p => p.MANV == User.UserName).FirstOrDefault();

                            var model_item = new EntityFramework.KhoSoi.NL_CTXUATNHAP();
                            model_item.SOCTXN = SOCTXN_Nhap.Trim();
                            model_item.MAVT = model.MAVT;
                            model_item.MAVTTAM = model.MAVTTAM;
                            model_item.HIEU = model.HIEU;
                            model_item.LO = model.LO;
                            model_item.MAVT = model.MAVT;
                            model_item.MADH = model.MADH;
                            model_item.SOLUONGYC = Convert.ToDecimal(model.SOLUONGYC);
                            model_item.SOLUONGTT = Convert.ToDecimal(model.SOLUONGTT);
                            model_item.MAPHIEUPD = model.MAPHIEUPD;
                            model_item.SOHOADON = model.SOHOADON == null ? "" : model.SOHOADON.Trim();
                            model_item.GHICHU = (model.GHICHU == null || model.GHICHU == "") ? "Kế thừa từ " + SOCTXN_Xuat : model.GHICHU;
                            model_item.NGAYKETOAN = (DateTime)dm_nhap.NGAY;
                            var KeyKhoa = Guid.NewGuid();
                            model_item.KHOAKEYXN = dm_nhap.MAHTHUC.Trim() + "_KeyKhoa_" + KeyKhoa;
                            model_item.ID_XN = KeyKhoa;
                            soi_.NL_CTXUATNHAP.Add(model_item);
                            soi_.SaveChanges();
                        }
                    }
                    else
                    {
                        return Json(new { status = -2, title = "", text = "Thêm không thành công. Dữ liệu không tồn có.  Kiểm tra lại.", obj = "" }, JsonRequestBehavior.AllowGet);
                    }
                    var kq = soi_.VATTU2026_UPDATE_STATUS_NL_DMPHIEUXN(SOCTXN_Xuat, SOCTXN_Nhap).ToList();
                    if (kq.Count < 1)
                    {
                        return Json(new { status = -2, title = "", text = "Thêm không thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
                    }
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
                //try
                //{
                var dm_nhap = soi_.NL_DMPHIEUXN.FirstOrDefault(c => c.SOCTXN == SOCTXN_HKGC);
                // var dm_xuat = soi_.NL_DMPHIEUXN.FirstOrDefault(c => c.SOCTXN == SOCTXN_Xuat);
                var ct_bangke = soi_.VATTU2024_SP_LOAD_DATA_SOIGIACONG_CHUAHOIKHO(dm_nhap.MAKHONHAN, SOPHIEU_HKGC).ToList();
                var ct_nhap = soi_.NL_CTXUATNHAP.Where(p => p.SOCTXN == SOCTXN_HKGC).ToList();


                if (dm_nhap != null)
                {
                    foreach (var model in ct_bangke)
                    {

                        //#region Check tồn tại
                        //if (ct_nhap.FirstOrDefault(p => p.MAVT == model.MAVT && p.SOCTXN == SOCTXN_HKGC && p.LO == model.LO) != null)
                        //{
                        //    return Json(new { status = -2, title = "", text = "Thêm không thành công. Mã VT đã tồn tại. Kiểm tra lại.", obj = "" }, JsonRequestBehavior.AllowGet);
                        //}
                        //#endregion


                        var dmnhanvien = vt_.DMNHANVIENs.Where(p => p.MANV == User.UserName).FirstOrDefault();

                        var model_item = new EntityFramework.KhoSoi.NL_CTXUATNHAP();
                        model_item.SOCTXN = SOCTXN_HKGC.Trim();
                        model_item.MAVT = model.MAVT;
                        model_item.MAVTTAM = model.MAVT;
                        model_item.HIEU = model.HIEU;
                        model_item.LO = model.LO;
                        model_item.MAVT = model.MAVT;
                        model_item.MADH = SOPHIEU_HKGC;
                        model_item.MANVMODIFY = User.UserName;
                        model_item.NGAYMODIFY = DateTime.Now;
                        model_item.MADH = SOPHIEU_HKGC;
                        model_item.SOLUONGYC = Convert.ToDecimal(model.SOLUONGTT);
                        model_item.SOLUONGTT = Convert.ToDecimal(model.SOLUONGTT); 
                        model_item.MAPHIEUPD = model.MAPHIEUPD;
                        model_item.KHOATHAMCHIEU = model.KHOATHAMCHIEU;
                        model_item.SOHOADON = "";
                        model_item.DAXUATKHO = 0;
                        model_item.MADINHMUC = SOPHIEU_HKGC;
                        model_item.NK_STATIC = true;
                        model_item.GHICHU = "Kế thừa từ số phiếu bảng kê: " + SOPHIEU_HKGC;
                        model_item.NGAYKETOAN = (DateTime)dm_nhap.NGAY;
                        var KeyKhoa = Guid.NewGuid();
                        model_item.KHOAKEYXN = dm_nhap.MAHTHUC.Trim() + "_KeyKhoa_" + KeyKhoa;
                        model_item.ID_XN = KeyKhoa;
                        soi_.NL_CTXUATNHAP.Add(model_item);
                        soi_.SaveChanges();
                    }

                }
                else
                {
                    return Json(new { status = -2, title = "", text = "Thêm không thành công. Dữ liệu không tồn có.  Kiểm tra lại.", obj = "" }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { status = 1, title = "", text = "Thêm thành công.", obj = "" }, JsonRequestBehavior.AllowGet);

                //}

                //catch (Exception ex)
                //{
                //    return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
                //}
            }
            else
            {
                return Json(new { status = -2, title = "", text = "Lưu không thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Load View Thêm mới
        public ActionResult _Insert()
        {

            var makhoxuat = soi_.SP_MAKHOBYMANV_LOAD(User.UserName).ToList();
            ViewBag.makhoxuat = makhoxuat;
            var makhonhan = soi_.SP_MAKHOBYMANV_LOAD(User.UserName).ToList();
            ViewBag.makhonhan = makhonhan;
            var masocp = vt_.SP_NL_LOADMSCP().ToList();
            ViewBag.masocp = masocp;
            var loaihinhthuc = soi_.SP_NL_LOADDMHINHTHUC_NHAPKHOTHUKHO().ToList();
            ViewBag.loaihinhthuc = loaihinhthuc;
            var SOCTXN = soi_.SP_NL_TAOSOCHUNGTU(loaihinhthuc.FirstOrDefault().MAHTHUC).FirstOrDefault();
            ViewBag.SOCTXN = SOCTXN;
            var dmdonvi = vt_.VATTU2024_SP_LOAD_QUYEN_SUDUNG_DMDONVI(User.UserName).ToList();
            var f_iddonvi = dmdonvi.FirstOrDefault().IDMaDV;
            ViewBag.DMDONVI = dmdonvi;
            ViewBag.manvnhan = vt_.DMNHANVIENs.Where(p => p.IDMaDV == f_iddonvi && p.HieuLuc == 1).ToList();

            return PartialView("_Insert", new NL_DMPHIEUXNViewModels { /*Sophieu = Sophieu.FirstOrDefault().Sophieu*/ });
        }
        #endregion

        #region OnChange
        public JsonResult _Change_SOCTXN(string MAHTHUCid)
        {
            try
            {
                var SOCTXN = soi_.SP_NL_TAOSOCHUNGTU(MAHTHUCid).FirstOrDefault();

                return Json(new { status = 1, title = "", text = "", obj = SOCTXN }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { status = -1, title = "", text = e.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult _Onchange_Location_MANV(string IDMADVNHAN)
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

        public JsonResult _Load_DMLO(string HIEUid = null, string MAVTid = null)
        {
            try
            {
                dynamic DMLO = "";
                if (HIEUid != null)
                {
                    DMLO = soi_.SP_LOAD_NL_DMLO_BY_HIEU(HIEUid).ToList();
                }
                dynamic MADINHMUCID = "";
                if (MAVTid != null || MAVTid != "")
                {
                    MADINHMUCID = soi_.SP_LOAD_MADINHMUC_TUBANGKEGCSOI(MAVTid).Select(p => p.SOPHIEU).Distinct().ToList();
                }

                return Json(new { status = 1, title = "", text = "", obj = DMLO, objdm = MADINHMUCID }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { status = -1, title = "", text = e.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Load View Update _UpdateCTPhieu_NK 
        public ActionResult _UpdateCTPhieu_NK(Guid ID_XN, string SOCTXN)
        {
            var ctphieu = soi_.NL_CTXUATNHAP.FirstOrDefault(p => p.ID_XN == ID_XN && p.SOCTXN == SOCTXN);
            var dmphieu = soi_.NL_DMPHIEUXN.Where(p => p.SOCTXN == SOCTXN).FirstOrDefault();

            ViewBag.ctphieu = ctphieu;
            ViewBag.dmphieu = dmphieu;
            return PartialView("_UpdateCTPhieu_NK", new NL_CTXUATNHAPViewModels { ID_XN = ID_XN });
        }
        #endregion

        #region Update CT phiếu NK //ok
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _ActionUpdateCTPhieu_NK(NL_CTXUATNHAPViewModels model)
        {
            //try
            //{
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
            var dmxuatnhap = soi_.NL_DMPHIEUXN.FirstOrDefault(p => p.SOCTXN == model.SOCTXN);
            var f_LOAIXN = dmxuatnhap.MAHTHUC;
            var f_SOCTXN = model.SOCTXN;
            var item = soi_.NL_CTXUATNHAP.FirstOrDefault(p => p.ID_XN == model.ID_XN && p.SOCTXN == model.SOCTXN);
            if (item == null)
            {
                return Json(new { status = -1, title = "", text = "Không tìm thấy chi tiết phiếu.", obj = "" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                item.SOHOADON = model.SOHOADON == null ? "" : model.SOHOADON;
                item.SOLUONGTT = Convert.ToDecimal(model.SOLUONGTT);
                item.NGAYKETOAN = (DateTime)model.NGAYCT;
                item.GHICHU = model.GHICHU;
                item.LO = model.LO;
                item.HIEU = model.HIEU;
                soi_.Entry(item).State = EntityState.Modified;
                soi_.SaveChanges();
                return Json(new { status = 1, title = "", text = "Cập nhật thành công.", obj = f_SOCTXN, obj2 = f_LOAIXN }, JsonRequestBehavior.AllowGet);
            }

            //}
            //catch (Exception ex)
            //{
            //    return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            //}
        }
        #endregion

        #region Insert
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _InsertFun(NL_DMPHIEUXNViewModels model)
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

            var tb = soi_.NL_DMPHIEUXN.FirstOrDefault(p => p.SOCTXN == model.SOCTXN);

            if (tb == null)
            {
                #region Check tồn tại
                if (soi_.NL_DMPHIEUXN.FirstOrDefault(p => p.SOCTXN == model.SOCTXN) != null)
                {
                    return Json(new { status = -2, title = "", text = "Thêm không thành công. SOCTXN đã tồn tại. Kiểm tra lại.", obj = model.SOCTXN }, JsonRequestBehavior.AllowGet);
                }
                if (model.MAHTHUC == "" || model.MAKHONHAN == "" || model.MAKHOXUAT == "")
                {
                    return Json(new { status = -2, title = "", text = "Thêm không thành công. Phiếu không được trống. Kiểm tra lại.", obj = model.SOCTXN }, JsonRequestBehavior.AllowGet);
                }
                if (model.MAHTHUC == "NK" && model.SOPO.Trim() == "")
                {
                    return Json(new { status = -2, title = "", text = "Thêm không thành công. số PO không được trống. Kiểm tra lại.", obj = model.SOCTXN }, JsonRequestBehavior.AllowGet);
                }
                #endregion

                #region Add DM Phiếu
                var dmnhanvien = vt_.DMNHANVIENs.FirstOrDefault(p => p.MANV == User.UserName);
                var model_copy = new EntityFramework.KhoSoi.NL_DMPHIEUXN();
                model_copy.SOCTXN = model.SOCTXN.Trim();
                model_copy.MAHTHUC = model.MAHTHUC.Trim();
                model_copy.NGAY = model.NGAYCT;
                model_copy.IDMADVNHAN = model.IDMADVNHAN;
                model_copy.MAKHONHAN = model.MAKHONHAN;
                model_copy.MANVNHAN = model.MANVNHAN;
                model_copy.IDMADVXUAT = model.IDMADVXUAT;
                model_copy.MAKHOXUAT = model.MAKHOXUAT;
                model_copy.IDMADV = dmnhanvien.IDMaDV;
                model_copy.MABP = dmnhanvien.MADV;
                model_copy.SOPO = model.SOPO;
                model_copy.MAKHO = model.MAKHONHAN.Trim();
                model_copy.NOIDEN = "";
                model_copy.GHICHU = (model.GHICHU == null || model.GHICHU == "") ? "" : model.GHICHU;
                model_copy.MaNVYC = User.UserName;

                soi_.NL_DMPHIEUXN.Add(model_copy);
                soi_.SaveChanges();
                #endregion

                #region Netsuite
                //if (tb.Count > 0)
                //{
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

                if (model.MAHTHUC.Trim() == "NK")
                {
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
                        var tb_update = soi_.NL_DMPHIEUXN.FirstOrDefault(p => p.SOCTXN == model.SOCTXN);
                        tb_update.InternalidPO = internalidcreatedfrom;
                        soi_.Entry(tb_update).State = EntityState.Modified;
                        soi_.SaveChanges();
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

                                if (itemfields.customFieldList != null)
                                {
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
                                }

                                try
                                {
                                    dynamic ctphieumuahang_ = "";
                                    dynamic dmphieumuahang_ = "";
                                    var itemHieu = "";
                                    var madh = "";
                                    var LOAIDONHANG = "";
                                    if (soi_.TBLDMNGUYENLIEUx.FirstOrDefault(p => p.MAVATTU == itemname) == null)
                                    {
                                        return Json(new { status = -1, title = "", text = "MAVT không tồn tại", obj = "" }, JsonRequestBehavior.AllowGet);
                                    }

                                    if (dondenghimuahang != "")
                                    {
                                        ctphieumuahang_ = soi_.NL_CTPHIEU.Where(p => p.MAPHIEU == dondenghimuahang && p.MAVT == itemname).FirstOrDefault();
                                        if (ctphieumuahang_ == null)
                                        {
                                            return Json(new { status = -1, title = "", text = "Mã " + itemname + " này không tồn tại ở phiếu mua hàng (PR) " + dondenghimuahang + " đã khai báo, vui lòng kiểm tra lại ", obj = "" }, JsonRequestBehavior.AllowGet);
                                        }
                                        dmphieumuahang_ = soi_.NL_DMPHIEU.Where(p => p.MAPHIEU == dondenghimuahang).FirstOrDefault();
                                        itemHieu = ctphieumuahang_.HIEU;
                                        madh = ctphieumuahang_.MADH == null ? "" : ctphieumuahang_.MADH;
                                        LOAIDONHANG = dmphieumuahang_.LOAIDONHANG; 
                                    }

                                    var model_item = new EntityFramework.KhoSoi.NL_CTXUATNHAP();
                                    model_item.SOCTXN = model.SOCTXN.Trim();
                                    model_item.MAPHIEUPD = dondenghimuahang == "" ? model.SOPO : dondenghimuahang;
                                    model_item.MADH = madh;
                                    model_item.MAVT = itemname;
                                    model_item.HIEU = itemHieu;
                                    model_item.LO = "";
                                    model_item.RatePO = Convert.ToDecimal(rate); 
                                    model_item.DVT_TIEN = Currency;
                                    model_item.MAVTTAM = itemname;
                                    model_item.NK_STATIC = false;
                                    model_item.LOAIDONHANG = LOAIDONHANG;
                                    model_item.SOLUONGYC = Convert.ToDecimal(soluong);
                                    model_item.SOLUONGTT = Convert.ToDecimal(soluong);
                                    model_item.DonGia = Convert.ToDecimal(0.00);
                                    model_item.GHICHU = (model.GHICHU == null || model.GHICHU == "") ? "" : model.GHICHU;
                                    model_item.NGAYKETOAN = (DateTime)model.NGAYCT;

                                    var KeyKhoa = Guid.NewGuid();
                                    model_item.KHOAKEYXN = itemname + "||" + model.MAHTHUC.Trim() + "_KeyKhoa_" + KeyKhoa;
                                    model_item.ID_XN = KeyKhoa;
                                    soi_.NL_CTXUATNHAP.Add(model_item);
                                    soi_.SaveChanges();
                                }
                                catch (Exception ex)
                                {
                                    var messageERR = ex.InnerException.InnerException.Message;
                                    return Json(new { status = -1, title = "", text = messageERR, obj = "" }, JsonRequestBehavior.AllowGet);
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
            return Json(new { status = -2, title = "", text = "Thêm không thành công.", obj = "" }, JsonRequestBehavior.AllowGet);           
           
        }
        #endregion

        #region Insert CT  _InsertFunCTPhieuXN
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _InsertFunCTPhieuXN(NL_CTXUATNHAPViewModels model)
        {
            //if (ModelState.IsValid)
            //{

            #region Xử lý ngày
            CultureInfo cul = CultureInfo.GetCultureInfo("en-GB");
            var NgayLapTuSearch_ = new DateTime();
            if (!string.IsNullOrEmpty(model.NGAYKETOAN_))
            {
                try
                {
                    NgayLapTuSearch_ = DateTime.ParseExact(model.NGAYKETOAN_, "dd/MM/yyyy", cul);

                    model.NGAYKETOAN = new DateTime(NgayLapTuSearch_.Year,
                        NgayLapTuSearch_.Month, NgayLapTuSearch_.Day, 00, 00, 01);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
            #endregion
            try
            {
                var tb = soi_.NL_DMPHIEUXN.Where(p => p.SOCTXN == model.SOCTXN).ToList();
                
                if (tb.Count > 0)
                {
                    #region Check tồn tại
                    if (tb.FirstOrDefault().GUIAPI == "1")
                    {
                        return Json(new { status = -2, title = "", text = "Thêm không thành công. Phiếu đã được đồng bộ netsuite, không thể thêm mới, Kiểm tra lại.", obj = "" }, JsonRequestBehavior.AllowGet);
                    }
                    if (model.SOLUONGTT == 0)
                    {
                        return Json(new { status = -2, title = "", text = "Thêm không thành công. Số lượng phải lơn hơn 0. Kiểm tra lại.", obj = "" }, JsonRequestBehavior.AllowGet);
                    }
                    #endregion
                    var model_copy = new EntityFramework.KhoSoi.NL_CTXUATNHAP(); ;
                    var ID_XN = Guid.NewGuid();
                    model_copy.KHOAKEYXN = model.MAVT + "||" + model.HIEU + "||" + model.LO + "||" + model.MAPHIEUPD + "||" + model.MAHTHUC + "||" + ID_XN;
                    model_copy.MAVT = model.MAVT.Trim().ToUpper();
                    model_copy.HIEU = model.HIEU;
                    model_copy.LO = model.LO;
                    model_copy.SOLUONGYC = Convert.ToDecimal(model.SOLUONGTT);
                    model_copy.SOLUONGTT = Convert.ToDecimal(model.SOLUONGTT);
                    model_copy.MAPHIEUPD = model.MAPHIEUPD;
                    model_copy.GHICHU = model.GHICHU;
                    model_copy.NGAYKETOAN = (DateTime)model.NGAYKETOAN;
                    model_copy.SOCTXN = model.SOCTXN;
                    model_copy.MAVTTAM = model.MAVT;
                    model_copy.KHOATHAMCHIEU = ID_XN.ToString();
                    model_copy.ID_XN = ID_XN;
                    model_copy.LOAIDONHANG = model.LOAIDONHANG;
                    model_copy.NK_STATIC = true;
                    model_copy.DAXUATKHO = 0;
                    model_copy.GUIAPI = "0";
                    model_copy.MADINHMUC = model.MADINHMUC;
                    soi_.NL_CTXUATNHAP.Add(model_copy);
                    soi_.SaveChanges();
                }
                else
                {
                    return Json(new { status = -2, title = "", text = "Thêm không thành công. Dữ liệu không tồn có.  Kiểm tra lại.", obj = "" }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { status = 1, title = "", text = "Thêm thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
            }

            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
            //}
            //else
            //{
            //    return Json(new { status = -2, title = "", text = "Lưu không thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
            //}
        }
        #endregion

        #region Insert CT  _InsertFunCTPhieu_HK
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _InsertFunCTPhieu_HK(NL_CTXUATNHAPViewModels model)
        {
            //if (ModelState.IsValid)
            //{

            #region Xử lý ngày
            CultureInfo cul = CultureInfo.GetCultureInfo("en-GB");
            var NgayLapTuSearch_ = new DateTime();
            if (!string.IsNullOrEmpty(model.NGAYKETOAN_))
            {
                try
                {
                    NgayLapTuSearch_ = DateTime.ParseExact(model.NGAYKETOAN_, "dd/MM/yyyy", cul);

                    model.NGAYKETOAN = new DateTime(NgayLapTuSearch_.Year,
                        NgayLapTuSearch_.Month, NgayLapTuSearch_.Day, 00, 00, 01);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
            #endregion
            //try
            //{
            var tb = 1;

            if (tb > 0)
            {
                #region Check tồn tại
                if (model.SOLUONGTT == 0)
                {
                    return Json(new { status = -2, title = "", text = "Thêm không thành công. Số lượng phải lơn hơn 0. Kiểm tra lại.", obj = "" }, JsonRequestBehavior.AllowGet);
                }

                #endregion

                var model_copy = new EntityFramework.KhoSoi.NL_CTXUATNHAP();
                var ID_XN = Guid.NewGuid();
                model_copy.KHOAKEYXN = model.MAVT + "||" + model.HIEU + "||" + model.LO + "||" + model.MAPHIEUPD + "||" + model.MAHTHUC + "||" + ID_XN;
                model_copy.MAVT = model.MAVT.Trim().ToUpper();
                model_copy.HIEU = model.HIEU;
                model_copy.LO = model.LO == null ? "" : model.LO;
                model_copy.SOLUONGYC = Convert.ToDecimal(model.SOLUONGTT);
                model_copy.SOLUONGTT = Convert.ToDecimal(model.SOLUONGTT);
                model_copy.MAPHIEUPD = model.SOCTXN;
                model_copy.GHICHU = model.GHICHU;
                model_copy.NGAYKETOAN = (DateTime)model.NGAYKETOAN;
                model_copy.SOCTXN = model.SOCTXN;
                model_copy.MAVTTAM = model.MAVT;
                model_copy.KHOATHAMCHIEU = ID_XN.ToString();
                model_copy.ID_XN = ID_XN;
                model_copy.LOAIDONHANG = model.LOAIDONHANG;
                model_copy.NK_STATIC = true;
                model_copy.DAXUATKHO = 0;
                model_copy.MADINHMUC = model.MADINHMUC;
                soi_.NL_CTXUATNHAP.Add(model_copy);
                soi_.SaveChanges();
            }
            else
            {
                return Json(new { status = -2, title = "", text = "Thêm không thành công. Dữ liệu không tồn có.  Kiểm tra lại.", obj = "" }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { status = 1, title = "", text = "Thêm thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
            //}
            //catch (Exception ex)
            //{
            //    return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            //}
            //}
            //else
            //{
            //    return Json(new { status = -2, title = "", text = "Lưu không thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
            //}
        }
        #endregion

        #region Insert CT  _InsertFunCTPhieu_NDC
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _InsertFunCTPhieu_NDC(NL_CTXUATNHAPViewModels model)
        {
            //if (ModelState.IsValid)
            //{

            #region Xử lý ngày
            CultureInfo cul = CultureInfo.GetCultureInfo("en-GB");
            var NgayLapTuSearch_ = new DateTime();
            if (!string.IsNullOrEmpty(model.NGAYKETOAN_))
            {
                try
                {
                    NgayLapTuSearch_ = DateTime.ParseExact(model.NGAYKETOAN_, "dd/MM/yyyy", cul);

                    model.NGAYKETOAN = new DateTime(NgayLapTuSearch_.Year,
                        NgayLapTuSearch_.Month, NgayLapTuSearch_.Day, 00, 00, 01);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
            #endregion
            try
            {
                #region Check tồn tại
                if (model.SOLUONGTT == 0)
                {
                    return Json(new { status = -2, title = "", text = "Thêm không thành công. Số lượng phải lơn hơn 0. Kiểm tra lại.", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                if (model.MAHTHUC == "NHTA" && Convert.ToDecimal(model.SOLUONGTT) < 0)
                {
                    return Json(new { status = -2, title = "", text = "Thêm không thành công. Số lượng phải lơn hơn 0. Kiểm tra lại.", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                if (model.MAHTHUC == "NHGA" && Convert.ToDecimal(model.SOLUONGTT) > 0)
                {
                    return Json(new { status = -2, title = "", text = "Thêm không thành công. Số lượng phải nhỏ hơn 0. Kiểm tra lại.", obj = "" }, JsonRequestBehavior.AllowGet);
                }

                #endregion
                var dmphieu = soi_.NL_DMPHIEUXN.FirstOrDefault(p => p.SOCTXN == model.SOCTXN);
                var kyhieu = vt_.DMDONVIs.FirstOrDefault(p => p.IDMaDV == dmphieu.IDMADVNHAN).KiHieu.Trim().ToUpper();
                var model_copy = new EntityFramework.KhoSoi.NL_CTXUATNHAP(); ;
                var ID_XN = Guid.NewGuid();
                model_copy.KHOAKEYXN = model.MAVT + "||" + model.HIEU + "||" + model.LO + "||" + model.MAPHIEUPD + "||" + model.MAHTHUC + "||" + ID_XN;
                model_copy.MAVT = model.MAVT.Trim().ToUpper();
                model_copy.HIEU = model.HIEU;
                model_copy.LO = model.LO == null ? "" : model.LO;
                model_copy.SOLUONGYC = Convert.ToDecimal(model.SOLUONGTT);
                model_copy.SOLUONGTT = Convert.ToDecimal(model.SOLUONGTT);
                model_copy.MAPHIEUPD = (model.MAPHIEUPD == null || model.MAPHIEUPD == "") ? (kyhieu.ToUpper() + "_" + model.SOCTXN) : model.MAPHIEUPD;
                model_copy.GHICHU = model.GHICHU;
                model_copy.NGAYKETOAN = (DateTime)model.NGAYKETOAN;
                model_copy.SOCTXN = model.SOCTXN;
                model_copy.MAVTTAM = model.MAVT;
                model_copy.KHOATHAMCHIEU = ID_XN.ToString();
                model_copy.ID_XN = ID_XN;
                model_copy.LOAIDONHANG = model.LOAIDONHANG;
                model_copy.NK_STATIC = true;
                model_copy.DAXUATKHO = 0;
                model_copy.MADINHMUC = model.MADINHMUC;
                soi_.NL_CTXUATNHAP.Add(model_copy);
                soi_.SaveChanges();
                return Json(new { status = 1, title = "", text = "Thêm thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
            //}
            //else
            //{
            //    return Json(new { status = -2, title = "", text = "Lưu không thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
            //}
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
                if (soi_.NL_CTXUATNHAP.FirstOrDefault(p => p.SOCTXN == SOCTXN) != null)
                {
                    return Json(new { status = -2, title = "", text = "Xóa không thành công. Số phiếu còn tồn tại trong chi tiết. Vui lòng xóa chi tiết trước!.", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                #endregion

                var item = soi_.NL_DMPHIEUXN.FirstOrDefault(p => p.SOCTXN == SOCTXN);
                soi_.NL_DMPHIEUXN.Remove(item);
                soi_.SaveChanges();
                return Json(new { status = 1, title = "", text = "Xóa thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Delete ct
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _DeletectFun(Guid Khoa)
        {
            try
            {
                //    var manvhanhdong = db_.sp_update_manvhanhdong_ghilog(User.UserName, "", Khoa);


                var item = soi_.NL_CTXUATNHAP.FirstOrDefault(p => p.ID_XN == Khoa);


                soi_.NL_CTXUATNHAP.Remove(item);
                soi_.SaveChanges();
                return Json(new { status = 1, title = "", text = "Xóa thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
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
        public JsonResult _DeleteALLFun(string SOCTXN)
        {
            try
            {
                var item = soi_.NL_CTXUATNHAP.Where(p => p.SOCTXN == SOCTXN).ToList();


                foreach (var items in item)
                {
                    soi_.NL_CTXUATNHAP.Remove(items);
                }

                soi_.SaveChanges();
                return Json(new { status = 1, title = "", text = "Xóa tất cả thành công.", obj = SOCTXN }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
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
                var item = soi_.NHAPDIENGIAI_SOI.FirstOrDefault(p => p.KEYNHAP == KEYNHAP);
                soi_.NHAPDIENGIAI_SOI.Remove(item);
                soi_.SaveChanges();
                return Json(new { status = 1, title = "", text = "Xóa thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Print
        public ActionResult _ReportPrint(string SOCTXN)
        {
            var ct_Phieu = soi_.NL_CTXUATNHAP.FirstOrDefault(c => c.SOCTXN == SOCTXN);
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
                var CTPhieu = soi_.SP_RPT_NL_PYC_XUATVT(SOCTXN).ToList();
                if (CTPhieu == null)
                {
                    return Json(new { status = -1, text = "Không có chi tiết phiếu. Kiểm tra lại!", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    ViewBag.List = CTPhieu;
                }
            }
            return PartialView("_ReportPrint");
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
                    var soPO = soi_.NL_DMPHIEUXN.FirstOrDefault(p => p.SOCTXN == SOCTXN).SOPO;
                    var data = soi_.VATTU2025_LOAD_PRINT_DATA_NHAPDIENGIAI_SOI_SOCTXN(SOCTXN).Where(p => p.SOCTXN_A == SOCTXN).ToList();
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

        #region Đồng bộ (nhập kho)
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _TransAPI(string SOCTXN = null, string MAHINHTHUC = null)
        {
            if (ModelState.IsValid)
            {
                #region Xử lý dữ liệu 
                var dmphieuxn = soi_.NL_DMPHIEUXN.Where(p => p.SOCTXN == SOCTXN).ToList();
                var f_dmphieuxn = dmphieuxn.FirstOrDefault();
                var ct_phieuxn = soi_.NL_CTXUATNHAP.Where(p => p.SOCTXN == SOCTXN).ToList();

                var bophansx = nstt_.Donvi_mapping_Department.Where(p => p.mabp == f_dmphieuxn.MABP).ToList();
                var f_bophansx = bophansx.Count < 1 ? "" : bophansx.FirstOrDefault().externalid;
                var nhamaysx = nstt_.Khoes.Where(p => p.makho == f_dmphieuxn.MAKHONHAN).ToList();
                var f_nhamaysx = nhamaysx.Count < 1 ? "" : nhamaysx.FirstOrDefault().externalid;
                var key = soi_.NL_DMHINHTHUC.Where(p => p.MAHTHUC == MAHINHTHUC).FirstOrDefault();
                var xuat3buoc = soi_.NL_DMHINHTHUC.FirstOrDefault(p => p.MAHTHUC == MAHINHTHUC).XUAT3BUOC;

                var department = nstt_.Donvi_mapping_Department.Where(p => p.idmadv == f_dmphieuxn.IDMADVNHAN).ToList();
                if (f_dmphieuxn.IDMADVNHAN == "30" || f_dmphieuxn.IDMADVNHAN == "92")
                {
                    department = nstt_.Donvi_mapping_Department.Where(p => p.idmacs == f_dmphieuxn.IDMADVNHAN).ToList();
                }

                var kNhan = nstt_.Khoes.Where(p => p.makho == f_dmphieuxn.MAKHONHAN).ToList();
                var kXuat = nstt_.Khoes.Where(p => p.makho == f_dmphieuxn.MAKHOXUAT).ToList();

                if (kNhan.Count < 1)
                {
                    kNhan = nstt_.Khoes.Where(p => p.externalid == f_dmphieuxn.MAKHONHAN).ToList();

                }
                if (kXuat.Count < 1)
                {
                    kXuat = nstt_.Khoes.Where(p => p.externalid == f_dmphieuxn.MAKHOXUAT).ToList();

                }


                #endregion
                try
                {
                    #region
                    //if (tb.Count > 0)
                    //{
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

                    if (ns != null)
                    {
                        if (MAHINHTHUC == "NK")
                        {
                            #region Nhập kho mua ngoài IRR                     

                            #region Code
                            PurchaseOrder record = new PurchaseOrder();
                            PurchaseOrderItemList fields = (PurchaseOrderItemList)record.itemList;// list field trong 1 record

                            ItemReceiptItemList orderItemList = new ItemReceiptItemList();
                            List<ItemReceiptItem> orderItems = new List<ItemReceiptItem>();
                            CustomFieldRef[] cusDetail = new CustomFieldRef[100];

                            InitializeRef initializeRef = new InitializeRef()
                            {
                                internalId = f_dmphieuxn.InternalidPO,
                                type = InitializeRefType.purchaseOrder,
                                typeSpecified = true,
                            };

                            InitializeRecord initializeRecord = new InitializeRecord() { reference = initializeRef, type = InitializeType.itemReceipt };
                            ReadResponse read = ns.Service.initialize(initializeRecord);
                            ItemReceipt ir = (ItemReceipt)read.record;
                            ir.exchangeRateSpecified = false;
                            ir.tranDate = Convert.ToDateTime(f_dmphieuxn.NGAY);
                            ir.tranDateSpecified = true;
                            ir.tranId = f_dmphieuxn.SOCTXN;
                            ir.externalId = f_dmphieuxn.SOCTXN;
                            ir.memo = f_dmphieuxn.GHICHU;
                            ir.landedCostPerLine = true; // kế toán yêu cầu 30012026
                            ir.landedCostPerLineSpecified = true;

                            ir.customForm = new RecordRef() { internalId = "127" }; //id:127 Mua ngoài vattu

                            for (int j = 0; j < ir.itemList.item.Length; j++)
                            {
                                ItemReceiptItem itemLine = ir.itemList.item[j];
                                var ctphieulist = ct_phieuxn.Where(p => p.MAVT == itemLine.itemName).ToList();
                                var f_Hieu = ctphieulist.Count < 1 ? "" : ctphieulist.FirstOrDefault().HIEU;
                                var f_LO = ctphieulist.Count < 1 ? "" : ctphieulist.FirstOrDefault().LO;
                                var LOT = f_Hieu + ((f_LO == null || f_LO == "") ? "" : "-" + f_LO);
                                var orline = itemLine.orderLine;
                                itemLine.itemReceive = ctphieulist.Count < 1 ? false : true;
                                itemLine.itemReceiveSpecified = true;
                                itemLine.quantitySpecified = ctphieulist.Count < 1 ? false : true;


                                RecordRef location_item = new RecordRef();
                                location_item.externalId = kNhan.FirstOrDefault().externalid; // Kho nhận
                                itemLine.location = location_item;

                                if (itemLine.itemReceive == true)
                                {
                                    itemLine.quantity = ctphieulist.Count < 1 ? (Double)0 : (Double)ctphieulist.Sum(p => p.SOLUONGTT);

                                    InventoryDetail invdetail = new InventoryDetail();
                                    InventoryAssignmentList inventoryAssignmentList = new InventoryAssignmentList();
                                    List<InventoryAssignment> List_inventoryAssignment = new List<InventoryAssignment>();
                                    InventoryAssignment inventoryAssignment = new InventoryAssignment()
                                    {
                                        receiptInventoryNumber = LOT,
                                        quantity = ctphieulist.Count < 1 ? (Double)0 : (Double)ctphieulist.FirstOrDefault().SOLUONGTT,  
                                        quantitySpecified = true,
                                    };

                                    List_inventoryAssignment.Add(inventoryAssignment);
                                    inventoryAssignmentList.inventoryAssignment = List_inventoryAssignment.ToArray();
                                    invdetail.inventoryAssignmentList = inventoryAssignmentList;
                                    itemLine.inventoryDetail = invdetail;// Update this
                                }
                            }

                            WriteResponse response = ns.Service.add(ir);
                            #endregion //End Code

                            #region Return 
                            if (response.status.isSuccess == false)
                            {
                                var mess = response.status.statusDetail.FirstOrDefault().message;
                                return Json(new { status = 0, title = "Error", text = mess, obj = "" }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                var update_dm = soi_.NL_DMPHIEUXN.Where(p => p.SOCTXN == SOCTXN).FirstOrDefault();
                                update_dm.GUIAPI = "1";
                                update_dm.NGAYGUIAPI = DateTime.Now;
                                soi_.Entry(update_dm).State = EntityState.Modified;
                                soi_.SaveChanges();
                            }
                            #endregion // End Return

                            #endregion // End Nhập kho mua ngoài

                        }
                        if (MAHINHTHUC == "HKGC")
                        {
                            #region Tạo đơn hàng yc cấu sản xuất

                            #region Call data
                            var MADINHMUC = ct_phieuxn.FirstOrDefault().MAPHIEUPD;
                            var info_bangke = soi_.VATTU2024_SPLOAD_GC_DMPHIEU_BANGKENHAPSOI(MADINHMUC, "").FirstOrDefault();//không null 
                            var bophansx_hkgc = nstt_.Donvi_mapping_Department.Where(p => p.idmadv == info_bangke.MADV).ToList();
                            var f_bophansx_hkgc = bophansx_hkgc.Count < 1 ? "" : bophansx_hkgc.FirstOrDefault().externalid;
                            var IDMADV_TH = nstt_.Donvi_mapping_Department.Where(p => p.idmadv == info_bangke.MADV).ToList();
                            var IDMADV_YC = nstt_.Donvi_mapping_Department.Where(p => p.idmadv == info_bangke.MAKH).ToList();
                            var f_IDMADV_TH = IDMADV_TH.Count < 1 ? "" : IDMADV_TH.FirstOrDefault().externalid;
                            var f_IDMADV_YC = IDMADV_YC.Count < 1 ? "" : IDMADV_YC.FirstOrDefault().externalid;
                            var nhamaysx_location_hkgc = nstt_.Khoes.Where(p => p.makho == f_dmphieuxn.MAKHOXUAT).ToList();
                            var f_nhamaysx_location_hkgc = nhamaysx_location_hkgc.Count < 1 ? "" : nhamaysx_location_hkgc.FirstOrDefault().externalid;
                            var internalid_SoiGC = (info_bangke.LoaiYCSX == null || info_bangke.LoaiYCSX == "") ? UtilsNetsuite.LoaiYeuCauSanXuat.GCSoi : info_bangke.LoaiYCSX;
                            var sumsl = soi_.VATTU2024_SPLOAD_GC_CTPHIEUXUATGC_SUMSOLUONG(MADINHMUC, "").ToList();
                            #endregion

                            #region Check tồn định mức
                            foreach (var chk in ct_phieuxn)
                            {
                                var dinhmuccp_chk = soi_.VATTU2024_DINHMUC_SOIGC_TONGSLVATTUCAN(chk.MAVT, chk.MADINHMUC, chk.SOLUONGTT).ToList();
                                if (dinhmuccp_chk.Count < 1)
                                {
                                    return Json(new { status = -1, title = "", text = "Đồng bộ thất bại, kiểm tra lại trạng thái xem xét định mức", obj = chk.MADINHMUC }, JsonRequestBehavior.AllowGet);
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
                            customRec.name = info_bangke.SOPHIEU + "_DH_GCSOI"; // gán mã đơn hàng 
                            customRec.externalId = info_bangke.SOPHIEU + "_DH_GCSOI"; // gán ext cho đơn hàng
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
                            foreach (var items in ct_phieuxn)
                            {
                                #region Call data
                                var bom_externalid = items.MAVT + "_" + items.MADINHMUC + "_BOMV1";
                                var dinhmuccp = soi_.DINHMUC_SOIGC(items.MAVT, items.MADINHMUC).ToList();
                                if (dinhmuccp.Count < 1)
                                {
                                    return Json(new { status = -1, title = "", text = "Đồng bộ thất bại, kiểm tra lại trạng thái xem xét định mức", obj = items.MADINHMUC }, JsonRequestBehavior.AllowGet);
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
                                        var bomRev_externalid = items.MAVT + "_" + items.MADINHMUC + "_Rev";
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

                                        #region Assemblies mới
                                        // 1️. Lấy thông tin assembly hiện có
                                        RecordRef assemblyRef = new RecordRef()
                                        {
                                            type = RecordType.lotNumberedAssemblyItem,
                                            externalId = dinhmuccp.FirstOrDefault().Masoi
                                        };

                                        ReadResponse readResponse = ns.Service.get(assemblyRef);

                                        if (readResponse.status.isSuccess)
                                        {
                                            LotNumberedAssemblyItem existingAssembly = (LotNumberedAssemblyItem)readResponse.record;

                                            // 2️. Lấy danh sách BOM cũ (nếu có)
                                            var existingList = existingAssembly.billOfMaterialsList?.lotNumberedAssemblyItemBillOfMaterials?.ToList()
                                                              ?? new List<LotNumberedAssemblyItemBillOfMaterials>();

                                            // 3️. Tạo BOM mới cần thêm
                                            LotNumberedAssemblyItemBillOfMaterials newBOM = new LotNumberedAssemblyItemBillOfMaterials()
                                            {
                                                billOfMaterials = new RecordRef()
                                                {
                                                    type = RecordType.bom,
                                                    externalId = bom_externalid
                                                }
                                            };

                                            // 4️. Gộp danh sách
                                            existingList.Add(newBOM);

                                            // 5️. Gán lại vào assembly
                                            existingAssembly.billOfMaterialsList = new LotNumberedAssemblyItemBillOfMaterialsList()
                                            {
                                                lotNumberedAssemblyItemBillOfMaterials = existingList.ToArray()
                                            };

                                            // 6️. Upsert lại assembly
                                            WriteResponse responseBOM_assemblyItem = ns.Service.upsert(existingAssembly);
                                        }
                                        #endregion



                                        #region Assemblies     cũ đóng 10/11/2025                             
                                        //LotNumberedAssemblyItem assemblyItem = new LotNumberedAssemblyItem();
                                        //assemblyItem.externalId = dinhmuccp.FirstOrDefault().Masoi;
                                        //LotNumberedAssemblyItemBillOfMaterialsList lotNumberedAssemblyItemBillOfMaterialsList = new LotNumberedAssemblyItemBillOfMaterialsList();
                                        //LotNumberedAssemblyItemBillOfMaterials assemblyItemBillOfMaterials = new LotNumberedAssemblyItemBillOfMaterials();
                                        //assemblyItemBillOfMaterials.billOfMaterials = new RecordRef()
                                        //{
                                        //    type = RecordType.bom,
                                        //    externalId = bom_externalid
                                        //};
                                        //lotNumberedAssemblyItemBillOfMaterialsList.lotNumberedAssemblyItemBillOfMaterials = new LotNumberedAssemblyItemBillOfMaterials[] { assemblyItemBillOfMaterials };
                                        //assemblyItem.billOfMaterialsList = lotNumberedAssemblyItemBillOfMaterialsList;

                                        //WriteResponse responseBOM_assemblyItem = ns.Service.upsert(assemblyItem);
                                        #endregion
                                    }
                                    #endregion
                                }

                                #region Nhập thành phẩm

                                #region Call data
                                var keyhh_idcp = nstt_.Table_Mapping_DinhMuc_KeyHH_MOC.Where(p => p.KeyHH == items.MAVT + "_" + items.MADINHMUC + "_Rev").OrderByDescending(p => p.SoLanTinh_KeyHH).ToList();
                                var i_bom_rev_internalid = keyhh_idcp.Count < 1 ? "" : keyhh_idcp.FirstOrDefault().Internalid_BOM_Rev;
                                var i_bom_internalid = keyhh_idcp.Count < 1 ? "" : keyhh_idcp.FirstOrDefault().Internalid_BOM;

                                #endregion

                                CustomRecord customRec_TPSX = new CustomRecord();
                                RecordRef RecordRef_TPSX = new RecordRef();
                                RecordRef_TPSX.internalId = UtilsNetsuite.CustomRecord.ThanhPhamSanXuat;
                                customRec_TPSX.recType = RecordRef_TPSX; // Chọn CustomRecord
                                customRec_TPSX.name = items.MAVT; // gán mã đơn hàng 
                                customRec_TPSX.externalId = items.ID_XN.ToString(); // gán ext cho đơn hàng

                                CustomFieldRef[] fields_TPSX = new CustomFieldRef[99];

                                SelectCustomFieldRef f_dh_yc_sx = new SelectCustomFieldRef()
                                {
                                    scriptId = UtilsNetsuite.CustomFieldRef.custrecord_btm_tt_m_dh_yc_sx,
                                    value = new ListOrRecordRef()
                                    {
                                        externalId = items.MADH + "_DH_GCSOI"
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

                                StringCustomFieldRef fi_macay_LOT = new StringCustomFieldRef()
                                {
                                    scriptId = UtilsNetsuite.CustomFieldRef.custrecord_btm_tt_lot_serial_number,
                                    value = items.HIEU + "-" + items.LO
                                };

                                StringCustomFieldRef fi_SOCT_NHAP = new StringCustomFieldRef()
                                {
                                    scriptId = UtilsNetsuite.CustomFieldRef.custrecord_btm_tt_so_phieu_nhap,
                                    value = items.SOCTXN
                                };



                                DateCustomFieldRef f_Ngaysx = new DateCustomFieldRef()
                                {
                                    scriptId = UtilsNetsuite.CustomFieldRef.custrecord_btm_tt_ngay_sx,
                                    value = (DateTime)items.NGAYKETOAN,
                                };

                                StringCustomFieldRef fi_soluongthanhpham = new StringCustomFieldRef()
                                {
                                    scriptId = UtilsNetsuite.CustomFieldRef.custrecord_btm_tt_sl_thanh_pham,
                                    value = items.SOLUONGTT < 0 ? "0" : items.SOLUONGTT.ToString()
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
                                fields_TPSX[3] = fi_macay_LOT;
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
                        if (MAHINHTHUC == "HK")
                        {
                            #region Inventory Transfer
                            foreach (var items in dmphieuxn)
                            {
                                #region Gọi thông tin data đã mapping

                                #endregion

                                #region Xử lý add vào Netsuite 423
                                //var manetsuite = db_.SP_API_Load_MaNetSuiteYC().FirstOrDefault().MaNetSuiteYC;
                                InventoryTransfer IT_PhieuXkho = new InventoryTransfer();
                                //Date
                                IT_PhieuXkho.tranDate = Convert.ToDateTime(f_dmphieuxn.NGAY);
                                IT_PhieuXkho.tranDateSpecified = true;

                                //Số phiếu// Ref.No
                                IT_PhieuXkho.tranId = items.SOCTXN; //-- bị chặn trên netsuite

                                IT_PhieuXkho.externalId = items.SOCTXN;

                                //From location

                                RecordRef FromLocation = new RecordRef();
                                FromLocation.externalId = kXuat.FirstOrDefault().externalid;
                                //  FromLocation.externalId = Khoxuat.Count < 1 ? "" : Khoxuat.FirstOrDefault().externalid;
                                IT_PhieuXkho.location = FromLocation;

                                //From TransLocation 

                                RecordRef ToLocation = new RecordRef();
                                ToLocation.externalId = kNhan.FirstOrDefault().externalid;
                                // ToLocation.externalId = Khonhan.Count < 1 ? "" : Khonhan.FirstOrDefault().externalid;
                                IT_PhieuXkho.transferLocation = ToLocation;

                                //Memo / Ghi chú
                                IT_PhieuXkho.memo = items.GHICHU == null ? "" : items.GHICHU;

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

                                StringCustomFieldRef REF_SOCTXN = new StringCustomFieldRef();
                                REF_SOCTXN.scriptId = "custbody_btm_tt_ma_don_yeu_cau";
                                REF_SOCTXN.value = items.SOCTXN.ToString();
                                Cus_PhieuXkho[1] = REF_SOCTXN;

                                //// CHI TIẾT PHIẾU YC

                                var CTphieu = soi_.NL_CTXUATNHAP.Where(p => p.SOCTXN == items.SOCTXN).ToList();
                                InventoryTransferInventoryList orderItemList = new InventoryTransferInventoryList();
                                List<InventoryTransferInventory> orderItems = new List<InventoryTransferInventory>();

                                foreach (var ctitem in CTphieu)
                                {
                                    InventoryTransferInventory item_PhieuXkho = new InventoryTransferInventory();

                                    RecordRef Item_rec = new RecordRef();
                                    Item_rec.externalId = ctitem.MAVT;
                                    item_PhieuXkho.item = Item_rec;
                                    item_PhieuXkho.adjustQtyBy = Convert.ToDouble(ctitem.SOLUONGTT);
                                    item_PhieuXkho.adjustQtyBySpecified = true;

                                    InventoryDetail inventoryDetail = new InventoryDetail();
                                    InventoryAssignmentList inventoryAssignmentList = new InventoryAssignmentList();
                                    List<InventoryAssignment> List_inventoryAssignment = new List<InventoryAssignment>();
                                    InventoryAssignment inventoryAssignment = new InventoryAssignment();

                                    RecordRef lotRecord = new RecordRef();
                                    lotRecord.externalId = ctitem.MAVT + "_" + ctitem.HIEU + ((ctitem.LO == null || ctitem.LO == "") ? "" : "-" + ctitem.LO);
                                    inventoryAssignment.issueInventoryNumber = lotRecord;
                                    inventoryAssignment.quantity = Convert.ToDouble(ctitem.SOLUONGTT);
                                    inventoryAssignment.quantitySpecified = true;
                                    List_inventoryAssignment.Add(inventoryAssignment);
                                    inventoryAssignmentList.inventoryAssignment = List_inventoryAssignment.ToArray();
                                    inventoryDetail.inventoryAssignmentList = inventoryAssignmentList;

                                    item_PhieuXkho.inventoryDetail = inventoryDetail;
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
                                    return Json(new { status = 0, title = "Error", text = messdetail, obj = "" }, JsonRequestBehavior.AllowGet);
                                }
                                else
                                {
                                    #region update GuiAPI = 1
                                    var IsAPIs = soi_.NL_DMPHIEUXN.Where(c => c.SOCTXN == items.SOCTXN).FirstOrDefault();
                                    IsAPIs.GUIAPI = "1";
                                    IsAPIs.NGAYGUIAPI = DateTime.Now;
                                    soi_.Entry(IsAPIs).State = EntityState.Modified;
                                    soi_.SaveChanges();
                                    #endregion
                                }
                            }
                            return Json(new { status = 1, title = "", text = "Thêm thành công.", obj = "" }, JsonRequestBehavior.AllowGet);

                            #endregion
                        }

                        if (xuat3buoc == "0") // (nhập kho NZZZ phiên mã vải)
                        {
                            #region Nhập điều chỉnh
                            #region Call data                     

                            var customform = "152"; // Điều chỉnh tồn                        
                                                    //var customform = "169"; // Điều chỉnh tồn                        
                                                    // var cls = nstt_.Classes.FirstOrDefault(p => p.externalid == "BAN_PTVT");
                            var tk_iad = soi_.NL_DMHINHTHUC.FirstOrDefault(p => p.MAHTHUC == MAHINHTHUC).Account_KT;
                            var int_iad = nstt_.Chart_of_Accounts.FirstOrDefault(p => p.Number_TT == tk_iad).InternalID_NS;
                            var taikhoanketoan = int_iad; // Account: 1571
                            var ex_location = kNhan.FirstOrDefault().externalid;
                            #endregion

                            #region InventoryAdjustment
                            InventoryAdjustment InAdj = new InventoryAdjustment();
                            CustomFieldRef[] cusfield = new CustomFieldRef[100];

                            RecordRef cusfRec = new RecordRef();
                            cusfRec.internalId = customform;
                            InAdj.customForm = cusfRec;

                            InAdj.externalId = f_dmphieuxn.SOCTXN + "_IAD";
                            InAdj.tranId = f_dmphieuxn.SOCTXN + "_IAD";

                            RecordRef account = new RecordRef();
                            account.internalId = taikhoanketoan;
                            InAdj.account = account;

                            InAdj.tranDateSpecified = true;
                            InAdj.tranDate = Convert.ToDateTime(f_dmphieuxn.NGAY);

                            RecordRef khonhan = new RecordRef();
                            // khoxuat.externalId = kNhan.Count < 1 ? "" : kNhan.FirstOrDefault().externalid;
                            khonhan.externalId = ex_location;
                            InAdj.adjLocation = khonhan;
                            InAdj.memo = f_dmphieuxn.GHICHU;

                            if (f_dmphieuxn.IDMADVNHAN != "30")
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
                            foreach (var items in ct_phieuxn)
                            {

                                var IAD_detail_department = nstt_.Donvi_mapping_Department.Where(p => p.idmadv == f_dmphieuxn.IDMADVNHAN).ToList();
                                var f_IAD_detail_department = IAD_detail_department.Count < 1 ? "" : IAD_detail_department.FirstOrDefault().externalid;

                                CustomFieldRef[] cusDetail = new CustomFieldRef[100];
                                InventoryAdjustmentInventory line_IA = new InventoryAdjustmentInventory();

                                RecordRef refItem = new RecordRef();
                                refItem.externalId = items.MAVT;
                                line_IA.item = refItem;
                                line_IA.memo = items.GHICHU;

                                RecordRef location_item = new RecordRef();
                                location_item.externalId = kNhan.FirstOrDefault().externalid; // Kho nhận
                                line_IA.location = location_item;
                                line_IA.adjustQtyBySpecified = true;
                                line_IA.adjustQtyBy = Convert.ToDouble(items.SOLUONGTT);
                                var chkquantity = Convert.ToDouble(items.SOLUONGTT);
                                var LOT = items.HIEU + ((items.LO == null || items.LO == "") ? "" : ("-" + items.LO));

                                InventoryDetail inventoryDetail = new InventoryDetail();

                                InventoryAssignmentList inventoryAssignmentList = new InventoryAssignmentList();

                                List<InventoryAssignment> List_inventoryAssignment = new List<InventoryAssignment>();

                                InventoryAssignment inventoryAssignment = new InventoryAssignment();

                                if (chkquantity > 0)
                                {
                                    RecordRef lotRecord = new RecordRef();
                                    // lotRecord.externalId = "AA030001002_HAND";
                                    inventoryAssignment.receiptInventoryNumber = LOT;
                                    inventoryAssignment.issueInventoryNumber = lotRecord;
                                    inventoryAssignment.quantity = Convert.ToDouble(items.SOLUONGTT);
                                    inventoryAssignment.quantitySpecified = true;
                                    List_inventoryAssignment.Add(inventoryAssignment);
                                    inventoryAssignmentList.inventoryAssignment = List_inventoryAssignment.ToArray();   
                                    inventoryDetail.inventoryAssignmentList = inventoryAssignmentList;
                                    line_IA.inventoryDetail = inventoryDetail;
                                    orderItems_IA.Add(line_IA);
                                }
                                else
                                {
                                    RecordRef lotRecord = new RecordRef();
                                    // lotRecord.externalId = "AA030001002_HAND";
                                    lotRecord.externalId = items.MAVT + "_" + LOT;
                                    inventoryAssignment.issueInventoryNumber = lotRecord;
                                    inventoryAssignment.quantity = Convert.ToDouble(items.SOLUONGTT);
                                    inventoryAssignment.quantitySpecified = true;
                                    List_inventoryAssignment.Add(inventoryAssignment);
                                    inventoryAssignmentList.inventoryAssignment = List_inventoryAssignment.ToArray();
                                    inventoryDetail.inventoryAssignmentList = inventoryAssignmentList;
                                    line_IA.inventoryDetail = inventoryDetail;
                                    orderItems_IA.Add(line_IA);
                                }
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
                        #region update trạng thái API   

                        var up_dmphieuxn = soi_.NL_DMPHIEUXN.Where(p => p.SOCTXN == SOCTXN).FirstOrDefault();
                        up_dmphieuxn.GUIAPI = "1";
                        up_dmphieuxn.Externalid_IAD = f_dmphieuxn.SOCTXN + "_IAD";
                        up_dmphieuxn.NGAYGUIAPI = DateTime.Now;
                        soi_.Entry(up_dmphieuxn).State = EntityState.Modified;
                        soi_.SaveChanges();
                        #endregion
                        return Json(new { status = 1, title = "", text = "Đồng bộ thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
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

                //return Json(new { status = -2, title = "", text = "Thêm không thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
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

        #region
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                vt_?.Dispose();
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}