using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ToolsApp.App_Start;
using ToolsApp.Authentication;
using ToolsApp.EntityFramework.KhoSoi;
using ToolsApp.Models;
using ToolsApp.Utilities;

namespace ToolsApp.Areas.Admin.Controllers
{
    [Authorize]
    [CustomAuthorize(Function = "PageManagement/Index")]
    public class PageManagementController : BaseController
    {
        wqlkhosoiEntities db_ = new wqlkhosoiEntities();
        // GET: UserManagement
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult _GetList()
        {
            ViewBag.List = db_.QLKhoSoi_Page.Where(p => p.isDelete == null || p.isDelete == false).OrderBy(p => p.Code).ToList();
            return PartialView(new HomePageViewModel());
        }
        public ActionResult _Insert(int Id)
        {
            return PartialView(new HomePageViewModel { Id = 0 });
        }
        public ActionResult _Edit(int Id)
        {
            var model = db_.QLKhoSoi_Page.FirstOrDefault(p => p.Id == Id);
            return PartialView(Mapper.MapFrom(model));
        }
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _InsertFun(HomePageViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var model_copy = Mapper.MapFrom(model);
                    model_copy.DatetimeCreate = DateTime.Now;
                    model_copy.UserCreate = User.UserName;
                    model_copy.UserFullnameCreate = User.FullName;

                    model_copy.DatetimeUpdate = DateTime.Now;
                    model_copy.UserUpdate = User.UserName;
                    model_copy.UserFullnameUpdate = User.FullName;

                    db_.QLKhoSoi_Page.Add(model_copy);
                    db_.SaveChanges();

                    return Json(new { status = 1, title = "", text = "Created.", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new { status = -1, title = "", text = "Error: " + UtilsLocal.ModelStateError(ModelState), obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _EditFun(HomePageViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var item = db_.QLKhoSoi_Page.FirstOrDefault(p => p.Id == model.Id);
                    item.Code = model.Code;
                    item.Info = model.Info;
                    item.Controler = model.Controler;
                    item.Action = model.Action;
                    item.DatetimeUpdate = DateTime.Now;
                    item.UserUpdate = User.UserName;
                    item.UserFullnameUpdate = User.FullName;

                    db_.Entry(item).State = EntityState.Modified;
                    db_.SaveChanges();
                    return Json(new { status = 1, title = "", text = "Updated.", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new { status = -1, title = "", text = "Error: " + UtilsLocal.ModelStateError(ModelState), obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        [ValidateInput(false)]
        [HttpPost]
        public ActionResult _DeleteFun(int Id)
        {
            try
            {
                var item = db_.QLKhoSoi_Page.FirstOrDefault(p => p.Id == Id);

                item.isDelete = true;
                item.DatetimeUpdate = DateTime.Now;
                item.UserUpdate = User.UserName;
                item.UserFullnameUpdate = User.FullName;
                db_.Entry(item).State = EntityState.Modified;
                db_.SaveChanges();
                return Json(new { status = 1, title = "", text = "Deleted.", obj = "" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}