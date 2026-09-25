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
    public class XuatKhoTheoPhieuYCController : BaseController
    {

        wqlvattuEntities vt_ = new wqlvattuEntities();
        wqlkhosoiEntities soi_ = new wqlkhosoiEntities();
        private NetsuiteTTEntities1 nstt_ = new NetsuiteTTEntities1();

        #region Index
        // GET: XuatKhoTheoPhieuYCController
        public ActionResult Index()
        {
            #region
            var khoxuat = soi_.SP_MAKHOBYMANV_LOAD(User.UserName).ToList();
            ViewBag.makhoxuat = khoxuat;
            #endregion
            #region
            var phieudaxuat = soi_.Sp_load_PhieuDaXuatkho_SOI("").ToList();
            ViewBag.phieudaxuat = phieudaxuat;
            #endregion
            #region
            var kho = khoxuat.Count == 0 ? "" : khoxuat.FirstOrDefault().MAKHO;

            ViewBag.Phieuyc = soi_.Sp_load_PhieuYCXuatkho_SOI("").Where(p =>
             (kho == null || kho == "" || p.MAKHO.Contains(kho))

                ).OrderByDescending(p => p.SOCTXN).ToList();
            #endregion


            return View();
        }
        #endregion

        #region Load View
        public ActionResult _GetList_tab2(string SOCTXNSearch = "")
        {
            var List = soi_.NL_DMPHIEUXN.Where(p =>
           (
               p.SOCTXN == SOCTXNSearch

           )).OrderByDescending(p => p.NGAY).OrderByDescending(c => c.SOCTXN).ToList();

            ViewBag.List = List;

            return PartialView();
        }
        #endregion
        #region Load View
        public ActionResult _GetList(string SOCTXNSearch)
        {
            var List = soi_.NL_CTXUATNHAP.Where(p => p.SOCTXN == SOCTXNSearch


              &&
              p.DAXUATKHO == 0
          ).OrderByDescending(p => p.NGAYKETOAN).OrderByDescending(c => c.SOCTXN).ToList();

            ViewBag.List = List;

            return PartialView();
        }
        #endregion

        #region OnChange
        public JsonResult _Change_Phieuyc(string makho_xuatid)
        {
            try
            {
                var Phieuyc = soi_.Sp_load_PhieuYCXuatkho_SOI("").Where(p =>
               (makho_xuatid == null || makho_xuatid == "" || p.MAKHO.Contains(makho_xuatid))

                  ).OrderByDescending(p => p.SOCTXN).ToList();

                return Json(new { status = 1, title = "", text = "", obj = Phieuyc }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { status = -1, title = "", text = e.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult _LoadPhieuDaxuat()
        {
            try
            {
                var phieudaxuat = soi_.NL_CTXUATNHAP.Where(p => p.DAXUATKHO == 1 && p.GUIAPI != "1").ToList();

                return Json(new { status = 1, title = "", text = "", obj = phieudaxuat }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { status = -1, title = "", text = e.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult _Change_khoxuat(string makho_xuatid)
        {
            try
            {
                var khoxuat = soi_.SP_MAKHOBYMANV_LOAD(User.UserName).ToList();


                return Json(new { status = 1, title = "", text = "", obj = khoxuat }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { status = -1, title = "", text = e.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }


        #endregion

        #region Xuất Kho
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _Xuatkho(string SOCTXN, string NGAYKETOAN)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    #region Xử lý ngày
                    CultureInfo cul = CultureInfo.GetCultureInfo("en-GB");
                    var NgayKeToan_ = new DateTime();

                    NgayKeToan_ = DateTime.ParseExact(NGAYKETOAN, "dd/MM/yyyy", cul);
                    NgayKeToan_ = new DateTime(NgayKeToan_.Year,
                    NgayKeToan_.Month, NgayKeToan_.Day, 0, 0, 0);

                    #endregion

                    var ctitem = soi_.NL_CTXUATNHAP.Where(p => p.SOCTXN == SOCTXN).ToList();
                    foreach (var item in ctitem)
                    {
                        item.DAXUATKHO = 1;
                        item.MANVXUATKHO = User.UserName;
                        item.NGAYXUATKHO = DateTime.Now;
                        item.NGAYKETOAN = NgayKeToan_;
                        soi_.Entry(item).State = EntityState.Modified;
                    }

                    soi_.SaveChanges();
                    var tb = soi_.NL_CTXUATNHAP.Where(p => p.SOCTXN == SOCTXN && p.DAXUATKHO == 0 && (p.GUIAPI == "0" || p.GUIAPI == null)).ToList();
                    if (tb.Count < 1)
                    {
                        var item = soi_.NL_DMPHIEUXN.FirstOrDefault(p => p.SOCTXN == SOCTXN);
                        item.DAXUATKHO_ = true;
                        item.MANVXUAT = User.UserName;
                        soi_.Entry(item).State = EntityState.Modified;
                        soi_.SaveChanges();
                    }
                    return Json(new { status = 1, title = "", text = "Xuất Kho thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new { status = -2, title = "", text = "Xuất Kho không thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Load Phiếu Đã xuất
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _LoadPhieudaxuat()
        {
            try
            {
                var phieudaxuat = soi_.Sp_load_PhieuDaXuatkho_SOI("").ToList();

                return Json(new { status = 1, title = "", text = "", obj = phieudaxuat }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { status = -1, title = "", text = e.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Cập nhật sl xuất kho
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _Update_Xuatsoluongtt(Decimal SoluongTT, Guid ID_XN)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var item = soi_.NL_CTXUATNHAP.FirstOrDefault(p => p.ID_XN == ID_XN);
                    item.SOLUONGTT = Convert.ToDecimal(SoluongTT);
                    item.MODIFIED = DateTime.Now;
                    soi_.Entry(item).State = EntityState.Modified;
                    soi_.SaveChanges();
                    return Json(new { status = 1, title = "", text = "Cập nhật thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new { status = -2, title = "", text = "Cập nhật không thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Hủy Xuất kho, Chưa gửi API
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _HuyXuatKho(string SOCTXN)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var item = soi_.NL_CTXUATNHAP.Where(p => p.SOCTXN == SOCTXN).ToList();
                    foreach (var huyx in item)
                    {
                        huyx.DAXUATKHO = 0;
                        soi_.Entry(huyx).State = EntityState.Modified;
                    }
                    soi_.SaveChanges();


                    var tb = soi_.NL_CTXUATNHAP.Where(p => p.SOCTXN == SOCTXN && p.DAXUATKHO == 1 && (p.GUIAPI == "0" || p.GUIAPI == null)).ToList();
                    if (tb.Count < 1)
                    {

                        var dm = soi_.NL_DMPHIEUXN.FirstOrDefault(p => p.SOCTXN == SOCTXN);
                        dm.DAXUATKHO_ = false;
                        dm.MANVXUAT = User.UserName;

                        soi_.Entry(dm).State = EntityState.Modified;
                        soi_.SaveChanges();

                    }


                    return Json(new { status = 1, title = "", text = "Hủy thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new { status = -2, title = "", text = "Hủy không thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
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
                var tb = soi_.NL_DMPHIEUXN.Where(p => p.SOCTXN == SOCTXN).ToList();
                var f_tb = tb.FirstOrDefault();

                var ctxuatnhap = soi_.NL_CTXUATNHAP.Where(p => p.SOCTXN == SOCTXN).ToList();
                var mahthuc = tb.FirstOrDefault().MAHTHUC;
                var mahinhthuc = soi_.NL_DMHINHTHUC.Where(p => p.MAHTHUC == mahthuc).ToList();
                var mahinhthuc_item = mahinhthuc.Count < 1 ? "" : mahinhthuc.FirstOrDefault().XUAT3BUOC;

                var KhoX = nstt_.Khoes.Where(p => p.makho == f_tb.MAKHOXUAT).ToList();
                var KhoN = nstt_.Khoes.Where(p => p.makho == f_tb.MAKHONHAN).ToList();
                if (KhoN.Count < 1)
                {

                    KhoN = nstt_.Khoes.Where(p => p.externalid == f_tb.MAKHONHAN).ToList();
                }
                if (KhoX.Count < 1)
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

                                    var ctx = soi_.NL_DMPHIEUXN.Where(p => p.SOCTXN == SOCTXN).FirstOrDefault();
                                    var ct_ctx = soi_.NL_CTXUATNHAP.Where(p => p.SOCTXN == SOCTXN).ToList();

                                    TransferOrder[] orders = new TransferOrder[1];
                                    TransferOrder order = new TransferOrder();

                                    #region CUSTOM FORM
                                    RecordRef cusfRec = new RecordRef();
                                    cusfRec.internalId = "113";
                                    order.customForm = cusfRec;

                                    RecordRef incotern = new RecordRef();
                                    incotern.internalId = "1"; // incotern = "DAP"
                                    order.incoterm = incotern;
                                    #endregion

                                    #region active field to edit
                                    order.tranDateSpecified = true;
                                    order.orderStatusSpecified = true;
                                    //order.totalSpecified = true;
                                    #endregion

                                    //order.tranId = ctx.SOCTXN;
                                    order.memo = ctx.GHICHU;
                                    order.tranId = ctx.SOCTXN;
                                    order.externalId = ctx.SOCTXN;
                                    order.orderStatus = TransferOrderOrderStatus._pendingFulfillment;
                                    //order.total = ct_ctx.Sum(c => (c.SOLUONG == null ? 0 : (double)c.SOLUONG.Value));

                                    #region DATE
                                    DateCustomFieldRef dateF = new DateCustomFieldRef();
                                    dateF.value = Convert.ToDateTime(f_tb.NGAY);
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
                                    else
                                    {
                                        #region update GuiAPI = 1
                                        var IsAPI = soi_.NL_DMPHIEUXN.Where(c => c.SOCTXN == SOCTXN).FirstOrDefault();
                                        IsAPI.GUIAPI = "1";
                                        IsAPI.NGAYGUIAPI = DateTime.Now;
                                        soi_.Entry(IsAPI).State = EntityState.Modified;
                                        soi_.SaveChanges();
                                        #endregion
                                    }

                                    return Json(new { status = 1, title = "", text = "Đồng bộ thành công", obj = jsonString }, JsonRequestBehavior.AllowGet);
                                    #endregion
                                }
                                if (mahinhthuc_item == "1")
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
                                        IT_PhieuXkho.tranDate = Convert.ToDateTime(f_tb.NGAY);
                                        IT_PhieuXkho.tranDateSpecified = true;
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
                                            lotRecord.externalId = ctitem.MAVT + "_" + ctitem.HIEU + ((ctitem.LO == null || ctitem.LO == "") ? "" : ("-" + ctitem.LO));
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
                                            #region Tạo NNB  + ZZZZ
                                            // 1. Tối ưu lấy mã hình thức (Tránh lỗi NullReferenceException)
                                            var MA_HINHTHUC = soi_.NL_DMHINHTHUC
                                                .Where(p => p.MAHTHUC == "NKNB")
                                                .Select(p => p.MAHTHUC)
                                                .FirstOrDefault()?.Trim();

                                            if (string.IsNullOrEmpty(MA_HINHTHUC)) return Json(new { status = -1, text = "Không tìm thấy mã hình thức NKNB" }, JsonRequestBehavior.AllowGet);

                                            var SOCTXN_NNB = soi_.SP_NL_TAOSOCHUNGTU(MA_HINHTHUC).FirstOrDefault();

                                            // 2. Sử dụng Any() để kiểm tra nhanh
                                            if (soi_.NL_DMPHIEUXN.Any(p => p.KETHUA == SOCTXN))
                                            {
                                                return Json(new { status = -1, title = "", text = "Phiếu đã xuất kho hoặc đã kế thừa, vui lòng kiểm tra lại", obj = "" }, JsonRequestBehavior.AllowGet);
                                            }

                                            if (!soi_.NL_DMPHIEUXN.Any(p => p.SOCTXN == SOCTXN_NNB))
                                            {
                                                #region Insert DM_XUATNHAP
                                                var model_copy = new EntityFramework.KhoSoi.NL_DMPHIEUXN
                                                {
                                                    SOCTXN = SOCTXN_NNB.ToUpper().Trim(),
                                                    MAHTHUC = MA_HINHTHUC,
                                                    SOPO = f_tb.SOPO,
                                                    NGAY = f_tb.NGAY,
                                                    MANVNHAN = f_tb.MANVNHAN,
                                                    IDMADVNHAN = f_tb.IDMADVNHAN,
                                                    IDMADVXUAT = f_tb.IDMADVXUAT,
                                                    MAKHOXUAT = f_tb.MAKHOXUAT.Trim(),
                                                    MAKHONHAN = f_tb.MAKHONHAN.Trim(),
                                                    GHICHU = string.IsNullOrEmpty(f_tb.GHICHU) ? "" : f_tb.GHICHU,
                                                    MaNVYC = User.UserName
                                                };

                                                soi_.NL_DMPHIEUXN.Add(model_copy);
                                                #endregion
                                          
                                                if (ctxuatnhap != null && ctxuatnhap.Any())
                                                {
                                                    foreach (var model in ctxuatnhap)
                                                    {
                                                        var NewGuid = Guid.NewGuid();
                                                        var model_item = new EntityFramework.KhoSoi.NL_CTXUATNHAP
                                                        {                                          
                                                            SOCTXN = SOCTXN_NNB.ToUpper().Trim(),
                                                            MAPHIEUPD = model.MAPHIEUPD,
                                                            MAVT = model.MAVT,
                                                            LO = model.LO,
                                                            HIEU = model.HIEU,
                                                            MAVTTAM = model.MAVT,
                                                            SOLUONGYC = Convert.ToDecimal(model.SOLUONGYC),
                                                            SOLUONGTT = Convert.ToDecimal(model.SOLUONGTT),
                                                            GHICHU = string.IsNullOrEmpty(model.GHICHU) ? "Kế thừa từ " + f_tb.SOCTXN : model.GHICHU,
                                                            MADH = model.MADH,
                                                            NGAYKETOAN = model.NGAYKETOAN,
                                                            NGAYMODIFY = DateTime.Now,
                                                            KHOAKEYXN = "NBB_KeyKhoa_" + NewGuid,
                                                            ID_XN = NewGuid,
                                                            KHOATHAMCHIEU = NewGuid.ToString(),
                                                        };
                                                        soi_.NL_CTXUATNHAP.Add(model_item);
                                                    }
                                                }

                                                #region update GuiAPI = 1 (Đưa lên trên để gộp SaveChanges)
                                                var IsAPI = soi_.NL_DMPHIEUXN.FirstOrDefault(c => c.SOCTXN == SOCTXN);
                                                if (IsAPI != null)
                                                {
                                                    IsAPI.GUIAPI = "1";
                                                    IsAPI.NGAYGUIAPI = DateTime.Now;                                  
                                                }
                                                #endregion

                                                // Gộp tất cả các lệnh Insert và Update ở trên vào 1 lần lưu duy nhất
                                                soi_.SaveChanges();

                                                // Chạy Store Procedure sau khi dữ liệu đã được ghi vào DB
                                                var kq = soi_.VATTU2026_UPDATE_STATUS_NL_DMPHIEUXN(SOCTXN, SOCTXN_NNB).ToList();
                                            }
                                            #endregion
                                        }
                                    }
                                    return Json(new { status = 1, title = "", text = "Thêm thành công.", obj = "" }, JsonRequestBehavior.AllowGet);

                                    #endregion
                                }
                                if (mahinhthuc_item == "0")
                                {
                                    #region Xuất sử dụng, 
                                    #region Call data 
                                    var Acc_Coa = mahinhthuc.FirstOrDefault().Account_KT;       //account kt: 1571    
                                    var customform = "152"; // Điều chỉnh tồn                        
                                                            //var customform = "169"; // Điều chỉnh tồn                         
                                                            // var cls = nstt_.Classes.FirstOrDefault(p => p.externalid == "BAN_PTVT");
                                    var taikhoanketoan = Acc_Coa; // Chi phí SX
                                    var ex_location = f_KhoX;
                                    #endregion

                                    #region InventoryAdjustment
                                    InventoryAdjustment InAdj = new InventoryAdjustment();
                                    CustomFieldRef[] cusfield = new CustomFieldRef[100];

                                    RecordRef cusfRec = new RecordRef();
                                    cusfRec.internalId = customform;
                                    InAdj.customForm = cusfRec;

                                    InAdj.externalId = f_tb.SOCTXN;
                                    InAdj.tranId = f_tb.SOCTXN;

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

                                        InventoryDetail inventoryDetail = new InventoryDetail();

                                        InventoryAssignmentList inventoryAssignmentList = new InventoryAssignmentList();

                                        List<InventoryAssignment> List_inventoryAssignment = new List<InventoryAssignment>();

                                        InventoryAssignment inventoryAssignment = new InventoryAssignment();

                                        RecordRef lotRecord = new RecordRef();
                                        // lotRecord.externalId = "AA030001002_HAND";
                                        lotRecord.externalId = items.MAVT + "_" + items.HIEU + ((items.LO == null || items.LO == "") ? "" : ("-" + items.LO));
                                        inventoryAssignment.issueInventoryNumber = lotRecord;
                                        inventoryAssignment.quantity = Convert.ToDouble(-items.SOLUONGTT);
                                        inventoryAssignment.quantitySpecified = true;
                                        List_inventoryAssignment.Add(inventoryAssignment);
                                        inventoryAssignmentList.inventoryAssignment = List_inventoryAssignment.ToArray();
                                        inventoryDetail.inventoryAssignmentList = inventoryAssignmentList;

                                        line_IA.inventoryDetail = inventoryDetail;

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
                                        return Json(new { status = -1, title = "Error", text = messdetail, obj = "" }, JsonRequestBehavior.AllowGet);
                                    }
                                    else
                                    {
                                        #region update GuiAPI = 1
                                        var IsAPI = soi_.NL_DMPHIEUXN.Where(c => c.SOCTXN == SOCTXN).FirstOrDefault();
                                        IsAPI.Externalid_IAD = SOCTXN;
                                        IsAPI.GUIAPI = "1";
                                        IsAPI.NGAYGUIAPI = DateTime.Now;
                                        soi_.Entry(IsAPI).State = EntityState.Modified;
                                        soi_.SaveChanges();
                                        #endregion

                                        return Json(new { status = 1, title = "", text = "Thêm thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
                                    }
                                    #endregion end Xuất sử dụng
                                }
                                if (mahinhthuc_item == "5")
                                {
                                    var lines = ctxuatnhap.Select(x => new NL_CTXUATNHAPViewModels
                                    {
                                        SOCTXN = x.SOCTXN,      // cột lưu line của SO trên NetSuite
                                        MAVT = x.MAVT,
                                        SOLUONGTT = (decimal)x.SOLUONGTT,
                                        HIEU = x.HIEU,           // null nếu vật tư không có lot
                                        LO = x.LO,
                                        KHOAKEYXN = x.KHOAKEYXN
                                    }).ToList();

                                    bool iff = Services.NetSuiteItemFulfillmentService.CreateYarnSalesOrderFulfillment(f_tb.SOPO,f_tb.InternalidPO, f_tb.SOCTXN, f_tb.NGAY.Value, lines);
                                    if (iff == false)
                                    {
                                        return Json(new { status = -1, title = "", text = "Xuất bán thất bại, Kiểm tra lại tồn kho, Approve SO, Add Location SO", obj = "" }, JsonRequestBehavior.AllowGet);
                                    }

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