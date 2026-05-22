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
using ToolsApp.Helper;

namespace ToolsApp.Controllers
{
    [Authorize]
    public class PhieuXuatKhoController : BaseController
    {

        wqlvattuEntities vt_ = new wqlvattuEntities();
        wqlkhosoiEntities soi_ = new wqlkhosoiEntities();
        private NetsuiteTTEntities1 nstt_ = new NetsuiteTTEntities1();

        #region Index
        // GET: PhieuXuatKho
        public ActionResult Index()
        {
            return View();
        }
        #endregion

        #region Load View
        public ActionResult _Getlist(string SOCTXNSearch = null)
        {
            var List = soi_.NL_DMPHIEUXN.Where(p =>
           (
               (SOCTXNSearch == null || SOCTXNSearch == "" || p.SOCTXN.Contains(SOCTXNSearch)) &&
               p.MaNVYC == User.UserName
           && p.SOCTXN.Substring(0,1) == "X"


           )).OrderByDescending(p => p.NGAY).OrderByDescending(c => c.STT).ToList();

            ViewBag.List = List;
            ViewBag.User = User.UserName;
            return PartialView();
        }
        #endregion

        #region Load View _GetListCTPhieu_View _
        public ActionResult _GetListCTPhieu_View(string SOCTXN, string KEY)
        {         
            var List = soi_.NL_CTXUATNHAP.Where(p => p.SOCTXN == SOCTXN).ToList();
            var APIs = soi_.NL_DMPHIEUXN.Where(c => c.SOCTXN == SOCTXN).ToList();
            ViewBag.APIs = APIs;
            ViewBag.List = List;

            return PartialView("_GetListCTPhieu_View", new NL_CTXUATNHAPViewModels { SOCTXN = SOCTXN });
        }
        #endregion          

        #region Load View CTphieu 
        public ActionResult _GetListCTPhieu(string SOCTXN, string NOIDEN, string MAHTHUC, string MAKHO)
        {

            var APIs = soi_.NL_DMPHIEUXN.Where(c => c.SOCTXN == SOCTXN).ToList();
            ViewBag.APIs = APIs;
            var dhsoi = soi_.VATTU2024_SPLOAD_MADONHANG_ND_XK("").ToList();
            ViewBag.dhsoi = dhsoi;

            ViewBag.SOCTXN = SOCTXN;           
            ViewBag.ChkAPI = APIs.FirstOrDefault().GUIAPI;           
            ViewBag.MAHTHUC = MAHTHUC;
            ViewBag.MAKHO = MAKHO;
            var imahthuc = "";
            if (MAHTHUC == "XBAN")
            {
                imahthuc = "XBAN";  
            }    
            var masoi = soi_.LOAD_DATA_MASOI_XUATKHO_THEO_XKNB(MAHTHUC, MAKHO).ToList();
        
            ViewBag.masoi = masoi;

            return PartialView("_GetListCTPhieu"+ imahthuc, new NL_CTXUATNHAPViewModels { SOCTXN = SOCTXN });
        }
        #endregion

        #region Load View Thêm mới
        public ActionResult _Insert()
        {
            var MANVYC = soi_.Load_MANVYC_XNKHO_SOI().ToList();
            ViewBag.MANVYC = MANVYC;
            var makhoxuat = soi_.SP_MAKHOBYMANV_LOAD(User.UserName).ToList();
            ViewBag.makhoxuat = makhoxuat;
            var makhonhan = soi_.DANHMUC_KHO_NOIDEN_XUATKHO("").ToList();
            ViewBag.makhonhan = makhonhan;
            var masocp = vt_.SP_NL_LOADMSCP().ToList();
            ViewBag.masocp = masocp;
            var loaihinhthuc = soi_.SP_LOAD_MAHINHTHUC_XUATKHO().ToList();
            ViewBag.loaihinhthuc = loaihinhthuc;
            var SOCTXN = soi_.SP_NL_TAOSOCHUNGTU(loaihinhthuc.FirstOrDefault().MAHTHUC).FirstOrDefault();
            ViewBag.SOCTXN = SOCTXN;    
            var dmdonvi = vt_.VATTU2024_SP_LOAD_QUYEN_SUDUNG_DMDONVI(User.UserName).ToList();
            var f_iddonvi = dmdonvi.FirstOrDefault().IDMaDV;
            ViewBag.DMDONVI = dmdonvi;
            ViewBag.manvnhan = vt_.DMNHANVIENs.Where(p => p.IDMaDV == f_iddonvi && p.HieuLuc == 1).ToList();

            return PartialView("_Insert", new NL_DMPHIEUXNViewModels { /*Sophieu = Sophieu.FirstOrDefault().Sophieu*/ });
        }
        #endregion

        #region Load View Update _UpdateCTPhieu_XBAN
        public ActionResult _UpdateCTPhieu_XBAN(string KHOAKEYXN, string SOCTXN)
        {
            var ctphieu = soi_.NL_CTXUATNHAP.FirstOrDefault(p => p.KHOAKEYXN == KHOAKEYXN && p.SOCTXN == SOCTXN);
            //var makho = dbks_.NL_DMPHIEUXN.FirstOrDefault(p => p.SOCTXN == model.SOCTXN).MAKHO;
            var dmphieu = soi_.NL_DMPHIEUXN.Where(p => p.SOCTXN == SOCTXN).FirstOrDefault();
            var dvt = vt_.tblDMDVTINHs.ToList();

            ViewBag.HIEU = soi_.NL_DMNHACCAP.ToList();         
            ViewBag.maphieupd = soi_.VATTU2024_LOAD_DATA_MASOI_XUATKHO_THEO_XKNB_LOADMAPHIEUPD( "XBAN" ,dmphieu.MAKHOXUAT, ctphieu.MAVT).ToList();

            ViewBag.ctphieu = ctphieu;
            ViewBag.dmphieu = dmphieu;
            return PartialView("_UpdateCTPhieu_XBAN", new XUATNHAPViewModels { KHOAKEYXN = KHOAKEYXN });
        }
        #endregion


        #region Update CT phiếu XBAN //ok
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _ActionUpdateCTPhieu_XBAN(NL_CTXUATNHAPViewModels model)
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

                        model.NGAYKETOAN = new DateTime(NgayLapTuSearch_.Year,
                            NgayLapTuSearch_.Month, NgayLapTuSearch_.Day, 0, 0, 0);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                }
                #endregion
                var dmxuatnhap = soi_.NL_DMPHIEUXN.FirstOrDefault(p => p.SOCTXN == model.SOCTXN);
                var f_LOAIXN = dmxuatnhap.MAHTHUC;
                var f_SOCTXN = model.SOCTXN;
                var item = soi_.NL_CTXUATNHAP.FirstOrDefault(p => p.KHOAKEYXN == model.KHOAKEYXN && p.SOCTXN == model.SOCTXN);
                if (item == null)
                {
                    return Json(new { status = -1, title = "", text = "Không tìm thấy chi tiết phiếu.", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    item.GHICHU = model.GHICHU;
                    item.NGAYKETOAN = model.NGAYKETOAN;
                    item.HIEU = model.HIEU;
                    item.LO = model.LO;
                    item.MAPHIEUPD = model.MAPHIEUPD;
                    item.MANVMODIFY = User.UserName;
                    item.NGAYMODIFY = DateTime.Now;
                    soi_.Entry(item).State = EntityState.Modified;
                    soi_.SaveChanges();
                    return Json(new { status = 1, title = "", text = "Cập nhật thành công.", obj = f_SOCTXN, obj2 = f_LOAIXN }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region OnChange
        public JsonResult _Change_SOCTXN(string MAHTHUCid)
        {

            try
            {
                var SOCTXN = soi_.SP_NL_TAOSOCHUNGTU(MAHTHUCid).FirstOrDefault();

                return Json(new { status = 1, title = "", text = "", obj = SOCTXN }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { status = -1, title = "", text = e.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult _Onchange_Khonhan(string MAKHOXUAT, string MAHTHUC)
        {

            try
            {
                dynamic MAKHONHAN = "";
                if (MAHTHUC == "XKNB")
                {
                    MAKHONHAN = soi_.SP_MAKHOBYMANV_LOAD(User.UserName).ToList();   
                }
                if (MAHTHUC == "XGC")
                {
                    var idmadv = vt_.DMNHANVIENs.Where(p => p.MANV == User.UserName).FirstOrDefault().IDMaDV;
                    MAKHONHAN = soi_.SP_LOAD_DMKHO_BY_IDMADV_gc__(idmadv).ToList();
                }
                else
                {
                    MAKHONHAN = soi_.SP_MAKHOBYMANV_LOAD(User.UserName).ToList();
                }    
                return Json(new { status = 1, title = "", text = "", obj = MAKHONHAN }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { status = -1, title = "", text = e.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Insert
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _InsertFun(NL_DMPHIEUXNViewModels model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    soi_.SP_NL_DELETE_KHONGCHITIET_XUATNHAP(User.UserName); // xóa các phiếu trống không có chi tiết

                    var tb = soi_.NL_DMPHIEUXN.FirstOrDefault(p => p.SOCTXN == model.SOCTXN);

                    if (tb == null)
                    {
                        #region Check tồn tại
                        if (soi_.NL_DMPHIEUXN.FirstOrDefault(p => p.SOCTXN == model.SOCTXN) != null)
                        {
                            return Json(new { status = -2, title = "", text = "Thêm không thành công. SOCTXN đã tồn tại. Kiểm tra lại.", obj = model.SOCTXN }, JsonRequestBehavior.AllowGet);
                        }
                        if (model.MAHTHUC == "" || model.MAKHOXUAT == "")
                        {
                            return Json(new { status = -2, title = "", text = "Thêm không thành công. Phiếu không được trống. Kiểm tra lại.", obj = model.SOCTXN }, JsonRequestBehavior.AllowGet);
                        }
                     

                        #endregion
                        var mabp = vt_.DMNHANVIENs.Where(p => p.MANV == User.UserName).FirstOrDefault().MABP;
                        var model_copy = new EntityFramework.KhoSoi.NL_DMPHIEUXN();
                        model_copy.SOCTXN = model.SOCTXN.Trim();
                        model_copy.MAHTHUC = model.MAHTHUC.Trim();
                        model_copy.NGAY = DateTime.Now;
                        model_copy.MAKHO = model.MAKHOXUAT.Trim();
                        model_copy.NOIDEN = model.MAKHONHAN.Trim();
                        model_copy.IDMADVNHAN = model.IDMADVNHAN.Trim();
                        model_copy.IDMADVXUAT = model.IDMADVXUAT.Trim();
                        model_copy.MAKHONHAN = model.MAKHONHAN.Trim();
                        model_copy.MAKHOXUAT = model.MAKHOXUAT.Trim();
                        model_copy.MANVNHAN = model.MANVNHAN;                        
                        model_copy.GHICHU = (model.GHICHU == null || model.GHICHU == "") ? "" : model.GHICHU;
                        model_copy.MABP = (mabp == null || mabp == "") ? "" : mabp;
                        model_copy.MaNVYC = User.UserName; //model.MaNVYC;
                       // model_copy.MANVXUAT = User.UserName;
                      //  model_copy.NGAY = DateTime.Now;
                        model_copy.SOPO = (model.SOPO == null || model.SOPO == "") ? "" : model.SOPO;
                        soi_.NL_DMPHIEUXN.Add(model_copy);
                        soi_.SaveChanges();

                        if (model.MAHTHUC.Trim() == "XBAN")
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

                            #region Get PO
                            dynamic internalidcreatedfrom = "";
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
                                var po = (SalesOrder)result.recordList[0];
                                internalidcreatedfrom = po.internalId; // lấy internal PO

                                #region Update Internalid PO
                                var tb_update = soi_.NL_DMPHIEUXN.FirstOrDefault(p => p.SOCTXN == model.SOCTXN);
                                tb_update.InternalidPO = internalidcreatedfrom;
                                soi_.Entry(tb_update).State = EntityState.Modified;
                                soi_.SaveChanges();
                                #endregion

                                RecordRef salesOrderRef = new RecordRef();
                                salesOrderRef.internalId = internalidcreatedfrom;
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
                                      


                                        CustomFieldRef[] madonhang = itemfields.customFieldList;

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
                                            if (soi_.TBLDMNGUYENLIEUx.FirstOrDefault(p => p.MAVATTU == itemname) == null)
                                            {
                                                return Json(new { status = -1, title = "", text = "MAVT không tồn tại", obj = "" }, JsonRequestBehavior.AllowGet);
                                            }




                                            var model_item = new EntityFramework.KhoSoi.NL_CTXUATNHAP();
                                            var ID_XN = Guid.NewGuid();
                                            //model_copy.KHOAKEYXN = model.MAVT + "||" + model.HIEU + "||" + model.LO + "||" + model.MAPHIEUPD + "||" + model.MAHTHUC + "||" + ID_XN;
                                            model_item.KHOAKEYXN = itemname + "||" + model.MAHTHUC.Trim() + "_KeyKhoa_" + ID_XN;
                                            model_item.MAVT = itemname.Trim().ToUpper();
                                            model_item.HIEU = "30032024";
                                            model_item.LO = "";
                                            model_item.SOLUONGYC = Convert.ToDecimal(soluong);
                                            model_item.SOLUONGTT = Convert.ToDecimal(soluong);
                                            model_item.MAPHIEUPD = "";
                                            model_item.DonGia = Convert.ToDecimal(tonggiaban);
                                            model_item.GHICHU = model.GHICHU;
                                            model_item.NGAYKETOAN = DateTime.Now;
                                            model_item.SOCTXN = model.SOCTXN;
                                            model_item.MAVTTAM = itemname;
                                            model_item.MADH = model.MADH == null ? "" : model.MADH;
                                            model_item.KHOATHAMCHIEU = ID_XN.ToString();
                                            model_item.ID_XN = ID_XN;
                                            model_item.SOHOADON = "";
                                            model_item.LOAIDONHANG = "";
                                            model_item.NK_STATIC = true;
                                            model_item.DAXUATKHO = 0;


                                            soi_.NL_CTXUATNHAP.Add(model_item);
                                            soi_.SaveChanges();
                                        }

                                        catch (Exception ex)
                                        {
                                            return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
                                        }

                                    }

                                }
                            }
                            else
                            {

                                return Json(new { status = -1, text = "Không tìm thấy PO, Vui lòng kiểm tra số PO trên Netsuite ở mục Purchase Order", obj = "" }, JsonRequestBehavior.AllowGet);
                            }

                            
                        }

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

        #region Insert CT 
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _InsertFunCTPhieuXN(NL_CTXUATNHAPViewModels model)
        {
            if (ModelState.IsValid)
            {

                #region Xử lý ngày
                CultureInfo cul = CultureInfo.GetCultureInfo("en-GB");
                var NgayLapTuSearch_ = new DateTime();
                if (!string.IsNullOrEmpty(model.NGAYKETOAN_))
                {
                    try
                    {
                        NgayLapTuSearch_ = DateTime.ParseExact(model.NGAYKETOAN_, "dd/MM/yyyy", cul);

                        model.NGAYKETOAN = new DateTime(NgayLapTuSearch_.Year,
                            NgayLapTuSearch_.Month, NgayLapTuSearch_.Day, 00, 00, 01);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                }
                #endregion
                try
                {
                    var tb = 1;

                    if (tb > 0)
                    {


                        #region Check tồn tại
                        if (model.SOLUONGYC == 0)
                        {
                            return Json(new { status = -2, title = "", text = "Thêm không thành công. Số lượng phải lơn hơn 0. Kiểm tra lại.", obj = "" }, JsonRequestBehavior.AllowGet);
                        }
                        if (model.SOLUONGYC > model.SOLUONGTON)
                        {
                            return Json(new { status = -2, title = "", text = "Thêm không thành công. Số lượng yêu cầu không được lớn hơn số lượng tồn. Kiểm tra lại.", obj = "" }, JsonRequestBehavior.AllowGet);
                        }
                        if (model.MAVT == null || model.MAVT == "")
                        {
                            return Json(new { status = -2, title = "", text = "Thêm không thành công. Vui lòng chọn MAVT.", obj = "" }, JsonRequestBehavior.AllowGet);
                        }
                        //if (soi_.NL_CTXUATNHAP.Where(p => p.MAVT == model.MAVT && p.SOCTXN == model.SOCTXN).ToList().Count > 0)
                        //{
                        //    return Json(new { status = -2, title = "", text = "Thêm không thành công. MAVT đã tồn tại, Vui lòng chọn MAVT khác.", obj = "" }, JsonRequestBehavior.AllowGet);
                        //}
                        if ((model.MAHTHUC == "XD1" || model.MAHTHUC == "XD2" || model.MAHTHUC == "XAH") && (model.MADH == "" || model.MADH == null))
                        {
                            return Json(new { status = -2, title = "", text = "Thêm không thành công. MADH không được trống. Kiểm tra lại.", obj = model.SOCTXN }, JsonRequestBehavior.AllowGet);
                        }

                        #endregion


                        var model_copy = new EntityFramework.KhoSoi.NL_CTXUATNHAP(); 
                        var ID_XN = Guid.NewGuid();
                        //model_copy.KHOAKEYXN = model.MAVT + "||" + model.HIEU + "||" + model.LO + "||" + model.MAPHIEUPD + "||" + model.MAHTHUC + "||" + ID_XN;
                        model_copy.KHOAKEYXN = model.MAVT + "||" + model.MAHTHUC.Trim() + "_KeyKhoa_" + ID_XN;
                        model_copy.MAVT = model.MAVT.Trim().ToUpper();
                        model_copy.HIEU = model.HIEU;
                        model_copy.LO = model.LO == null ? "" : model.LO;
                        model_copy.SOLUONGYC = Convert.ToDecimal(model.SOLUONGYC);
                        model_copy.SOLUONGTT = Convert.ToDecimal(model.SOLUONGYC);
                        model_copy.MAPHIEUPD = model.MAPHIEUPD;
                        model_copy.GHICHU = model.GHICHU;
                        model_copy.NGAYKETOAN = (DateTime)model.NGAYKETOAN;
                        model_copy.SOCTXN = model.SOCTXN;
                        model_copy.MAVTTAM = model.MAVT;
                        model_copy.MADH = model.MADH == null ? "" : model.MADH;
                        model_copy.KHOATHAMCHIEU = ID_XN.ToString();
                        model_copy.ID_XN = ID_XN;
                        model_copy.SOHOADON = model.SOHOADON;
                        model_copy.LOAIDONHANG = model.LOAIDONHANG;
                        model_copy.NK_STATIC = true;
                        model_copy.DAXUATKHO = 0;


                        soi_.NL_CTXUATNHAP.Add(model_copy);
                        soi_.SaveChanges();


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

        #region Delete
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _DeleteFun(string SOCTXN)
        {
            try
            {
                #region Check tồn tại
                if (soi_.NL_CTXUATNHAP.FirstOrDefault(p => p.SOCTXN == SOCTXN) != null)
                {
                    return Json(new { status = -2, title = "", text = "Xóa không thành công. SOCTXN còn tồn tại trong chi tiết. Vui lòng xóa chi tiết trước!.", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                #endregion

                var item = soi_.NL_DMPHIEUXN.FirstOrDefault(p => p.SOCTXN == SOCTXN);
                soi_.NL_DMPHIEUXN.Remove(item);
                soi_.SaveChanges();
                return Json(new { status = 1, title = "", text = "Xóa thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Delete ct
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _DeletectFun(Guid Khoa)
        {
            try
            {
                //    var manvhanhdong = db_.sp_update_manvhanhdong_ghilog(User.UserName, "", Khoa);


                var item = soi_.NL_CTXUATNHAP.FirstOrDefault(p => p.ID_XN == Khoa);


                //item.NguoiHD_logtrigger = User.UserName;
                //db_.Entry(item).State = EntityState.Modified;
                //db_.SaveChanges();

                soi_.NL_CTXUATNHAP.Remove(item);
                soi_.SaveChanges();
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
        public JsonResult _DeleteALLFun(string SOCTXN)
        {
            try
            {
                var item = soi_.NL_CTXUATNHAP.Where(p => p.SOCTXN == SOCTXN).ToList();


                foreach (var items in item)
                {
                    soi_.NL_CTXUATNHAP.Remove(items);
                }

                soi_.SaveChanges();
                return Json(new { status = 1, title = "", text = "Xóa tất cả thành công.", obj = SOCTXN }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Inventory Transfer (Xuất kho)
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _GuiAPIs(string SOCTXN = null)
        {
            if (ModelState.IsValid)
            {
                #region Xử lý dữ liệu 
                var tb = soi_.NL_DMPHIEUXN.Where(p => p.SOCTXN == SOCTXN && (p.GUIAPI == "0" || p.GUIAPI == null)).ToList();
                var f_tb = tb.FirstOrDefault();
              
                var ctxuatnhap = soi_.NL_CTXUATNHAP.Where(p => p.SOCTXN == SOCTXN).ToList();
                var mahthuc = tb.FirstOrDefault().MAHTHUC;
                var mahinhthuc = soi_.NL_DMHINHTHUC.Where(p => p.MAHTHUC == mahthuc).ToList();
                var mahinhthuc_item = mahinhthuc.Count < 1 ? "" : mahinhthuc.FirstOrDefault().XUAT3BUOC;

                var KhoX = nstt_.Khoes.Where(p => p.makho == f_tb.MAKHOXUAT).ToList();                                 
                var KhoN = nstt_.Khoes.Where(p => p.makho == f_tb.MAKHONHAN).ToList();
                if(KhoN.Count < 1)
                {
                 
                     KhoN = nstt_.Khoes.Where(p => p.externalid == f_tb.MAKHONHAN).ToList();
                }
                if ( KhoX.Count < 1)
                {
                    KhoX = nstt_.Khoes.Where(p => p.externalid == f_tb.MAKHOXUAT).ToList();
                   
                }

                if (KhoX.Count < 1 || KhoX.Count < 1)
                {
                    return Json(new { status = -1, title = "", text = "Chưa mapping được Kho xuất && Kho nhập, vui lòng liên hệ ITC kiểm tra lại", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                var f_KhoX = KhoX.FirstOrDefault().externalid;
                var f_KhoN = KhoN.FirstOrDefault().externalid;

                var department = nstt_.Donvi_mapping_Department.Where(p => p.madv == f_tb.MABP).ToList();
                #endregion

                try
                {
                    if (tb.Count > 0)
                    {
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
                        if (ns != null)
                        {
                            try
                            {
                                if (mahinhthuc_item == "3")
                                {
                                    #region TRANSFER ORDERS

                                    var ctx = soi_.NL_DMPHIEUXN.Where(p => p.SOCTXN == SOCTXN && (p.GUIAPI == "0" || p.GUIAPI == null)).FirstOrDefault();
                                    var ct_ctx = soi_.NL_CTXUATNHAP.Where(p => p.SOCTXN == SOCTXN).ToList();

                                    TransferOrder[] orders = new TransferOrder[1];
                                    TransferOrder order = new TransferOrder();

                                    #region CUSTOM FORM
                                    RecordRef cusfRec = new RecordRef();
                                    cusfRec.internalId = "110";
                                    order.customForm = cusfRec;
                                    #endregion

                                    #region active field to edit
                                    order.tranDateSpecified = true;
                                    order.orderStatusSpecified = true;
                                    //order.totalSpecified = true;
                                    #endregion

                                    //order.tranId = ctx.SOCTXN;
                                    order.memo = ctx.GHICHU;
                                    order.externalId = ctx.SOCTXN;
                                    order.orderStatus = TransferOrderOrderStatus._pendingFulfillment;
                                    //order.total = ct_ctx.Sum(c => (c.SOLUONG == null ? 0 : (double)c.SOLUONG.Value));

                                    #region DATE
                                    DateCustomFieldRef dateF = new DateCustomFieldRef();
                                    dateF.value = Convert.ToDateTime(ctx.NGAY);
                                    order.tranDate = dateF.value;
                                    #endregion

                                    #region FROM LOCATION
                                    RecordRef location = new RecordRef();
                                    location.externalId = f_KhoX;
                                    order.location = location;
                                    #endregion
                                    //externalid: PS-A3.04: TPH SX -Phúc Long. MAY
                                    //externalid: PS-A2.04: TPH SX -Vĩnh Lộc. MAY
                                    //externalid: PM-A7.06: TPH MA -Quận 3. DPH
                                    #region TO LOCATION
                                    RecordRef transferlocation = new RecordRef();
                                    transferlocation.externalId = f_KhoN;
                                    order.transferLocation = transferlocation;
                                    #endregion

                                    #region CLASS
                                    //RecordRef classes = new RecordRef();
                                    //classes.externalId = "SP_MAY_ND_LE";
                                    //order.@class = classes;
                                    #endregion

                                    #region DEPARTMENT
                                  
                                    RecordRef PYC_Department = new RecordRef();
                                    PYC_Department.externalId = department.Count < 1 ? "" : department.FirstOrDefault().externalid;
                                    order.department = PYC_Department;

                                    #endregion

                                    //region Inventory items to be added to this Sales Order
                                    TransferOrderItemList orderItemList = new TransferOrderItemList();
                                    List<TransferOrderItem> orderItems = new List<TransferOrderItem>(); //1 Line item
                                                                                                        //TransactionBodyCustomField[] cusfield = new TransactionBodyCustomField[100];

                                    int i = 0;
                                    CustomFieldRef[] fields = new CustomFieldRef[20];

                                    #region CHỨNG TỪ THAM CHIẾU
                                    //StringCustomFieldRef SoLenhDieuDong = new StringCustomFieldRef();
                                    //SoLenhDieuDong.scriptId = "custbody_btm_tt_chung_tu_tham_chieu";
                                    //SoLenhDieuDong.value = ctx.SoLenhDieuDong;
                                    //fields[i] = SoLenhDieuDong;
                                    //i++;
                                    #endregion

                                    #region PHÂN LOẠI PHIẾU ĐIỀU/ VẬN CHUYỂN NỘI BỘ
                                    SelectCustomFieldRef loaiyeucau = new SelectCustomFieldRef();
                                    ListOrRecordRef listref_loaiyeucau = new ListOrRecordRef();
                                    listref_loaiyeucau.internalId = "2";
                                    loaiyeucau.scriptId = "custbody_btm_tt_loai_phieu_van_chuyen";
                                    loaiyeucau.value = listref_loaiyeucau;
                                    fields[i] = loaiyeucau;
                                    i++;
                                    #endregion

                                    #region Line item
                                    foreach (var ct in ct_ctx)
                                    {
                                        TransferOrderItem line = new TransferOrderItem();
                                        RecordRef recordRef_mamh = new RecordRef();
                                        recordRef_mamh.externalId = ct.MAVT; //Item's external id
                                        line.item = recordRef_mamh;
                                        line.quantitySpecified = true;
                                        line.quantity = ct.SOLUONGTT == 0 ? 0 : (double)ct.SOLUONGTT;

                                        #region inventory details
                                        InventoryDetail inventoryDetail = new InventoryDetail();
                                        InventoryAssignmentList inventoryAssignmentList = new InventoryAssignmentList();

                                        List<InventoryAssignment> List_inventoryAssignment = new List<InventoryAssignment>();
                                        InventoryAssignment inventoryAssignment = new InventoryAssignment();

                                        RecordRef lotRecord = new RecordRef();
                                       // lotRecord.externalId = ct.MAVT + "_" + ct.LO;
                                        lotRecord.externalId = ct.MAVT + "_" + ct.HIEU + ((ct.LO == null || ct.LO == "") ? "" : ("-" + ct.LO));
                                        inventoryAssignment.issueInventoryNumber = lotRecord;
                                        inventoryAssignment.quantitySpecified = true;
                                        inventoryAssignment.quantity = (double)ct.SOLUONGTT;

                                        List_inventoryAssignment.Add(inventoryAssignment);
                                        inventoryAssignmentList.inventoryAssignment = List_inventoryAssignment.ToArray();
                                        inventoryDetail.inventoryAssignmentList = inventoryAssignmentList;

                                        line.inventoryDetail = inventoryDetail;
                                        #endregion

                                        orderItems.Add(line);
                                    }
                                    #endregion

                                    orderItemList.item = orderItems.ToArray();
                                    order.itemList = orderItemList;
                                    order.customFieldList = fields;
                                    WriteResponse response = ns.Service.upsert(order);

                                    var jsonString = "{ \"resultStatus\":" + Newtonsoft.Json.JsonConvert.SerializeObject(response.status) + ",";
                                    jsonString += "\"resultData\":" + Newtonsoft.Json.JsonConvert.SerializeObject(order) + "}";

                                    if (response.status.isSuccess == false)
                                    {
                                        return Json(new { status = -1, title = "", text = response.status.statusDetail.FirstOrDefault().message, obj = jsonString }, JsonRequestBehavior.AllowGet);
                                    }

                                    return Json(new { status = 1, title = "", text = "Đồng bộ thành công", obj = jsonString }, JsonRequestBehavior.AllowGet);
                                    #endregion
                                }
                                if(mahinhthuc_item == "1")
                                {
                                    #region Inventory Transfer
                                    foreach (var items in tb)
                                    {
                                        #region Gọi thông tin data đã mapping

                                        #endregion

                                        #region Xử lý add vào Netsuite 423
                                        //var manetsuite = db_.SP_API_Load_MaNetSuiteYC().FirstOrDefault().MaNetSuiteYC;
                                        InventoryTransfer IT_PhieuXkho = new InventoryTransfer();
                                        //Date
                                        IT_PhieuXkho.tranDate = DateTime.Now;

                                        //Số phiếu// Ref.No
                                         IT_PhieuXkho.tranId = items.SOCTXN; //-- bị chặn trên netsuite


                                        IT_PhieuXkho.externalId = items.SOCTXN;

                                        //From location
                                      
                                        RecordRef FromLocation = new RecordRef();
                                        FromLocation.externalId = f_KhoX;
                                        //  FromLocation.externalId = Khoxuat.Count < 1 ? "" : Khoxuat.FirstOrDefault().externalid;
                                        IT_PhieuXkho.location = FromLocation;

                                        //From TransLocation 
                          
                                        RecordRef ToLocation = new RecordRef();
                                        ToLocation.externalId = f_KhoN;
                                       // ToLocation.externalId = Khonhan.Count < 1 ? "" : Khonhan.FirstOrDefault().externalid;
                                        IT_PhieuXkho.transferLocation = ToLocation;

                                        //Memo / Ghi chú
                                        IT_PhieuXkho.memo = items.GHICHU == null ? "" : items.GHICHU;

                                        // đơn vị nhận// Department
                                  
                                        RecordRef PYC_Department = new RecordRef();
                                        PYC_Department.externalId = department.Count < 1 ? "" : department.FirstOrDefault().externalid;
                                        IT_PhieuXkho.department = PYC_Department;

                                        // Phân loại phiếu
                                        CustomFieldRef[] Cus_PhieuXkho = new CustomFieldRef[2];

                                        //var phanloai = nstt_.Table_LoaiPhieuVanChuyenNoiBo
                                        SelectCustomFieldRef select_cus_phanloaiphieu = new SelectCustomFieldRef();
                                        ListOrRecordRef List_phanloaiphieu = new ListOrRecordRef();
                                        List_phanloaiphieu.internalId = "2";
                                        select_cus_phanloaiphieu.scriptId = "custbody_btm_tt_loai_phieu_van_chuyen";
                                        select_cus_phanloaiphieu.value = List_phanloaiphieu;
                                        Cus_PhieuXkho[0] = select_cus_phanloaiphieu;

                                        StringCustomFieldRef REF_SOCTXN = new StringCustomFieldRef();
                                        REF_SOCTXN.scriptId = "custbody_btm_tt_ma_don_yeu_cau";
                                        REF_SOCTXN.value = items.SOCTXN.ToString();
                                        Cus_PhieuXkho[1] = REF_SOCTXN;

                                        //// CHI TIẾT PHIẾU YC

                                        var CTphieu = soi_.NL_CTXUATNHAP.Where(p => p.SOCTXN == items.SOCTXN && p.DAXUATKHO == 1).ToList();
                                        InventoryTransferInventoryList orderItemList = new InventoryTransferInventoryList();
                                        List<InventoryTransferInventory> orderItems = new List<InventoryTransferInventory>();

                                        foreach (var ctitem in CTphieu)
                                        {
                                            InventoryTransferInventory item_PhieuXkho = new InventoryTransferInventory();

                                            //Item // Mặt hàng// MAVT
                                            RecordRef Item_rec = new RecordRef();
                                            Item_rec.externalId = ctitem.MAVT;
                                            //Item_rec.type = RecordType.serializedInventoryItem;
                                            //Item_rec.typeSpecified = true;
                                            item_PhieuXkho.item = Item_rec;
                                            item_PhieuXkho.adjustQtyBy = Convert.ToDouble(ctitem.SOLUONGTT);
                                            item_PhieuXkho.adjustQtyBySpecified = true;

                                            InventoryDetail inventoryDetail = new InventoryDetail();

                                            InventoryAssignmentList inventoryAssignmentList = new InventoryAssignmentList();

                                            List<InventoryAssignment> List_inventoryAssignment = new List<InventoryAssignment>();

                                            InventoryAssignment inventoryAssignment = new InventoryAssignment();

                                            RecordRef lotRecord = new RecordRef();
                                            // lotRecord.externalId = "AA030001002_HAND";
                                            lotRecord.externalId = ctitem.MAVT + "_" + ctitem.HIEU + ((ctitem.LO == null || ctitem.LO == "") ? "" : ("-"+ ctitem.LO));

                                            inventoryAssignment.issueInventoryNumber = lotRecord;
                                            inventoryAssignment.quantity = Convert.ToDouble(ctitem.SOLUONGTT);
                                            inventoryAssignment.quantitySpecified = true;
                                            List_inventoryAssignment.Add(inventoryAssignment);
                                            inventoryAssignmentList.inventoryAssignment = List_inventoryAssignment.ToArray();
                                            inventoryDetail.inventoryAssignmentList = inventoryAssignmentList;

                                            item_PhieuXkho.inventoryDetail = inventoryDetail;

                                            orderItems.Add(item_PhieuXkho);
                                        }

                                        orderItemList.inventory = orderItems.ToArray();

                                        IT_PhieuXkho.customFieldList = Cus_PhieuXkho;
                                        IT_PhieuXkho.inventoryList = orderItemList;

                                        WriteResponse response = ns.Service.upsert(IT_PhieuXkho);

                                        #endregion

                                        if (response.status.isSuccess == false)
                                        {
                                            var messerror = ((ToolsApp.com.netsuite.webservices.Status)response.status).statusDetail;
                                            var messdetail = messerror.FirstOrDefault().message;
                                            return Json(new { status = 0, title = "Error", text = messdetail, obj = "" }, JsonRequestBehavior.AllowGet);
                                        }
                                        else
                                        {
                                            //#region update GuiAPI = 1
                                            //var IsAPIslcaymoc = db_.tblSLCayMocs.Where(c => c.Khoa1 == items.Khoa1).FirstOrDefault();
                                            //IsAPIslcaymoc.GUIAPI = true;
                                            //IsAPIslcaymoc.MaNetsuiteYC = manetsuite;
                                            //IsAPIslcaymoc.NgayGuiAPIHD = DateTime.Now;
                                            //db_.Entry(IsAPIslcaymoc).State = EntityState.Modified;
                                            //db_.SaveChanges();
                                            //#endregion
                                        }
                                    }
                                    return Json(new { status = 1, title = "", text = "Thêm thành công.", obj = "" }, JsonRequestBehavior.AllowGet);

                                    #endregion
                                }
                                if(mahinhthuc_item == "0")
                                {                                    
                                        #region Xuất sử dụng, 
                                        #region Call data                     

                                        var customform = "207"; // Điều chỉnh tồn                        
                                                                //var customform = "169"; // Điều chỉnh tồn                        
                                                                // var cls = nstt_.Classes.FirstOrDefault(p => p.externalid == "BAN_PTVT");
                                        var taikhoanketoan = mahinhthuc.FirstOrDefault().Account_KT;
                                        var ex_location = f_KhoX;
                                        #endregion

                                        #region InventoryAdjustment
                                        InventoryAdjustment InAdj = new InventoryAdjustment();
                                        CustomFieldRef[] cusfield = new CustomFieldRef[100];

                                        RecordRef cusfRec = new RecordRef();
                                        cusfRec.internalId = customform;
                                        InAdj.customForm = cusfRec;

                                        InAdj.externalId = f_tb.Externalid_IAD;
                                        InAdj.tranId = f_tb.Externalid_IAD;

                                        RecordRef account = new RecordRef();
                                        account.internalId = taikhoanketoan;
                                        InAdj.account = account;

                                        InAdj.tranDateSpecified = true;
                                        InAdj.tranDate = Convert.ToDateTime(f_tb.NGAY);

                                        RecordRef khoxuat = new RecordRef();
                                        // khoxuat.externalId = kNhan.Count < 1 ? "" : kNhan.FirstOrDefault().externalid;
                                        khoxuat.externalId = ex_location;
                                        InAdj.adjLocation = khoxuat;
                                        InAdj.memo = f_tb.GHICHU;

                                        RecordRef mabp = new RecordRef();
                                        mabp.externalId = department.Count < 1 ? "" : department.FirstOrDefault().externalid;
                                        InAdj.department = mabp;

                                        //RecordRef class_ = new RecordRef();
                                        //class_.externalId = cls.externalid;
                                        //InAdj.@class = class_;

                                        int j = 0;

                                        cusfield[j] = CUSTOMRECORD.FUNC_SelectCustomFieldRef("custbody_btm_tt_phan_loai_dieu_chinh",
                                                      ENUM_ID.PhanLoaiDieuChinh.DieuChinhTon); j++;
                                        cusfield[j] = CUSTOMRECORD.FUNC_SelectCustomFieldRef("custbody_btm_tt_loai_sp", "",
                                                      "VATTU"); j++;
                                        cusfield[j] = CUSTOMRECORD.FUNC_SelectCustomFieldRef("custbody_btm_tt_nguon_goc",
                                                      ENUM_ID.NGUONGOC.MUANGOAIND); j++;
                                        cusfield[j] = CUSTOMRECORD.FUNC_SelectCustomFieldRef("custbody_btm_tt_nguon_goc_hang",
                                                      ENUM_ID.NGUONGOCHANG.MUANGOAI); j++;

                                        InAdj.customFieldList = cusfield;

                                        #region Transfer CTPHIEU data

                                        InventoryAdjustmentInventoryList orderItemList_IA = new InventoryAdjustmentInventoryList();

                                        List<InventoryAdjustmentInventory> orderItems_IA = new List<InventoryAdjustmentInventory>();
                                        foreach (var items in ctxuatnhap)
                                        {
                                            CustomFieldRef[] cusDetail = new CustomFieldRef[100];
                                            InventoryAdjustmentInventory line_IA = new InventoryAdjustmentInventory();

                                            RecordRef refItem = new RecordRef();
                                            refItem.externalId = items.MAVT;
                                            line_IA.item = refItem;

                                            line_IA.adjustQtyBySpecified = true;
                                            line_IA.adjustQtyBy = Convert.ToDouble(-items.SOLUONGTT);

                                            RecordRef reflocation = new RecordRef();
                                            // reflocation.externalId = kNhan.FirstOrDefault().externalid;
                                            reflocation.externalId = ex_location;
                                            line_IA.location = reflocation;
                                            orderItems_IA.Add(line_IA);
                                        }

                                        #endregion

                                        orderItemList_IA.inventory = orderItems_IA.ToArray();
                                        InAdj.inventoryList = orderItemList_IA;

                                        WriteResponse response_IA = ns.Service.upsert(InAdj);

                                        #endregion end InventoryAdjustment

                                        if (response_IA.status.isSuccess == false)
                                        {
                                            var messerror = ((ToolsApp.com.netsuite.webservices.Status)response_IA.status).statusDetail;
                                            var messdetail = messerror.FirstOrDefault().message;
                                            return Json(new { status = 0, title = "Error", text = messdetail, obj = "" }, JsonRequestBehavior.AllowGet);
                                        }
                                        #endregion end Xuất sử dụng  
                                }
                            }
                            catch (Exception ex)
                            {
                                return Json(new { status = 0, title = "Error", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                    else
                    {
                        return Json(new { status = -1, title = "", text = "Không có dữ liệu hoặc đã được đẩy API.", obj = "" }, JsonRequestBehavior.AllowGet);
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



    }
}