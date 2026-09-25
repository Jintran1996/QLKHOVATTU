using NSClient;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using ToolsApp.Authentication;
using ToolsApp.com.netsuite.webservices;
using ToolsApp.EntityFramework.NetsuiteTT;
using ToolsApp.EntityFramework.VatTu;
using ToolsApp.EntityFramework.KhoSoi;
using ToolsApp.Models;
using System.Globalization;
using System.Collections.Generic;
using ToolsApp.Utilities;
using ToolsApp.Helper;

namespace ToolsApp.Controllers
{
    [Authorize]
    public class PhieuYCXuatKho_VattuController : BaseController
    {

        wqlvattuEntities vt_ = new wqlvattuEntities();
        wqlkhosoiEntities soi_ = new wqlkhosoiEntities();
        NetsuiteTTEntities1 nstt_ = new NetsuiteTTEntities1();

        #region Index
        // GET: PhieuYCXuatKho_VattuController
        public ActionResult Index()
        {
            return View();
        }
        #endregion

        #region Load View //OK
        public ActionResult _Getlist(string SOCTXNSearch = null)
        {
            var List = vt_.VATTU2024_SP_GET_DM_XUATNHAP(User.UserName, "X").Where(p =>
            (
                (SOCTXNSearch == null || SOCTXNSearch == "" || p.SOCTXN.Contains(SOCTXNSearch))

            )).ToList();

            ViewBag.List = List;

            return PartialView();
        }
        #endregion

        #region GET_MAPHIEUPD_BY_RADCOMBOBOXMAVATTU
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
        #endregion

        #region Load View Update CTphieu_View_X 
        public ActionResult _GetListCTPhieu_View(string SOCTXN_CT)
        {
            var ctphieu = vt_.VATTU2024_LOADXUATNHAP_NEW2024(SOCTXN_CT).Where(p => p.SoCTXN == SOCTXN_CT).ToList();
            var dmphieu = vt_.DM_XUATNHAP.Where(p => p.SOCTXN == SOCTXN_CT).ToList();
            var dvt = vt_.tblDMDVTINHs.ToList();
            ViewBag.dvt = dvt;
            ViewBag.LOAIXN = dmphieu.FirstOrDefault().LOAIXN;
            ViewBag.List = ctphieu;
            ViewBag.list_Phieu = dmphieu;
            return PartialView("_GetListCTPhieu_View", new XUATNHAPViewModels { SoCTXN = SOCTXN_CT });
        }
        #endregion

        #region Load View Update _UpdateCTPhieu_XBAN
        public ActionResult _UpdateCTPhieu_XBAN(string KHOAKEYXN, string SOCTXN)
        {
            var ctphieu = vt_.XUATNHAPs.FirstOrDefault(p => p.KHOAKEYXN == KHOAKEYXN && p.SoCTXN == SOCTXN);
            //var makho = dbks_.NL_DMPHIEUXN.FirstOrDefault(p => p.SOCTXN == model.SOCTXN).MAKHO;
            var dmphieu = vt_.DM_XUATNHAP.Where(p => p.SOCTXN == SOCTXN).FirstOrDefault();
            var dvt = vt_.tblDMDVTINHs.ToList();

            ViewBag.dvt = dvt;
            ViewBag.maphieupd = vt_.VATTU2024_SP_LOADVTYEUCAUXUATKHO_DONVI_SELECT_MAPHIEUPD(dmphieu.MAKHOXUAT, "", ctphieu.MAVT).ToList();

            ViewBag.ctphieu = ctphieu;
            ViewBag.dmphieu = dmphieu;
            return PartialView("_UpdateCTPhieu_XBAN", new XUATNHAPViewModels { KHOAKEYXN = KHOAKEYXN });
        }
        #endregion

        #region Load View _GetListCTPhieu_X 
        public ActionResult _GetListCTPhieu(string SOCTXN, string LOAIXN, string MAKHONHAN, string MAKHOXUAT)
        {
            var f_dm_xuatnhap = vt_.DM_XUATNHAP.FirstOrDefault(p => p.SOCTXN == SOCTXN);
            var kyhieu = vt_.VATTU2024_SPLOAD_KiHieu_DV(User.UserName, f_dm_xuatnhap.IDMADVXUAT).FirstOrDefault();
            var check_loaixn = vt_.VATTU2024_SPLOAD_LOAIXN_SHOWFULL_ITEM(LOAIXN).ToList();  
            dynamic list_MAVT = vt_.VATTU2024_SP_LOADVTYEUCAUXUATKHO_DONVI(MAKHOXUAT, kyhieu, "", LOAIXN).ToList();
            var idmadv = vt_.DM_XUATNHAP.FirstOrDefault(p => p.SOCTXN == SOCTXN).IDMADVNHAN;    
            ViewBag.MAVT = list_MAVT;
            ViewBag.SOCTXN = SOCTXN;
            ViewBag.LOAIXN = LOAIXN;
            ViewBag.MAKHONHAN = MAKHONHAN;
            ViewBag.MAKHOXUAT = MAKHOXUAT;
            ViewBag.IDMACS = vt_.VATTU2024_SP_LOAD_IDMACS_BY_DMDONVI(idmadv).ToList();
            ViewBag.donhang = soi_.VATTU2024_SPLOAD_MADONHANG_ND_XK("").ToList();
            ViewBag.idmadv = idmadv;
            var iLOAIXN = "X";
            if (LOAIXN == "XBAN")
            {
                iLOAIXN = "XBAN";
            }
            if (LOAIXN == "XHCMT")
            {
                ViewBag.MAVTHC = vt_.VATTU2024_SPLOAD_DANHMUCVATTU(LOAIXN).ToList();
                iLOAIXN = "XHCMT";
            }
            if (LOAIXN == "XTU")
            {
                ViewBag.MAVTHC = vt_.VATTU2024_SPLOAD_DANHMUCVATTU(LOAIXN).ToList();
                iLOAIXN = "XHCMT";
            }
            if (LOAIXN == "XUAT_SX")
            {
                ViewBag.SOCTXN_KETHUA = vt_.VATTU2024_SOCTKETHUA_XUATSX(MAKHOXUAT, "").ToList();
                iLOAIXN = "XUAT_SX";
            }
            return PartialView("_GetListCTPhieu_" + iLOAIXN, new XUATNHAPViewModels { SoCTXN = SOCTXN });
            //return PartialView("_GetListCTPhieu_X", new XUATNHAPViewModels { SoCTXN = SOCTXN });
        }
        #endregion

        #region Update CT phiếu XBAN //ok
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _ActionUpdateCTPhieu_XBAN(XUATNHAPViewModels model)
        {
            try
            {
                #region Xử lý ngày
                CultureInfo cul = CultureInfo.GetCultureInfo("en-GB");
                var NgayLapTuSearch_ = new DateTime();
                if (!string.IsNullOrEmpty(model.NGAYCT_))
                {
                    try
                    {
                        NgayLapTuSearch_ = DateTime.ParseExact(model.NGAYCT_, "dd/MM/yyyy", cul);

                        model.NgayKeToan = new DateTime(NgayLapTuSearch_.Year,
                            NgayLapTuSearch_.Month, NgayLapTuSearch_.Day, 0, 0, 0);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                }
                #endregion
                var dmxuatnhap = vt_.DM_XUATNHAP.FirstOrDefault(p => p.SOCTXN == model.SoCTXN); 
                var f_LOAIXN = dmxuatnhap.LOAIXN;
                var f_SOCTXN = model.SoCTXN;
                var item = vt_.XUATNHAPs.FirstOrDefault(p => p.KHOAKEYXN == model.KHOAKEYXN && p.SoCTXN == model.SoCTXN);
                if (item == null)
                {
                    return Json(new { status = -1, title = "", text = "Không tìm thấy chi tiết phiếu.", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    item.GHICHU = model.GHICHU;
                    item.NgayKeToan = model.NgayKeToan;
                    item.MaPhieuPD = model.MaPhieuPD;
                    item.MANVMODIFY = User.UserName;
                    item.NGAYMODIFY = DateTime.Now;
                    vt_.Entry(item).State = EntityState.Modified;
                    vt_.SaveChanges();
                    return Json(new { status = 1, title = "", text = "Cập nhật thành công.", obj = f_SOCTXN, obj2 = f_LOAIXN }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Load View Thêm mới //ok
        public ActionResult _Insert()
        {
            var kyhieu = vt_.sp_Load_NLKiHieuDV(User.UserName).FirstOrDefault();
            ViewBag.makhoxuat = "";
            var makhonhan = vt_.VATTU2024_SP_LOADKHOQUYENNHAP(User.UserName, "1").ToList();
            ViewBag.makhonhan = makhonhan;
            var makhoxuat = vt_.VATTU2024_SP_LOADKHOQUYENNHAP(User.UserName, "2").ToList();
            ViewBag.makhoxuat = makhoxuat;
            var loaixuatnhap = vt_.VATTU2024_SPLOAD_DMHINHTHUC_XUATKHO(User.UserName).ToList();
            var f_loaixuatnhap = loaixuatnhap.Count < 1 ? "" : loaixuatnhap.FirstOrDefault().LOAIXN;
            ViewBag.loaixuatnhap = loaixuatnhap;
            var SOCTXN = vt_.VATTU2024_SP_TAOSCT_NHAPKHO(kyhieu.ToUpper(), f_loaixuatnhap).ToList();
            var f_SOCTXN = SOCTXN.Count < 1 ? "" : SOCTXN.FirstOrDefault().SOCTXN;
            ViewBag.SOCTXN = f_SOCTXN.ToUpper();
            var NL_DMNHACCAP = soi_.NL_DMNHACCAP.ToList();
            ViewBag.NL_DMNHACCAP = NL_DMNHACCAP;
            var dmdonvi = vt_.VATTU2024_SP_LOAD_QUYEN_SUDUNG_DMDONVI(User.UserName).ToList();
            var f_iddonvi = dmdonvi.FirstOrDefault().IDMaDV;
            ViewBag.DMDONVI = dmdonvi;
            ViewBag.manvnhan = vt_.DMNHANVIENs.Where(p => p.IDMaDV == f_iddonvi && p.HieuLuc == 1).ToList();

            return PartialView("_Insert", new DM_XUATNHAPViewModels { /*Sophieu = Sophieu.FirstOrDefault().Sophieu*/ });
        }
        #endregion

        #region OnChange // ok
        public JsonResult _Change_SOCTXN(string LOAIXNid)
        {
            try
            {
                var kyhieu = vt_.sp_Load_NLKiHieuDV(User.UserName).FirstOrDefault();
                var SOCTXN = vt_.VATTU2024_SP_TAOSCT_NHAPKHO(kyhieu.ToUpper(), LOAIXNid).FirstOrDefault().SOCTXN;
                return Json(new { status = 1, title = "", text = "", obj = SOCTXN.ToUpper() }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { status = -1, title = "", text = e.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult _Onchange_Location_MANV(string IDMADVNHAN)
        {
            try
            {

                var manvnhan = vt_.DMNHANVIENs.Where(p => p.IDMaDV == IDMADVNHAN && p.HieuLuc == 1).ToList();

                return Json(new { status = 1, title = "", text = "", obj = manvnhan }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { status = -1, title = "", text = e.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult _Change_NCC_KHOXUAT(string LOAIXNid)
        {
            dynamic KHOXUAT = "";
            var kyhieu = vt_.sp_Load_NLKiHieuDV(User.UserName).FirstOrDefault();
            //if (LOAIXNid == "X" || LOAIXNid == "XBAN")
            //{
            KHOXUAT = vt_.VATTU2024_SP_LOADKHOQUYENNHAP(User.UserName, "1").ToList();
            //}
            return Json(new { status = 1, title = "", text = "", obj = KHOXUAT }, JsonRequestBehavior.AllowGet);
        }


        #endregion

        #region Load View Update CTphieu_NK // ok
        public ActionResult _UpdateCTPhieu(string KHOAKEYXN, string SOCTXN)
        {
            var ctphieu = vt_.XUATNHAPs.FirstOrDefault(p => p.KHOAKEYXN == KHOAKEYXN && p.SoCTXN == SOCTXN);
            //var makho = dbks_.NL_DMPHIEUXN.FirstOrDefault(p => p.SOCTXN == model.SOCTXN).MAKHO;
            var dmphieu = vt_.DM_XUATNHAP.Where(p => p.SOCTXN == SOCTXN).FirstOrDefault();
            var dvt = vt_.tblDMDVTINHs.ToList();
            ViewBag.dvt = dvt;
            ViewBag.ctphieu = ctphieu;
            ViewBag.dmphieu = dmphieu;
            return PartialView("_FormCTPhieu_X", new XUATNHAPViewModels { KHOAKEYXN = KHOAKEYXN });
        }
        #endregion

        #region Update CT phiếu NK //ok
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _ActionUpdateCTPhieu_NK(XUATNHAPViewModels model)
        {
            try
            {
                #region Xử lý ngày
                CultureInfo cul = CultureInfo.GetCultureInfo("en-GB");
                var NgayLapTuSearch_ = new DateTime();
                if (!string.IsNullOrEmpty(model.NGAYCT_))
                {
                    try
                    {
                        NgayLapTuSearch_ = DateTime.ParseExact(model.NGAYCT_, "dd/MM/yyyy", cul);

                        model.NgayKeToan = new DateTime(NgayLapTuSearch_.Year,
                            NgayLapTuSearch_.Month, NgayLapTuSearch_.Day, 0, 0, 0);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                }
                #endregion
                var dmxuatnhap = vt_.DM_XUATNHAP.FirstOrDefault(p => p.SOCTXN == model.SoCTXN);
                var f_LOAIXN = dmxuatnhap.LOAIXN;
                var f_SOCTXN = model.SoCTXN;
                var item = vt_.XUATNHAPs.FirstOrDefault(p => p.KHOAKEYXN == model.KHOAKEYXN && p.SoCTXN == model.SoCTXN);
                if (item == null)
                {
                    return Json(new { status = -1, title = "", text = "Không tìm thấy chi tiết phiếu.", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    item.SoCTKeToan = model.SoCTKeToan;
                    item.SoLuongTT = Convert.ToDecimal(model.SoLuongTT);
                    item.NgayKeToan = dmxuatnhap.NGAY;
                    item.GHICHU = model.GHICHU;
                    item.TenVT_NCC = model.TenVT_NCC;
                    item.DVT_NCC = model.DVT_NCC;
                    item.SOLUONGTT_NCC = Convert.ToDecimal(model.SOLUONGTT_NCC);
                    item.MANVMODIFY = User.UserName;
                    item.NGAYMODIFY = DateTime.Now;
                    vt_.Entry(item).State = EntityState.Modified;
                    vt_.SaveChanges();
                    return Json(new { status = 1, title = "", text = "Cập nhật thành công.", obj = f_SOCTXN, obj2 = f_LOAIXN }, JsonRequestBehavior.AllowGet);

                }

            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region _InsertFun //ok
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _InsertFun(DM_XUATNHAPViewModels model)
        {
            try
            {
                #region Xử lý ngày
                CultureInfo cul = CultureInfo.GetCultureInfo("en-GB");
                var NgayLapTuSearch_ = new DateTime();
                if (!string.IsNullOrEmpty(model.NGAYCT_))
                {
                    try
                    {
                        NgayLapTuSearch_ = DateTime.ParseExact(model.NGAYCT_, "dd/MM/yyyy", cul);

                        model.NGAYCT = new DateTime(NgayLapTuSearch_.Year,
                            NgayLapTuSearch_.Month, NgayLapTuSearch_.Day, 0, 0, 0);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                }
                #endregion

                var tb = vt_.DM_XUATNHAP.FirstOrDefault(p => p.SOCTXN == model.SOCTXN);

                if (tb == null)
                {
                    #region Check tồn tại
                    if (vt_.DM_XUATNHAP.FirstOrDefault(p => p.SOCTXN == model.SOCTXN) != null)
                    {
                        return Json(new { status = -2, title = "", text = "Thêm không thành công. SOCTXN đã tồn tại. Kiểm tra lại.", obj = model.SOCTXN }, JsonRequestBehavior.AllowGet);
                    }
                    if (vt_.XUATNHAPs.FirstOrDefault(p => p.SoCTXN == model.SOCTXN) != null)
                    {
                        return Json(new { status = -2, title = "", text = "Thêm không thành công. SOCTXN đã tồn tại.LH ITC Kiểm tra lại.", obj = model.SOCTXN }, JsonRequestBehavior.AllowGet);
                    }
                    if (model.LOAIXN == "" || model.LOAIXN == null)
                    {
                        return Json(new { status = -2, title = "", text = "Thêm không thành công. LOAIXN không được trống. Kiểm tra lại.", obj = model.SOCTXN }, JsonRequestBehavior.AllowGet);
                    }
                    if (model.MAKHONHAN == "" || model.MAKHONHAN == null)
                    {
                        return Json(new { status = -2, title = "", text = "Thêm không thành công. MAKHONHAN không được trống. Kiểm tra lại.", obj = model.SOCTXN }, JsonRequestBehavior.AllowGet);
                    }
                    if (model.MAKHOXUAT == "" || model.MAKHOXUAT == null)
                    {
                        return Json(new { status = -2, title = "", text = "Thêm không thành công. MAKHOXUAT không được trống. Kiểm tra lại.", obj = model.SOCTXN }, JsonRequestBehavior.AllowGet);
                    }
                    if (model.SOPO == "" || model.SOPO == null && model.LOAIXN == "XBAN")
                    {
                        return Json(new { status = -2, title = "", text = "Thêm không thành công. SO SaleOrder không được trống. Kiểm tra lại.", obj = model.SOCTXN }, JsonRequestBehavior.AllowGet);
                    }
                    #endregion
                    var dmnhanvien = vt_.DMNHANVIENs.FirstOrDefault(p => p.MANV == User.UserName);
                    #region Insert DM_XUATNHAP
                    var model_copy = new EntityFramework.VatTu.DM_XUATNHAP();
                    model_copy.SOCTXN = model.SOCTXN.Trim();
                    model_copy.LOAIXN = model.LOAIXN.Trim();
                    model_copy.NGAY = model.NGAYCT;
                    model_copy.IDMADVNHAN = model.IDMADVNHAN;
                    model_copy.IDMADVXUAT = model.IDMADVXUAT;
                    //model_copy.IDMADVXUAT = model.NGAYCT;
                    model_copy.MAKHOXUAT = model.MAKHOXUAT.Trim();
                    model_copy.MAKHONHAN = model.MAKHONHAN.Trim();
                    model_copy.NGUOINHAN = model.NGUOINHAN;
                    model_copy.SOPO = (model.SOPO == null || model.SOPO == "") ? "" : model.SOPO;
                    model_copy.GHICHU = (model.GHICHU == null || model.GHICHU == "") ? "" : model.GHICHU;
                    model_copy.MaNVYC = User.UserName;
                    model_copy.MANVCN = User.UserName;
                    model_copy.NGAYCN = DateTime.Now;

                    vt_.DM_XUATNHAP.Add(model_copy);
                    vt_.SaveChanges();
                    #endregion

                    if (model.LOAIXN.Trim() == "XBAN")
                    {
                        #region Netsuite
                        //if (tb.Count > 0)
                        //{
                        System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                        // In order to enable SOAPscope to work through SSL. Refer to FAQ for more details
                        ServicePointManager.ServerCertificateValidationCallback += delegate (object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
                        {
                            return true;
                        };

                        NSClient.NSClient ns = null;
                        try
                        {
                            ns = new NSClient.NSClient();
                            NSBase.Client = ns;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error while loading the application:" + ex.Message);
                            Console.WriteLine("Press Enter to quit ... ");
                            Console.ReadKey();
                        }

                        #endregion
                        //int i = 0;
                        dynamic internalidSO = "";
                        // var phieu = tb.FirstOrDefault();

                        #region Get PO
                        var tranId = model.SOPO;
                        TransactionSearchBasic search = new TransactionSearchBasic();
                        search.type = new SearchEnumMultiSelectField();
                        search.type.@operator = SearchEnumMultiSelectFieldOperator.anyOf;
                        search.type.operatorSpecified = true;
                        search.type.searchValue = new string[] { "salesOrder" }; // Lọc theo loại giao dịch là salesOrder
                        search.tranId = new SearchStringField();
                        search.tranId.@operator = SearchStringFieldOperator.@is;
                        search.tranId.operatorSpecified = true;
                        search.tranId.searchValue = tranId; // Đặt giá trị tranId cần tìm


                        SearchPreferences searchPrefs = new SearchPreferences();
                        SearchResult result = ns.Service.search(search); // Thực hiện tìm kiếm
                        #endregion

                        if (result.status.isSuccess && result.recordList != null && result.recordList.Length > 0)
                        {
                            var so = (SalesOrder)result.recordList[0];
                            internalidSO = so.internalId; // lấy internal SO

                            #region Update Internalid PO
                            var tb_update = vt_.DM_XUATNHAP.FirstOrDefault(p => p.SOCTXN == model.SOCTXN);
                            tb_update.InternalidPO = internalidSO;
                            vt_.Entry(tb_update).State = EntityState.Modified;
                            vt_.SaveChanges();
                            #endregion

                            RecordRef salesOrderRef = new RecordRef();
                            salesOrderRef.internalId = internalidSO;
                            salesOrderRef.type = RecordType.salesOrder;
                            salesOrderRef.typeSpecified = true;

                            // Lấy thông tin chi tiết của đơn đặt hàng mua hàng
                            ReadResponse salesOrderRecords = ns.Service.get(salesOrderRef);

                            if (!salesOrderRecords.status.isSuccess)
                            {
                                return Json(new { status = -1, text = "Không tìm thấy PO", obj = "" }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                SalesOrder record = (SalesOrder)salesOrderRecords.record;
                                string maphieuInternalID = record.internalId;
                                SalesOrderItemList fields = (SalesOrderItemList)record.itemList;// list field trong 1 record

                                for (int k = 0; k < fields.item.Length; k++)
                                {
                                    SalesOrderItem itemfields = (SalesOrderItem)fields.item[k];
                                    var itemname = itemfields.item.name;
                                    var soluong = itemfields.quantity;
                                    var dvt = itemfields.units.name;
                                    var tonggiaban = itemfields.grossAmt;
                                    var dondenghimuahang = "";

                                    //CustomFieldRef[] madonhang = itemfields.customFieldList;

                                    //for (int j = 0; j < madonhang.Length; j++)
                                    //{
                                    //    CustomFieldRef madonhangfield = madonhang[j];
                                    //    if (madonhangfield.scriptId == "custcol_btm_tt_purchase_request")
                                    //    {
                                    //        if (madonhangfield.GetType() == typeof(SelectCustomFieldRef))
                                    //        {
                                    //            SelectCustomFieldRef valField = madonhangfield as SelectCustomFieldRef;
                                    //            dondenghimuahang = valField.value.name;
                                    //        }
                                    //    }

                                    //}

                                    try
                                    {
                                        if (vt_.DANHMUCVATTUs.FirstOrDefault(p => p.MAVT == itemname) == null)
                                        {
                                            return Json(new { status = -1, title = "", text = "MAVT không tồn tại", obj = "" }, JsonRequestBehavior.AllowGet);
                                        }

                                        var donvi = vt_.DMDONVIs.Where(p => p.IDMaDV == model.IDMADVXUAT).FirstOrDefault();

                                        var model_item = new EntityFramework.VatTu.XUATNHAP();
                                        model_item.SoCTXN = model.SOCTXN.Trim();
                                        model_item.LoaiXN = model.LOAIXN.Trim();
                                        model_item.DonViYeuCau = dmnhanvien.MADV;
                                        model_item.MaBP = dmnhanvien.MABP;
                                        model_item.MaPhieuPD = dondenghimuahang;
                                        model_item.MAVT = itemname;
                                        model_item.MaVTTam = itemname;
                                        model_item.SoLuongYC = Convert.ToDecimal(soluong);
                                        model_item.SoLuongTT = Convert.ToDecimal(soluong);
                                        model_item.TongGia = Convert.ToDecimal(tonggiaban);
                                        model_item.MaKhoXuat = model.MAKHOXUAT.Trim();
                                        model_item.MaKho = model.MAKHOXUAT.Trim();
                                        model_item.GHICHU = (model.GHICHU == null || model.GHICHU == "") ? "" : model.GHICHU;
                                        model_item.MaNVYC = User.UserName;
                                        model_item.MANVCN = User.UserName;
                                        model_item.Ngay = DateTime.Now;
                                        model_item.NGAYCN = DateTime.Now;
                                        model_item.NgayKeToan = tb_update.NGAY;
                                      
                                        var KeyKhoa = Guid.NewGuid();
                                        model_item.KHOAKEYXN = model.LOAIXN.Trim() + "_KeyKhoa_" + KeyKhoa;
                                        vt_.XUATNHAPs.Add(model_item);
                                        vt_.SaveChanges();
                                    }

                                    catch (Exception ex)
                                    {
                                        var messageERR = ex.InnerException.InnerException.Message;
                                        return Json(new { status = -1, title = "", text = messageERR, obj = "" }, JsonRequestBehavior.AllowGet);
                                    }

                                }

                            }
                        }
                        else
                        {
                            return Json(new { status = -1, text = "Không tìm thấy PO, Vui lòng kiểm tra số PO trên Netsuite ở mục Purchase Order", obj = "" }, JsonRequestBehavior.AllowGet);
                        }


                        return Json(new { status = 1, title = "", text = "Thêm thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
                    }

                    return Json(new { status = 1, title = "", text = "Thêm thành công.", obj = "" }, JsonRequestBehavior.AllowGet);

                }
                else
                {
                    return Json(new { status = -2, title = "", text = "Thêm không thành công. SOCTXN đã tồn tại vui lòng làm mới SOCTXN bằng cách ấn f5 để load lại chương trình ", obj = "" }, JsonRequestBehavior.AllowGet);

                }
            }
            catch (Exception ex)
            {
                var messageERR = ex.InnerException.InnerException.Message;
                return Json(new { status = -1, title = "", text = messageERR, obj = "" }, JsonRequestBehavior.AllowGet);
            }

        }
        #endregion

        #region Insert CT theo đơn hàng
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _InsertFunCTPhieu(XUATNHAPViewModels model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (model.SoCTXN != null)
                    {
                        #region Check tồn tại
                        if (vt_.DM_XUATNHAP.FirstOrDefault(p => p.SOCTXN == model.SoCTXN && p.DAXUATKHO_ == true && p.GUIAPI == "1") != null)
                        {
                            return Json(new { status = -2, title = "", text = "Thêm không thành công. phiếu này đã được xuất kho.", obj = "" }, JsonRequestBehavior.AllowGet);
                        }
                        #endregion

                        var items = vt_.DM_XUATNHAP.Where(p => p.SOCTXN == model.SoCTXN).FirstOrDefault();
                        var dmnhanvien = vt_.DMNHANVIENs.Where(p => p.MANV == User.UserName).FirstOrDefault();
                        var donvi = vt_.DMDONVIs.Where(p => p.IDMaDV == items.IDMADVNHAN).FirstOrDefault();

                        var model_item = new EntityFramework.VatTu.XUATNHAP();
                        model_item.SoCTXN = model.SoCTXN.Trim();
                        model_item.LoaiXN = items.LOAIXN;
                        model_item.DonViYeuCau = donvi == null ? dmnhanvien.MADV : donvi.MaDonVi;
                        model_item.MaBP = model.MaBP == null ? dmnhanvien.MABP : model.MaBP;
                        model_item.MaPhieuPD = model.MaPhieuPD == null ? "" : model.MaPhieuPD;
                        model_item.MAVT = model.MAVT;
                        model_item.MaVTTam = model.MAVT;
                        model_item.SoLuongYC = Convert.ToDecimal(model.SoLuongYC);
                        model_item.SoLuongTT = Convert.ToDecimal(model.SoLuongYC);
                        model_item.TongGia = Convert.ToDecimal(0.00);
                        model_item.MaKhoXuat = items.MAKHOXUAT.Trim();
                        model_item.MaKho = items.MAKHOXUAT.Trim();
                        model_item.GHICHU = (model.GHICHU == null || model.GHICHU == "") ? "" : model.GHICHU;
                        model_item.MaNVYC = User.UserName;
                        model_item.MANVCN = User.UserName;
                        model_item.MADH_CN = model.MADH_CN;
                        model_item.Ngay = DateTime.Now;
                        model_item.NgayKeToan = items.NGAY;
                        model_item.NGAYCN = DateTime.Now;

                        var KeyKhoa = Guid.NewGuid();
                        model_item.KHOAKEYXN = items.LOAIXN.Trim() + "_KeyKhoa_" + KeyKhoa;
                        vt_.XUATNHAPs.Add(model_item);
                        vt_.SaveChanges();
                    }
                    else
                    {
                        return Json(new { status = -2, title = "", text = "Thêm không thành công. Dữ liệu không tồn có.  Kiểm tra lại.", obj = "" }, JsonRequestBehavior.AllowGet);
                    }

                    return Json(new { status = 1, title = "", text = "Thêm thành công.", obj = "" }, JsonRequestBehavior.AllowGet);

                }

                catch (Exception ex)
                {
                    return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new { status = -2, title = "", text = "Lưu không thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Kế thừa phiếu xuất XUAT_SX
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _KethuaSOCTXN_xuatsx(XUATNHAPViewModels model)
        {
            if (ModelState.IsValid)
            {
                //try
                //{
                if (model.SoCTXN != null)
                {
                    var list_kethua = vt_.XUATNHAPs.Where(p => p.SoCTXN == model.SOCTXN_xuatsx_kethua).ToList();
                    var items = vt_.DM_XUATNHAP.Where(p => p.SOCTXN == model.SoCTXN).FirstOrDefault();
                    var dmnhanvien = vt_.DMNHANVIENs.Where(p => p.MANV == User.UserName).FirstOrDefault();
                    var donvi = vt_.DMDONVIs.Where(p => p.IDMaDV == items.IDMADVNHAN).FirstOrDefault();

                    var insertkethua = vt_.VATTU2024_KETHUA_SOCTXN_XUATSX_(items.SOCTXN.Trim(), User.UserName, items.MAKHOXUAT.Trim(), model.SOCTXN_xuatsx_kethua, items.LOAIXN);             

                }
                else
                {
                    return Json(new { status = -2, title = "", text = "Thêm không thành công. Dữ liệu không tồn có.  Kiểm tra lại.", obj = "" }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { status = 1, title = "", text = "Thêm thành công.", obj = "" }, JsonRequestBehavior.AllowGet);

                //}

                //catch (Exception ex)
                //{
                //    return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
                //}
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
        public JsonResult _DeleteFun(string SOCTXN)
        {
            try
            {
                //var items = vt_.XUATNHAPs.Where(p => p.SoCTXN == SOCTXN).ToList();
                //foreach (var itemz in items)
                //{
                //    vt_.XUATNHAPs.Remove(itemz);
                //}
                //vt_.SaveChanges();

                #region Check tồn tại
                if (vt_.XUATNHAPs.FirstOrDefault(p => p.SoCTXN == SOCTXN) != null)
                {
                    return Json(new { status = -2, title = "", text = "Xóa không thành công. Số phiếu còn tồn tại trong chi tiết. Vui lòng xóa chi tiết trước!.", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                #endregion

                var item = vt_.DM_XUATNHAP.FirstOrDefault(p => p.SOCTXN == SOCTXN);
                vt_.DM_XUATNHAP.Remove(item);
                vt_.SaveChanges();
                return Json(new { status = 1, title = "", text = "Xóa thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var messageERR = ex.InnerException.InnerException.Message;
                return Json(new { status = -1, title = "", text = messageERR, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion      

        #region Delete CT
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _DeleteCTFun(string KHOAKEYXN)
        {
            try
            {
                var items = vt_.XUATNHAPs.FirstOrDefault(p => p.KHOAKEYXN == KHOAKEYXN);
                vt_.XUATNHAPs.Remove(items);
                vt_.SaveChanges();
                return Json(new { status = 1, title = "", text = "Xóa thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion      

        #region Delete ALL Chi tiết theo số phiếu
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _DeleteALLFun(string Sophieu)
        {
            try
            {
                var item = soi_.NL_CTXUATNHAP.Where(p => p.SOCTXN == Sophieu).ToList();


                foreach (var items in item)
                {
                    soi_.NL_CTXUATNHAP.Remove(items);
                }

                soi_.SaveChanges();
                return Json(new { status = 1, title = "", text = "Xóa tất cả thành công.", obj = Sophieu }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var messageERR = ex.InnerException.InnerException.Message;
                return Json(new { status = -1, title = "", text = messageERR, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Onchange GetSLTonTheoMAVT
        public JsonResult GetSLTonTheoMAVT(string MAVT = null, string maphieu = null)
        {
            try
            {
                var MAKHOXUAT = vt_.DM_XUATNHAP.Where(p => p.SOCTXN == maphieu)?.FirstOrDefault()?.MAKHOXUAT;              

                var itemns = CUSTOMSEARCH.SEARCH_ITEM_INTERNALID(MAVT, MAKHOXUAT);
                var sltonkho = itemns.quantityonhand;


                var list = vt_.VATTU2026_SP_BAOCAOTONKHOTHEOTHOIGIAN_HANSUDUNG(MAKHOXUAT,DateTime.Now, MAVT)
                               .ToList();
                var maphieupd = list?.FirstOrDefault()?.MAPHIEUPD;

                return Json(new { status = 1, obj = sltonkho, maphieupd = maphieupd }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { status = -1, text = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region _InsertFunCTPhieu_XHCMT Action
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _InsertFunCTPhieu_XHCMT(string LOAIXN, string SOCTXN, string MAKHOXUAT, string MAVT_XHCMT, string NGAYKETOAN_XHCMT, string SoLuongTT_XHCMT, string GHICHU_XHCMT, string Maphieupd_XHCMT = null)
        {
            try
            {
        

                var dmphieu = vt_.DM_XUATNHAP.FirstOrDefault(P => P.SOCTXN == SOCTXN);
                var tb = vt_.XUATNHAPs.FirstOrDefault(P => P.SoCTXN == SOCTXN && P.MAVT == MAVT_XHCMT);
                var kyhieu = vt_.VATTU2024_SP_LOAD_QUYEN_SUDUNG_DMDONVI(User.UserName).FirstOrDefault(p => p.IDMaDV == dmphieu.IDMADVXUAT.Trim())?.KiHieu.Trim().ToUpper() ?? "";
                if (tb == null)
                {
                    #region Insert DM_XUATNHAP
                    var model_copy = new XUATNHAP();
                    model_copy.SoCTXN = SOCTXN.Trim();
                    model_copy.MAVT = MAVT_XHCMT.Trim();
                    model_copy.LoaiXN = LOAIXN.Trim();
                    model_copy.MaPhieuPD = (Maphieupd_XHCMT == null || Maphieupd_XHCMT == "") ? (kyhieu + "_" + SOCTXN.ToUpper().Trim()) : Maphieupd_XHCMT.Trim().ToUpper();
                    model_copy.SoLuongYC = Convert.ToDecimal(SoLuongTT_XHCMT);
                    model_copy.SoLuongTT = Convert.ToDecimal(SoLuongTT_XHCMT);
                    model_copy.TongGia = Convert.ToDecimal(0.00);
                    model_copy.MaKhoXuat = "";
                    model_copy.MaKho = MAKHOXUAT.Trim();
                    model_copy.GHICHU = (GHICHU_XHCMT == null || GHICHU_XHCMT == "") ? "" : GHICHU_XHCMT;
                    model_copy.MaNVYC = User.UserName;
                    model_copy.MANVCN = User.UserName;
                    model_copy.Ngay = DateTime.Now;
                    model_copy.NGAYCN = DateTime.Now;
                    model_copy.NgayKeToan = dmphieu.NGAY;
                    var KeyKhoa = Guid.NewGuid();
                    model_copy.KHOAKEYXN = LOAIXN.Trim() + "_KeyKhoa_" + KeyKhoa;
                    vt_.XUATNHAPs.Add(model_copy);
                    vt_.SaveChanges();
                    #endregion

                    return Json(new { status = 1, title = "", text = "Thêm thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = -2, title = "", text = "Thêm không thành công. ", obj = "" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var messageERR = ex.InnerException.InnerException.Message;
                return Json(new { status = -1, title = "", text = messageERR, obj = "" }, JsonRequestBehavior.AllowGet);
            }

        }
        #endregion


    }
}