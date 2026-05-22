using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using ToolsApp.Authentication;
using ToolsApp.EntityFramework.KhoSoi;
using ToolsApp.EntityFramework.VatTu;
using ToolsApp.Models;

namespace ToolsApp.Controllers
{
    [Authorize]
    public class PhieuYCMuaSoiController : BaseController
    {
        private wqlkhosoiEntities dbks = new wqlkhosoiEntities();
        private wqlvattuEntities dbvt = new wqlvattuEntities();

        #region Index
        public ActionResult Index()
        {
            return View();
        }
        #endregion

        #region _GetList
        public ActionResult _GetList(string maphieu)
        {

            var model = dbks.NL_DMPHIEU.Where(p =>
                ((maphieu == null || maphieu == "" || p.MAPHIEU.Contains(maphieu))
                  && (p.NGAYYC.Value.Year >= 2023)
                )).OrderByDescending(c => c.NGAYYC).ToList();
            ViewBag.List = model;

            return PartialView();
        }
        #endregion

        #region _GetList chi tiết phiếu
        public ActionResult _GetListCTPhieu(string maphieu)
        {

            var model = dbks.NL_CTPHIEU.Where(p => p.MAPHIEU==maphieu).ToList();
            ViewBag.ListCT = model;

            return PartialView();
        }
        #endregion

        #region details chi tiết
        public ActionResult _Details(string maphieu)
        {
            var model = dbks.NL_CTPHIEU.Where(p => p.MAPHIEU == maphieu).ToList();
            ViewBag.ListCT = model;

            return PartialView();
        }
        #endregion

        #region form insert
        public ActionResult _DetailForEdit(string maphieu = null)
        {
            var kyhieu = dbvt.sp_kyhieudonvi_by_manv(User.UserName).FirstOrDefault().KiHieu;
            ViewBag.KYHIEUDV = kyhieu;
            ViewBag.MAPHIEU = dbks.SP_TAOMAPHIEUYEUCAUMUAHANG(kyhieu).FirstOrDefault().MAPHIEU;
            ViewBag.NGUOIXX = dbvt.SP_LOADXEMXETBYMANV(User.UserName).ToList();
            var loaidh = dbks.sp_Load_NL_DMloaidonhang_2023().ToList();
            ViewBag.LOAIDH = loaidh;
            ViewBag.DMSOI = dbks.SP_LOAD_DANHMUCSOI_2023("").ToList();
            ViewBag.HIEU = dbks.sp_Load_NL_DMNHACCAP_2023().ToList();
            ViewBag.DONVIYC = dbvt.SP_DonVi().ToList();
            ViewBag.MASOCHIPHI = dbvt.SP_NL_LOADMSCP_2023().ToList();
            ViewBag.LOAIYC = dbks.spLoad_NL_DMYEUCAU_ByLoaidonhang(loaidh.FirstOrDefault().LOAIDONHANG).ToList();

            if (maphieu == null || maphieu == string.Empty)
            {
                return PartialView("_Insert", new PhieuYCMuaHangViewModels { MAPHIEU = maphieu });
            }
            else
            {
                ViewBag.PHIEU = dbks.NL_DMPHIEU.Where(c => c.MAPHIEU == maphieu && c.XXET == 0).Select(c => c.MAPHIEU).ToList();
                ViewBag.CTPHIEU = dbks.NL_CTPHIEU.Where(c => c.MAPHIEU == maphieu).ToList();

                return PartialView("_Update", new PhieuYCMuaHangViewModels { MAPHIEU = maphieu });
            }

        }
        #endregion

        #region form Update
        public ActionResult _Update(string maphieu = null)
        {
            var kyhieu = dbvt.sp_kyhieudonvi_by_manv(User.UserName).FirstOrDefault().KiHieu;
            ViewBag.KYHIEUDV = kyhieu;
            ViewBag.MAPHIEU = dbks.SP_TAOMAPHIEUYEUCAUMUAHANG(kyhieu).FirstOrDefault().MAPHIEU;
            ViewBag.NGUOIXX = dbvt.SP_LOADXEMXETBYMANV(User.UserName).ToList();
            var loaidh = dbks.sp_Load_NL_DMloaidonhang_2023().ToList();
            ViewBag.LOAIDH = loaidh;
            ViewBag.DMSOI = dbks.SP_LOAD_DANHMUCSOI_2023("").ToList();
            ViewBag.HIEU = dbks.sp_Load_NL_DMNHACCAP_2023().ToList();
            ViewBag.DONVIYC = dbvt.SP_DonVi().ToList();
            ViewBag.MASOCHIPHI = dbvt.SP_NL_LOADMSCP_2023().ToList();
            ViewBag.LOAIYC = dbks.spLoad_NL_DMYEUCAU_ByLoaidonhang(loaidh.FirstOrDefault().LOAIDONHANG).ToList();
            ViewBag.PHIEU = dbks.NL_DMPHIEU.Where(c => c.XXET == 0 && c.MAPHIEU.StartsWith(kyhieu)).Select(c => c.MAPHIEU).ToList();

            return PartialView("_Update", new PhieuYCMuaHangViewModels { MAPHIEU = maphieu });

        }
        #endregion

        #region Lưu phiếu

        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _InsertFun(PhieuYCMuaHangViewModels model)
        {
            try
            {
                #region check data client
                if (dbks.NL_DMPHIEU.FirstOrDefault(c => c.MAPHIEU == model.MAPHIEU) != null)
                {
                    return Json(new { status = 2, title = "", text = string.Format("Phiếu {0} đã được lập, có thể cập nhật chi tiết phiếu.", model.MAPHIEU), obj = "" }, JsonRequestBehavior.AllowGet);
                }
                if (dbks.NL_DMPHIEU.FirstOrDefault(c => c.MAPHIEU == model.MAPHIEU && c.XXET == 1) != null)
                {
                    return Json(new { status = -1, title = "", text = string.Format("Phiếu {0} đã được xét duyệt. Vui lòng tạo phiếu mới", model.MAPHIEU), obj = "" }, JsonRequestBehavior.AllowGet);
                }
                if (model.MAPHIEU == null)
                {
                    return Json(new { status = -1, title = "", text = string.Format("Không tạo được mã phiếu yêu cầu", model.MAPHIEU), obj = "" }, JsonRequestBehavior.AllowGet);
                }
                if (model.NGUOIXX == null)
                {
                    return Json(new { status = -1, title = "", text = string.Format("Không tìm thấy người xem xét", model.MAPHIEU), obj = "" }, JsonRequestBehavior.AllowGet);
                }
                if (model.LOAIDONHANG == null)
                {
                    return Json(new { status = -1, title = "", text = string.Format("Không tìm thấy Loại đơn hàng", model.MAPHIEU), obj = "" }, JsonRequestBehavior.AllowGet);
                }
                if (model.LOAIDONHANG == null)
                {
                    return Json(new { status = -1, title = "", text = string.Format("Không tìm thấy Loại đơn hàng", model.MAPHIEU), obj = "" }, JsonRequestBehavior.AllowGet);
                }
                #endregion

                EntityFramework.KhoSoi.NL_DMPHIEU dMPHIEU = new EntityFramework.KhoSoi.NL_DMPHIEU();
                dMPHIEU.MAPHIEU = model.MAPHIEU;
                dMPHIEU.NGUOIXX = model.NGUOIXX;
                dMPHIEU.NGUOILAPPHIEU = User.UserName;
                dMPHIEU.LOAIDONHANG = model.LOAIDONHANG;
                dMPHIEU.ND = model.ND.ToString() == "ND" ? (byte?)1 : 0;
                dMPHIEU.NK = model.ND.ToString() == "NK" ? (byte?)1 : 0;
                dMPHIEU.KH = model.KH.ToString() == "KH" ? (byte?)1 : 0;
                dMPHIEU.DX = model.KH.ToString() == "DX" ? (byte?)1 : 0;
                dMPHIEU.LYDOSD = model.LYDOSD;
                dMPHIEU.LOAIPHIEU = "03";
                dMPHIEU.NGAYYC = DateTime.Now;
                dMPHIEU.NGAYXX = new DateTime(1900, 01, 01, 00, 00, 00, 000);
                dMPHIEU.NGAYTN = new DateTime(1900, 01, 01, 00, 00, 00, 000);
                dMPHIEU.NGAYPD = new DateTime(1900, 01, 01, 00, 00, 00, 000);
                dMPHIEU.XXET = 0;
                dMPHIEU.PDUYET = 0;
                dMPHIEU.TNHANYC = 0;
                dMPHIEU.NGUOIPD = "";
                dMPHIEU.NGUOITIEPNHAN = "";
                dMPHIEU.MAPHIEUPD = "";
                dMPHIEU.HIEULUC = false;
                dMPHIEU.NumberCheck = 0;
                dMPHIEU.FinishCheck = false;

                dbks.NL_DMPHIEU.Add(dMPHIEU);
                dbks.SaveChanges();


                return Json(new { status = 1, title = "", text = "Lập phiếu hoàn tất.", obj = "" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }

        }
        #endregion

        #region Lưu chi tiết phiếu

        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _InsertCTPhieuFun(CTPhieuYCMuaHangViewModels model)
        {
            #region Xử lý ngày
            CultureInfo cul = CultureInfo.GetCultureInfo("en-GB");
            var NgayLapTuSearch_ = new DateTime();
            if (!string.IsNullOrEmpty(model.THOIDIEMSD_))
            {
                try
                {
                    NgayLapTuSearch_ = DateTime.ParseExact(model.THOIDIEMSD_, "dd/MM/yyyy", cul);

                    model.THOIDIEMSD = new DateTime(NgayLapTuSearch_.Year,
                        NgayLapTuSearch_.Month, NgayLapTuSearch_.Day, 0, 0, 0);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
            #endregion

            try
            {
                #region check data client
                if (dbks.NL_DMPHIEU.FirstOrDefault(c => c.MAPHIEU == model.MAPHIEU) == null)
                {
                    return Json(new { status = -1, title = "", text = string.Format("Vui lòng tạo phiếu trước khi thêm chi tiết phiếu", model.MAPHIEU), obj = "" }, JsonRequestBehavior.AllowGet);
                }
                if (model.MAPHIEU == null)
                {
                    return Json(new { status = -1, title = "", text = string.Format("Không tạo được mã phiếu yêu cầu"), obj = "" }, JsonRequestBehavior.AllowGet);
                }
                if (model.MAVT == null)
                {
                    return Json(new { status = -1, title = "", text = string.Format("Không tìm thấy mã vật tư"), obj = "" }, JsonRequestBehavior.AllowGet);
                }
                if (model.HIEU == null)
                {
                    return Json(new { status = -1, title = "", text = string.Format("Không tìm thấy nhãn hiệu"), obj = "" }, JsonRequestBehavior.AllowGet);
                }
                if (model.SLYCAU == null || model.SLYCAU <= 0)
                {
                    return Json(new { status = -1, title = "", text = string.Format("Số lượng yêu cầu phải lớn hơn 0"), obj = "" }, JsonRequestBehavior.AllowGet);
                }
                if (model.LOAIYEUCAU == null)
                {
                    return Json(new { status = -1, title = "", text = string.Format("Không tìm thấy loại yêu cầu"), obj = "" }, JsonRequestBehavior.AllowGet);
                }
                if (model.IDMaDV_YCMUA == null)
                {
                    return Json(new { status = -1, title = "", text = string.Format("Không tìm thấy đơn vị yêu cầu mua"), obj = "" }, JsonRequestBehavior.AllowGet);
                }
             
                if (dbks.NL_CTPHIEU.FirstOrDefault(c => c.MAVT == model.MAVT && c.IDMaDV_YCMUA == model.IDMaDV_YCMUA && c.MAPHIEU == model.MAPHIEU) != null)
                {
                    return Json(new { status = -1, title = "", text = string.Format("Trùng mã vật tư {0}, đơn vị {1}", model.MAVT, model.IDMaDV_YCMUA), obj = "" }, JsonRequestBehavior.AllowGet);
                }
                #endregion

                var tonObj = dbks
         .VATTU2025_LOADTONKHO_THEOMAVT_SOI(model.MAVT, model.HIEU)
         .FirstOrDefault();

                decimal slton = tonObj?.SLTON ?? 0;


                EntityFramework.KhoSoi.NL_CTPHIEU ct = new EntityFramework.KhoSoi.NL_CTPHIEU();
                ct.MAPHIEU = model.MAPHIEU;            
                ct.MAVT = model.MAVT;
                ct.SLYCAU = model.SLYCAU;
                ct.SLXXET = model.SLYCAU;
                ct.SLPD = model.SLYCAU;
                ct.SLTONDVI = slton;
                ct.SLTONCTY = slton;
                ct.THOIDIEMSD = model.THOIDIEMSD;
                ct.GHICHU = model.GHICHU;
                ct.LO = "";
                ct.HIEU = model.HIEU;
                ct.NK_STATIC = true;
                ct.IDMaDV_YCMUA = model.IDMaDV_YCMUA;
                ct.LOAIYEUCAU = model.LOAIYEUCAU;
                ct.KHOATHAMCHIEU = model.MAPHIEU + "-" + model.MAVT + "-" + DateTime.Now.Ticks.ToString().ToUpper();

                dbks.NL_CTPHIEU.Add(ct);
                dbks.SaveChanges();


                return Json(new { status = 1, title = "", text = "Thêm mới thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }

        }
        #endregion

        #region Sửa chi tiết

        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _EditFun(PhieuYCMuaHangViewModels model)
        {
            try
            {
                var phieu = dbks.NL_DMPHIEU.FirstOrDefault(c => c.MAPHIEU == model.MAPHIEU);
                if (phieu != null)
                {
                    #region gán dữ liệu
                    phieu.MAPHIEU = model.MAPHIEU;
                    List<EntityFramework.KhoSoi.NL_CTPHIEU> ctphieu = new List<EntityFramework.KhoSoi.NL_CTPHIEU>();
                    foreach (var item in model.Detail)
                    {
                        EntityFramework.KhoSoi.NL_CTPHIEU ct = new EntityFramework.KhoSoi.NL_CTPHIEU();
                        ct.MADH = item.MADH;
                        ct.MAVT = item.MAVT;
                        ct.MAPHIEU = model.MAPHIEU;

                        ctphieu.Add(ct);
                    }
                    phieu.NL_CTPHIEU = ctphieu;

                    dbks.SaveChanges();

                    #endregion

                    dbks.SaveChanges();

                    return Json(new { status = 1, title = "", text = "Cập nhật thành công", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { status = -1, title = "", text = "Cập nhật thất bại", obj = "" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }

        }
        #endregion

        #region Xóa phiếu
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _DeleteFun(string maphieu)
        {
            try
            {
                var phieu = dbks.NL_DMPHIEU.Find(maphieu);
                if (phieu != null)
                {
                    if(phieu.NGUOILAPPHIEU != User.UserName)
                    {
                        return Json(new { status = -1, title = "", text = string.Format("Tài khoản {0} không có quyền xóa phiếu {1}.",User.UserName, maphieu), obj = "" }, JsonRequestBehavior.AllowGet);
                    }
                    dbks.NL_DMPHIEU.Remove(phieu);
                    dbks.SaveChanges();

                    return Json(new { status = 1, title = "", text = "Xóa thành công", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { status = -1, title = "", text = "Không tìm thấy phiếu.", obj = "" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = ex.Message, text = "Xóa thất bại. Vui lòng xóa chi tiết phiếu trước.", obj = "" }, JsonRequestBehavior.AllowGet);
            }

        }
        #endregion

        #region Xóa chi tiết phiếu
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _DeleteCTFun(string khoathamchieu)
        {
            try
            {
                var ctphieu = dbks.NL_CTPHIEU.Find(khoathamchieu);
                if (ctphieu != null)
                {
                    dbks.NL_CTPHIEU.Remove(ctphieu);
                    dbks.SaveChanges();

                    return Json(new { status = 1, title = "", text = "Xóa thành công", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { status = -1, title = "", text = "Không tìm thấy mã vật tư", obj = "" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }

        }
        #endregion

        #region onchange
        public JsonResult LoaiYeuCau(string loaidh, string maphieu)
        {
            var loaiyc = dbks.spLoad_NL_DMYEUCAU_ByLoaidonhang(loaidh).ToList();
            if (maphieu != null)
            {
                var loaiycdmphieu = dbks.NL_DMPHIEU.FirstOrDefault(c => c.MAPHIEU == maphieu).LOAIDONHANG;
                loaiyc = dbks.spLoad_NL_DMYEUCAU_ByLoaidonhang(loaiycdmphieu).ToList();
            }
            return Json(new { status = 1, text = "", obj = loaiyc }, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}