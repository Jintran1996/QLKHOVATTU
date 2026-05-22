
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using ToolsApp.Authentication;
using ToolsApp.EntityFramework.KhoSoi;
using ToolsApp.EntityFramework.VatTu;
using ToolsApp.EntityFramework.NetsuiteTT;
using ToolsApp.EntityFramework.SPMAY;

using System.Net;
using NSClient;
using ToolsApp.com.netsuite.webservices;
using ToolsApp.Models;
using System.Data.Entity;
using System.IO;
using SpreadsheetLight;
using System.Data.Entity.Validation;
using System.Globalization;

namespace ToolsApp.Controllers
{
    [Authorize]
    public class NhapHoaDonMuaHangController : BaseController
    {
        private wqlvattuEntities vt_ = new wqlvattuEntities();
        private wqlkhosoiEntities soi_ = new wqlkhosoiEntities();
        private NetsuiteTTEntities1 nstt_ = new NetsuiteTTEntities1();
        private QLSANPHAMMAY2023Entities SPMAY = new QLSANPHAMMAY2023Entities();

        #region Index
        public ActionResult Index()
        {

            ViewBag.User = User.UserName;

            return View();
        }
        #endregion

        #region _GetList
        public ActionResult _GetList(string SOCTXNSearch = null, string KyhieuSearch = null, string SohoadonSearch = null, string MSTSearch = null, string ToKhaiSearch = null)
        {

            var model = vt_.TABLE_LUUTRU_SOHOADON.Where(p =>
                (SOCTXNSearch == null || SOCTXNSearch == "" || p.SOCTXN.Contains(SOCTXNSearch)) &&
                (KyhieuSearch == null || KyhieuSearch == "" || p.kyhieuhoadon.Contains(KyhieuSearch)) &&
                (SohoadonSearch == null || SohoadonSearch == "" || p.sohoadon.Contains(SohoadonSearch)) &&
                (MSTSearch == null || MSTSearch == "" || p.MST.Contains(MSTSearch)) &&
                (ToKhaiSearch == null || ToKhaiSearch == "" || p.TOKHAI.Contains(ToKhaiSearch)) &&
                ( p.MANVCN == User.UserName)
                
                ).ToList();
            ViewBag.List = model;

            return PartialView();
        }
        #endregion

        #region Load View Thêm mới _Insert
        public ActionResult _Insert(string SOCTXN)
        {
            var ListSOCTXN = vt_.VATTU2025_LOAD_SOCTXN_VATTU_SOI_HANGMUANGOAI(SOCTXN).ToList();
            ViewBag.ListSOCTXN = ListSOCTXN;
            ViewBag.ListQG = vt_.VATTU2025_DanhSachQuocGia.ToList();
            ViewBag.ListNgoaiTe = vt_.VATTU2025_NGOAITE.ToList();

            return PartialView();
        }
        #endregion

        #region Insert //ok
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _InsertFun(TABLE_LUUTRU_SOHOADONViewModel model)
        {
            try
            {
                if (model.SOCTXN == null)
                {
                    return Json(new { status = -1, title = "", text = "SOCTXN không được đẽ trống. Vui lòng kiểm tra lại.", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                foreach (var mSOCTXN in model.SOCTXN)
                {
                    var ktrasoctxn = vt_.TABLE_LUUTRU_SOHOADON.FirstOrDefault(p => p.SOCTXN == mSOCTXN);
                    var ik = model.FileDataPDFBase64;



                    #region Kiểm tra ko tồn tại
                    if (ktrasoctxn != null)
                    {
                        return Json(new { status = -1, title = "", text = "SOCTXN đã tồn tại. Vui lòng kiểm tra lại.", obj = "" }, JsonRequestBehavior.AllowGet);
                    }
                    //if (model.kyhieuhoadon == null)
                    //{
                    //    return Json(new { status = -1, title = "", text = "Ký hiệu hóa đơn không được để trống.", obj = "" }, JsonRequestBehavior.AllowGet);
                    //}
                    //if (model.sohoadon == null)
                    //{
                    //    return Json(new { status = -1, title = "", text = "Số hóa đơn không được để trống.", obj = "" }, JsonRequestBehavior.AllowGet);
                    //}
                    if (model.LinkHD != null)
                    {
                        if (model.LinkHD.Contains(",") && !model.LinkHD.Contains(";"))
                        {
                            return Json(new { status = -1, title = "", text = "Sai định dạng! Các đường dẫn phải phân tách bằng dấu ';'", obj = "" }, JsonRequestBehavior.AllowGet);

                        }
                    }
                    #endregion

                    #region Xử lý ngày
                    CultureInfo cul = CultureInfo.GetCultureInfo("en-GB");
                    var NgayLapTuSearch_ = new DateTime();
                    if (!string.IsNullOrEmpty(model.NGAYHD_))
                    {
                        try
                        {
                            NgayLapTuSearch_ = DateTime.ParseExact(model.NGAYHD_, "dd/MM/yyyy", cul);

                            model.NGAYHD = new DateTime(NgayLapTuSearch_.Year,
                                NgayLapTuSearch_.Month, NgayLapTuSearch_.Day, 0, 0, 0);
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(ex.Message);
                        }
                    }
                    #endregion

                    #region Xử lý ngày

                    var NgayKhaiSearch_ = new DateTime();
                    if (!string.IsNullOrEmpty(model.NGAYKHAI_))
                    {
                        try
                        {
                            NgayKhaiSearch_ = DateTime.ParseExact(model.NGAYKHAI_, "dd/MM/yyyy", cul);

                            model.NGAYKHAI = new DateTime(NgayKhaiSearch_.Year,
                                NgayKhaiSearch_.Month, NgayKhaiSearch_.Day, 0, 0, 0);
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(ex.Message);
                        }
                    }
                    #endregion

                    #region Năm

                    var ThuMucNam = DateTime.Now.ToString("yyyy");

                    string subPath = "~/Upload/" + ThuMucNam; // your code goes here

                    bool exists = System.IO.Directory.Exists(Server.MapPath(subPath));

                    if (!exists)
                    {
                        System.IO.Directory.CreateDirectory(Server.MapPath(subPath));
                    }

                    #endregion

                    #region Tháng
                    var ThuMucThang = DateTime.Now.ToString("MM");

                    subPath = "~/Upload/" + ThuMucNam + "/" + ThuMucThang;

                    exists = System.IO.Directory.Exists(Server.MapPath(subPath));

                    if (!exists)
                    {
                        System.IO.Directory.CreateDirectory(Server.MapPath(subPath));
                    }
                    #endregion

                    #region SOCTXN
                    var ThumucSOCTXN = mSOCTXN;

                    subPath = "~/Upload/" + ThuMucNam + "/" + ThuMucThang + "/" + ThumucSOCTXN;

                    exists = System.IO.Directory.Exists(Server.MapPath(subPath));

                    if (!exists)
                    {
                        System.IO.Directory.CreateDirectory(Server.MapPath(subPath));
                    }
                    #endregion

                    var uploadPath = Server.MapPath(subPath);

                    //#region PDF

                    //var tempfilenamePDF = model.SOCTXN + "_" + Guid.NewGuid().ToString() + ".pdf";

                    //var tempfilenameandlocationPDF = Path.Combine(uploadPath, Path.GetFileName(tempfilenamePDF));

                    //Byte[] bytes = null;
                    //if (model.FileDataPDFBase64 != null)
                    //{
                    //    bytes = Convert.FromBase64String(model.FileDataPDFBase64);
                    //    System.IO.File.WriteAllBytes(tempfilenameandlocationPDF, bytes);
                    //}
                    //#endregion
                    #region PDF
                    if (!string.IsNullOrEmpty(model.FileDataPDFBase64))
                    {
                        var pdfFiles = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PdfFileModel>>(model.FileDataPDFBase64);

                        List<string> PDFPaths = new List<string>();
                        List<string> PDFBase64List = new List<string>();

                        foreach (var pdf in pdfFiles)
                        {
                            // Tạo tên file ngẫu nhiên để tránh trùng
                            var tempfilenamePDF = mSOCTXN + "_" + pdf.name + ".pdf";
                            var tempfilenameandlocationPDF = Path.Combine(uploadPath, Path.GetFileName(tempfilenamePDF));

                            // base64 thường có dạng "data:application/pdf;base64,xxxx"
                            string base64Data = pdf.data.Contains(",") ? pdf.data.Split(',')[1] : pdf.data;
                            byte[] bytes = Convert.FromBase64String(base64Data);

                            // Lưu file vật lý
                            System.IO.File.WriteAllBytes(tempfilenameandlocationPDF, bytes);

                            // Lưu thông tin để sau dùng
                            PDFPaths.Add(Path.GetFileName(tempfilenamePDF)); // chỉ lấy tên file để lưu DB
                            PDFBase64List.Add(base64Data);
                        }

                        // Ghép lại thành 1 chuỗi, phân tách bằng dấu ";"
                        model.FileNamePDF = string.Join(";", PDFPaths);
                    }
                    #endregion

                    #region Hình Ảnh               

                    var files = Request.Files;
                    List<string> imagePaths = new List<string>();
                    for (int i = 0; i < files.Count; i++)
                    {
                        var file = files[i];

                        if (file != null && file.ContentLength > 0 && file.ContentType.StartsWith("image/"))
                        {
                            var fileName = Path.GetFileName(file.FileName);
                            if (!Directory.Exists(uploadPath))
                                Directory.CreateDirectory(uploadPath);

                            var fullPath = Path.Combine(uploadPath, fileName);
                            file.SaveAs(fullPath);

                            imagePaths.Add(fileName);
                        }
                    }
                    model.TenHinhAnh = string.Join(";", imagePaths);
                    var HINHANHs = model.TenHinhAnh;

                    #endregion

                    #region Lấy data PO
                    var dmphieu = vt_.VATTU2025_LOAD_SOCTXN_VATTU_SOI_HANGMUANGOAI(mSOCTXN).FirstOrDefault(p => p.SOCTXN == mSOCTXN);
                    if (dmphieu == null)
                    {
                        return Json(new { status = -1, title = "", text = "SOCTXN không tồn tại. Vui lòng kiểm tra lại.", obj = "" }, JsonRequestBehavior.AllowGet);
                    }
                    var PO = dmphieu.SOPO;

                    #endregion

                    #region Add database

                    TABLE_LUUTRU_SOHOADON item_copy = new TABLE_LUUTRU_SOHOADON();
                    item_copy.SOCTXN = mSOCTXN;
                    item_copy.PO = PO;
                    item_copy.kyhieuhoadon = (model.kyhieuhoadon == null || model.kyhieuhoadon == "") ? "" :  model.kyhieuhoadon.Trim();
                    item_copy.sohoadon = (model.sohoadon == null || model.sohoadon == "") ? "" : model.sohoadon.Trim();
                    item_copy.TenHinhAnh = HINHANHs; // lưu vào 1 cột;
                                                     // item_copy.HinhAnh = combinedFileNames.TrimEnd(';'), // lưu vào 1 cột;
                                                     // item_copy.FileNamePDF = (tempfilenamePDF == null || tempfilenamePDF == "") ? "" : tempfilenamePDF;
                    item_copy.FileNamePDF = (model.FileNamePDF == null || model.FileNamePDF == "") ? "" : model.FileNamePDF;
                    item_copy.ContentTypePDF = "application/pdf";
                    //  item_copy.FileDataPDF =  (bytes == null ) ? null : bytes;
                    item_copy.LinkHD = model.LinkHD;
                    item_copy.Thumuc = ThuMucNam + "/" + ThuMucThang + "/" + ThumucSOCTXN;
                    item_copy.NGAYHD = model.NGAYHD;
                    item_copy.MANVCN = User.UserName;
                    item_copy.NGAYCN = DateTime.Now;
                    item_copy.NAM = DateTime.Now.Year;
                    item_copy.THANG = DateTime.Now.Month;
                    item_copy.GhiChu = model.GhiChu;
                    item_copy.NGAYKHAI = model.NGAYKHAI;
                    item_copy.TOKHAI = model.TOKHAI;
                    item_copy.MST = model.MST;
                    item_copy.NgoaiTe = model.NgoaiTe;
                    item_copy.TyGia_NgoaiTe = model.TyGia_NgoaiTe;
                    item_copy.XUATXU = model.XUATXU;
                    vt_.TABLE_LUUTRU_SOHOADON.Add(item_copy);
                    vt_.SaveChanges();
                    #endregion
                }

                return Json(new { status = 1, title = "", text = "Thêm thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
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
        public JsonResult _DeleteFun(string SOCTXN)
        {
            try
            {
                var item = vt_.TABLE_LUUTRU_SOHOADON.FirstOrDefault(p => p.SOCTXN == SOCTXN);


                if(item.MANVCN != User.UserName)
                {
                    return Json(new { status = -1, title = "", text = "Xóa không thành công: chỉ người tạo phiếu mới có quyền thao tác này.", obj = "" }, JsonRequestBehavior.AllowGet);
                }

                string folderPath = Server.MapPath("~/Upload/" + item.Thumuc);

                if (Directory.Exists(folderPath))
                {
                    Directory.Delete(folderPath, true); // Xóa cả thư mục và file bên trong
                }

                vt_.TABLE_LUUTRU_SOHOADON.Remove(item);
                vt_.SaveChanges();
                return Json(new { status = 1, title = "", text = "Xóa thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        public class PdfFileModel
        {
            public string name { get; set; }
            public string data { get; set; }
        }

    }
}