using NSClient;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using ToolsApp.Authentication;
using ToolsApp.com.netsuite.webservices;

using ToolsApp.EntityFramework.VatTu;
using ToolsApp.EntityFramework.KhoSoi;
using ToolsApp.Models;
using System.Globalization;

namespace ToolsApp.Controllers
{
    [Authorize]
    public class PhieuXuatChuyenSXController : BaseController
    {

        wqlvattuEntities vt_ = new wqlvattuEntities();
        wqlkhosoiEntities soi_ = new wqlkhosoiEntities();

        #region Index
        // GET: PhieuNhapKho
        public ActionResult Index()
        {
            return View();
        }
        #endregion

        #region Load View
        public ActionResult _Getlist( string SOCTXNSearch = null)
        {
            var List = soi_.NL_DMPHIEUNHAP_CHUYEN.Where(p =>
           (
               (SOCTXNSearch == null || SOCTXNSearch == "" || p.SOCTNHAP.Contains(SOCTXNSearch))

           )).OrderByDescending(p => p.NGAYCN).ToList();

            ViewBag.List = List;

            return PartialView();
        }
        #endregion



        #region Load View CTphieu _
        public ActionResult _GetListCTPhieu_View(string SOCTNHAP, string KEY)
        {
            var item = soi_.NL_DMPHIEUNHAP_CHUYEN.FirstOrDefault(p => p.SOCTNHAP == SOCTNHAP);
           // var APIs = db_.tblphieukhoes.Where(c => c.Sophieu == SOCTXN).ToList();
            //ViewBag.APIs = APIs;
            var List = soi_.NL_CTPHIEUNHAP_CHUYEN.Where(p => p.SOCTNHAP == SOCTNHAP).ToList();

            ViewBag.List = List;

            return PartialView();
        }
        #endregion

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
 
     


        //#region Load View CTphieu _Load CT Sản lượng theo đh đã kiểm tra chất lượng
        //public ActionResult _GetListCTPhieu_theomadh(string Sophieu, string MAKHO)
        //{

        //    var List = soi_.SP_NHAPKHOTHEOPHIEUPHEDUYET_MAVT(MAKHO).ToList();

        //    ViewBag.List = List;

        //    return PartialView("_GetlistCTPhieu_View_theomadh", new NL_CTXUATNHAPViewModels { Sophieu = Sophieu });
        //}
        //#endregion

        #region Load View Thêm mới
        public ActionResult _Insert()
        {
            var SOCTXN_XD = soi_.Sp_Load_SOCTXN_XD1_XD2_XuatChuyenSX(User.UserName, "").ToList();
            ViewBag.SOCTXN_XD = SOCTXN_XD;
            var makhoxuat = soi_.SP_MAKHOBYMANV_LOAD(User.UserName).ToList();
            ViewBag.makhoxuat = makhoxuat;
            var makhonhan = soi_.NL_DMKHO.ToList();
            ViewBag.makhonhan = makhonhan;
        
            var loaihinhthuc = soi_.NL_DMHINHTHUC.Where(p => p.MAHTHUC == "NKSX").ToList();
            ViewBag.loaihinhthuc = loaihinhthuc;
            //var SOCTXN = soi_.SP_NL_TAOSOCHUNGTU(loaihinhthuc.FirstOrDefault().MAHTHUC).FirstOrDefault();
            //ViewBag.SOCTXN = SOCTXN;

            return PartialView("_Insert", new NL_DMPHIEUNHAP_CHUYENViewModels { /*Sophieu = Sophieu.FirstOrDefault().Sophieu*/ });
        }
        #endregion

        //#region OnChange
        public JsonResult _Change_SOCTXN(string MAHTHUCid , string SOCTXN_XDid)
        {

            try
            {
                //var SOCTXN = soi_.SP_NL_TAOSOCHUNGTU(MAHTHUCid).FirstOrDefault();
               
                string SOCTXN = SOCTXN_XDid.Replace("XD", "ND");
         

                return Json(new { status = 1, title = "", text = "", obj = SOCTXN }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { status = -1, title = "", text = e.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }

      


    

        #region Insert
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _InsertFun(NL_DMPHIEUNHAP_CHUYENViewModels model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                   // soi_.SP_NL_DELETE_KHONGCHITIET_XUATNHAP(User.UserName); // xóa các phiếu trống không có chi tiết

                    var tb = soi_.NL_DMPHIEUNHAP_CHUYEN.FirstOrDefault(p => p.SOCTXUAT == model.SOCTXUAT);

                    if (tb == null)
                    {
                        #region Check tồn tại
                        if (soi_.NL_DMPHIEUNHAP_CHUYEN.FirstOrDefault(p => p.SOCTXUAT == model.SOCTXUAT) != null)
                        {
                            return Json(new { status = -2, title = "", text = "Thêm không thành công. SOCTXuat đã tồn tại. Kiểm tra lại.", obj = model.SOCTXUAT }, JsonRequestBehavior.AllowGet);
                        }
                        
                        #endregion
                        var sp = soi_.ADD_MASOI_NHAPKHO_SANXUAT(User.UserName, model.SOCTXN_XD);
                      

                        return Json(new { status = 1, title = "", text = "Thêm thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { status = -2, title = "", text = "Thêm không thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { status = -2, title = "", text = "Lưu không thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion


        #region Delete
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _DeleteFun(string SOCTNHAP)
        {
            try
            {
                var del = soi_.Delete_MASOI_NHAPKHO_SANXUAT(User.UserName, SOCTNHAP);

                return Json(new { status = 1, title = "", text = "Xóa thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

    

    

    }
}