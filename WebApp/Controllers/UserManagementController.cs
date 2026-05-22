using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ToolsApp.App_Start;
using ToolsApp.Authentication;
using ToolsApp.EntityFramework.KhoSoi;
using ToolsApp.EntityFramework.VatTu;
using ToolsApp.Models;
using ToolsApp.Utilities;

namespace ToolsApp.Areas.Admin.Controllers
{
    [Authorize]
    [CustomAuthorize(Function = "UserManagement/Index")]
    public class UserManagementController : BaseController
    {
        wqlkhosoiEntities db_ = new wqlkhosoiEntities();

        wqlvattuEntities dbVatTu_ = new wqlvattuEntities();
        // GET: UserManagement
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult _GetList()
        {
            var List = db_.QLKhoSoi_User.Where(p => p.isDelete == null || p.isDelete == false).OrderBy(p => p.SalesRepCode).ToList();

            var userIds = List.Select(p => p.Username).ToList();

            ViewBag.List = List;

            return PartialView(new HomeUserViewModel());
        }
        public ActionResult _Insert(int Id)
        {
            var users = db_.QLKhoSoi_User.Select(p => p.Username).ToList();

            var Active = EnumLocal.StringActiveCode.A;

            #region Load user from DKSH db
            var allUsers = dbVatTu_.DMNHANVIENs.Where(p => p.HieuLuc == Active &&
                !users.Contains(p.MANV)
            ).ToList();
            #endregion

            ViewBag.allUsers = allUsers;

            #region Load role
            var roles = db_.QLKhoSoi_Role.ToList();
            ViewBag.roles = roles;
            #endregion

            #region Load page
            var pages = db_.QLKhoSoi_Page.ToList();
            ViewBag.pages = pages;
            #endregion            

            return PartialView(new HomeUserViewModel { Id = 0 });
        }

        public ActionResult _InsertKeThua(int Id)
        {
            var users = db_.QLKhoSoi_User.Select(p => p.Username).ToList();
            var QLKhoSoi_User = db_.QLKhoSoi_User.ToList();
            ViewBag.QLKhoSoi_User = QLKhoSoi_User;
            var Active = EnumLocal.StringActiveCode.A;

            #region Load user from DKSH db
            var allUsers = dbVatTu_.DMNHANVIENs.Where(p => p.HieuLuc == Active
            ).ToList();
            #endregion

            ViewBag.allUsers = allUsers;

            #region Load role
            var roles = db_.QLKhoSoi_Role.ToList();
            ViewBag.roles = roles;
            #endregion

            #region Load page
            var pages = db_.QLKhoSoi_Page.ToList();
            ViewBag.pages = pages;
            #endregion            

            return PartialView(new HomeUserViewModel { Id = 0 });
        }

        #region Mở form _InsertChangepassword
        public ActionResult _InsertChangepassword(int Id)
        {
            return PartialView(new HomeUserViewModel { Id = 0 });
        }

        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _InsertFunChangepassword(ChangePasswordViewModel model)
        {
            //if (ModelState.IsValid)
            //{
            try
            {
                #region item
                var user = dbVatTu_.DMNHANVIENs.FirstOrDefault(p => p.MANV == model.ChangePwd_MANV);
                    if (user != null)
                    {
                        if (model.ChangePwd_Password != model.ReChangePwd_Password)
                        {
                            return Json(new { status = -1, title = "", text = "Mật khẩu lặp lại không trùng khớp !", obj = "" }, JsonRequestBehavior.AllowGet);
                        }
                        var pass = ToolsApp.Utilities.UtilsLocal.mahoaS(model.ChangePwd_Password.Trim());

                        user.Passwordid = pass;
                        user.NgayHieuLucPass = DateTime.Now.AddYears(2);
                        dbVatTu_.Entry(user).State = EntityState.Modified;
                        dbVatTu_.SaveChanges();




                        return Json(new { status = 1, title = "", text = "Đổi mật khẩu thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
                    }
                    #endregion

                    return Json(new { status = -1, title = "", text = "User không tồn tại!", obj = "" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
            //}
            //else
            //{
            //    var message = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

            //    return Json(new { status = -1, title = "", text = "Error: " + message, obj = "" }, JsonRequestBehavior.AllowGet);
            //}
        }
        #endregion
        public ActionResult _Edit(int Id)
        {
            var model = db_.QLKhoSoi_User.FirstOrDefault(p => p.Id == Id);

            #region Load role
            var roles = db_.QLKhoSoi_Role.ToList();
            ViewBag.roles = roles;
            #endregion

            #region Load page
            var pages = db_.QLKhoSoi_Page.ToList();
            ViewBag.pages = pages;
            #endregion                                    

            return PartialView(model);
        }
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _InsertFun(HomeUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    #region item
                    var user = dbVatTu_.DMNHANVIENs.FirstOrDefault(p => p.MANV == model.Username);
                    if (user != null)
                    {
                        var model_copy = Mapper.MapFrom(model);
                        model_copy.Fullname = user.HOTEN;
                        model_copy.Email = user.EMail;
                        model_copy.DatetimeCreate = DateTime.Now;
                        model_copy.UserCreate = User.UserName;
                        model_copy.UserFullnameCreate = User.FullName;
                        model_copy.DatetimeUpdate = DateTime.Now;
                        model_copy.UserUpdate = User.UserName;
                        model_copy.UserFullnameUpdate = User.FullName;

                        #region Add page
                        if (model.Pages != null && model.Pages.Length > 0)
                        {
                            model_copy.QLKhoSoi_User_Page = new List<QLKhoSoi_User_Page>();

                            var Pages = model.Pages.Split(',');

                            foreach (var item in Pages)
                            {
                                var data = new QLKhoSoi_User_Page
                                {
                                    PageId = int.Parse(item),
                                    DatetimeCreate = DateTime.Now,
                                    UserCreate = User.UserName,
                                    UserFullnameCreate = User.FullName,
                                    DatetimeUpdate = DateTime.Now,
                                    UserUpdate = User.UserName,
                                    UserFullnameUpdate = User.FullName
                                };

                                model_copy.QLKhoSoi_User_Page.Add(data);
                            }
                        }
                        #endregion

                        #region Add role
                        if (model.Roles != null && model.Roles.Length > 0)
                        {
                            model_copy.QLKhoSoi_User_Role = new List<QLKhoSoi_User_Role>();

                            var Roles = model.Roles.Split(',');

                            foreach (var item in Roles)
                            {
                                var data = new QLKhoSoi_User_Role
                                {
                                    RoleId = int.Parse(item),
                                    DatetimeCreate = DateTime.Now,
                                    UserCreate = User.UserName,
                                    UserFullnameCreate = User.FullName,
                                    DatetimeUpdate = DateTime.Now,
                                    UserUpdate = User.UserName,
                                    UserFullnameUpdate = User.FullName
                                };

                                model_copy.QLKhoSoi_User_Role.Add(data);
                            }
                        }
                        #endregion                        

                        db_.QLKhoSoi_User.Add(model_copy);
                        db_.SaveChanges();

                        return Json(new { status = 1, title = "", text = "Created.", obj = "" }, JsonRequestBehavior.AllowGet);
                    }
                    #endregion

                    return Json(new { status = -1, title = "", text = "User not exsist!", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                var message = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

                return Json(new { status = -1, title = "", text = "Error: " + message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _InsertFunKeThua(HomeUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (model.LoaiKethua == "2")
                    {
                        var items_userkethuanhanh = dbVatTu_.SP_Load_kethuaquyen_User().ToList();
                        foreach (var item in items_userkethuanhanh)
                        {


                            var users = db_.QLKhoSoi_User.Where(p => p.Username == model.Username).ToList();
                            if (users.Count < 1)
                            {
                                #region add new user
                                var user = dbVatTu_.DMNHANVIENs.FirstOrDefault(p => p.MANV == item.MaNV);
                                if (user != null)
                                {
                                    var model_copy = new QLKhoSoi_User();
                                    model_copy.Username = model.UserKeThua;
                                    model_copy.Fullname = user.HOTEN;
                                    model_copy.Email = user.EMail;
                                    model_copy.DatetimeCreate = DateTime.Now;
                                    model_copy.UserCreate = User.UserName;
                                    model_copy.UserFullnameCreate = User.FullName;
                                    model_copy.DatetimeUpdate = DateTime.Now;
                                    model_copy.UserUpdate = User.UserName;
                                    model_copy.UserFullnameUpdate = User.FullName;

                                    db_.QLKhoSoi_User.Add(model_copy);
                                    db_.SaveChanges();
                                }
                                #endregion

                            }
                            #region Kế thừa user cũ
                            var listusersys = db_.QLKhoSoi_User.Where(c => c.Username == item.MaNV);
                            if (listusersys != null)
                            {
                                db_.Sp_KethuaQuyenUser_KhoVT_Soi2023(model.Username, item.MaNV);
                            }
                            #endregion
                        }
                    }
                    else
                    {



                        var users = db_.QLKhoSoi_User.Where(p => p.Username == model.Username).ToList();
                        if (users.Count < 1)
                        {
                            #region add new user
                            var user = dbVatTu_.DMNHANVIENs.FirstOrDefault(p => p.MANV == model.UserKeThua);
                            if (user != null)
                            {
                                var model_copy = new QLKhoSoi_User();
                                model_copy.Username = model.UserKeThua;
                                model_copy.Fullname = user.HOTEN;
                                model_copy.Email = user.EMail;
                                model_copy.DatetimeCreate = DateTime.Now;
                                model_copy.UserCreate = User.UserName;
                                model_copy.UserFullnameCreate = User.FullName;
                                model_copy.DatetimeUpdate = DateTime.Now;
                                model_copy.UserUpdate = User.UserName;
                                model_copy.UserFullnameUpdate = User.FullName;

                                db_.QLKhoSoi_User.Add(model_copy);
                                db_.SaveChanges();
                            }
                            #endregion

                        }
                        #region Kế thừa user cũ
                        var listusersys = db_.QLKhoSoi_User.Where(c => c.Username == model.UserKeThua);
                        if (listusersys != null)
                        {
                            db_.Sp_KethuaQuyenUser_KhoVT_Soi2023(model.Username, model.UserKeThua);
                        }
                        #endregion

                    }

                    return Json(new { status = 1, title = "", text = "Phân quyền thành công", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                var message = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

                return Json(new { status = -1, title = "", text = "Error: " + message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _EditFun(HomeUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var item = db_.QLKhoSoi_User.FirstOrDefault(p => p.Id == model.Id);
                    item.DatetimeUpdate = DateTime.Now;
                    item.UserUpdate = User.UserName;
                    item.UserFullnameUpdate = User.FullName;

                    #region Page
                    #region Del
                    if (item.QLKhoSoi_User_Page != null && item.QLKhoSoi_User_Page.Count > 0)
                    {
                        var list = item.QLKhoSoi_User_Page.ToList();
                        foreach (var data in list)
                        {
                            db_.QLKhoSoi_User_Page.Remove(data);
                        }
                    }
                    #endregion

                    #region Add
                    if (model.Pages != null && model.Pages.Length > 0)
                    {
                        var Pages = model.Pages.Split(',');

                        foreach (var item_ in Pages)
                        {
                            var data = new QLKhoSoi_User_Page
                            {
                                PageId = int.Parse(item_),
                                DatetimeCreate = DateTime.Now,
                                UserCreate = User.UserName,
                                UserFullnameCreate = User.FullName,
                                DatetimeUpdate = DateTime.Now,
                                UserUpdate = User.UserName,
                                UserFullnameUpdate = User.FullName
                            };

                            item.QLKhoSoi_User_Page.Add(data);
                        }
                    }
                    #endregion
                    #endregion

                    #region Role
                    #region Del
                    if (item.QLKhoSoi_User_Role != null && item.QLKhoSoi_User_Role.Count > 0)
                    {
                        var list = item.QLKhoSoi_User_Role.ToList();
                        foreach (var data in list)
                        {
                            db_.QLKhoSoi_User_Role.Remove(data);
                        }
                    }
                    #endregion

                    #region Add
                    if (model.Roles != null && model.Roles.Length > 0)
                    {
                        var Roles = model.Roles.Split(',');

                        foreach (var item_ in Roles)
                        {
                            var data = new QLKhoSoi_User_Role
                            {
                                RoleId = int.Parse(item_),
                                DatetimeCreate = DateTime.Now,
                                UserCreate = User.UserName,
                                UserFullnameCreate = User.FullName,
                                DatetimeUpdate = DateTime.Now,
                                UserUpdate = User.UserName,
                                UserFullnameUpdate = User.FullName
                            };

                            item.QLKhoSoi_User_Role.Add(data);
                        }
                    }
                    #endregion
                    #endregion

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
                var message = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

                return Json(new { status = -1, title = "", text = "Error: " + message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        [ValidateInput(false)]
        [HttpPost]
        public ActionResult _DeleteFun(int Id)
        {
            try
            {
                var item = db_.QLKhoSoi_User.FirstOrDefault(p => p.Id == Id);

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