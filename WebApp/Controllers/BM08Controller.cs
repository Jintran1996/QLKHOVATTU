using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ToolsApp.Authentication;
using ToolsApp.EntityFramework.VatTu;
using ToolsApp.Models;

namespace ToolsApp.Controllers
{
    [Authorize]
    public class BM08Controller : BaseController
    {
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
            ViewBag.List = dbvt.DMPHIEUx.Where(c => c.LOAIPHIEU == "05").Take(100).OrderByDescending(c => c.NgayYC).ToList();
            return PartialView();
        }
        #endregion

        #region _GetList chi tiết phiếu
        public ActionResult _GetListCTPhieu(string maphieu)
        {
            var ctphieu05 = dbvt.SP_SEARCHCTPHIEUBM05(maphieu, User.UserName).ToList();
            ViewBag.CTPHIEU05 = ctphieu05;
            return PartialView();
        }
        #endregion

        #region details chi tiết
        public ActionResult _Details(string maphieu)
        {
            var kihieu = dbvt.sp_kyhieudonvi_by_manv(User.UserName).FirstOrDefault().KiHieu;
            //var ctphieu05 = dbvt.SP_SEARCHCTPHIEUBM05(maphieu, User.UserName).ToList();
            var ctphieu05 = dbvt.SP_BM05TUDONVI(kihieu, maphieu, User.UserName).ToList();
            ViewBag.CTPHIEU05 = ctphieu05;

            return PartialView();
        }
        #endregion

        #region form insert
        public ActionResult _DetailForEdit(string maphieu = null)
        {
            var kyhieudv = dbvt.sp_kyhieudonvi_by_manv(User.UserName).FirstOrDefault().KiHieu;
            if (kyhieudv == "adm" /*|| User.UserName == "2464"*/)
            {
                #region NGUOIXEMXET
                var nguoixx = dbvt.SP_LOADNGUOIXEMXETFORWEB(User.UserName).ToList();
                ViewBag.NGUOIXX = nguoixx;
                #endregion

                #region LOAIVT
                var loaivt = dbvt.DMLOAIVTs.Where(c => c.HieuLuc == 1).ToList();
                ViewBag.LOAIVT = loaivt;
                #endregion

                #region CREATE STT + MAPHIEU
                ViewBag.STT = dbvt.BM05_CREATE_MAPHIEU_STT("", "", "").FirstOrDefault().STT;
                ViewBag.MAPHIEU = dbvt.BM05_CREATE_MAPHIEU_STT("", "", "").FirstOrDefault().maphieu;
                #endregion

                #region MAPHIEU
                var dsphieu = dbvt.SP_LoadPhieuBM05New("").ToList();
                ViewBag.DSPHIEU = dsphieu;
                #endregion

                #region MAVATTU
                var mavt = dbvt.SP_LOADVTPYCFORWEB_HANHCHANH("VPP", "").ToList();
                ViewBag.VATTU = mavt;
                #endregion

                #region CHIPHI
                var chiphi = dbvt.SP_LoadDanhMucChiPhi01().ToList();
                ViewBag.CHIPHI = chiphi;
                #endregion

                return PartialView("_Insert");
            }
            return new HttpStatusCodeResult(404);
        }
        #endregion

        #region Lưu phiếu

        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _InsertFun(DMPHIEUViewModels model)
        {
            try
            {
                #region check data client
                var thangyc = DateTime.Now.AddMonths(1).Month;
                var kehoachthangVPP = dbvt.DMPHIEUx.Where(c => c.LoaiVT == "VPP" && c.LOAIPHIEU == "05").OrderByDescending(c => c.NgayYC).FirstOrDefault().KEHOACHTHANG.Value.Month;
                var kehoachthangNYP = dbvt.DMPHIEUx.Where(c => c.LoaiVT == "NYP" && c.LOAIPHIEU == "05").OrderByDescending(c => c.NgayYC).FirstOrDefault().KEHOACHTHANG.Value.Month;
                if (thangyc == kehoachthangVPP && model.LOAIPHIEU == "VPP")
                {
                    return Json(new { status = -1, title = "", text = string.Format("Đã tạo phiếu VPP tháng này"), obj = "" }, JsonRequestBehavior.AllowGet);
                }
                if (thangyc == kehoachthangNYP && model.LOAIPHIEU == "NYP")
                {
                    return Json(new { status = -1, title = "", text = string.Format("Đã tạo phiếu NYP tháng này"), obj = "" }, JsonRequestBehavior.AllowGet);
                }
                if (model.MAPHIEU == null)
                {
                    return Json(new { status = -1, title = "", text = string.Format("Không tìm thấy mã phiếu"), obj = "" }, JsonRequestBehavior.AllowGet);
                }
                if (dbvt.DMPHIEUx.FirstOrDefault(c => c.MAPHIEU == model.MAPHIEU) != null)
                {
                    return Json(new { status = -1, title = "", text = string.Format("Phiếu đã tồn tại"), obj = "" }, JsonRequestBehavior.AllowGet);
                }
                if (model.LOAIPHIEU == null)
                {
                    return Json(new { status = -1, title = "", text = string.Format("Không tìm thấy loại phiếu", model.LOAIPHIEU), obj = "" }, JsonRequestBehavior.AllowGet);
                }
                #endregion

                var kyhieudv = dbvt.sp_kyhieudonvi_by_manv(User.UserName).FirstOrDefault().KiHieu;
                var nguoitn = dbvt.TBLTHUKYBANTGDs.FirstOrDefault(c => c.KYHIEUDONVI == kyhieudv).MATIEPNHAN;

                #region add data
                var dmphieu = new DMPHIEU();
                dmphieu.MAPHIEU = model.MAPHIEU;
                dmphieu.KEHOACHTHANG = model.KEHOACHTHANG;
                dmphieu.Nam = model.Nam;
                dmphieu.STT = model.STT;
                dmphieu.LOAIPHIEU = "05";
                dmphieu.NguoiXX = model.NguoiXX;
                dmphieu.NguoiTiepNhan = nguoitn;
                dmphieu.LoaiVT = model.LOAIPHIEU;
                dmphieu.NgayYC = DateTime.Now;
                dmphieu.NguoiLapPhieu = User.UserName;
                dmphieu.LyDoSD = model.LOAIPHIEU;
                dmphieu.DX = 0;
                dmphieu.NK = 0;
                dmphieu.ND = 1;
                dmphieu.KH = 1;
                dmphieu.HieuLuc = 1;
                dmphieu.KyHieu = "adm";
                dmphieu.NgayXX = new DateTime(1900, 01, 01, 00, 00, 00, 000);
                dmphieu.NgayPD = new DateTime(1900, 01, 01, 00, 00, 00, 000);
                dmphieu.NgayTN = new DateTime(1900, 01, 01, 00, 00, 00, 000);
                dmphieu.XXet = 0;
                dmphieu.PDuyet = 0;
                dmphieu.TNhanYC = 0;
                #endregion

                dbvt.DMPHIEUx.Add(dmphieu);
                dbvt.SaveChanges();

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
        public JsonResult _InsertCTPhieuFun(CTPHIEUViewModels model)
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
                var slton = dbvt.SP_SoLuongTonKhoVT(model.MAVT).FirstOrDefault().Value;
                var mabp = dbvt.sp_kyhieudonvi_by_manv(User.UserName).FirstOrDefault().MaDonVi;

                #region check data client
                if (model.MAPHIEU == null)
                {
                    return Json(new { status = -1, title = "", text = string.Format("Không tìm thấy mã phiếu"), obj = "" }, JsonRequestBehavior.AllowGet);
                }
                if (dbvt.CTPHIEU05.FirstOrDefault(c => c.MAVT == model.MAVT && c.MaBP == mabp) != null)
                {
                    return Json(new { status = 2, title = "", text = string.Format("Vật tư {0} cho đơn vị {1} đã được đặt.", model.MAVT, mabp), obj = "" }, JsonRequestBehavior.AllowGet);
                }
                #endregion
                

                var insertctphieu = dbvt.SP_ADDCHITIETPHIEUYEUCAU05(model.STT, model.MAVT, model.SLYCAU, model.MASOCP, model.TINHTRANG, model.THOIDIEMSD
                                                                , model.GHICHU, model.MAPHIEU, slton, User.UserName, mabp, model.DACTINHKYTHUAT);


                return Json(new { status = 1, title = "", text = "Đã lưu.", obj = "" }, JsonRequestBehavior.AllowGet);
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
                var dmphieu05 = dbvt.DMPHIEUx.Where(c => c.MAPHIEU == maphieu && c.NguoiLapPhieu == User.UserName).FirstOrDefault();
                if (dmphieu05 != null)
                {
                    dbvt.DMPHIEUx.Remove(dmphieu05);
                    dbvt.SaveChanges();

                    return Json(new { status = 1, title = "", text = "Xóa thành công.", obj = "" }, JsonRequestBehavior.AllowGet);

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
        public JsonResult _DeleteCTFun(string maphieu, string mavt)
        {
            try
            {
                var ctphieu05 = dbvt.CTPHIEU05.Where(c => c.MAVT == mavt && c.MAPHIEU == maphieu).ToList();
                if(ctphieu05 != null)
                {
                    dbvt.CTPHIEU05_DELETE(maphieu, mavt, User.UserName);
                    dbvt.SaveChanges();

                    return Json(new { status = 1, title = "", text = "Xóa thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
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
        public JsonResult LoadMaVatTu(string loaiyc, string mavt)
        {
            var vattu = dbvt.SP_LOADVTPYCFORWEB_HANHCHANH(loaiyc, mavt).ToList();

            return Json(new { status = 1, text = "", obj = vattu }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadDSMaPhieu()
        {
            var dsphieu = dbvt.SP_LoadPhieuBM05New("").ToList();

            return Json(new { status = 1, text = "", obj = dsphieu }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult CreateSTT_MAPHIEU()
        {
            #region CREATE STT + MAPHIEU
            var STT = dbvt.BM05_CREATE_MAPHIEU_STT("", "", "").FirstOrDefault().STT;
            var MAPHIEU = dbvt.BM05_CREATE_MAPHIEU_STT("", "", "").FirstOrDefault().maphieu;
            #endregion

            return Json(new { status = 1, text = "", objSTT = STT, objMAPHIEU = MAPHIEU }, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}