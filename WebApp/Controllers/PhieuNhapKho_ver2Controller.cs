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

namespace ToolsApp.Controllers
{
    [Authorize]
    public class PhieuNhapKho_ver2Controller : BaseController
    {

        wqlvattuEntities vt_ = new wqlvattuEntities();
        wqlkhosoiEntities soi_ = new wqlkhosoiEntities();
        NetsuiteTTEntities1 nstt_ = new NetsuiteTTEntities1();

        #region Index
        // GET: PhieuNhapKho
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
               (SOCTXNSearch == null || SOCTXNSearch == "" || p.SOCTXN.Contains(SOCTXNSearch))

           )).OrderByDescending(p => p.NGAY).OrderByDescending(c => c.STT).ToList();

            ViewBag.List = List;

            return PartialView();
        }
        #endregion



        #region Load View CTphieu _
        public ActionResult _GetListCTPhieu_View(string SOCTXN, string KEY)
        {
            var item = soi_.NL_DMPHIEUXN.FirstOrDefault(p => p.SOCTXN == SOCTXN);
            // var APIs = db_.tblphieukhoes.Where(c => c.Sophieu == SOCTXN).ToList();
            //ViewBag.APIs = APIs;
            var List = soi_.NL_CTXUATNHAP.Where(p => p.SOCTXN == SOCTXN).ToList();

            ViewBag.List = List;

            return PartialView("_GetListCTPhieu_View", new NL_CTXUATNHAPViewModels { SOCTXN = SOCTXN });
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
        #region Load View CTphieu Load madh`
        public ActionResult _GetListCTPhieu(string SOCTXN, string NOIDEN, string MAHTHUC, string MAKHO)
        {

            var APIs = soi_.NL_DMPHIEUXN.Where(c => c.SOCTXN == SOCTXN).ToList();
            ViewBag.APIs = APIs;

            var masoi = soi_.LOAD_DATA_MASOI_NHAPKHO_NKNB_XBAN_XGC_v1(NOIDEN, MAHTHUC).ToList();
            ViewBag.masoi = masoi;

            ViewBag.MAHTHUC = MAHTHUC;

            dynamic NK = "";
            switch (MAHTHUC)
            {
                case "NK":
                    NK = soi_.SP_NHAPKHOTHEOPHIEUPHEDUYET_MAVT(MAKHO).ToList();
                    break;
                case "NKNB":
                    NK = soi_.LOAD_DATA_MASOI_NHAPKHO_NKNB_XBAN_XGC_v1(NOIDEN, MAKHO).ToList();
                    break;
                case "HKGC":
                    NK = soi_.SP_LOAD_DATA_SOIGIACONG_CHUAHOIKHO_V2(MAKHO, "").ToList();
                    break;
            }
            ViewBag.NK = NK;

            var masocp = vt_.SP_NL_LOADMSCP().ToList();
            ViewBag.masocp = masocp;

            var DMLO = soi_.SP_LOAD_NL_DMLO_BY_HIEU("").ToList();
            ViewBag.DMLO = DMLO;

            var NL_DMNHACCAP = soi_.NL_DMNHACCAP.ToList();
            ViewBag.NL_DMNHACCAP = NL_DMNHACCAP;




            return PartialView("_GetListCTPhieu", new NL_CTXUATNHAPViewModels { SOCTXN = SOCTXN });
        }
        #endregion


      

        #region Load View Thêm mới
        public ActionResult _Insert()
        {
            //ViewBag.madh = db_.Sp_API_LoadDonHang_CoKeyHH().ToList();
            //var idmadv = vt_.sp_kyhieudonvi_by_manv(User.UserName).FirstOrDefault().IDMaDV;
            //var kihieudonvi = vt_.sp_kyhieudonvi_by_manv(User.UserName).FirstOrDefault().KiHieu;


            //var donvinhan = db_.spLoad_DonVi("").ToList();
            //ViewBag.donvinhan = donvinhan;

            var makhoxuat = soi_.SP_MAKHOBYMANV_LOAD(User.UserName).ToList();
            ViewBag.makhoxuat = makhoxuat;
            var makhonhan = soi_.NL_DMKHO.ToList();
            ViewBag.makhonhan = makhonhan;
            var masocp = vt_.SP_NL_LOADMSCP().ToList();
            ViewBag.masocp = masocp;
            var loaihinhthuc = soi_.SP_NL_LOADDMHINHTHUC_NHAPKHOTHUKHO().ToList();
            ViewBag.loaihinhthuc = loaihinhthuc;
            var SOCTXN = soi_.SP_NL_TAOSOCHUNGTU(loaihinhthuc.FirstOrDefault().MAHTHUC).FirstOrDefault();
            ViewBag.SOCTXN = SOCTXN;

            return PartialView("_Insert", new NL_DMPHIEUXNViewModels { /*Sophieu = Sophieu.FirstOrDefault().Sophieu*/ });
        }
        #endregion

        //#region OnChange
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

        public JsonResult _Load_DMLO(string HIEUid = null)
        {
            try
            {
                dynamic DMLO = "";
                if (HIEUid != null)
                {
                    DMLO = soi_.SP_LOAD_NL_DMLO_BY_HIEU(HIEUid).ToList();
                }

                return Json(new { status = 1, title = "", text = "", obj = DMLO }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { status = -1, title = "", text = e.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }



        #region Insert
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _InsertFun(NL_DMPHIEUXNViewModels model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // soi_.SP_NL_DELETE_KHONGCHITIET_XUATNHAP(User.UserName); // xóa các phiếu trống không có chi tiết

                    var tb = soi_.NL_DMPHIEUXN.FirstOrDefault(p => p.SOCTXN == model.SOCTXN);

                    if (tb == null)
                    {
                        #region Check tồn tại
                        if (soi_.NL_DMPHIEUXN.FirstOrDefault(p => p.SOCTXN == model.SOCTXN) != null)
                        {
                            return Json(new { status = -2, title = "", text = "Thêm không thành công. SOCTXN đã tồn tại. Kiểm tra lại.", obj = model.SOCTXN }, JsonRequestBehavior.AllowGet);
                        }
                        if (model.MAHTHUC == "" || model.MAKHONHAN == "" || model.MAKHOXUAT == "")
                        {
                            return Json(new { status = -2, title = "", text = "Thêm không thành công. Phiếu không được trống. Kiểm tra lại.", obj = model.SOCTXN }, JsonRequestBehavior.AllowGet);
                        }
                        #endregion

                        var model_copy = new EntityFramework.KhoSoi.NL_DMPHIEUXN();
                        model_copy.SOCTXN = model.SOCTXN.Trim();
                        model_copy.MAHTHUC = model.MAHTHUC.Trim();
                        model_copy.NGAY = DateTime.Now;

                        model_copy.MAKHO = model.MAKHOXUAT.Trim();
                        model_copy.NOIDEN = model.MAKHONHAN.Trim();

                        model_copy.GHICHU = (model.GHICHU == null || model.GHICHU == "") ? "Nhập kho_" + DateTime.Now : model.GHICHU;
                        model_copy.MaNVYC = User.UserName;
                        model_copy.NGAY = DateTime.Now;
                        soi_.NL_DMPHIEUXN.Add(model_copy);
                        soi_.SaveChanges();

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
            //if (ModelState.IsValid)
            //{

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
            //try
            //{
            var tb = 1;

            if (tb > 0)
            {


                #region Check tồn tại
                if (model.SOLUONGTT == 0)
                {
                    return Json(new { status = -2, title = "", text = "Thêm không thành công. Số lượng phải lơn hơn 0. Kiểm tra lại.", obj = "" }, JsonRequestBehavior.AllowGet);
                }

                #endregion


                var model_copy = new EntityFramework.KhoSoi.NL_CTXUATNHAP(); 
                var ID_XN = Guid.NewGuid();
                model_copy.KHOAKEYXN = model.MAVT + "||" + model.HIEU + "||" + model.LO + "||" + model.MAPHIEUPD + "||" + model.MAHTHUC + "||" + ID_XN;
                model_copy.MAVT = model.MAVT.Trim().ToUpper();
                model_copy.HIEU = model.HIEU;
                model_copy.LO = model.LO;
                model_copy.SOLUONGYC = Convert.ToDecimal(model.SOLUONGTT);
                model_copy.SOLUONGTT = Convert.ToDecimal(model.SOLUONGTT);
                model_copy.MAPHIEUPD = model.MAPHIEUPD;
                model_copy.GHICHU = model.GHICHU;
                model_copy.NGAYKETOAN = (DateTime)model.NGAYKETOAN;
                model_copy.SOCTXN = model.SOCTXN;
                model_copy.MAVTTAM = model.MAVT;
                model_copy.KHOATHAMCHIEU = ID_XN.ToString();
                model_copy.ID_XN = ID_XN;
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

            //}

            //catch (Exception ex)
            //{
            //    return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            //}
            //}
            //else
            //{
            //    return Json(new { status = -2, title = "", text = "Lưu không thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
            //}
        }
        #endregion

        //#region Delete
        //[ValidateInput(false)]
        //[HttpPost]
        //public JsonResult _DeleteFun(string Sophieu)
        //{
        //    try
        //    {
        //        #region Check tồn tại
        //        if (db_.tblctphieukhoes.FirstOrDefault(p => p.Sophieu == Sophieu) != null)
        //        {
        //            return Json(new { status = -2, title = "", text = "Xóa không thành công. Số phiếu còn tồn tại trong chi tiết. Vui lòng xóa chi tiết trước!.", obj = "" }, JsonRequestBehavior.AllowGet);
        //        }
        //        #endregion

        //        var item = db_.tblphieukhoes.FirstOrDefault(p => p.Sophieu == Sophieu);
        //        db_.tblphieukhoes.Remove(item);
        //        db_.SaveChanges();
        //        return Json(new { status = 1, title = "", text = "Xóa thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
        //    }
        //}
        //#endregion

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
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region IRR (nhập kho)
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _TransAPI(string SOCTXN = null, string MAHINHTHUC = null, string MAKHONHAN = null)
        {
            if (ModelState.IsValid)
            {
                #region Xử lý dữ liệu 
                //    var tb = vt_.DMPHIEUx.Where(p => p.SOCTXN == SOCTXN && (p.GUIAPI == "0" || p.GUIAPI == null)).ToList();
                // var mahthuc = tb.FirstOrDefault().MAHTHUC;
                // var mahinhthuc = soi_.NL_DMHINHTHUC.Where(p => p.MAHTHUC == mahthuc).ToList();
                // var mahinhthuc_item = mahinhthuc.Count < 1 ? "" : mahinhthuc.FirstOrDefault().XUAT3BUOC;
                #endregion
                try
                {

                    #region
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

                    if (ns != null)
                    {


                        if (MAHINHTHUC == "NK")
                        {
                            #region Nhập kho mua ngoài

                            int i = 0;
                            dynamic internalidcreatedfrom = "";
                            // var phieu = tb.FirstOrDefault();
                            #region
                            //var manetsuite = db_.SP_API_Load_MaNetSuiteYC().FirstOrDefault().MaNetSuiteYC;
                            ItemReceipt ItemReceipt = new ItemReceipt();

                            ItemReceipt.tranDate = DateTime.Now;
                            ItemReceipt.tranId = "NK15122023-34582";
                            ItemReceipt.externalId = "NK15122023-34582";

                            RecordRef form = new RecordRef();
                            form.internalId = "133";
                            ItemReceipt.customForm = form;

                            //RecordRef vendor = new RecordRef();
                            //vendor.externalId = "FTWWWL001";
                            //ItemReceipt.entity = vendor;

                            #region Get PO
                            var tranId = "CC-HYF-TT-2023-001";
                            TransactionSearchBasic search = new TransactionSearchBasic();
                            search.type = new SearchEnumMultiSelectField();
                            search.type.@operator = SearchEnumMultiSelectFieldOperator.anyOf;
                            search.type.operatorSpecified = true;
                            search.type.searchValue = new string[] { "purchaseOrder" }; // Lọc theo loại giao dịch là Purchase Order
                            search.tranId = new SearchStringField();
                            search.tranId.@operator = SearchStringFieldOperator.@is;
                            search.tranId.operatorSpecified = true;
                            search.tranId.searchValue = tranId; // Đặt giá trị tranId cần tìm

                            SearchPreferences searchPrefs = new SearchPreferences();
                            SearchResult result = ns.Service.search(search); // Thực hiện tìm kiếm
                            if (result.status.isSuccess && result.recordList != null && result.recordList.Length > 0)
                            {
                                var po = (PurchaseOrder)result.recordList[0];
                                internalidcreatedfrom = po.internalId;

                            }

                            #endregion

                            RecordRef createdfrom = new RecordRef();
                            createdfrom.internalId = internalidcreatedfrom;

                            ItemReceipt.createdFrom = createdfrom;

                            ItemReceipt.memo = "NK15122023-34582";

                            CustomFieldRef[] Cus_PhieuNkho = new CustomFieldRef[99];

                            SelectCustomFieldRef loaiphieu = new SelectCustomFieldRef();
                            ListOrRecordRef listloaiphieu = new ListOrRecordRef();
                            listloaiphieu.internalId = "1";
                            loaiphieu.scriptId = "custbody_btm_tt_loai_phieu_mua_hang";
                            loaiphieu.value = listloaiphieu;
                            Cus_PhieuNkho[i] = loaiphieu;
                            i++;

                            SelectCustomFieldRef cust_loaiyeucau = new SelectCustomFieldRef();
                            ListOrRecordRef List_loaiyeucau = new ListOrRecordRef();
                            List_loaiyeucau.internalId = "1";
                            cust_loaiyeucau.scriptId = "custbody_btm_tt_loai_yeu_cau";
                            cust_loaiyeucau.value = List_loaiyeucau;
                            Cus_PhieuNkho[i] = cust_loaiyeucau;
                            i++;


                            #region Transfer CTPHIEU data

                            ItemReceiptItemList orderItemList = new ItemReceiptItemList();


                            List<ItemReceiptItem> orderItems = new List<ItemReceiptItem>();

                            CustomFieldRef[] cusDetail = new CustomFieldRef[100];

                            ItemReceiptItem line = new ItemReceiptItem();
                            line.itemReceive = true;
                            line.itemReceiveSpecified = true;
                            line.orderLine = 1;
                            line.orderLineSpecified = true;


                            RecordRef refItem = new RecordRef();
                            refItem.externalId = "AJ068001011"; // Item trong Item Receipt phải giống với Item trong PO (không được khác). Mua gì thì nhập kho đó chứ không được nhập hàng ngoài đơn mua
                            line.item = refItem;

                            // var tongSL = ctphieu.Where(p => p.MAVT == items.MAVT).Sum(p => p.SOLUONGTT);
                            line.quantitySpecified = true;
                            line.quantity = Convert.ToDouble(1);



                            InventoryDetail inventoryDetail = new InventoryDetail();

                            InventoryAssignmentList inventoryAssignmentList = new InventoryAssignmentList();

                            List<InventoryAssignment> List_inventoryAssignment = new List<InventoryAssignment>();


                            InventoryAssignment inventoryAssignment = new InventoryAssignment();



                            inventoryAssignment.receiptInventoryNumber = "itctest123";
                            inventoryAssignment.quantity = Convert.ToDouble(1);
                            inventoryAssignment.quantitySpecified = true;

                            List_inventoryAssignment.Add(inventoryAssignment);


                            inventoryAssignmentList.inventoryAssignment = List_inventoryAssignment.ToArray();
                            inventoryDetail.inventoryAssignmentList = inventoryAssignmentList;
                            line.inventoryDetail = inventoryDetail;
                            orderItems.Add(line);

                            #endregion

                            orderItemList.item = orderItems.ToArray();
                            ItemReceipt.itemList = orderItemList;

                            ItemReceipt.customFieldList = Cus_PhieuNkho;
                            WriteResponse response = ns.Service.upsert(ItemReceipt);


                            #endregion

                            if (response.status.isSuccess == false)
                            {



                                var mess = response.status.statusDetail.FirstOrDefault().message;
                                return Json(new { status = 0, title = "Error", text = mess, obj = "" }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                RecordRef recordRef = new RecordRef();
                                recordRef.internalId = internalidcreatedfrom;
                                recordRef.type = RecordType.purchaseOrder;
                                recordRef.typeSpecified = true;
                                ReadResponse response_get = ns.Service.get(recordRef);

                                if (!response_get.status.isSuccess)
                                {

                                }
                                else
                                {
                                    PurchaseOrder purorder = (PurchaseOrder)response_get.record;
                                    var CT = new List<EntityFramework.KhoSoi.NL_CTXUATNHAP> ();
                                    foreach (var items in purorder.itemList.item.ToList())
                                    {
                                        var a = items.item.name;
                                        var b = items.quantity;
                                    }
                                }




                            }

                            #endregion
                        }
                        if (MAHINHTHUC == "HKGC")
                        {
                            #region Nhận nhập GC

                            #region Call data
                           
                            var dmphieu = soi_.NL_DMPHIEUXN.Where(p => p.SOCTXN == SOCTXN).ToList();
                            var fdmphieu = dmphieu.FirstOrDefault();
                            var ctphieu = soi_.NL_CTXUATNHAP.Where(p => p.SOCTXN == SOCTXN).ToList();
                            var code = 0;
                            var message = "";
                            var customform = "169";
                            var department = nstt_.Donvi_mapping_Department.Where(p => p.madv == fdmphieu.MABP).ToList();
                            var fdepartment = department.FirstOrDefault();
                            var cls = nstt_.Classes.FirstOrDefault(p => p.externalid == "GC_SOICHAP");
                            var kNhan = nstt_.Khoes.FirstOrDefault(p => p.makho == fdmphieu.MAKHO && p.loai == "VT");
                           // var dinhmucsoi = soi_.DINHMUC_SOIGC();
                            #endregion

                        

                        


                           #endregion

                            #region Check data

                                if (kNhan == null)
                            {
                                return Json(new { status = -1, title = "", text = "Không tìm thấy kho nhận để đồng bộ.", obj = "" }, JsonRequestBehavior.AllowGet);
                            }
                            #endregion

                            #region Transfer DMPHIEU data

                            CustomFieldRef[] cusfield = new CustomFieldRef[100];

                            InventoryAdjustment order = new InventoryAdjustment();


                            #region Thông tin phiếu
                            RecordRef cusfRec = new RecordRef();
                            cusfRec.internalId = customform;
                            order.customForm = cusfRec;

                            order.externalId = SOCTXN;

                            order.tranId = SOCTXN;

                            RecordRef account = new RecordRef();
                            account.internalId = "217";
                            order.account = account;

                            order.tranDateSpecified = true;
                            order.tranDate = Convert.ToDateTime(fdmphieu.NGAY);

                            RecordRef khonhan = new RecordRef();
                            khonhan.externalId = kNhan.externalid;
                            order.adjLocation = khonhan;

                            order.memo = fdmphieu.GHICHU;

                            RecordRef mabp = new RecordRef();
                            mabp.externalId = fdepartment.externalid;
                            order.department = mabp;

                            RecordRef class_ = new RecordRef();
                            class_.externalId = cls.externalid;
                            order.@class = class_;

                            SelectCustomFieldRef sel_loaisp = new SelectCustomFieldRef();
                            ListOrRecordRef list_loaisp = new ListOrRecordRef();
                            list_loaisp.externalId = "VATTU";
                            sel_loaisp.scriptId = "custbody_btm_tt_loai_sp";
                            sel_loaisp.value = list_loaisp;
                            cusfield[0] = sel_loaisp;

                            SelectCustomFieldRef sel_nguongochang = new SelectCustomFieldRef();
                            ListOrRecordRef list_nguongochang = new ListOrRecordRef();
                            list_nguongochang.internalId = "1";
                            sel_nguongochang.scriptId = "custbody_btm_tt_nguon_goc_hang";
                            sel_nguongochang.value = list_nguongochang;
                            cusfield[1] = sel_nguongochang;

                            SelectCustomFieldRef sel_nguongoc = new SelectCustomFieldRef();
                            ListOrRecordRef list_nguongoc = new ListOrRecordRef();
                            list_nguongoc.internalId = "203";
                            sel_nguongoc.scriptId = "custbody_btm_tt_nguon_goc";
                            sel_nguongoc.value = list_nguongoc;
                            cusfield[2] = sel_nguongoc;


                            #region Get PO
                            var internalidcreatedfrom = "";
                            var tranId = "GC-NXK-DM-TT-2023-001";
                            TransactionSearchBasic search = new TransactionSearchBasic();
                            search.type = new SearchEnumMultiSelectField();
                            search.type.@operator = SearchEnumMultiSelectFieldOperator.anyOf;
                            search.type.operatorSpecified = true;
                            search.type.searchValue = new string[] { "purchaseOrder" }; // Lọc theo loại giao dịch là Purchase Order
                            search.tranId = new SearchStringField();
                            search.tranId.@operator = SearchStringFieldOperator.@is;
                            search.tranId.operatorSpecified = true;
                            search.tranId.searchValue = tranId; // Đặt giá trị tranId cần tìm

                            SearchPreferences searchPrefs = new SearchPreferences();
                            SearchResult result = ns.Service.search(search); // Thực hiện tìm kiếm
                            if (result.status.isSuccess && result.recordList != null && result.recordList.Length > 0)
                            {
                                var po = (PurchaseOrder)result.recordList[0];
                                internalidcreatedfrom = po.internalId;

                            }

                            #endregion





                            SelectCustomFieldRef sel_pogiacong = new SelectCustomFieldRef();
                            ListOrRecordRef list_pogiacong = new ListOrRecordRef();
                            list_pogiacong.internalId = internalidcreatedfrom;
                            sel_pogiacong.scriptId = "custbody_btm_po_gia_cong";
                            sel_pogiacong.value = list_pogiacong;
                            cusfield[3] = sel_pogiacong;

                            order.customFieldList = cusfield;

                            #endregion

                            #endregion

                            #region Transfer CTPHIEU data

                            InventoryAdjustmentInventoryList orderItemList = new InventoryAdjustmentInventoryList();

                            List<InventoryAdjustmentInventory> orderItems = new List<InventoryAdjustmentInventory>();
                            foreach (var items in ctphieu)
                            {
                                CustomFieldRef[] cusDetail = new CustomFieldRef[100];

                                InventoryAdjustmentInventory line = new InventoryAdjustmentInventory();

                                RecordRef refItem = new RecordRef();
                                refItem.externalId = items.MAVT;
                                line.item = refItem;

                                var tongSL = ctphieu.Where(p => p.MAVT == items.MAVT).Sum(p => p.SOLUONGTT);
                                line.adjustQtyBySpecified = true;
                                line.adjustQtyBy = Convert.ToDouble(tongSL);

                                RecordRef reflocation = new RecordRef();
                                reflocation.externalId = kNhan.externalid;
                                line.location = reflocation;

                                var LO = ctphieu.Where(p => p.MAVT == items.MAVT).Select(p => new { Lo = p.LO, slXuat = p.SOLUONGTT }).ToList();


                                InventoryDetail inventoryDetail = new InventoryDetail();

                                InventoryAssignmentList inventoryAssignmentList = new InventoryAssignmentList();

                                List<InventoryAssignment> List_inventoryAssignment = new List<InventoryAssignment>();

                                foreach (var sLOT in LO)
                                {
                                    InventoryAssignment inventoryAssignment = new InventoryAssignment();


                                    inventoryAssignment.receiptInventoryNumber = sLOT.Lo;

                                    inventoryAssignment.quantity = Convert.ToDouble(sLOT.slXuat);
                                    inventoryAssignment.quantitySpecified = true;

                                    List_inventoryAssignment.Add(inventoryAssignment);
                                }

                                inventoryAssignmentList.inventoryAssignment = List_inventoryAssignment.ToArray();
                                inventoryDetail.inventoryAssignmentList = inventoryAssignmentList;
                                line.inventoryDetail = inventoryDetail;

                                orderItems.Add(line);
                            }

                            #endregion

                            orderItemList.inventory = orderItems.ToArray();
                            order.inventoryList = orderItemList;

                            WriteResponse response = ns.Service.upsert(order);

                            

                            #region LOG

                            if (response.status.isSuccess == false)
                            {
                                code = -1;
                                message = "Đồng bộ phiếu nhập không thành công.";

                                var jsonString = "{ \"resultStatus\":" + Newtonsoft.Json.JsonConvert.SerializeObject(response.status) + ",";
                                jsonString += "\"resultData\":" + Newtonsoft.Json.JsonConvert.SerializeObject(order) + "}";

                                #region Log
                                var log = new DongBoVATTU_SOI__Json_Log();
                                log.Id_Json = SOCTXN;
                                log.Source = "PhieuNhapKhoController";
                                log.JsonNotification = jsonString;
                                log.Error = true;
                                log.Action = "Đồng bộ phiếu xuất không thành công.";
                                log.CreateDate = DateTime.Now;
                                log.CreateBy = User.UserName;
                                log.MayTinh = Environment.MachineName;
                                log.Type = MAHINHTHUC;
                                nstt_.DongBoVATTU_SOI__Json_Log.Add(log);
                                nstt_.SaveChanges();
                                #endregion
                            }
                            else
                            {
                                code = 1;
                                message = "Đồng bộ phiếu nhập thành công.";
                            }
                            return Json(new { status = code, title = "", text = message + "<br />" + response.status.statusDetail[0].message, obj = "" }, JsonRequestBehavior.AllowGet);
                            #endregion
                        }


                        if (MAHINHTHUC == "NK_MUAGC")
                        {
                            #region NK_MUAGC

                            #region Call data
                            var dmphieu = soi_.NL_DMPHIEUXN.Where(p => p.SOCTXN == SOCTXN).ToList();
                            var fdmphieu = dmphieu.FirstOrDefault();
                            var ctphieu = soi_.NL_CTXUATNHAP.Where(p => p.SOCTXN == SOCTXN).ToList();
                            var code = 0;
                            var message = "";
                            var customform = "169";
                            var department = nstt_.Donvi_mapping_Department.Where(p => p.madv == fdmphieu.MABP).ToList();
                            var fdepartment = department.FirstOrDefault();
                            var cls = nstt_.Classes.FirstOrDefault(p => p.externalid == "GC_SOICHAP");
                            var kNhan = nstt_.Khoes.FirstOrDefault(p => p.makho == fdmphieu.MAKHO && p.loai == "VT");
                            #endregion

                            #region Check data

                            if (kNhan == null)
                            {
                                return Json(new { status = -1, title = "", text = "Không tìm thấy kho nhận để đồng bộ.", obj = "" }, JsonRequestBehavior.AllowGet);
                            }
                            #endregion

                            #region Transfer DMPHIEU data

                            CustomFieldRef[] cusfield = new CustomFieldRef[100];

                            InventoryAdjustment order = new InventoryAdjustment();


                            #region Thông tin phiếu
                            RecordRef cusfRec = new RecordRef();
                            cusfRec.internalId = customform;
                            order.customForm = cusfRec;

                            order.externalId = SOCTXN;

                            order.tranId = SOCTXN;

                            RecordRef account = new RecordRef();
                            account.internalId = "217";
                            order.account = account;

                            order.tranDateSpecified = true;
                            order.tranDate = Convert.ToDateTime(fdmphieu.NGAY);

                            RecordRef khonhan = new RecordRef();
                            khonhan.externalId = kNhan.externalid;
                            order.adjLocation = khonhan;
                            order.memo = fdmphieu.GHICHU;

                            RecordRef mabp = new RecordRef();
                            mabp.externalId = fdepartment.externalid;
                            order.department = mabp;

                            RecordRef class_ = new RecordRef();
                            class_.externalId = cls.externalid;
                            order.@class = class_;

                            SelectCustomFieldRef sel_loaisp = new SelectCustomFieldRef();
                            ListOrRecordRef list_loaisp = new ListOrRecordRef();
                            list_loaisp.externalId = "VATTU";
                            sel_loaisp.scriptId = "custbody_btm_tt_loai_sp";
                            sel_loaisp.value = list_loaisp;
                            cusfield[0] = sel_loaisp;

                            SelectCustomFieldRef sel_nguongochang = new SelectCustomFieldRef();
                            ListOrRecordRef list_nguongochang = new ListOrRecordRef();
                            list_nguongochang.internalId = "1";
                            sel_nguongochang.scriptId = "custbody_btm_tt_nguon_goc_hang";
                            sel_nguongochang.value = list_nguongochang;
                            cusfield[1] = sel_nguongochang;

                            SelectCustomFieldRef sel_nguongoc = new SelectCustomFieldRef();
                            ListOrRecordRef list_nguongoc = new ListOrRecordRef();
                            list_nguongoc.internalId = "203";
                            sel_nguongoc.scriptId = "custbody_btm_tt_nguon_goc";
                            sel_nguongoc.value = list_nguongoc;
                            cusfield[2] = sel_nguongoc;



                            #region Get PO
                            var internalidcreatedfrom = "";
                            var tranId = "GC-NXK-DM-TT-2023-001";
                            TransactionSearchBasic search = new TransactionSearchBasic();
                            search.type = new SearchEnumMultiSelectField();
                            search.type.@operator = SearchEnumMultiSelectFieldOperator.anyOf;
                            search.type.operatorSpecified = true;
                            search.type.searchValue = new string[] { "purchaseOrder" }; // Lọc theo loại giao dịch là Purchase Order
                            search.tranId = new SearchStringField();
                            search.tranId.@operator = SearchStringFieldOperator.@is;
                            search.tranId.operatorSpecified = true;
                            search.tranId.searchValue = tranId; // Đặt giá trị tranId cần tìm

                            SearchPreferences searchPrefs = new SearchPreferences();
                            SearchResult result = ns.Service.search(search); // Thực hiện tìm kiếm
                            if (result.status.isSuccess && result.recordList != null && result.recordList.Length > 0)
                            {
                                var po = (PurchaseOrder)result.recordList[0];
                                internalidcreatedfrom = po.internalId;

                            }

                            #endregion





                            SelectCustomFieldRef sel_pogiacong = new SelectCustomFieldRef();
                            ListOrRecordRef list_pogiacong = new ListOrRecordRef();
                            list_pogiacong.internalId = internalidcreatedfrom;
                            sel_pogiacong.scriptId = "custbody_btm_po_gia_cong";
                            sel_pogiacong.value = list_pogiacong;
                            cusfield[3] = sel_pogiacong;

                            order.customFieldList = cusfield;

                            #endregion

                            #endregion

                            #region Transfer CTPHIEU data

                            InventoryAdjustmentInventoryList orderItemList = new InventoryAdjustmentInventoryList();

                            List<InventoryAdjustmentInventory> orderItems = new List<InventoryAdjustmentInventory>();
                            foreach (var items in ctphieu)
                            {
                                CustomFieldRef[] cusDetail = new CustomFieldRef[100];

                                InventoryAdjustmentInventory line = new InventoryAdjustmentInventory();

                                RecordRef refItem = new RecordRef();
                                refItem.externalId = items.MAVT;
                                line.item = refItem;

                                var tongSL = ctphieu.Where(p => p.MAVT == items.MAVT).Sum(p => p.SOLUONGTT);
                                line.adjustQtyBySpecified = true;
                                line.adjustQtyBy = Convert.ToDouble(tongSL);

                                RecordRef reflocation = new RecordRef();
                                reflocation.externalId = kNhan.externalid;
                                line.location = reflocation;

                                var LO = ctphieu.Where(p => p.MAVT == items.MAVT).Select(p => new { Lo = p.LO, slXuat = p.SOLUONGTT }).ToList();


                                InventoryDetail inventoryDetail = new InventoryDetail();

                                InventoryAssignmentList inventoryAssignmentList = new InventoryAssignmentList();

                                List<InventoryAssignment> List_inventoryAssignment = new List<InventoryAssignment>();

                                foreach (var sLOT in LO)
                                {
                                    InventoryAssignment inventoryAssignment = new InventoryAssignment();


                                    inventoryAssignment.receiptInventoryNumber = sLOT.Lo;

                                    inventoryAssignment.quantity = Convert.ToDouble(sLOT.slXuat);
                                    inventoryAssignment.quantitySpecified = true;

                                    List_inventoryAssignment.Add(inventoryAssignment);
                                }

                                inventoryAssignmentList.inventoryAssignment = List_inventoryAssignment.ToArray();
                                inventoryDetail.inventoryAssignmentList = inventoryAssignmentList;
                                line.inventoryDetail = inventoryDetail;

                                orderItems.Add(line);
                            }

                            #endregion

                            orderItemList.inventory = orderItems.ToArray();
                            order.inventoryList = orderItemList;

                            WriteResponse response = ns.Service.upsert(order);

                            #endregion

                            #region LOG

                            if (response.status.isSuccess == false)
                            {
                                code = -1;
                                message = "Đồng bộ phiếu nhập không thành công.";

                                var jsonString = "{ \"resultStatus\":" + Newtonsoft.Json.JsonConvert.SerializeObject(response.status) + ",";
                                jsonString += "\"resultData\":" + Newtonsoft.Json.JsonConvert.SerializeObject(order) + "}";

                                #region Log
                                var log = new DongBoVATTU_SOI__Json_Log();
                                log.Id_Json = SOCTXN;
                                log.Source = "PhieuNhapKhoController";
                                log.JsonNotification = jsonString;
                                log.Error = true;
                                log.Action = "Đồng bộ phiếu xuất không thành công.";
                                log.CreateDate = DateTime.Now;
                                log.CreateBy = User.UserName;
                                log.MayTinh = Environment.MachineName;
                                log.Type = MAHINHTHUC;
                                nstt_.DongBoVATTU_SOI__Json_Log.Add(log);
                                nstt_.SaveChanges();
                                #endregion
                            }
                            else
                            {
                                code = 1;
                                message = "Đồng bộ phiếu nhập thành công.";
                            }
                            return Json(new { status = code, title = "", text = message + "<br />" + response.status.statusDetail[0].message, obj = "" }, JsonRequestBehavior.AllowGet);
                            #endregion
                        }

                    }
                    else
                    {


                        return Json(new { status = -1, title = "", text = "Đồng bộ không thành công", obj = "" }, JsonRequestBehavior.AllowGet);
                    }


                    //}
                    //else
                    //{
                    //    return Json(new { status = -1, title = "", text = "Không có dữ liệu hoặc đã được đẩy API.", obj = "" }, JsonRequestBehavior.AllowGet);
                    //}
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