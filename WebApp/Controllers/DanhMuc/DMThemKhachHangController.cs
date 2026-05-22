using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ToolsApp.Authentication;
using ToolsApp.EntityFramework.VatTu;
using ToolsApp.Models;

namespace ToolsApp.Controllers.DanhMuc
{
    public class DMThemKhachHangController : BaseController
    {
        private wqlvattuEntities vt_ = new wqlvattuEntities();
        // GET: DMThemKhachHang
        public ActionResult Index()
        {            
            return View();
        }
        public ActionResult _GetList(string MAKHSearch)
        {
            var data = vt_.KhachHangs.Where(p=> p.MaKH == "" || p.MaKH.Contains(MAKHSearch) || p.MaKH == null).ToList();
            ViewBag.List = data;
            return PartialView();
        }
        #region form insert
        public ActionResult _DetailForEdit(string MaKH = null)
        {            
            if (MaKH == null || MaKH == string.Empty)
            {
                return PartialView("_Insert", new KhachHangViewModels { MaKH = MaKH });
            }
            else
            {
                var model = vt_.KhachHangs.FirstOrDefault(c => c.MaKH == MaKH);
                ViewBag.data = model;
                return PartialView("_Update");
            }
        }
        #endregion
        #region Thêm dm khách hàng

        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _InsertFun(KhachHangViewModels model)
        {
            try
            {
                if (vt_.KhachHangs.FirstOrDefault(c => c.MaKH == model.MaKH) != null)
                {
                    return Json(new { status = -1, title = "", text = "Thêm không thành công. MaKH đã tồn tại. Kiểm tra lại.", obj = "" }, JsonRequestBehavior.AllowGet);
                }

                var model_copy = new KhachHang();
                model_copy.MaKH = model.MaKH;
                model_copy.TenKH = model.TenKH;
                model_copy.DiaChi = model.DiaChi;
                model_copy.MaSothue = model.MaSothue;
                model_copy.Ngaycapnhat = DateTime.Now;
                model_copy.Manvcapnhat = User.UserName;
                vt_.KhachHangs.Add(model_copy);

                vt_.SaveChanges();
                return Json(new { status = 1, title = "", text = "Thêm mới thành công", obj = "" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }

        }
        #endregion
        #region Sửa dm mã hàng + chi tiết

        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _EditFun(KhachHangViewModels model)
        {
            try
            {
                var makh = vt_.KhachHangs.FirstOrDefault(p=>p.MaKH == model.MaKH);
                if (makh != null)
                {
                    #region gán dữ liệu

                    makh.TenKH = model.TenKH;
                    makh.DiaChi = model.DiaChi;
                    makh.MaSothue = model.MaSothue;  
                    makh.Ngaymodify = DateTime.Now;
                    makh.Manvmodify = User.UserName;
                    vt_.Entry(makh).State = EntityState.Modified;
                    #endregion

                    vt_.SaveChanges();

                    return Json(new { status = 1, title = "", text = "Update thành công", obj = "" }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { status = -1, title = "", text = "Update không thành công", obj = "" }, JsonRequestBehavior.AllowGet);
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
        public JsonResult _DeleteFun(string MaKH)
        {
            try
            {
                var item = vt_.KhachHangs.FirstOrDefault(p => p.MaKH == MaKH);
                var imakho = item.MaKH;
                vt_.KhachHangs.Remove(item);
                vt_.SaveChanges();
                return Json(new { status = 1, title = "", text = "Xóa thành công.", obj = MaKH }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
    }
}