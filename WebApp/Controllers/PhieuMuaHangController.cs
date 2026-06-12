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
using ToolsApp.EntityFramework.GiaThanhSPM;
using ToolsApp.EntityFramework.VatTu;
using ToolsApp.EntityFramework.SPMAY;
using ToolsApp.EntityFramework.NetsuiteTT;
using ToolsApp.Models;
using System.Net.Mail;
using System.Web.Script.Serialization;
using System.Globalization;
using ToolsApp.Helper;
using System.Net;
using NSClient;
using ToolsApp.com.netsuite.webservices;
using Newtonsoft.Json;

namespace ToolsApp.Controllers
{

    [Authorize]
    public class PhieuMuaHangController : BaseController
    {

        private wqlvattuEntities vt_ = new wqlvattuEntities();
        private GiaThanh_SPMEntities giathanh_spm = new GiaThanh_SPMEntities();
        private NetsuiteTTEntities1 nstt_ = new NetsuiteTTEntities1();
        private QLSANPHAMMAY2023Entities SPMAY = new QLSANPHAMMAY2023Entities();

        // GET:PhieuMuaHangController
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult SearchVatTu(string search, int page = 1, string loaivt ="")
        {
            int pageSize = 30;
            var kihieudonvi = vt_.sp_kyhieudonvi_by_manv(User.UserName).FirstOrDefault().KiHieu;
         //   var item = vt_.SP_LOAD_VATTUYEUCAU_BYDONVI_BYLOAIPHIEU_27062016(kihieudonvi, "03", "", LoaiVT).ToList();


            var data = vt_.SP_LOAD_VATTUYEUCAU_BYDONVI_BYLOAIPHIEU_27062016(kihieudonvi, "03", "", loaivt)
                .Where(x => x.MAVT.Contains(search) || x.TENVT.Contains(search))
                .OrderBy(x => x.MAVT)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new
                {
                    id = x.MAVT,
                    text = x.MAVT + " || " + x.TENVT
                })
                .ToList();

            bool more = data.Count == pageSize;

            return Json(new
            {
                items = data,
                more = more
            }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult _Upload()
        {
            return PartialView();
        }
        private string new_MaPhieu(string userName, string kihieu)
        {
            string stt = "";
            var data = vt_.DMPHIEUx.Where(a => a.MAPHIEU.Contains(kihieu)).OrderByDescending(a => a.NgayYC).Take(1).ToList();
            string ngayYCStr = data.Count < 1 ? DateTime.Now.Year.ToString() : data[0].NgayYC.Value.ToString("yyyy"); // Chuỗi ngày tháng
         
            if (data.Count < 1 ||(data.Count < 1 ? 0 : data[0].STT) > 999 || ngayYCStr != DateTime.Now.Year.ToString() )
            {
                stt = "001";
            }
            else
            {
                int i = Convert.ToInt32(data[0].STT) + 1;
                stt = String.Format("{0:D3}", i);
            }
            string Kytucuoi = String.Format("{0:D3}", stt);
            DateTime date = DateTime.Now;
            string day = String.Format("{0:D2}", date.Day);
            string month = String.Format("{0:D2}", date.Month);
            string year = String.Format("{0:D4}", date.Year);
            string MaDh = kihieu + day + month + year + "-" + Kytucuoi;
            return MaDh;

        }

        #region Load View
        public ActionResult _Getlist(SearchDMPhieuYCMuaHang search)
        {
            var List = vt_.SpLoad_DMPhieu_Phieumuahang(User.UserName).Where(p =>
           (
               (search.MaphieuSearch == null || search.MaphieuSearch == "" || p.MAPHIEU.Contains(search.MaphieuSearch)) &&
               (search.MaphieuPDSearch == null || search.MaphieuPDSearch == "" || p.MaPhieuPD == search.MaphieuPDSearch) &&
               (search.LoaiVTSearch == null || search.LoaiVTSearch == "" || p.LoaiVT == search.LoaiVTSearch)
               && p.NguoiLapPhieu == User.UserName && p.XXet == 0
           )).ToList();

            ViewBag.List = List;

            return PartialView();
        }
        #endregion

        #region Load View CTphieu
        public ActionResult _GetListCTPhieu(string iMAPHIEU)
        {
            var list_Phieu = vt_.DMPHIEUx.Where(p => p.MAPHIEU == iMAPHIEU).FirstOrDefault();
            ViewBag.list_Phieu = list_Phieu;

            var List = vt_.VATTU2024_SPLOAD_CTPHIEU_2024(iMAPHIEU,"").Where(p => (
               (iMAPHIEU == null || iMAPHIEU == "" || p.MAPHIEU.Contains(iMAPHIEU))
           )).OrderByDescending(p => p.THOIDIEMDAPUNG).ToList();

            ViewBag.List = List;

            return PartialView();
        }
        #endregion

        #region Load Detail _InsertCTPhieu
        public ActionResult _InsertCTPhieu(string iMAPHIEU, string LoaiVT, string LOAIPHIEU)
        {
            #region Người XX
            var nguoixx = vt_.SP_LoadXemXetBM03TTKD(User.UserName).ToList();
            ViewBag.nguoixx = nguoixx;
            #endregion

            #region Loại VT
            var kihieu = vt_.sp_kyhieudonvi_by_manv(User.UserName).ToList();
            ViewBag.kihieu = kihieu;

            ViewData["MaPhieu"] = iMAPHIEU;
            var loaivt = vt_.SP_GET_DANHSACH_DMLOAIVT_THEO_DONVISUDUNG("", kihieu[0].KiHieu).ToList();
            ViewBag.loaivt = loaivt;
            var floaivt = loaivt.FirstOrDefault().LoaiVT;
            ViewData["loaivt_index"] = LoaiVT;
            ViewData["LOAIPHIEU"] = LOAIPHIEU;
            #endregion

            #region Mã Chi Phí
            var cMachiphi = vt_.SP_CHIPHI_28062016(User.UserName, floaivt).ToList();
            ViewBag.cMachiphi = cMachiphi;
            #endregion

            #region Mã đơn vị
            var madv = (from a in vt_.DMNHANVIENs
                        where a.MANV == User.UserName
                        select a.IDMaDV
                        ).FirstOrDefault();
            ViewBag.madv = madv;
            #endregion

            #region Mã bộ phận
            var mabp = vt_.DMNHANVIENs.Where(a => a.MANV == User.UserName).ToList();
            ViewBag.mabp = mabp;
            #endregion

            #region Load MAVT
            if (LOAIPHIEU == "04")
            {
                var item = vt_.SP_LOAD_DANHMUCVATTU_ByKey_LoaiVt(LoaiVT).ToList();
                ViewBag.iMAVT = item;             
                  
            }
            else
            {
                var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                serializer.MaxJsonLength = Int32.MaxValue;
                var kihieudonvi = vt_.sp_kyhieudonvi_by_manv(User.UserName).FirstOrDefault().KiHieu;
                var item = vt_.SP_LOAD_VATTUYEUCAU_BYDONVI_BYLOAIPHIEU_27062016(kihieudonvi, "03", "", LoaiVT).ToList();
                var check_nqth = vt_.Sp_DropList_LoaiVT_NQTH(LoaiVT).ToList();
                ViewBag.iMAVT = item;
                ViewBag.MAVTtd = vt_.VATTU2026_SPLOAD_MAVTTUONGDUONG_COGIANHAP("").ToList();
            }
            if(LOAIPHIEU == "08")
            {
                return PartialView("_InsertCTPhieu_08");
            }   
            else
            {
                return PartialView("_InsertCTPhieu");
            }    
            #endregion
          
        }
        #endregion

        #region Load View Edit CTphieu
        public ActionResult _Editview(int IDKHOA)
        {

            var model = vt_.CTPHIEUx.FirstOrDefault(p => p.IDKHOA == IDKHOA);
            var kihieu = vt_.sp_kyhieudonvi_by_manv(User.UserName).ToList();
            ViewBag.ctphieu = model;
            var dmphieu = vt_.DMPHIEUx.Where(p => p.MAPHIEU == model.MAPHIEU).FirstOrDefault();
            ViewBag.dmphieu = dmphieu;

            #region Mã Chi Phí
            string LoaiVT = dmphieu.LoaiVT.ToString();
            var cMachiphi = vt_.SP_CHIPHI_28062016(User.UserName, dmphieu.LoaiVT).ToList();
            ViewBag.cMachiphi = cMachiphi;
            #endregion

            #region Mã Vật Tư
            var cMaVT = vt_.SP_LOAD_VATTUYEUCAU_BYDONVI_BYLOAIPHIEU_27062016(kihieu[0].KiHieu, "03", "", dmphieu.LoaiVT).ToList();
            ViewBag.cMaVT = cMaVT;
            #endregion

            //return PartialView("_UpdateCTPhieu", Mapper.MapFrom(model));
            return PartialView("_UpdateCTPhieu");
        }
        #endregion

        #region Kiểm tra thực hiện Insert hay Update
        public ActionResult _DetailForEdit(string maphieu)
        {
            #region Người XX
            var nguoixx = vt_.SP_LoadXemXetBM03TTKD(User.UserName).ToList();
            ViewBag.nguoixx = nguoixx;
            #endregion
            #region Loại VT
            var kihieu = vt_.sp_kyhieudonvi_by_manv(User.UserName).ToList();
            ViewBag.kihieu = kihieu;

            string _maPhieu = new_MaPhieu(User.UserName, kihieu[0].KiHieu);
            ViewData["MaPhieu"] = _maPhieu;

            var loaivt = vt_.SP_GET_DANHSACH_DMLOAIVT_THEO_DONVISUDUNG("", kihieu[0].KiHieu).ToList();
            ViewBag.loaivt = loaivt;
            var floaivt = loaivt.FirstOrDefault().LoaiVT;
            #endregion

            #region Người lập phiếu
            var nguoilp = vt_.SP_NguoiLapPhieu(_maPhieu).FirstOrDefault();
            ViewBag.nguoilp = nguoilp;
            #endregion

            #region Mã Chi Phí
            var cMachiphi = vt_.SP_CHIPHI_28062016(User.UserName, floaivt).ToList();
            ViewBag.cMachiphi = cMachiphi;
            #endregion
            #region locations
            var locations = vt_.VATTU2024_SP_LOADKHOQUYENNHAP(User.UserName, "5").ToList();
            ViewBag.locations = locations;
            #endregion

            #region Mã đơn vị
            var madv = (from a in vt_.DMNHANVIENs
                        where a.MANV == User.UserName
                        select a.IDMaDV
                        ).FirstOrDefault();
            ViewBag.madv = madv;
            #endregion

            #region Mã bộ phận
            var mabp = vt_.DMNHANVIENs.Where(a => a.MANV == User.UserName).ToList();
            ViewBag.mabp = mabp;
            #endregion

            //#region Mã Vật Tư
            //var dmphieu = vt_.DMPHIEUx.FirstOrDefault(p => p.MAPHIEU == maphieu);
            //var cMaVT = vt_.SP_LOAD_VATTUYEUCAU_BYDONVI_BYLOAIPHIEU_27062016(kihieu[0].KiHieu, "03", "", dmphieu.LoaiVT).ToList();
            //ViewBag.cMaVT = cMaVT;
            //#endregion

            if (maphieu == "")
            {
                return PartialView("_Insert");
            }
            else
            {
                var model = vt_.DMPHIEUx.FirstOrDefault(p => p.MAPHIEU == maphieu);
                ViewBag.giaban = model;
                return PartialView("_Update");
            }
        }
        #endregion

        #region Insert
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _InsertFun(DMPHIEUViewModels model, string LoaiPhieu)
        {
            try
            {
                if (model.LoaiVT == null)
                {
                    return Json(new { status = -1, title = "", text = "Vui lòng chọn loại vật tư.", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                var tb = vt_.DMPHIEUx.FirstOrDefault(p => p.MAPHIEU == model.MAPHIEU);

                if (tb == null)
                {
                    var model_copy = new DMPHIEU();
                    model_copy.STT = decimal.Parse(model.MAPHIEU.Substring(model.MAPHIEU.Length - 3));
                    model_copy.MAPHIEU = model.MAPHIEU.Trim();
                    model_copy.NgayYC = DateTime.Now;
                    model_copy.KEHOACHTHANG = model.KEHOACHTHANG;
                    model_copy.LoaiVT = model.LoaiVT == null ? "" : model.LoaiVT.Trim();
                    model_copy.NguoiXX = model.NguoiXX;
                    model_copy.Nam = DateTime.Now.ToString("yyyy");
                    model_copy.NguoiLapPhieu = User.UserName;
                    model_copy.KyHieu = model.MAPHIEU.Substring(0, 3);
                    model_copy.LOAIPHIEU = LoaiPhieu;
                    model_copy.ND = model.ND.ToString() == "ND" ? (byte?)1 : 0;
                    model_copy.NK = model.ND.ToString() == "NK" ? (byte?)1 : 0;
                    model_copy.KH = model.KH.ToString() == "KH" ? (byte?)1 : 0;
                    model_copy.DX = model.KH.ToString() == "DX" ? (byte?)1 : 0;
                    model_copy.LyDoSD = model.LyDoSD;
                    model_copy.XXet = 0;
                    model_copy.locations = model.locations;
                    vt_.DMPHIEUx.Add(model_copy);
                    vt_.SaveChanges();

                    return Json(new { status = 1, title = "", text = "Thêm thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = -1, title = "", text = "Mã phiếu đã tồn tại.", obj = "" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "Thêm không thành công.", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region _SaveCTPhieu
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _SaveCTPhieu(CTPHIEUViewModels model)
        {
            #region Xử lý ngày
            CultureInfo cul = CultureInfo.GetCultureInfo("en-GB");
            var THOIDIEMSD_ = new DateTime();
            if (!string.IsNullOrEmpty(model.THOIDIEMSD_String))
            {
                try
                {
                    THOIDIEMSD_ = DateTime.ParseExact(model.THOIDIEMSD_String, "dd/MM/yyyy", cul);
                    if (THOIDIEMSD_ < DateTime.Now)
                    {
                        return Json(new { status = -1, title = "", text = "Thời điểm sử dụng phải lớn hơn ngày lập phiếu.", obj = "" }, JsonRequestBehavior.AllowGet);
                    }
                    model.THOIDIEMSD = new DateTime(THOIDIEMSD_.Year,
                        THOIDIEMSD_.Month, THOIDIEMSD_.Day, 0, 0, 0);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
            #endregion

            var listMAVTtd = model.MAVT_TUONGDUONG_;   
            string MAVTtd = (listMAVTtd != null && listMAVTtd.Any())
                                ? string.Join(",", listMAVTtd)
                                : "";

            try
            {
                var loaivt_nqth = vt_.Sp_DropList_LoaiVT_NQTH(model.LoaiVT_2).ToList();
                var iMAVT = model.MAVT;
                var iMADH = "";
                if (model.LoaiVT_2 == "TM-NK" || model.LoaiVT_2 == "TM-ND" || model.LoaiVT_2 == "TM-VAIMOC-NK" || model.LoaiVT_2 == "TM-VAIMOC-ND")
                {
                    var iMAVT_ = vt_.SP_GET_DETAIL_BY_MADH_MAVT(model.MAVT).ToList();
                    iMAVT = iMAVT_.Count > 0 ? iMAVT_.FirstOrDefault().MAVT : model.MAVT;
                    iMADH = iMAVT_.Count > 0 ? iMAVT_.FirstOrDefault().MADH : iMADH;
                }
                if (loaivt_nqth.Count > 0  && (vt_.DANHMUCVATTUs.Where(p => p.MAVT == iMAVT).FirstOrDefault()) == null)
                {
                    var mavt_nqth = vt_.SP_DROPLIST_MAVT_NQTH().Where(p => p.MASP == iMAVT).FirstOrDefault();
                    var model_danhmucvattu = new DANHMUCVATTU();
                    model_danhmucvattu.MAVT = iMAVT.Trim();
         
                    model_danhmucvattu.TenVT = mavt_nqth.TENSANPHAM == null ? iMAVT : mavt_nqth.TENSANPHAM.Trim();
                    model_danhmucvattu.LoaiVT = model.LoaiVT_2;
                    model_danhmucvattu.DVT = "CAI";
                    model_danhmucvattu.DonViSuDungtac = "P.NQTH";
                    model_danhmucvattu.HieuLuc = true;
                    model_danhmucvattu.Ghichu = "thêm mới tự động khi nhập ở phiếu mua hàng";
                    model_danhmucvattu.Manhom = model.LoaiVT_2;

                    model_danhmucvattu.MACP = mavt_nqth.MACP;
                    model_danhmucvattu.MANVCAPNHAT = User.UserName;
                    model_danhmucvattu.NGAYCN = DateTime.Now;
                    vt_.DANHMUCVATTUs.Add(model_danhmucvattu);
                    vt_.SaveChanges();

                }

                if (model.SLYCAU <= 0)
                {
                    return Json(new { status = -1, title = "", text = "Số lượng yêu cầu phải lớn hơn 0.", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                if ((vt_.DANHMUCVATTUs.Where(p => p.MAVT == iMAVT).FirstOrDefault()) == null )
                {
                    return Json(new { status = -1, title = "", text = "Vật tư không tồn tại, vui lòng thêm " + iMAVT.ToUpper() + " vào danh mục vật tư .", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                if ((vt_.CTPHIEUx.Where(a => a.MAPHIEU == model.MAPHIEU && a.MAVT == model.MAVT ).FirstOrDefault() != null))
                {
                    return Json(new { status = -1, title = "", text = "Vật tư đã tồn tại, vui lòng kiểm tra " + iMAVT.ToUpper() + " danh sách bên dưới .", obj = "" }, JsonRequestBehavior.AllowGet);
                }

                if (iMAVT.Length != 11 && (model.LoaiVT_2 == "TM-VAIMOC-NK" || model.LoaiVT_2 == "TM-VAIMOC-ND"))
                {
                    return Json(new { status = -1, title = "", text = "Mã vật tư không phù hợp, Vui lòng thông tin cho TT.QLCL tạo mã vải mộc đúng qui định.", obj = "" }, JsonRequestBehavior.AllowGet);
                }

                var status_xxem = vt_.DMPHIEUx.Where(p => p.MAPHIEU == model.MAPHIEU.Trim() && p.XXet == 1).ToList();
                if (status_xxem.Count > 0)
                {
                    return Json(new { status = -1, title = "", text = "Phiếu đã xem xét , vui lòng tạo phiếu khác.", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                var tb = vt_.CTPHIEUx.Where(a => a.MAPHIEU == model.MAPHIEU && a.MAVT == model.MAVT ).FirstOrDefault();
                DateTime dataDate = DateTime.Now;
                if (tb == null)
                {
                    #region lấy tồn
                 //   var tonkho  = vt_.VATTU2025_SPLOAD_SOLUONGTON_THEOMAVT_DONVI_TOANCTY_PhieuMuaHang(model.MAVT.Trim(), model.MAPHIEU.Trim()).FirstOrDefault();
                  //  model.SLTONDVI = tonkho?.SLTONDVI ?? 0;
                   // model.SLTONDVI = tonkho?.SLTONDVI ?? 0;
                  //  model.SLTONCTY = tonkho?.SLTONCTY ?? 0;
                  //  model.SLTONCTY = tonkho?.SLTONCTY ?? 0;
                    #endregion

                    #region Add CTPHIEU

                    var model_copy = new CTPHIEU();
                    model_copy.MAPHIEU = model.MAPHIEU.Trim();
                    model_copy.MASOCP = model.MASOCP == null ? "" : model.MASOCP.Trim();
                    model_copy.MAVT = iMAVT == null ? model.MAVT.Trim() : iMAVT;
                    model_copy.MAVT_TUONGDUONG = MAVTtd;
                    model_copy.SLYCAU = model.SLYCAU;
                    model_copy.THOIDIEMSD = model.THOIDIEMSD;
                    model_copy.DACTINHKYTHUAT = model.DACTINHKYTHUAT;                  
                    model_copy.GHICHU = model.GHICHU;
                    model_copy.SLXXET = model.SLYCAU;
                    model_copy.SLPD = model.SLYCAU;
                    model_copy.SLTONDVI = Convert.ToDecimal(model.SLTONDVI_);
                    model_copy.SLTONCTY = Convert.ToDecimal(model.SLTONCTY_);
                    model_copy.TINHTRANG = model.TINHTRANG;
                    model_copy.MADH = iMADH;
                    model_copy.THOIDIEMDAPUNG = model.THOIDIEMDAPUNG;
                    vt_.CTPHIEUx.Add(model_copy);
                    vt_.SaveChanges();
                    #endregion

                    if(MAVTtd != null || MAVTtd != "")
                    {
                        vt_.VATTU2026_SP_INSERT_CTPHIEU_MAVT_TUONGDUONG(model.MAPHIEU, model.MAVT, MAVTtd, User.UserName);
                    }    
                    return Json(new { status = 1, title = "", text = "Thêm thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    tb.SLYCAU = tb.SLYCAU + model.SLYCAU;
                    tb.THOIDIEMSD = model.THOIDIEMSD;
                    tb.DACTINHKYTHUAT = model.DACTINHKYTHUAT;
                    tb.GHICHU = model.GHICHU;
                    vt_.Entry(tb).State = EntityState.Modified;
                    vt_.SaveChanges();
                    return Json(new { status = 1, title = "", text = "Cập nhật thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region _SaveCTPhieu 08
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _SaveCTPhieu_08(CTPHIEUViewModels model)
        {
            #region Xử lý ngày
            CultureInfo cul = CultureInfo.GetCultureInfo("en-GB");
            var THOIDIEMSD_ = new DateTime();
            if (!string.IsNullOrEmpty(model.THOIDIEMSD_String))
            {
                try
                {
                    THOIDIEMSD_ = DateTime.ParseExact(model.THOIDIEMSD_String, "dd/MM/yyyy", cul);
                    if (THOIDIEMSD_ < DateTime.Now)
                    {
                        return Json(new { status = -1, title = "", text = "Thời điểm sử dụng phải lớn hơn ngày lập phiếu.", obj = "" }, JsonRequestBehavior.AllowGet);
                    }
                    model.THOIDIEMSD = new DateTime(THOIDIEMSD_.Year,
                        THOIDIEMSD_.Month, THOIDIEMSD_.Day, 0, 0, 0);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
            #endregion

            try
            {
                if(model.MADH == null || model.MADH == "")
                {
                    return Json(new { status = -1, title = "", text = "Đơn hàng kế thùa không được để trống.", obj = "" }, JsonRequestBehavior.AllowGet);
                }    
             
                var kethuamavt = giathanh_spm.GET_MAMH_TAO_PR_SPM(model.MADH.Trim()).ToList();
       
                foreach( var item in kethuamavt)
                {
                    if(item.MAMH == null)
                        return Json(new { status = -1, title = "", text = "Lỗi load mã hàng bị null, kiểm tra lại đơn đặt hàng.", obj = "" }, JsonRequestBehavior.AllowGet);

                    var listcoa = vt_.TBL_DMLOAIVT_COA.FirstOrDefault(p => p.LoaiVT == item.LOAIVT);
                    if ((vt_.DANHMUCVATTUs.Where(p => p.MAVT == item.MAMH).FirstOrDefault()) == null)
                    {
                     
                        var model_danhmucvattu = new DANHMUCVATTU();
                        model_danhmucvattu.MAVT = item.MAMH;
                        model_danhmucvattu.TenVT = item.TENHANG;
                        model_danhmucvattu.LoaiVT = item.LOAIVT;
                        model_danhmucvattu.DVT = "CAI";
                        model_danhmucvattu.DonViSuDungtac = "MAY";
                        model_danhmucvattu.HieuLuc = true;
                        model_danhmucvattu.Ghichu = "thêm mới tự động khi nhập ở phiếu mua hàng";
                        model_danhmucvattu.Manhom = item.LOAIVT;
                        model_danhmucvattu.ASSET = listcoa.ASSET;
                        model_danhmucvattu.COGS = listcoa.COGS;
                        model_danhmucvattu.INCOME = listcoa.INCOME;
                        model_danhmucvattu.TK_ChiPhi_SX = listcoa.TK_PhiSX;
                        model_danhmucvattu.MANVCAPNHAT = User.UserName;
                        model_danhmucvattu.NGAYCN = DateTime.Now;
                        vt_.DANHMUCVATTUs.Add(model_danhmucvattu);
                        vt_.SaveChanges();

                    }

                    var result = _IsDongBo(item.MAMH);
                    var json = JsonConvert.SerializeObject(result.Data);
                    var resultData = JsonConvert.DeserializeObject<SOAPResultViewModel>(json);
                    if (resultData.status < 1)
                    {
                        return Json(new { status = -1, title = "", text = resultData.text, obj = "" }, JsonRequestBehavior.AllowGet);
                    }

                    var status_xxem = vt_.DMPHIEUx.Where(p => p.MAPHIEU == model.MAPHIEU.Trim() && p.XXet == 1).ToList();
                    if (status_xxem.Count > 0)
                    {
                        return Json(new { status = -1, title = "", text = "Phiếu đã xem xét , vui lòng tạo phiếu khác.", obj = "" }, JsonRequestBehavior.AllowGet);
                    }
                    var tb = vt_.CTPHIEUx.Where(a => a.MAPHIEU == model.MAPHIEU && a.MAVT == item.MAMH).FirstOrDefault();
                    DateTime dataDate = DateTime.Now;
                    if (tb == null)
                    {

                        #region Add CTPHIEU
                        var model_copy = new CTPHIEU();
                        model_copy.MAPHIEU = model.MAPHIEU.Trim();
                        model_copy.MASOCP = model.MASOCP == null ? "" : model.MASOCP.Trim();
                        model_copy.MAVT = item.MAMH.Trim();
                        model_copy.MAVT_TUONGDUONG = "";
                        model_copy.SLYCAU = item.SOLUONG;
                        model_copy.THOIDIEMSD = model.THOIDIEMSD;
                        model_copy.DACTINHKYTHUAT = model.DACTINHKYTHUAT;
                        model_copy.GHICHU = model.GHICHU;
                        model_copy.SLXXET = item.SOLUONG;
                        model_copy.SLPD = item.SOLUONG;
                        model_copy.SLTONDVI = 0;
                        model_copy.SLTONCTY = 0;
                        model_copy.TINHTRANG = model.TINHTRANG;
                        model_copy.MADH = model.MADH;
                        model_copy.THOIDIEMDAPUNG = model.THOIDIEMDAPUNG;
                        vt_.CTPHIEUx.Add(model_copy);            
                        #endregion                    
                    }
                    else
                    {
                        tb.SLYCAU = tb.SLYCAU + model.SLYCAU;
                        tb.THOIDIEMSD = model.THOIDIEMSD;
                        tb.DACTINHKYTHUAT = model.DACTINHKYTHUAT;
                        tb.GHICHU = model.GHICHU;
                        vt_.Entry(tb).State = EntityState.Modified;              
                    }
                }

                vt_.SaveChanges();
                return Json(new { status = 1, title = "", text = "Cập nhật thành công.", obj = "" }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Update
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _EditFunCT(CTPHIEUViewModels model)
        {
            #region Xử lý ngày
            CultureInfo cul = CultureInfo.GetCultureInfo("en-GB");
            var THOIDIEMSD_ = new DateTime();
            if (!string.IsNullOrEmpty(model.THOIDIEMSD_String))
            {
                try
                {
                    THOIDIEMSD_ = DateTime.ParseExact(model.THOIDIEMSD_String, "dd/MM/yyyy", cul);
                    if (THOIDIEMSD_ < DateTime.Now)
                    {
                        return Json(new { status = -1, title = "", text = "Thời điểm sử dụng phải lớn hơn ngày lập phiếu.", obj = "" }, JsonRequestBehavior.AllowGet);
                    }
                    model.THOIDIEMSD = new DateTime(THOIDIEMSD_.Year,
                        THOIDIEMSD_.Month, THOIDIEMSD_.Day, 0, 0, 0);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
            #endregion

            try
            {
                var item = vt_.CTPHIEUx.FirstOrDefault(p => p.IDKHOA == model.IDKHOA && p.MAPHIEU == model.MAPHIEU);
                if (item == null)
                {
                    return Json(new { status = -1, title = "", text = "Không tìm thấy chi tiết phiếu.", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var check_PheDuyet = vt_.DMPHIEUx.Where(a => a.PDuyet == 1 && a.XXet == 1 && a.MAPHIEU == model.MAPHIEU).FirstOrDefault();
                    if (check_PheDuyet == null)
                    {
                        if (model.SLYCAU < 0)
                        {
                            return Json(new { status = -1, title = "", text = "Số lượng yêu cầu phải lớn hơn hoặc bằng không.", obj = "" }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            var mphieu = item.MAPHIEU;
                            item.SLYCAU = model.SLYCAU;
                            item.THOIDIEMSD = model.THOIDIEMSD;
                            item.DACTINHKYTHUAT = model.DACTINHKYTHUAT;
                            item.GHICHU = model.GHICHU;
                            vt_.Entry(item).State = EntityState.Modified;
                            vt_.SaveChanges();
                            return Json(new { status = 1, title = "", text = "Cập nhật thành công.", obj = mphieu }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        return Json(new { status = -1, title = "", text = "Phiếu đã được phê duyệt.", obj = "" }, JsonRequestBehavior.AllowGet);
                    }
                }

            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Onchange LoadMaVT
        public JsonResult LoadMaVT(string loaivt, string LoaiPhieu)
        {
            try
            {
                if (LoaiPhieu == "04")
                {
                    var item = vt_.SP_LOAD_DANHMUCVATTU_ByKey_LoaiVt(loaivt).ToList();
                    return Json(new
                    {
                        status = 1,
                        title = "",
                        text = "",
                        obj = item
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                    serializer.MaxJsonLength = Int32.MaxValue;

                    var kihieu = vt_.sp_kyhieudonvi_by_manv(User.UserName).FirstOrDefault().KiHieu;
                    var item = vt_.SP_LOAD_VATTUYEUCAU_BYDONVI_BYLOAIPHIEU_27062016(kihieu, "03", "", loaivt).ToList();
                    ViewBag.item = item;

                    dynamic selectedItems = "";
                    if (loaivt == "PT-VT")
                    {
                        selectedItems = item.Select(i => new { i.MAVT, i.TENVT, i.TONKHODONVI }).ToList();
                    }
                    else
                    {
                        selectedItems = item.Select(i => new { i.MAVT, i.VT }).ToList();
                    }
                    return Json(new
                    {
                        status = 1,
                        title = "",
                        text = "",
                        obj = selectedItems
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                return Json(new { status = -1, title = "", text = e.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Onchange _LoadMASOCP
        public JsonResult _LoadMASOCP(string LoaiVT)
        {
            try
            {
                var item = vt_.SP_CHIPHI_28062016(User.UserName, LoaiVT).ToList();
                ViewBag.item = item;

                return Json(new { status = 1, title = "", text = "", obj = item }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { status = -1, title = "", text = e.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Onchange GetSLTonTheoMAVT
        public JsonResult GetSLTonTheoMAVT(string MAVT = null, string maphieu = null)
        {
            try
            {
                var item = vt_.VATTU2025_SPLOAD_SOLUONGTON_THEOMAVT_DONVI_TOANCTY_PhieuMuaHang(MAVT, maphieu).ToList();
                var sltondv = item.FirstOrDefault()?.SLTONDVI ?? 0;

                var itemns = CUSTOMSEARCH.SEARCH_ITEM_INTERNALID(MAVT, "NS-A4.01");
                var sltoncty = itemns.sumquantityonhand;

                return Json(new { status = 1, objdvi = sltondv , objcty = sltoncty }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { status = -1, text = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Onchange _LoadLoaiVT
        public JsonResult _LoadLoaiVT(string LoaiPhieu)
        {
            try
            {
                var kihieu = vt_.sp_kyhieudonvi_by_manv(User.UserName).FirstOrDefault();
                if (LoaiPhieu == "04")
                {
                    var models = vt_.SP_LOAD_DMLOAIVT_ByKey_Donvisd(kihieu.KiHieu).ToList();
                    ViewBag.item = models;
                    return Json(new { status = 1, title = "", text = "", obj = models }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var loaivt = vt_.SP_GET_DANHSACH_DMLOAIVT_THEO_DONVISUDUNG("", kihieu.KiHieu).ToList();
                    ViewBag.loaivt = loaivt;
                    return Json(new { status = 1, title = "", text = "", obj = loaivt }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                return Json(new { status = -1, title = "", text = e.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Delete
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _DeleteFun(string maPhieu)
        {
            try
            {
                var item = vt_.DMPHIEUx.FirstOrDefault(p => p.MAPHIEU == maPhieu);
                var ctitem = vt_.CTPHIEUx.Where(p => p.MAPHIEU == item.MAPHIEU).ToList();

                if (item.XXet == 0)
                {
                    if (ctitem.Count > 0)
                    {
                        for (int i = 0; i < ctitem.Count; i++)
                        {
                            vt_.CTPHIEUx.Remove(ctitem[i]);
                            vt_.SaveChanges();
                        }

                    }
                    vt_.DMPHIEUx.Remove(item);
                    vt_.SaveChanges();
                    return Json(new { status = 1, title = "", text = "Xóa thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = -1, title = "", text = "Mã phiếu đã được xem xét không được xóa", obj = "" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Delete CT
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _DeleteCTFun(int IDKHOA)
        {
            try
            {
                

                var item = vt_.CTPHIEUx.FirstOrDefault(p => p.IDKHOA == IDKHOA);
                var MAPHIEU = item.MAPHIEU;
                var chk = vt_.DMPHIEUx.FirstOrDefault(p => p.MAPHIEU == MAPHIEU).XXet;

                vt_.CTPHIEUx.Remove(item);
                vt_.SaveChanges();
                return Json(new { status = 1, title = "", text = "Xóa thành công.", obj = MAPHIEU }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Gửi mail yêu cầu xem xét
        [HttpPost]
        public string openOutlookemailbox(string MAPHIEU)
        {

            #region Lấy địa chỉ mail của người xx
            var item = vt_.DMPHIEUx.FirstOrDefault(p => p.MAPHIEU == MAPHIEU);
            var manvkt = vt_.DMNHANVIENs.FirstOrDefault(p => p.MANV == item.NguoiXX);
            #endregion

            MailMessage mail = new MailMessage();
            mail.To.Add(new MailAddress(manvkt.EMail));
            mail.IsBodyHtml = true;
            mail.Subject = item.MAPHIEU;
            mail.Body = "Xem xét phiếu yêu cầu mua hàng (" + item.MAPHIEU + ") trong chương trình Quản lý vật tư %0D%0A";
            var outlookmail = "mailto:" + mail.To.ToString()
                + "?subject=" + "XEM XÉT MÃ PHIẾU YÊU CẦU MUA HÀNG-" + mail.Subject.ToString()
                + "&body=" + "Kính gửi: TBP %0D%0A" + "" + mail.Body.ToString() + "" + "Trân trọng!";
            return outlookmail;
        }
        #endregion

        #region In phiếu xem xét
        public ActionResult _ReportPrintCT(string MaPhieu)
        {
            if (MaPhieu == null)
            {
                return Json(new { status = -1, text = "", obj = "" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var CTPhieu = vt_.SP_REPHIEUMUAHANGTHEOBM(MaPhieu).ToList();
                ViewBag.List = CTPhieu;
            }
            return PartialView();
        }
        #endregion

        #region Hình mô tả vật tư
        public JsonResult SetImage(string MAVT)
        {

            var data = (from a in vt_.DANHMUCVATTUs
                        join b in vt_.DMLOAIVTs on a.LoaiVT equals b.LoaiVT
                        where a.MAVT == MAVT
                        select new
                        {
                            MaVT = a.MAVT,
                            TenVT = a.TenVT,
                            Images = (a.Images == null ? "materials_images/NOPHOTO.JPG" : (a.Images.Substring(3, a.Images.Length))),
                            b,
                        }
                        ).ToList();
            if (data.Count() > 0)
            {
                return Json(new { status = 1, title = "", text = "", obj = data }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { status = -1, title = "", text = "", obj = "" }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Import Excel
        [HttpPost]
        [ValidateInput(false)]
        public JsonResult _FunImport()
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

                    var list_PhieuMuaHang_CT_NHAP = new List<CTPHIEU>();
                    #region Read
                    for (int i = 2; i <= stats.EndRowIndex; i++)
                    {
                        var stt = sl.GetCellValueAsString(i, 1);
                        var MAPHIEU = sl.GetCellValueAsString(i, 2);                        
                        var MAVT = sl.GetCellValueAsString(i, 3).ToUpper().Trim();
                        var SLYCAU = sl.GetCellValueAsString(i, 4).ToUpper().Trim();
                        var TINHTRANG = sl.GetCellValueAsString(i, 5).ToUpper().Trim();
                        var THOIDIEMSD = sl.GetCellValueAsDateTime(i, 6);
                        var GHICHU = sl.GetCellValueAsString(i, 7).ToUpper().Trim();
                        var THOIDIEMDAPUNG = sl.GetCellValueAsDateTime(i, 8);
                        var DACTINHKYTHUAT = sl.GetCellValueAsString(i, 9).ToUpper().Trim();
                         
                        if (!string.IsNullOrEmpty(stt))
                        {
                            #region Kiểm tra dữ liệu
                            var tenVT = vt_.DANHMUCVATTUs.FirstOrDefault(p => p.MAVT == MAVT);
                            var DMPHIEU = vt_.DMPHIEUx.FirstOrDefault(p => p.MAPHIEU == MAPHIEU);
                            if (tenVT.GuiAPI != "1" || tenVT.HieuLuc != true)
                            {
                                return Json((new { status = -1, title = "", text = string.Format("MAVT dòng {0} : "+ MAVT + " chưa đồng bộ Netsuite. Kiểm tra lại", numRecord), obj = "" }), JsonRequestBehavior.DenyGet);
                            }
                            if (tenVT.LoaiVT != DMPHIEU.LoaiVT)
                            {
                                return Json((new { status = -1, title = "", text = string.Format("MAVT dòng {0} : " + MAVT + " Không thuộc LoaiVT của phiếu. Kiểm tra lại", numRecord), obj = "" }), JsonRequestBehavior.DenyGet);
                            }
                            if (string.IsNullOrEmpty(MAVT))
                            {
                                return Json((new { status = -1, title = "", text = string.Format("Mã vật tư dòng {0} : " + MAVT + " không được bỏ trống.", numRecord), obj = "" }), JsonRequestBehavior.AllowGet);
                            }
                            if (string.IsNullOrEmpty(SLYCAU))
                            {
                                return Json((new { status = -1, title = "", text = string.Format("Số lượng yêu cầu dòng {0} : " + MAVT + " không được bỏ trống.", numRecord), obj = "" }), JsonRequestBehavior.AllowGet);
                            }
                            if (string.IsNullOrEmpty(THOIDIEMSD.ToString()))
                            {
                                return Json((new { status = -1, title = "", text = string.Format("Thời điểm sử dụng dòng {0} : " + MAVT + " không được bỏ trống.", numRecord), obj = "" }), JsonRequestBehavior.AllowGet);
                            }
                            if (string.IsNullOrEmpty(THOIDIEMDAPUNG.ToString()))
                            {
                                return Json((new { status = -1, title = "", text = string.Format("Thời điểm đáp ứng dòng {0} : " + MAVT + " không được bỏ trống.", numRecord), obj = "" }), JsonRequestBehavior.AllowGet);
                            }
                            if (vt_.DMPHIEUx.FirstOrDefault(p => p.MAPHIEU == MAPHIEU) == null)
                            {
                                return Json((new { status = -1, title = "", text = string.Format("Phiếu mua hàng không tồn tại. Kiểm tra lại", numRecord), obj = "" }), JsonRequestBehavior.DenyGet);
                            }
                            #endregion

                            #region Kiểm tra trùng trong excel
                            if (list_PhieuMuaHang_CT_NHAP.FirstOrDefault(p => (p.MAVT == MAVT)) != null)
                            {
                                return Json((new { status = -1, title = "", text = string.Format(" MAVT dòng {0} : " + MAVT + " trùng trong file. Vui lòng kiểm tra lại.", numRecord), obj = "" }), JsonRequestBehavior.AllowGet);
                            }
                            #endregion

                            #region Kiểm tra cập nhật hay insert        

                            var itemns = CUSTOMSEARCH.SEARCH_ITEM_INTERNALID(MAVT, "NS-A4.01");
                            var sltoncty = itemns.sumquantityonhand;
                            var model_detail = new CTPHIEU
                            {                             
                                MAVT = MAVT,
                                SLYCAU = Convert.ToDecimal(SLYCAU),
                                SLXXET = Convert.ToDecimal(SLYCAU),
                                SLPD = Convert.ToDecimal(SLYCAU),
                                SLTONCTY = Convert.ToDecimal(sltoncty),
                                TINHTRANG = TINHTRANG,
                                THOIDIEMSD = THOIDIEMSD,
                                GHICHU = GHICHU,
                                THOIDIEMDAPUNG = THOIDIEMDAPUNG,
                                DACTINHKYTHUAT = DACTINHKYTHUAT,
                                MAPHIEU = MAPHIEU,       
                        };
                            list_PhieuMuaHang_CT_NHAP.Add(model_detail);
                            #endregion
                            isHaveData = true;
                            numRecord++;
                        }
                    }
                    if (isHaveData)
                    {
                        vt_.CTPHIEUx.AddRange(list_PhieuMuaHang_CT_NHAP);
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


        private JsonResult _IsDongBo(string MAVT)
        {
            if (!ModelState.IsValid)
                return Json(new { status = -2, text = "Lưu không thành công." }, JsonRequestBehavior.AllowGet);

            try
            {
                var listVT = vt_.DANHMUCVATTUs
                                .Where(x => x.MAVT == MAVT && x.GuiAPI != "1")
                                .ToList();


                // ===== Init NetSuite =====
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback += (a, b, c, d) => true;

                var ns = new NSClient.NSClient();
                NSBase.Client = ns;

                // ===== Cache data (TRÁNH query trong loop) =====
                var loaiSPs = nstt_.LoaiSanPhams.ToList();
                var phanNhomSPs = nstt_.PhanNhomSanPhams.ToList();
                var loaiSPCTs = nstt_.LoaiSanPhamChiTiets.ToList();
                var coaList = nstt_.Chart_of_Accounts.ToList();
                var taxcode = nstt_.Table_TAX_CODE.FirstOrDefault(x => x.HieuLuc == true)?.Internalid_taxcode;

                foreach (var items in listVT)
                {
                    try
                    {

                        var LOAIVTs = vt_.DANHMUCVATTUs.Where(p => p.MAVT == items.MAVT).FirstOrDefault().LoaiVT;
                        var LoaiSanPhams = nstt_.LoaiSanPhams.ToList();
                        var PhanNhomSanPhams = nstt_.PhanNhomSanPhams.ToList();
                        var LoaiSanPhamChiTiets = nstt_.LoaiSanPhamChiTiets.ToList();
                        var item_PhanNhomSanPhams = PhanNhomSanPhams.Where(p => p.LoaiVT_TT == LOAIVTs.ToString()).ToList();
                        if (LOAIVTs == "PT-VT")
                        {
                            item_PhanNhomSanPhams = PhanNhomSanPhams.Where(p => p.LoaiVT_TT == LOAIVTs.ToString() && items.MAVT[0].ToString() == p.KyTu).ToList();
                        }
                        var item_LoaiSanPhams = LoaiSanPhams.Where(p => p.externalid == (item_PhanNhomSanPhams.Count < 1 ? "VATTU"
                                                              : item_PhanNhomSanPhams.FirstOrDefault().MaLoaiSP_TT))
                                                              .FirstOrDefault().externalid;
                        if (items.LoaiVT == "PTMUANGOAI")
                        {
                            item_LoaiSanPhams = LoaiSanPhams.Where(p => p.externalid == "MAY").FirstOrDefault().externalid;
                        }
                        if (items.LoaiVT == "VAI" || items.LoaiVT == "VAI_TT_DI_GC")
                        {
                            item_LoaiSanPhams = LoaiSanPhams.Where(p => p.externalid == "VAITHANHPHAM").FirstOrDefault().externalid;
                        }

                        var item_LoaiSanPhamChiTiets = LoaiSanPhamChiTiets.Where(p => p.Kytu1 == items.MAVT[0].ToString()
                                                                              && p.MaLoaiSPCT == items.MAVT[1].ToString()).ToList();
                        var dvt = vt_.VATTU2024_SpLoad_ConvertDVT(items.MAVT).FirstOrDefault().DVT_Convert;
                        var exiddvt = nstt_.Table_Mapping_Mavattu_DVT.Where(c => c.DVT == dvt).FirstOrDefault().Internalid;
                        dynamic item = "";
                        // dynamic int_form = "67"; // customform - 67 = TT vật tư / inventory
                        var int_form = vt_.TBL_DMLOAIVT_COA.FirstOrDefault(p => p.LoaiVT == items.LoaiVT).CustomForm;

                        if (items.LoaiVT == "MOC" || items.LoaiVT == "VAI" || items.LoaiVT == "QUANAO"
                            || items.LoaiVT == "QUANAO_GC" || items.LoaiVT == "VAI_TT_DI_GC"
                            || items.LoaiVT == "MOC_TT_DI_GC"
                            || items.LoaiVT == "PHUTRANG"
                            || items.LoaiVT == "PHUTRANG_GC" || items.LoaiVT == "PHUKIEN"
                            || items.LoaiVT == "SPMAYKHAC" || items.LoaiVT == "NHUONGQUYEN")
                        {
                            item = new LotNumberedInventoryItem();
                        }
                        else
                        {
                            item = new InventoryItem();
                        }



                        var LOAISPCT = nstt_.VATTU2024_LOAD_LOAISPCHITIET(items.MAVT).ToList();
                        var f_LOAISPCT = LOAISPCT.Count < 1 ? "" : LOAISPCT.FirstOrDefault().externalid;

                        //   InventoryItem itemsa = new InventoryItem();

                        RecordRef form = new RecordRef();
                        form.internalId = int_form;
                        item.customForm = form;
                        item.pricesIncludeTax = true;
                        item.pricesIncludeTaxSpecified = true;

                        RecordRef purchaseTaxCode = new RecordRef();
                        purchaseTaxCode.internalId = taxcode;
                        item.purchaseTaxCode = purchaseTaxCode;

                        RecordRef salesTax = new RecordRef();
                        salesTax.internalId = taxcode;
                        item.salesTaxCode = salesTax;


                        RecordRef unittype = new RecordRef();
                        unittype.internalId = exiddvt;
                        item.unitsType = unittype;


                        item.externalId = items.MAVT;
                        item.itemId = items.MAVT;
                        item.upcCode = items.MAVT;
                        item.displayName = items.TenVT == null ? "" : items.TenVT;
                        item.salesDescription = (items.TenVT == null ? "" : (items.MAVT + "_" + items.TenVT));

                        var f_coa_mavt = vt_.DANHMUCVATTUs.FirstOrDefault(p => p.MAVT == items.MAVT);
                        var tt_asset = f_coa_mavt.ASSET;
                        var tt_cogs = f_coa_mavt.COGS;
                        var tt_income = f_coa_mavt.INCOME;
                        var COA = nstt_.Chart_of_Accounts.ToList();
                        var intid_asset = tt_asset == "" ? "" : COA.FirstOrDefault(p => p.Number_TT == tt_asset).InternalID_NS;
                        var intid_cogs = tt_cogs == "" ? "" : COA.FirstOrDefault(p => p.Number_TT == tt_cogs).InternalID_NS;
                        var intid_income = tt_income == "" ? "" : COA.FirstOrDefault(p => p.Number_TT == tt_income).InternalID_NS;

                        RecordRef asset = new RecordRef();
                        asset.internalId = intid_asset;
                        item.assetAccount = asset;

                        RecordRef cogs = new RecordRef();
                        cogs.internalId = intid_cogs;
                        item.cogsAccount = cogs;

                        RecordRef income = new RecordRef();
                        income.internalId = intid_income;
                        item.incomeAccount = income;

                        CustomFieldRef[] a = new CustomFieldRef[99];

                        SelectCustomFieldRef cust_loaisp = new SelectCustomFieldRef();
                        ListOrRecordRef List_loaisp = new ListOrRecordRef();
                        List_loaisp.externalId = item_LoaiSanPhams;
                        cust_loaisp.scriptId = "custitem_btm_tt_loai_san_pham";
                        cust_loaisp.value = List_loaisp;
                        a[1] = cust_loaisp;

                        SelectCustomFieldRef cust_phannhomsp = new SelectCustomFieldRef();
                        ListOrRecordRef List_phannhomsp = new ListOrRecordRef();
                        List_phannhomsp.externalId = item_PhanNhomSanPhams.Count < 1 ? "" : item_PhanNhomSanPhams.FirstOrDefault().externalid;
                        cust_phannhomsp.scriptId = "custitem_btm_tt_nhom_sp_may";
                        cust_phannhomsp.value = List_phannhomsp;
                        a[2] = cust_phannhomsp;

                        var LIST_WIPSTATUS = vt_.TBL_DMLOAIVT_COA.Where(p => p.LoaiVT == LOAIVTs).ToList();
                        var WIPSTATUS = LIST_WIPSTATUS.Count < 1 ? "2" : LIST_WIPSTATUS.FirstOrDefault().WIPSTATUS == "WIP" ? "1" : "2";

                        SelectCustomFieldRef cust_statuswip = new SelectCustomFieldRef();
                        ListOrRecordRef List_statuswip = new ListOrRecordRef();
                        List_statuswip.internalId = WIPSTATUS;
                        cust_statuswip.scriptId = "custitem_btm_mc_wip_status";
                        cust_statuswip.value = List_statuswip;
                        a[3] = cust_statuswip;

                        SelectCustomFieldRef cust_phamvi = new SelectCustomFieldRef();
                        ListOrRecordRef List_phamvi = new ListOrRecordRef();
                        List_phamvi.internalId = "2";
                        cust_phamvi.scriptId = "cseg_btm_tt_pv_bh";
                        cust_phamvi.value = List_phamvi;
                        a[4] = cust_phamvi;

                        if (items.LoaiVT == "MOC" || items.LoaiVT == "VAI" || items.LoaiVT == "QUANAO" || items.LoaiVT == "VAI_TT_DI_GC" || items.LoaiVT == "MOC_TT_DI_GC"
                        || items.LoaiVT == "PHUTRANG" || items.LoaiVT == "QUANAO_GC" || items.LoaiVT == "PHUTRANG_GC"
                        || items.LoaiVT == "PHUKIEN" || items.LoaiVT == "SPMAYKHAC" || items.LoaiVT == "NHUONGQUYEN")
                        {
                            StringCustomFieldRef cust_dichdanhLOT = new StringCustomFieldRef();
                            cust_dichdanhLOT.scriptId = "custitem_btm_mc_dich_danh_lot";
                            cust_dichdanhLOT.value = "T";
                            a[5] = cust_dichdanhLOT;
                        }

                        item.customFieldList = a;
                        WriteResponse addmahang = ns.Service.upsert(item);
                        var messerror = ((ToolsApp.com.netsuite.webservices.Status)addmahang.status).statusDetail;
                        var messdetail = messerror.FirstOrDefault().message;
                        var check = ((ToolsApp.com.netsuite.webservices.RecordRef)addmahang.baseRef).internalId;
                        var status = addmahang.status.isSuccess;
                        if (status == true)
                        {

                            var dmvt = vt_.DANHMUCVATTUs.Where(p => p.MAVT == items.MAVT).FirstOrDefault();
                            dmvt.GuiAPI = "1";
                            dmvt.HieuLuc = true;
                            dmvt.NgayGuiAPI = DateTime.Now;
                            vt_.Entry(dmvt).State = EntityState.Modified;
                            vt_.SaveChanges();
                            if (dmvt.LoaiVT == "QUANAO" || dmvt.LoaiVT == "PHUTRANG" || dmvt.LoaiVT == "QUANAO_GC" || dmvt.LoaiVT == "PHUTRANG_GC" || dmvt.LoaiVT == "SPMAYKHAC" || dmvt.LoaiVT == "PHUKIEN" || dmvt.LoaiVT == "NHUONGQUYEN")
                            {
                                SPMAY.INSERT_FROM_DANHMUCVATTU_TO_DMMAHANG_SPM2023_HANGMUANGOAI(items.MAVT);
                            }
                        }
                        else
                        {
                            GhiLog_DMVattu_ErrorAPI_NS log = new GhiLog_DMVattu_ErrorAPI_NS();
                            log.HanhDong = "btnDongboMAVT DANHMUC VATTU _ " + items.LoaiVT;
                            log.id = Guid.NewGuid();
                            log.Externalid = items.MAVT;
                            log.Internalid = check;
                            log.MAVT = items.MAVT;
                            log.TenVT = items.TenVT;
                            log.DVT = items.DVT;
                            log.LogNS = messdetail == null ? "" : messdetail.ToString();
                            log.MANV = User.UserName;
                            log.Ngaylog = DateTime.Now;
                            nstt_.GhiLog_DMVattu_ErrorAPI_NS.Add(log);
                            nstt_.SaveChanges();

                            return Json(new { status = -1, text = "Thêm không thành công" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    catch (Exception)
                    {
                        // log từng item để không crash cả batch
                        continue;
                    }
                }

                return Json(new { status = 1, text = "Đồng bộ thành công" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { status = 0, text = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #region Dispose , clean
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