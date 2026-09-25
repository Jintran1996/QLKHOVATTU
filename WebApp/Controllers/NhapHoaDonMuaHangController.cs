
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

        #region _upload
        public ActionResult _Upload()
        {
            return PartialView();
        }

        #region Import Excel NQTH
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




                    var TABLE_LUUTRU_SOHOADON_ver2 = new List<TABLE_LUUTRU_SOHOADON>();
                    #region Read
                    for (int i = 2; i <= stats.EndRowIndex; i++)
                    {
                        var SOCTXN = sl.GetCellValueAsString(i, 1);
                        var kyhieuHD = sl.GetCellValueAsString(i, 2);
                        var soHD = sl.GetCellValueAsString(i, 3).ToUpper().Trim();
                        var link = sl.GetCellValueAsString(i, 4).ToUpper().Trim();
                        var ngaytokhai = sl.GetCellValueAsDateTime(i, 5);
                        var tokhai = sl.GetCellValueAsString(i, 6).ToUpper().Trim();
                        var mst = sl.GetCellValueAsString(i, 7).ToUpper().Trim();
                        var ngayhoadon = sl.GetCellValueAsDateTime(i, 8);
                        var xuatxu = sl.GetCellValueAsString(i, 9).ToUpper().Trim();
                        var ghichu = sl.GetCellValueAsString(i, 10).ToUpper().Trim();
                        var mangoaite = sl.GetCellValueAsString(i, 11).ToUpper().Trim();
                        var tygia = sl.GetCellValueAsString(i, 12).ToUpper().Trim();



                       
                        if (vt_.TABLE_LUUTRU_SOHOADON.Any(p => p.SOCTXN == SOCTXN))
                        {
                            return Json(new { status = -1, title = "", text = "SOCTXN đã tồn tại. Vui lòng kiểm tra lại.", obj = "" }, JsonRequestBehavior.AllowGet);
                        }
                        #region Lấy data PO
                        var dmphieu = vt_.VATTU2025_LOAD_SOCTXN_VATTU_SOI_HANGMUANGOAI(SOCTXN).FirstOrDefault(p => p.SOCTXN == SOCTXN);
                        if (dmphieu == null)
                        {
                            return Json(new { status = -1, title = "", text = "SOCTXN không tồn tại. Vui lòng kiểm tra lại.", obj = "" }, JsonRequestBehavior.AllowGet);
                        }
                        var PO = dmphieu.SOPO;

                        #endregion



                        if (!string.IsNullOrEmpty(SOCTXN))
                        {
                            #region Kiểm tra dữ liệu
                            if (string.IsNullOrEmpty(SOCTXN))
                            {
                                return Json((new { status = -1, title = "", text = string.Format("SOCTXN dòng {0} : " + SOCTXN + " không được bỏ trống.", numRecord), obj = "" }), JsonRequestBehavior.AllowGet);
                            }

                            if (string.IsNullOrEmpty(kyhieuHD))
                            {
                                return Json((new { status = -1, title = "", text = string.Format("Ký hiệu hóa đơn dòng {0} : " + kyhieuHD + " không được bỏ trống.", numRecord), obj = "" }), JsonRequestBehavior.AllowGet);
                            }
                            if (string.IsNullOrEmpty(soHD))
                            {
                                return Json((new { status = -1, title = "", text = string.Format("Loại vật tư dòng {0} : " + soHD + " không được bỏ trống.", numRecord), obj = "" }), JsonRequestBehavior.AllowGet);
                            }
                            if (string.IsNullOrEmpty(ngayhoadon.ToString()))
                            {
                                return Json((new { status = -1, title = "", text = string.Format("ngày hóa đơn dòng {0} : " + ngayhoadon + " không được bỏ trống.", numRecord), obj = "" }), JsonRequestBehavior.AllowGet);
                            }
                            if (string.IsNullOrEmpty(ngaytokhai.ToString()))
                            {
                                return Json((new { status = -1, title = "", text = string.Format("ngày tơ khai dòng {0} : " + ngaytokhai + " không được bỏ trống.", numRecord), obj = "" }), JsonRequestBehavior.AllowGet);
                            }

                            #endregion

                            #region Kiểm tra trùng trong excel
                            if (TABLE_LUUTRU_SOHOADON_ver2.FirstOrDefault(p => (p.SOCTXN == SOCTXN)) != null)
                            {
                                return Json((new { status = -1, title = "", text = string.Format(" SOCTXN dòng {0} : " + SOCTXN + " trùng trong file. Vui lòng kiểm tra lại.", numRecord), obj = "" }), JsonRequestBehavior.AllowGet);
                            }
                            #endregion

                            #region Kiểm tra cập nhật hay insert

                            if (vt_.TABLE_LUUTRU_SOHOADON.FirstOrDefault(c => c.SOCTXN == SOCTXN) == null)
                            {
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
                                var ThumucSOCTXN = SOCTXN;

                                subPath = "~/Upload/" + ThuMucNam + "/" + ThuMucThang + "/" + ThumucSOCTXN;

                                exists = System.IO.Directory.Exists(Server.MapPath(subPath));

                                if (!exists)
                                {
                                    System.IO.Directory.CreateDirectory(Server.MapPath(subPath));
                                }
                                #endregion
                                var model_copy = new TABLE_LUUTRU_SOHOADON();
                                {
                                    TABLE_LUUTRU_SOHOADON item_copy = new TABLE_LUUTRU_SOHOADON();
                                    item_copy.SOCTXN = SOCTXN;
                                    item_copy.PO = PO;
                                    item_copy.kyhieuhoadon = (kyhieuHD == null || kyhieuHD == "") ? "" : kyhieuHD.Trim();
                                    item_copy.sohoadon = (soHD == null || soHD == "") ? "" : soHD.Trim();
                                    item_copy.Thumuc = ThuMucNam + "/" + ThuMucThang + "/" + ThumucSOCTXN;
                                    item_copy.ContentTypePDF = "application/pdf";
                             
                                    item_copy.LinkHD = link;                            
                                    item_copy.NGAYHD = ngayhoadon;
                                    item_copy.MANVCN = User.UserName;
                                    item_copy.NGAYCN = DateTime.Now;
                                    item_copy.NAM = DateTime.Now.Year;
                                    item_copy.THANG = DateTime.Now.Month;                   
                                    item_copy.GhiChu = ghichu;                         
                                    item_copy.NGAYKHAI = ngaytokhai;
                                    item_copy.TOKHAI = tokhai;
                                    item_copy.MST = mst;
                                    item_copy.NgoaiTe = mangoaite;
                                    item_copy.TyGia_NgoaiTe = ( tygia == "" || tygia == null) ? 1 : Convert.ToDecimal(tygia);
                                    item_copy.XUATXU = xuatxu;

                                    TABLE_LUUTRU_SOHOADON_ver2.Add(item_copy);
                                };
                        
                            }
                            #endregion
                            isHaveData = true;
                            numRecord++;
                        }
                    }
                    if (isHaveData)
                    {
                        vt_.TABLE_LUUTRU_SOHOADON.AddRange(TABLE_LUUTRU_SOHOADON_ver2);
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
        #endregion

        #region xóa file 
        [HttpPost]
        public JsonResult _DeleteImage(string soctxn, string fileName)
        {
            try
            {
                // 1. Tìm bản ghi trong Database dựa vào SOCTXN
                var item = vt_.TABLE_LUUTRU_SOHOADON.FirstOrDefault(x => x.SOCTXN == soctxn);
                if (item == null)
                    return Json(new { status = 0, text = "Không tìm thấy dữ liệu!" });

                if (!string.IsNullOrEmpty(item.TenHinhAnh))
                {
                    // 2. Tách chuỗi thành danh sách các file (loại bỏ khoảng trống thừa)
                    var fileList = item.TenHinhAnh.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                    // 3. Kiểm tra xem file có tồn tại trong danh sách không và xóa khỏi list
                    if (fileList.Contains(fileName))
                    {
                        fileList.Remove(fileName);

                        // 4. Ghép lại danh sách mới bằng dấu ';'
                        item.TenHinhAnh = fileList.Count > 0 ? string.Join(";", fileList) : null;

                        // 5. Xóa file vật lý khỏi thư mục Server (Nếu cần)
                        string folderPath = Server.MapPath($"~/Upload/{item.Thumuc}");
                        string fullPath = Path.Combine(folderPath, fileName);
                        if (System.IO.File.Exists(fullPath))
                        {
                            System.IO.File.Delete(fullPath);
                        }

                        // 6. Lưu thay đổi vào Database
                        vt_.SaveChanges();

                        return Json(new { status = 1, text = "Xóa file thành công!" });
                    }
                }

                return Json(new { status = 0, text = "Không tìm thấy tên file trong database!" });
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, text = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult _DeletePDF(string soctxn, string fileName)
        {
            try
            {
                // 1. Tìm bản ghi trong Database dựa vào SOCTXN
                var item = vt_.TABLE_LUUTRU_SOHOADON.FirstOrDefault(x => x.SOCTXN == soctxn);
                if (item == null)
                    return Json(new { status = 0, text = "Không tìm thấy dữ liệu!" });

                if (!string.IsNullOrEmpty(item.FileNamePDF))
                {
                    // 2. Tách chuỗi thành danh sách các file (loại bỏ khoảng trống thừa)
                    var fileList = item.FileNamePDF.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                    // 3. Kiểm tra xem file có tồn tại trong danh sách không và xóa khỏi list
                    if (fileList.Contains(fileName))
                    {
                        fileList.Remove(fileName);

                        // 4. Ghép lại danh sách mới bằng dấu ';'
                        item.FileNamePDF = fileList.Count > 0 ? string.Join(";", fileList) : null;

                        // 5. Xóa file vật lý khỏi thư mục Server (Nếu cần)
                        string folderPath = Server.MapPath($"~/Upload/{item.Thumuc}");
                        string fullPath = Path.Combine(folderPath, fileName);
                        if (System.IO.File.Exists(fullPath))
                        {
                            System.IO.File.Delete(fullPath);
                        }

                        // 6. Lưu thay đổi vào Database
                        vt_.SaveChanges();

                        return Json(new { status = 1, text = "Xóa file thành công!" });
                    }
                }

                return Json(new { status = 0, text = "Không tìm thấy tên file trong database!" });
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, text = ex.Message });
            }
        }

        #endregion

        #region Upload file theo soctxn
        [HttpPost]
        public JsonResult UploadMultiplePDF(string soctxn, IEnumerable<HttpPostedFileBase> fileUploads)
        {
            try
            {
                if (fileUploads == null || !fileUploads.Any(f => f != null && f.ContentLength > 0))
                    return Json(new { status = 0, text = "Vui lòng chọn ít nhất một file!" });

                var item = vt_.TABLE_LUUTRU_SOHOADON.FirstOrDefault(x => x.SOCTXN == soctxn);
                if (item == null)
                    return Json(new { status = 0, text = "Không tìm thấy bản ghi!" });

                string folderPath = Server.MapPath($"~/Upload/{item.Thumuc}");
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                // Tạo một danh sách tạm để lưu tên các file mới upload thành công
                List<string> newFileNames = new List<string>();

                // Lặp qua từng file trong danh sách gửi lên
                foreach (var file in fileUploads)
                {
                    if (file != null && file.ContentLength > 0)
                    {
                        // Tạo tên file duy nhất
                        string newFileName = Guid.NewGuid().ToString().Substring(0, 8) + "_" + Path.GetFileName(file.FileName);
                        string fullPath = Path.Combine(folderPath, newFileName);

                        // Lưu file vật lý
                        file.SaveAs(fullPath);

                        // Thêm vào danh sách tạm
                        newFileNames.Add(newFileName);
                    }
                }

                if (newFileNames.Count > 0)
                {
                    // Ghép danh sách file mới vừa upload bằng dấu ';'
                    string stringNewFiles = string.Join(";", newFileNames);

                    // XỬ LÝ CẬP NHẬT CHUỖI VÀO DATABASE
                    if (string.IsNullOrEmpty(item.FileNamePDF))
                    {
                        item.FileNamePDF = stringNewFiles;
                    }
                    else
                    {
                        // Đảm bảo chuỗi cũ không kết thúc bằng dấu ';' trước khi nối chuỗi mới
                        string trimOldString = item.FileNamePDF.TrimEnd(';');
                        item.FileNamePDF = trimOldString + ";" + stringNewFiles;
                    }

                    vt_.SaveChanges();
                    return Json(new { status = 1, text = $"Đã tải lên {newFileNames.Count} file thành công!" });
                }

                return Json(new { status = 0, text = "Không có file hợp lệ nào được tải lên." });
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, text = ex.Message });
            }
        }


        [HttpPost]
        public JsonResult UploadMultipleImage(string soctxn, IEnumerable<HttpPostedFileBase> fileUploads)
        {
            try
            {
                if (fileUploads == null || !fileUploads.Any(f => f != null && f.ContentLength > 0))
                    return Json(new { status = 0, text = "Vui lòng chọn ít nhất một file!" });

                var item = vt_.TABLE_LUUTRU_SOHOADON.FirstOrDefault(x => x.SOCTXN == soctxn);
                if (item == null)
                    return Json(new { status = 0, text = "Không tìm thấy bản ghi!" });

                string folderPath = Server.MapPath($"~/Upload/{item.Thumuc}");
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                // Tạo một danh sách tạm để lưu tên các file mới upload thành công
                List<string> newFileNames = new List<string>();

                // Lặp qua từng file trong danh sách gửi lên
                foreach (var file in fileUploads)
                {
                    if (file != null && file.ContentLength > 0)
                    {
                        // Tạo tên file duy nhất
                        string newFileName = Guid.NewGuid().ToString().Substring(0, 8) + "_" + Path.GetFileName(file.FileName);
                        string fullPath = Path.Combine(folderPath, newFileName);

                        // Lưu file vật lý
                        file.SaveAs(fullPath);

                        // Thêm vào danh sách tạm
                        newFileNames.Add(newFileName);
                    }
                }

                if (newFileNames.Count > 0)
                {
                    // Ghép danh sách file mới vừa upload bằng dấu ';'
                    string stringNewFiles = string.Join(";", newFileNames);

                    // XỬ LÝ CẬP NHẬT CHUỖI VÀO DATABASE
                    if (string.IsNullOrEmpty(item.TenHinhAnh))
                    {
                        item.TenHinhAnh = stringNewFiles;
                    }
                    else
                    {
                        // Đảm bảo chuỗi cũ không kết thúc bằng dấu ';' trước khi nối chuỗi mới
                        string trimOldString = item.TenHinhAnh.TrimEnd(';');
                        item.TenHinhAnh = trimOldString + ";" + stringNewFiles;
                    }

                    vt_.SaveChanges();
                    return Json(new { status = 1, text = $"Đã tải lên {newFileNames.Count} file thành công!" });
                }

                return Json(new { status = 0, text = "Không có file hợp lệ nào được tải lên." });
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, text = ex.Message });
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