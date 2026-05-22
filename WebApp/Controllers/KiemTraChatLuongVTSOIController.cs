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
using ToolsApp.EntityFramework.DonHang09;
using System.Net;
using NSClient;
using ToolsApp.com.netsuite.webservices;
using ToolsApp.Models;
using System.Data.Entity;
using System.Data;
using System.Globalization;
using ToolsApp.Utilities;
using DocumentFormat.OpenXml.Spreadsheet;
using SpreadsheetLight;

namespace ToolsApp.Controllers
{
    public class KiemTraChatLuongVTSOIController : BaseController
    {
        private wqlkhosoiEntities dbks_ = new wqlkhosoiEntities();
        private wqlvattuEntities vt_ = new wqlvattuEntities();
        // GET: KiemTraChatLuongVTSOI
        public ActionResult Index()
        {
            var kihieu = vt_.SP_LOAD_KIHIEUDV_BY_MANV(User.UserName).FirstOrDefault(p => p.KiHieu != "pur");
            var soctvt = vt_.VATTU2025_SP_DVKTCL_LOADSOCT(kihieu.KiHieu, "").ToList();
            ViewBag.soCTVT = soctvt;
            var soctks = dbks_.SP_COMBOBOX_LOAD_SOCTXN_CHATLUONG("", User.UserName).ToList();
            ViewBag.soCTKS = soctks;
            return View();
        }
        #region Load View Vật Tư
        public ActionResult _GetList(string SOCTVT)
        {
            try
            {
                if (SOCTVT == null || SOCTVT == "")
                {
                    string message = "SOCTVT không hợp lệ.";
                    return Content(message);
                }
                else
                {
                    var kihieu = vt_.SP_LOAD_KIHIEUDV_BY_MANV(User.UserName).FirstOrDefault(p => p.KiHieu != "pur");
                    var data = vt_.SP_LOADCL_DONVIKIEMTRA(SOCTVT, "").ToList();
                    ViewBag.data = data;
                    return PartialView();

                }
            }
            catch
            {
                string errorMessage = "Đã xảy ra lỗi khi xử lý yêu cầu.";
                return Content(errorMessage);
            }

        }
        public ActionResult _GetListHL(string SOCTVT)
        {
            try
            {
                if (SOCTVT == null || SOCTVT == "")
                {
                    string message = "SOCTVT không hợp lệ.";
                    return Content(message);
                }
                else
                {
                    var data = vt_.SP_LoadCL_KHOXEM(SOCTVT).ToList();
                    ViewBag.data = data;
                    return PartialView();
                }
            }
            catch
            {
                string errorMessage = "Đã xảy ra lỗi khi xử lý yêu cầu.";
                return Content(errorMessage);
            }
        }
        #endregion
        #region Load View Kho Sợi
        public ActionResult _GetListKS(string SOCTXN)
        {
            try
            {
                if (SOCTXN == null || SOCTXN == "")
                {
                    string message = "SOCT không hợp lệ.";
                    return Content(message);
                }
                else
                {
                    var data = dbks_.SP_LOADCHITIET_KIEMTRACHATLUONG_DONVI(SOCTXN).ToList();
                    ViewBag.data = data;
                    return PartialView();
                }
            }
            catch
            {
                string errorMessage = "Đã xảy ra lỗi khi xử lý yêu cầu.";
                return Content(errorMessage);
            }

        }
        public ActionResult _GetListKSV(string SOCTXN)
        {
            try
            {
                if (SOCTXN == null || SOCTXN == "")
                {
                    string message = "SOCT không hợp lệ.";
                    return Content(message);
                }
                else
                {
                    var data = dbks_.SP_LOADCHITIET_KIEMTRACHATLUONG_DONVI(SOCTXN).ToList();
                    ViewBag.data = data;
                    return PartialView();
                }
            }
            catch
            {
                string errorMessage = "Đã xảy ra lỗi khi xử lý yêu cầu.";
                return Content(errorMessage);
            }
        }
        #endregion

        #region Lưu vật tư
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _LuuFun(ITBLKIEMTRACLViewModels model, CHATLUONGViewModels models)
        {
            try
            {
                var lblloi = "";
                //var data = vt_.SP_LOADCL_DONVIKIEMTRA(, "").ToList();

                if (model.Detail.Any())
                {
                    foreach (var items in model.Detail)
                    {
                        if (string.IsNullOrEmpty(items.DANGLOI))
                        {
                            return Json(new { status = -1, title = "", text = "Đơn vị kiểm tra không được để trống", obj = "" }, JsonRequestBehavior.AllowGet);
                        }
                        if (string.IsNullOrEmpty(items.LOAI))
                        {
                            return Json(new { status = -1, title = "", text = "Nhận/Không không được để trống", obj = "" }, JsonRequestBehavior.AllowGet);
                        }
                        var tbl = vt_.SP_TBLKIEMTRACL_UPDATE(items.SOCT, items.MAVT, items.THUC, 0,
                          (items.KHOKIEMTRA != null || items.KHOKIEMTRA != "") ? items.KHOKIEMTRA : items.DANGLOI
                          , items.DANGLOI, 0, User.UserName, items.LOAI, items.NHACC, items.MAPHIEUPD);
                    }
                    var soct = model.Detail[0].SOCT;
                   // var maphieupd = model.Detail[0].MAPHIEUPD;
                    var chatLuong = vt_.CHATLUONGs.Where(p => p.SOCT == soct).FirstOrDefault();

                    var cl = vt_.SP_CHATLUONG_UPDATE(model.Detail[0].SOCT, chatLuong.NGAYKHO, DateTime.Now, chatLuong.THUKHO, chatLuong.GDDV, chatLuong.KHOXN, chatLuong.DVKT, chatLuong.DaIn, chatLuong.FlagXoa, true);


                    #region GUI EMAIL TU DONG KHI DA KIEM TRA

                    try
                    {
                        SmtpClient SmtpServer = new SmtpClient
                        {
                            Host = "mail.thaituan.com.vn",
                            Port = 25,
                            EnableSsl = false,
                            DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network,
                            Credentials = new System.Net.NetworkCredential("EmailTuDong@thaituan.com.vn", "#It@12!")
                        };
                        MailMessage mail = new MailMessage();

                        var dataEmail = vt_.SP_LOAD_MANVXX_BY_SOCT(soct).ToList();

                        mail.From = new MailAddress("EmailTuDong@thaituan.com.vn", "QuanLyVatTu", System.Text.Encoding.UTF8);

                        Byte i;
                        for (i = 0; i < dataEmail.Count; i++)
                        {
                            if (!string.IsNullOrEmpty(dataEmail[i].EMail.ToString()))
                            {
                                mail.To.Add(dataEmail[i].EMail.ToString());
                            }

                            if (!string.IsNullOrEmpty(dataEmail[i].CC.ToString()))
                            {
                                mail.CC.Add(dataEmail[i].CC.ToString());
                            }
                        }
                        mail.Subject = "Chuong Trinh Quan Ly Vat Tu";

                        mail.Body = " Kinh gui Anh/Chi, De nghi BGD xac nhan chat luong hang mua vao SOCTXN:" + soct ;
                        mail.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
                        SmtpServer.Send(mail);
                    }
                    catch (Exception ex)
                    {
                        lblloi = ex.Message;
                    }

                    #endregion

                    return Json(new { status = 1, title = "", text = "Update thành công", obj = "" }, JsonRequestBehavior.AllowGet);

                }
                else
                {
                    return Json(new { status = -1, title = "", text = "Update không thành công", obj = "" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion
        #region Lưu Kho Sợi
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _LuuFunKS(NL_CHATLUONGKIEMTRAViewModels model, CHATLUONGViewModels models)
        {
            try
            {

                if (model.Detail.Any())
                {
                    foreach (var items in model.Detail)
                    {
                        if (string.IsNullOrEmpty(items.DONVIKIEMTRA))
                        {
                            return Json(new { status = -1, title = "", text = "Đơn vị kiểm tra không được để trống", obj = "" }, JsonRequestBehavior.AllowGet);
                        }
                        if (string.IsNullOrEmpty(items.LOAI))
                        {
                            return Json(new { status = -1, title = "", text = "Nhận/Không không được để trống", obj = "" }, JsonRequestBehavior.AllowGet);
                        }
                        var soct = items.SOCTXN;
                        var khosoi = dbks_.SP_KIEMTRACHATLUONG_UPDATE_DONVI(items.SOCTXN, items.MAVT, items.LO, items.HIEU, items.MAPHIEUPD, items.DONVIKIEMTRA, User.UserName, items.LOAI, items.DANGLOI, items.MANHACC);

                        var nlChatLuong = dbks_.SP_KIEMTRACHATLUONG_FLAGXOA_UPDATE_FLAGXOA(soct);
                    }

                    return Json(new { status = 1, title = "", text = "Update thành công", obj = "" }, JsonRequestBehavior.AllowGet);

                }
                else
                {
                    return Json(new { status = -1, title = "", text = "Update không thành công", obj = "" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region in báo cáo
        public ActionResult _PrintReport(string SOCTVT)
        {
            try
            {
                if(string.IsNullOrEmpty(SOCTVT))
                {
                    return Json(new { status = -1, title = "", text = "Vui lòng chọn Số CT", obj = "" }, JsonRequestBehavior.AllowGet);
                }    
                var ks = vt_.CHATLUONGs.Where(p => p.SOCT == SOCTVT).FirstOrDefault();
                if (ks.DVKT == false)
                {
                    return Json(new { status = -1, title = "", text = "Chưa kiểm tra không thể in", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                string PCName = GetComputerName();
                string IPPC = GetIPClient();
                string log = "Đã in phiếu kiểm tra chất lượng: " + SOCTVT.Trim();
                string log1 = "Đã xác nhận đơn vị kiểm tra chất lượng: " + SOCTVT.Trim();
                ViewData["URL_IMAGE"] = Utils.WebConfigKey.Domain;
                var baoCao = vt_.SP_CRCHATLUONG_NEW(SOCTVT).ToList();
                ViewBag.inVT = baoCao;
                var ghiLog = vt_.SP_GHIFILELOG(User.UserName, PCName, IPPC, log, DateTime.Now, "");
                var ghiLog1 = vt_.SP_GHIFILELOG(User.UserName, PCName, IPPC, log1, DateTime.Now, "");
                return PartialView();
            }

            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
        #region in báo cáo KS
        public ActionResult _PrintReportKS(string SOCTXN)
        {
            try
            {
                if (string.IsNullOrEmpty(SOCTXN))
                {
                    return Json(new { status = -1, title = "", text = "Vui lòng chọn Số CT", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                var ks = dbks_.NL_CHATLUONGKIEMTRA.Where(p => p.SOCTXN == SOCTXN).FirstOrDefault();
                if (string.IsNullOrEmpty(ks.DONVIKIEMTRA))
                {
                    return Json(new { status = -1, title = "", text = "Chưa kiểm tra không thể in", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                ViewData["URL_IMAGE"] = Utils.WebConfigKey.Domain;
                var baoCao = dbks_.SP_RPT_INPHIEUKIEMTRACHATLUONG_SOI(SOCTXN, "").ToList();
                ViewBag.inVT = baoCao;
                var ab = baoCao.FirstOrDefault();
                ViewBag.chuKy = vt_.TBL_DMCHUKY.Where(p => p.MANV == ab.THUKHO).ToList();
                ViewBag.chuKyGD = vt_.TBL_DMCHUKY.Where(p => p.MANV == ab.GDDV).ToList();
                return PartialView();
            }

            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
        public string GetComputerName()
        {
            try
            {
                string clientMachineName;
                clientMachineName = (Dns.GetHostEntry(Request.ServerVariables["remote_addr"]).HostName);

                return clientMachineName;
            }
            catch (Exception )
            {
                return string.Empty;
            }
        }

        public string GetIPClient()
        {
            try
            {
                string IP;
                IP = Request.UserHostAddress;

                return IP;
            }
            catch (Exception)
            {
                return "";
            }
        }

    }
}