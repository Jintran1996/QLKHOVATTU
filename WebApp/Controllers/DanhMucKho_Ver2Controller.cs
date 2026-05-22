using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ToolsApp.Authentication;
using ToolsApp.EntityFramework.VatTu;
using ToolsApp.Models;

namespace ToolsApp.Controllers
{
    public class DanhMucKho_Ver2Controller : BaseController
    {
        private wqlvattuEntities vt_ = new wqlvattuEntities();
        // GET: DanhMucKho_Ver2
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult _GetList()
        {
            var List = vt_.DMKHoes.ToList();
            ViewBag.List = List;

            return PartialView();
        }
        public ActionResult _Insert()
        {

            #region list kho nhận
            var khoNhan = vt_.DMKHONHANs.ToList();
            ViewBag.khoNhan = khoNhan;
            #endregion

            #region list kho nhận
            var thuKho = vt_.Load_NhanVien().ToList();
            ViewBag.thuKho = thuKho;
            #endregion


            return PartialView();
        }

        public ActionResult _InsertQXNKho()
        {
            #region list mã kho
            var maKhoXN = vt_.DMKHoes.ToList();
            ViewBag.maKhoXN = maKhoXN;
            #endregion

            #region list mã nhân viên
            var maNVXN = vt_.Load_NhanVien().ToList();
            ViewBag.maNVXN = maNVXN;
            #endregion

            #region Ký hiệu đơn vị
            var kyHieuDVXN = vt_.load_kihieukho().ToList();
            ViewBag.kyHieuDVXN = kyHieuDVXN;
            #endregion

            #region list mã kho ĐVSD
            var maKhoSD = vt_.DMKHoes.ToList();
            ViewBag.maKhoSD = maKhoSD;
            #endregion

            #region list ký hiệu ĐVSD
            var kyHieuDVSD = vt_.load_kihieukho().ToList();
            ViewBag.kyHieuDVSD = kyHieuDVSD;
            #endregion 

            return PartialView();
        }

        public ActionResult _GetListQXNKho()
        {
            var ListQuyen = vt_.DANHMUCQUYENs.ToList();
            ViewBag.ListQuyen = ListQuyen;

            return PartialView();
        }

        #region Lưu kho
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _SaveFun(DanhMucKhoViewModels model)
        {
            try
            {
                #region check data client
                if (vt_.DMKHoes.FirstOrDefault(c => c.MAKHO == model.maKho) != null)
                {
                    return Json(new { status = -1, title = "", text = "Kho {0} đã tồn tại" + model.maKho, obj = "" }, JsonRequestBehavior.AllowGet);
                }
                if (model.maKho == null)
                {
                    return Json(new { status = -1, title = "", text = "Nhập mã kho", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                if (model.tenKho == null)
                {
                    return Json(new { status = -1, title = "", text = "Nhập tên kho", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                #endregion

                EntityFramework.VatTu.DMKHo dm_Kho = new EntityFramework.VatTu.DMKHo();
                dm_Kho.MAKHO = model.maKho;
                dm_Kho.TENKHO = model.tenKho;
                dm_Kho.DIACHI = model.diaChi;
                dm_Kho.MAKHOKETOAN = model.maKhoKT;
                dm_Kho.KHONHAN = model.khoNhan;
                dm_Kho.ThuKho = model.thuKho;
                dm_Kho.XUATNOIBO = model.xuatNB;
                dm_Kho.hieuluc = true;
                vt_.DMKHoes.Add(dm_Kho);
                vt_.SaveChanges();


                return Json(new { status = 1, title = "", text = "Thêm kho thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }

        }
        #endregion

        #region Delete 
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _DeleteFun(string maKho)
        {
            try
            {
                var item = vt_.DMKHoes.FirstOrDefault(p => p.MAKHO == maKho);
                var imakho = item.MAKHO;

                vt_.DMKHoes.Remove(item);
                vt_.SaveChanges();
                return Json(new { status = 1, title = "", text = "Xóa thành công.", obj = maKho }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Load View Edit CTphieu
        public ActionResult _Editview(string maKho)
        {


            var model = vt_.DMKHoes.FirstOrDefault(p => p.MAKHO == maKho);
            ViewBag.model = model;
            #region list kho nhận
            var khoNhan = vt_.DMKHONHANs.ToList();
            ViewBag.khoNhan = khoNhan;
            #endregion

            #region list kho nhận
            var thuKho = vt_.Load_NhanVien().ToList();
            ViewBag.thuKho = thuKho;
            #endregion

            var dmKho = vt_.DMKHoes.Where(p => p.MAKHO == model.MAKHO).FirstOrDefault();
            ViewBag.dmKho = dmKho;


            return PartialView("_Editview");
        }
        #endregion

        #region Lưu hiệu chỉnh kho
        public JsonResult _EditFunCT(DanhMucKhoViewModels model)
        {
            try
            {
                if (model.tenKho == null)
                {
                    return Json(new { status = -1, title = "", text = string.Format("Nhập tên kho"), obj = "" }, JsonRequestBehavior.AllowGet);
                }

                var dm_Kho = vt_.DMKHoes.FirstOrDefault(a => a.MAKHO == model.maKho);
                dm_Kho.TENKHO = model.tenKho;
                dm_Kho.DIACHI = model.diaChi;
                dm_Kho.MAKHOKETOAN = model.maKhoKT;
                dm_Kho.KHONHAN = model.khoNhan;
                dm_Kho.ThuKho = model.thuKho;
                dm_Kho.XUATNOIBO = model.xuatNB;
                vt_.Entry(dm_Kho).State = EntityState.Modified;
                vt_.SaveChanges();


                return Json(new { status = 1, title = "", text = "Cập nhật kho thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }

        }
        #endregion

        #region Lưu quyền xuất nhập kho
        public JsonResult _SaveXNFun(DanhMucQuyenViewModels model)
        {
            try
            {
                if (vt_.DANHMUCQUYENs.FirstOrDefault(c => c.MAKHO == model.maKho && c.MANV == model.maNV && c.KIHIEU == model.kyHieuDV) != null)
                {
                    return Json(new { status = -1, title = "", text = "Phân quyền thất bại. Mã nhân viên và ký hiệu đã có trong kho.", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                if (model.maKho == null || model.maKho == "")
                {
                    return Json(new { status = -1, title = "", text = "Chọn mã kho", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                if (model.maNV == null || model.maNV == "")
                {
                    return Json(new { status = -1, title = "", text = "Chọn mã nhân viên", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                if (model.kyHieuDV == null || model.kyHieuDV == "")
                {
                    return Json(new { status = -1, title = "", text = "Chọn ký hiệu đơn vị", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var dm_Quyen = new DANHMUCQUYEN()
                    {
                        MAKHO = model.maKho,
                        KIHIEU = model.kyHieuDV,
                        MANV = model.maNV,
                        QuyenLP = 1,
                        QuyenN = 1,
                        QuyenX = 1,
                    };
                    vt_.DANHMUCQUYENs.Add(dm_Quyen);
                    vt_.SaveChanges();
                }

                return Json(new { status = 1, title = "", text = "Phân quyền thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }

        }
        #endregion

        #region Lưu đơn vị sử dụng
        public JsonResult _SaveSDFun(tblKiHieuNhapKhoViewModels model)
        {
            try
            {
                if (vt_.tblKiHieuNhapKhoes.FirstOrDefault(c => c.MAKHO == model.maKho && c.KIHIEU == model.kyHieuDV) != null)
                {
                    return Json(new { status = -1, title = "", text = "Thêm thất bại. kí hiệu đã có trong kho.", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                if (model.maKho == null || model.maKho == "")
                {
                    return Json(new { status = -1, title = "", text = "Chọn mã kho", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                if (model.kyHieuDV == null || model.kyHieuDV == "")
                {
                    return Json(new { status = -1, title = "", text = "Chọn ký hiệu đơn vị", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var dm_KyHieuNhapKho = new tblKiHieuNhapKho()
                    {
                        MAKHO = model.maKho,
                        KIHIEU = model.kyHieuDV,
                    };
                    vt_.tblKiHieuNhapKhoes.Add(dm_KyHieuNhapKho);
                    vt_.SaveChanges();
                }

                return Json(new { status = 1, title = "", text = "Thêm thành công", obj = "" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }

        }
        #endregion

        #region Delete Quyền xuất nhập kho
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _DeleteXNFun(string maKho)
        {
            try
            {
                var item = vt_.DANHMUCQUYENs.FirstOrDefault(p => p.MAKHO == maKho);
                var imakho = item.MAKHO;

                vt_.DANHMUCQUYENs.Remove(item);
                vt_.SaveChanges();
                return Json(new { status = 1, title = "", text = "Xóa thành công.", obj = maKho }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
    }
}