using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ToolsApp.Authentication;
using ToolsApp.EntityFramework.KhoSoi;
using ToolsApp.EntityFramework.DonHang09;
using ToolsApp.Models;
using System.Globalization;
using ToolsApp.Utilities;
using System.Data.Entity;
using SpreadsheetLight;
using System.IO;
using System.Data.Entity.Validation;

namespace ToolsApp.Controllers
{
    [Authorize]
    public class InvoiceController : BaseController
    {
        wqlkhosoiEntities dbsoi = new wqlkhosoiEntities();
        QLDonHang09Entities db09 = new QLDonHang09Entities();

        #region Index
        public ActionResult Index()
        {
            var data = dbsoi.INVOICEs.ToList();
            ViewBag.VATTU = data.Select(c => c.MAVATTU).Distinct().ToList();
            ViewBag.INVOICE1 = data.Select(c => c.INVOICE1).Distinct().ToList();
            ViewBag.SOHOPDONG = data.Select(c => c.SOHOPDONG).Distinct().ToList();
            ViewBag.QUOCGIA = data.Select(c => c.QUOCGIA).Distinct().ToList();
            //ViewBag.MAPHIEUPD = data.Select(c => c.MAPHIEUPD).Distinct().ToList();
            ViewBag.MAPHIEU = data.Select(c => c.MAPHIEU).Distinct().ToList();

            return View();
        }
        #endregion

        #region _GetList
        public ActionResult _GetList(string maphieupdsearch, string invoicesearch, string vattusearch, string quocgiasearch, string sohopdongsearch)
        {
            var data = dbsoi.INVOICEs.Where(c =>
                    (string.IsNullOrEmpty(maphieupdsearch) || c.MAPHIEU == maphieupdsearch) &&
                    //(string.IsNullOrEmpty(maphieupdsearch) || c.MAPHIEUPD == maphieupdsearch) &&
                    (string.IsNullOrEmpty(sohopdongsearch) || c.SOHOPDONG == sohopdongsearch) &&
                    (string.IsNullOrEmpty(invoicesearch) || c.INVOICE1 == invoicesearch) &&
                    (string.IsNullOrEmpty(vattusearch) || c.MAVATTU == vattusearch) &&
                    (string.IsNullOrEmpty(quocgiasearch) || c.QUOCGIA == quocgiasearch) 
                ).ToList();
            ViewBag.List = data;

            return PartialView();
        }
        #endregion

        #region _GetListCTPhieu
        public ActionResult _GetListCTPhieu(InvoiceViewModel inv)
        {
            inv.NGAYNHAPHANG = ParseType.TryParseDatetimeDMY(inv.NGAYNHAPHANG_);

            var data = dbsoi.Insert_Invoice_NL_DMPHIEU_Review(inv.MAPHIEU, inv.MAPHIEUPD, inv.SOHOPDONG, inv.INVOICE1, inv.TOKHAI, inv.GIANHAP, inv.QUOCGIA, inv.NGAYNHAPHANG, inv.MASPKHACH, inv.MAKH, User.UserName).ToList();
            ViewBag.List = data;
            //ViewBag.VT = dbsoi.TBLDMNGUYENLIEUx.ToList();
            return PartialView();
        }
        #endregion

        #region View insert
        public ActionResult _DetailForEdit(Guid khoa_id)
        {
            var invdata = dbsoi.INVOICEs.Find(khoa_id);
            ViewBag.DMQUOCGIA = db09.Load_QuocGia().ToList();
            ViewBag.KHACHHANG = db09.load_TBLKHACHHANG("", "").ToList();
            if (invdata == null)
            {
                ViewBag.DMPHIEU = dbsoi.NL_DMPHIEU.Where(c => !string.IsNullOrEmpty(c.MAPHIEU)).Select(c => c.MAPHIEU).Distinct().ToList();
                return PartialView("_Insert");
            }
            else
            {
                ViewBag.INVOICE = invdata;
                return PartialView("_Update");
            }
        }
        #endregion

        #region Insert
        public JsonResult _Insert(InvoiceViewModel model)
        {
            try
            {
                foreach (var item in model.Detail)
                {
                    #region CheckData
                    if (dbsoi.INVOICEs.Any(c => c.MAPHIEU == item.MAPHIEU && c.MAPHIEUPD == item.MAPHIEUPD && c.MAVATTU == item.MAVATTU && c.INVOICE1 == item.INVOICE1))
                    {
                        return Json(new { status = -1, text = string.Format("Trùng hóa đơn {0} cho vật tư {1}", item.INVOICE1, item.MAVATTU), obj = "" }, JsonRequestBehavior.AllowGet);
                    }
                    //if (string.IsNullOrEmpty(model.INVOICE1))
                    //{
                    //    return Json(new { status = -1, text = "Inv", obj = "" }, JsonRequestBehavior.AllowGet);
                    //}
                    #endregion
                    item.NGAYNHAPHANG = ParseType.TryParseDatetimeDMY(item.NGAYNHAPHANG_);
                    var invoice = new INVOICE();
                    invoice.MAPHIEU = item.MAPHIEU;
                    invoice.SOHOPDONG = item.SOHOPDONG;
                    invoice.MAVATTU = item.MAVATTU;
                    invoice.TENVT = item.TENVT;
                    invoice.INVOICE1 = item.INVOICE1;
                    invoice.TOKHAI = item.TOKHAI;
                    invoice.GIANHAP = item.GIANHAP;
                    invoice.QUOCGIA = item.QUOCGIA;
                    invoice.NGAYNHAPHANG = item.NGAYNHAPHANG;
                    invoice.MASPKHACH = item.MASPKHACH;
                    invoice.MAKH = item.MAKH;
                    invoice.MANVCAPNHAT = User.UserName;
                    invoice.NGAYCAPNHAT = DateTime.Now;
                    invoice.khoa_id = item.khoa_id;

                    dbsoi.INVOICEs.Add(invoice);
                    dbsoi.SaveChanges();
                }
                return Json(new { status = 1, text = "Thêm thành công", obj = "" }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { status = -1, text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Insert
        public JsonResult _InsertFun(InvoiceViewModel model,string khoa)
        {
            try
            {
                foreach (var item in model.Detail)
                {
                    if(item.MAVATTU == khoa)
                    {
                        #region CheckData
                        if (dbsoi.INVOICEs.Any(c => c.MAPHIEU == item.MAPHIEU && c.MAVATTU == item.MAVATTU && c.INVOICE1 == item.INVOICE1))
                        {
                            return Json(new { status = -1, text = string.Format("Trùng hóa đơn {0} cho vật tư {1}", item.INVOICE1, item.MAVATTU), obj = "" }, JsonRequestBehavior.AllowGet);
                        }
                        #endregion
                        //item.NGAYNHAPHANG = ParseType.TryParseDatetimeDMY(item.NGAYNHAPHANG_);
                        var invoice = new INVOICE();
                        invoice.MAPHIEU = item.MAPHIEU;
                        invoice.SOHOPDONG = item.SOHOPDONG;
                        invoice.MAVATTU = item.MAVATTU;
                        invoice.TENVT = item.TENVT;
                        invoice.INVOICE1 = item.INVOICE1;
                        invoice.TOKHAI = item.TOKHAI;
                        invoice.GIANHAP = item.GIANHAP;
                        invoice.QUOCGIA = item.QUOCGIA;
                        invoice.NGAYNHAPHANG = item.NGAYNHAPHANG;
                        invoice.MASPKHACH = item.MASPKHACH;
                        invoice.MAKH = item.MAKH;
                        invoice.MANVCAPNHAT = User.UserName;
                        invoice.NGAYCAPNHAT = DateTime.Now;
                        invoice.khoa_id = item.khoa_id;

                        dbsoi.INVOICEs.Add(invoice);
                        dbsoi.SaveChanges();
                    }    
                    
                }
                return Json(new { status = 1, text = "Thêm thành công", obj = "" }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { status = -1, text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Update
        public JsonResult _Update(InvoiceViewModel model)
        {
            try
            {
                model.NGAYNHAPHANG = ParseType.TryParseDatetimeDMY(model.NGAYNHAPHANG_);

                var invoice = dbsoi.INVOICEs.Find(model.khoa_id);
                invoice.SOHOPDONG = model.SOHOPDONG;
                invoice.INVOICE1 = model.INVOICE1;
                invoice.TOKHAI = model.TOKHAI;
                invoice.GIANHAP = model.GIANHAP;
                invoice.QUOCGIA = model.QUOCGIA;
                invoice.NGAYNHAPHANG = model.NGAYNHAPHANG;
                invoice.MASPKHACH = model.MASPKHACH;
                invoice.MAKH = model.MAKH;
                invoice.MANVMODIFY = User.UserName;
                invoice.NGAYMODIFY = DateTime.Now;

                dbsoi.Entry(invoice).State = EntityState.Modified;
                dbsoi.SaveChanges();
                return Json(new { status = 1, text = "Cập nhật thành công", obj = "" }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { status = -1, text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Delete
        public JsonResult Delete(Guid khoa_id)
        {
            try
            {
                var data = dbsoi.INVOICEs.Find(khoa_id);
                if (data.MANVCAPNHAT != User.UserName && data.MANVMODIFY != User.UserName)
                {
                    return Json(new { status = -1, text = "User này không phải là người tạo Invoice này!", obj = "" });

                }
                dbsoi.INVOICEs.Remove(data);
                dbsoi.SaveChanges();
                return Json(new { status = 1, text = "Xóa thành công", obj = "" });

            }
            catch (Exception ex)
            {
                return Json(new { status = -1, text = ex.Message, obj = "" });
            }
        }
        #endregion

        #region onchange
        public JsonResult LoadKhachHang(string quocgia)
        {
            var data = db09.load_TBLKHACHHANG("", quocgia).ToList();
            return Json(new { status = 1, text = "", obj = data }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        public ActionResult _Upload()
        {
            return PartialView();
        }
        #region Import Excel CT
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

                    SpreadsheetLight.SLDocument sl = new SLDocument(fileIn);

                    sl.SelectWorksheet(sl.GetSheetNames()[0]);

                    SLWorksheetStatistics stats = sl.GetWorksheetStatistics();

                    var isHaveData = false;

                    var numRecord = 1;

                    var list_ = new List<INVOICE>();



                    #region Read
                    for (int i = 2; i <= stats.EndRowIndex; i++)
                    {
                        var MAPHIEU = sl.GetCellValueAsString(i, 1);
                        var SOHOPDONG = sl.GetCellValueAsString(i, 2).Trim();
                        var MAVATTU = sl.GetCellValueAsString(i, 3).ToUpper().Trim();
                        var INVOICE = sl.GetCellValueAsString(i, 4).Trim();
                        var TOKHAI = sl.GetCellValueAsString(i, 5).Trim();
                        var GIANHAP = sl.GetCellValueAsString(i, 6).Trim();
                        var QUOCGIA = sl.GetCellValueAsString(i, 7).ToUpper().Trim();
                        var NGAYNHAPHANG = sl.GetCellValueAsString(i, 8).Trim();
                        var MASPKHACH = sl.GetCellValueAsString(i, 9).Trim();
                        var MAKH = sl.GetCellValueAsString(i, 10).Trim();

                        if (!string.IsNullOrEmpty(MAPHIEU))
                        {
                            var tenVt = dbsoi.TBLDMNGUYENLIEUx.Where(p => p.MAVATTU == MAVATTU).FirstOrDefault();
                            #region insert xuất kho

                            double ngaynhap = NGAYNHAPHANG == "" ? Convert.ToDouble("0") : Convert.ToDouble(NGAYNHAPHANG);
                            //var insert = dbsoi.INSERT_INVOICE_NL_DMPHIEU(MAPHIEUPD, SOHOPDONG, MAVATTU, INVOICE, TOKHAI, Convert.ToDecimal(GIANHAP), QUOCGIA, DateTime.Now, MASPKHACH, MAKH, User.UserName, DateTime.Now, Guid.NewGuid());
                            var model = new INVOICE
                            {

                                khoa_id = Guid.NewGuid(),
                                MAPHIEU = MAPHIEU,
                                SOHOPDONG = SOHOPDONG,
                                MAVATTU = MAVATTU,
                                INVOICE1 = INVOICE == "" ? "" : INVOICE,
                                TOKHAI = TOKHAI,
                                GIANHAP = GIANHAP == "" ? Convert.ToDecimal("0") : Convert.ToDecimal(GIANHAP),
                                QUOCGIA = QUOCGIA,
                                NGAYNHAPHANG = ngaynhap == 0 ? new DateTime(1900, 01, 01) : DateTime.FromOADate(ngaynhap),
                                MASPKHACH = MASPKHACH,
                                MAKH = MAKH,
                                NGAYCAPNHAT = DateTime.Now,
                                MANVCAPNHAT = User.UserName,
                                TENVT = tenVt == null ? "" : tenVt.Tenvattu,

                            };
                            list_.Add(model);
                            #endregion

                            isHaveData = true;

                            numRecord++;

                        }
                    }

                    if (isHaveData)
                    {
                        dbsoi.INVOICEs.AddRange(list_);


                        dbsoi.SaveChanges();

                        return Json((new { status = 1, title = "", text = string.Format("Import thành công {0} dòng.", numRecord - 1), obj = "" }), JsonRequestBehavior.DenyGet);
                    }
                    else
                    {
                        return Json((new { status = -1, title = "", text = "File excel không có dữ liệu.", obj = "" }), JsonRequestBehavior.DenyGet);
                    }
                    #endregion
                }
                else
                {
                    return Json((new { status = -1, title = "", text = "Vui lòng chọn file để import.", obj = "" }), JsonRequestBehavior.DenyGet);
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

    }
}