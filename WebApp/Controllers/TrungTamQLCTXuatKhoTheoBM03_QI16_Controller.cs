using NSClient;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ToolsApp.Authentication;
using ToolsApp.com.netsuite.webservices;
using ToolsApp.EntityFramework.KhoSoi;
using ToolsApp.EntityFramework.NetsuiteTT;
using ToolsApp.EntityFramework.VatTu;
using ToolsApp.Helper;
using ToolsApp.Services;

namespace ToolsApp.Controllers
{
    public class TrungTamQLCTXuatKhoTheoBM03_QI16_Controller : BaseController
    {
        private wqlvattuEntities db_ = new wqlvattuEntities();
        wqlvattuEntities vt_ = new wqlvattuEntities();
        wqlkhosoiEntities soi_ = new wqlkhosoiEntities();
        private NetsuiteTTEntities1 nstt_ = new NetsuiteTTEntities1();
        // GET: TrungTamQLCTXuatKhoTheoBM03_QI16_
        public ActionResult Index()
        {
            return View();
        }
        public JsonResult PhanQuyen()
        {
            bool isCheck = false;
            var data = db_.SP_LOADKHOQUYENXUAT_VER1(User.UserName).FirstOrDefault();
            if (data != null)
            {
                isCheck = true;
                return Json(new { status = 1, title = "", text = "", obj = isCheck }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { status = -1, title = "", text = "Những nhân viên phòng cung ứng mới được sử dụng được chức năng này.", obj = isCheck }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult Load_SOCTXN(string makho)
        {
            var data = db_.VATTU2024_SP_PHIEUCHOXUAT(makho).ToList();
            if (data.Count > 0)
            {
                return Json(new { status = 1, title = "", text = "", obj = data }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { status = -1, title = "", text = "", obj = "" }, JsonRequestBehavior.AllowGet);

        }
        public ActionResult _GetList(string SOCTXN)
        {
            var data = db_.VATTU2024_SP_THUKHOXUATKHO_LOAD(SOCTXN).ToList();
            ViewBag.data = data;
            return PartialView();
        }
        public ActionResult _Insert()
        {
            var data_MaKho = db_.SP_LOADKHOQUYENNHAP(User.UserName, "1").ToList();
            ViewBag.data_MaKho = data_MaKho;
            return PartialView();
        }
        public JsonResult Action_Insert(string MaKho, string SOCTXN, string IsXuatKho, string NgayKeToan, string searchType)
        {
            //try
            //{

            var searchType1 = (searchType == null || searchType == "") ? "DK1" : searchType;
            #region Xử lý ngày
            CultureInfo cul = CultureInfo.GetCultureInfo("en-GB");
            var NgayKeToan_ = new DateTime();
            #endregion

            if (!string.IsNullOrEmpty(NgayKeToan))
            {
                try
                {
                    NgayKeToan_ = DateTime.ParseExact(NgayKeToan, "dd/MM/yyyy", cul);        
                    NgayKeToan_ = new DateTime(NgayKeToan_.Year,
                    NgayKeToan_.Month, NgayKeToan_.Day, 0, 0, 0);

                    #region Update ngày
                    if (searchType == "DK2")
                    {
                        // 1. Lấy trực tiếp danh sách cần update (bỏ ToList() sớm để tránh tải dữ liệu không cần thiết nếu muốn, nhưng ở đây giữ nguyên để xử lý)
                        var listUpdate = db_.XUATNHAPs.Where(a => a.SoCTXN == SOCTXN).ToList();

                        // 2. Cập nhật trực tiếp trên danh sách (Không gọi vào DB nữa -> Giải quyết triệt để N+1)
                        foreach (var item in listUpdate)
                        {
                            item.NgayKeToan = NgayKeToan_;
           
                        }

                       
                        var check_dm = db_.DM_XUATNHAP.Where(a => a.SOCTXN == SOCTXN).FirstOrDefault();
                        if (check_dm != null)
                        {
                            check_dm.NGAY = NgayKeToan_;
                        }

                      
                        db_.SaveChanges();
                    }
                    #endregion

                }
                catch (Exception ex)
                {
                    var messageERR = ex.InnerException.InnerException.Message;
                    return Json(new { status = -1, title = "", text = messageERR, obj = "" }, JsonRequestBehavior.AllowGet);
                }
                if (MaKho.Trim() != string.Empty && SOCTXN.Trim() != string.Empty)
                {
                    #region NS
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

                    #region Khai báo data TTG
                    var dmxuatnhap = db_.DM_XUATNHAP.FirstOrDefault(p => p.SOCTXN == SOCTXN);
                    var ctxuatnhap = db_.XUATNHAPs.Where(p => p.SoCTXN == SOCTXN).ToList(); 
                    var xuat3buoc = db_.DMLOAIXNs.FirstOrDefault(p => p.LOAIXN == dmxuatnhap.LOAIXN);
                    //  var department = nstt_.Donvi_mapping_Department.Where(p => p.idmadv == dmxuatnhap.IDMADVNHAN).ToList();
                    var department = nstt_.Donvi_mapping_Department
                                    .Where(p => p.idmadv == dmxuatnhap.IDMADVNHAN || p.idmacs == dmxuatnhap.IDMADVNHAN)
                                    .ToList();
                    //var department_xuat = nstt_.Donvi_mapping_Department.Where(p => p.idmadv == dmxuatnhap.IDMADVXUAT).ToList();
                    var department_xuat = nstt_.Donvi_mapping_Department
                           .Where(p => p.idmadv == dmxuatnhap.IDMADVXUAT || p.idmacs == dmxuatnhap.IDMADVXUAT)
                           .ToList();

                    var f_department_nhan = department.Count < 1 ? "" : department.FirstOrDefault().externalid;
                    if (dmxuatnhap.IDMADVNHAN == "30" || dmxuatnhap.IDMADVNHAN == "92" || dmxuatnhap.IDMADVNHAN == "83")
                    {
                        department = nstt_.Donvi_mapping_Department.Where(p => p.idmacs == dmxuatnhap.IDMADVNHAN).ToList();
                    }

                    var kNhan = nstt_.Khoes.Where(p => p.makho == dmxuatnhap.MAKHONHAN).ToList();
                    var kXuat = nstt_.Khoes.Where(p => p.makho == dmxuatnhap.MAKHOXUAT).ToList();

                    if (kNhan.Count < 1)
                    {
                        kNhan = nstt_.Khoes.Where(p => p.externalid == dmxuatnhap.MAKHONHAN).ToList();
                    }
                    if (kXuat.Count < 1)
                    {
                        kXuat = nstt_.Khoes.Where(p => p.externalid == dmxuatnhap.MAKHOXUAT).ToList();
                    }
                    #endregion

                    #region Khai báo thông tin Netsuite
                    if (xuat3buoc.Xuat1buoc3buoc == "3")
                    {
                        #region Xuất 3 bước 
                        #region Gọi thông tin data đã mapping
                        #endregion

                        #region Xử lý add vào Netsuite 423
                        //var manetsuite = db_.SP_API_Load_MaNetSuiteYC().FirstOrDefault().MaNetSuiteYC;
                        TransferOrder TO_PhieuXkho = new TransferOrder();
                        //Date
                        TO_PhieuXkho.tranDate = Convert.ToDateTime(dmxuatnhap.NGAY);

                        //Số phiếu// Ref.No
                        TO_PhieuXkho.tranId = dmxuatnhap.SOCTXN; //-- bị chặn trên netsuite
                        TO_PhieuXkho.externalId = dmxuatnhap.SOCTXN;
                        #region CUSTOM FORM
                        RecordRef cusfRec = new RecordRef();
                        cusfRec.internalId = "113";
                        TO_PhieuXkho.customForm = cusfRec;
                        #endregion

                        TO_PhieuXkho.incoterm = new RecordRef() { internalId = "1" };
                        //From location

                        RecordRef FromLocation = new RecordRef();
                        // FromLocation.externalId = "NS-A1.00";
                        FromLocation.externalId = kXuat.Count < 1 ? "" : kXuat.FirstOrDefault().externalid;
                        TO_PhieuXkho.location = FromLocation;

                        //To TransLocation 

                        RecordRef ToLocation = new RecordRef();
                        //ToLocation.externalId = "NS-A1.03";
                        ToLocation.externalId = kNhan.Count < 1 ? "" : kNhan.FirstOrDefault().externalid;
                        TO_PhieuXkho.transferLocation = ToLocation;

                        //Memo / Ghi chú
                        TO_PhieuXkho.memo = dmxuatnhap.GHICHU == null ? "" : dmxuatnhap.GHICHU;

                        // đơn vị nhận// Department

                        RecordRef PYC_Department = new RecordRef();
                        PYC_Department.externalId = department.Count < 1 ? "" : department.FirstOrDefault().externalid;
                        TO_PhieuXkho.department = PYC_Department;

                        // Phân loại phiếu
                        CustomFieldRef[] Cus_PhieuXkho = new CustomFieldRef[2];

                        //var phanloai = nstt_.Table_LoaiPhieuVanChuyenNoiBo
                        SelectCustomFieldRef select_cus_phanloaiphieu = new SelectCustomFieldRef();
                        ListOrRecordRef List_phanloaiphieu = new ListOrRecordRef();
                        List_phanloaiphieu.internalId = "2";
                        select_cus_phanloaiphieu.scriptId = "custbody_btm_tt_loai_phieu_van_chuyen";
                        select_cus_phanloaiphieu.value = List_phanloaiphieu;
                        Cus_PhieuXkho[0] = select_cus_phanloaiphieu;

                        //StringCustomFieldRef allow_conversion = new StringCustomFieldRef();
                        //allow_conversion.scriptId = "custbody_btm_allow_conversion";
                        //allow_conversion.value = "T";
                        //Cus_PhieuXkho[1] = allow_conversion;

                        //// CHI TIẾT PHIẾU YC

                        //var CTphieu = db_.XUATNHAPs.Where(p => p.SoCTXN == SOCTXN && p.DaXuatKho == 1).ToList();
                        TransferOrderItemList orderItemList = new TransferOrderItemList();
                        List<TransferOrderItem> orderItems = new List<TransferOrderItem>();

                        foreach (var ctitem in ctxuatnhap)
                        {
                            TransferOrderItem item_PhieuXkho = new TransferOrderItem();

                            //Item // Mặt hàng// MAVT
                            RecordRef Item_rec = new RecordRef();
                            Item_rec.externalId = ctitem.MAVT;
                            //Item_rec.type = RecordType.serializedInventoryItem;
                            //Item_rec.typeSpecified = true;
                            item_PhieuXkho.item = Item_rec;
                            item_PhieuXkho.quantity = Convert.ToDouble(ctitem.SoLuongTT);
                            item_PhieuXkho.quantitySpecified = true;

                            CUSTOMRECORD.FUNC_StringCustomFieldRef("custcol_btm_line_key", ctitem.MAVT.ToString());

                            orderItems.Add(item_PhieuXkho);
                        }

                        orderItemList.item = orderItems.ToArray();

                        TO_PhieuXkho.customFieldList = Cus_PhieuXkho;
                        TO_PhieuXkho.itemList = orderItemList;

                        WriteResponse response = ns.Service.upsert(TO_PhieuXkho);

                        #endregion

                        if (response.status.isSuccess == false)
                        {
                            var messerror = ((ToolsApp.com.netsuite.webservices.Status)response.status).statusDetail;
                            var messdetail = messerror.FirstOrDefault().message;
                            return Json(new { status = -1, title = "Error", text = messdetail, obj = "" }, JsonRequestBehavior.AllowGet);
                        }
                        //   return Json(new { status = 1, title = "", text = "Đồng bộ thành công", obj = "" }, JsonRequestBehavior.AllowGet);
                        #endregion
                    }
                    else
                    {
                        if (xuat3buoc.Xuat1buoc3buoc == "0") // Xuất sử dụng
                        {
                            if (xuat3buoc.LOAIXN == "XUAT_SX" || xuat3buoc.LOAIXN == "XDUAN" || xuat3buoc.LOAIXN == "X")
                            {
                                if (xuat3buoc.LOAIXN == "XUAT_SX" || xuat3buoc.LOAIXN == "XDUAN")
                                {
                                    var stt = 1;
                                    foreach (var items in ctxuatnhap)
                                    {
                                        #region Xuất sử dụng
                                        #region Call data  
                                        var externlaid_IAD = dmxuatnhap.Externalid_IAD + "_" + stt; stt++;
                                        var Acc_Coa = "";
                                        if (dmxuatnhap.LOAIXN == "XUAT_SX")
                                        {
                                            var acc_chiphisx = vt_.DANHMUCVATTUs.Where(p => p.MAVT == items.MAVT).ToList();
                                            Acc_Coa = acc_chiphisx.Count < 1 ? "1571" : acc_chiphisx.FirstOrDefault().TK_ChiPhi_SX;  // 1571 là number, internalid = 2280
                                        }
                                        if (dmxuatnhap.LOAIXN == "X")
                                        {
                                            var Adj_Account = department.FirstOrDefault().Account_Donvi_Xsudung;
                                            if (Adj_Account == null)
                                            {
                                                return Json(new { status = 1, title = "Error", text = "Không có Account Đơn vị , Vui lòng liên hệ ITC", obj = "" }, JsonRequestBehavior.AllowGet);
                                            }

                                            Acc_Coa = Adj_Account;
                                        }
                                        if (dmxuatnhap.LOAIXN == "XDUAN")
                                        {
                                            var acc_chiphisx = vt_.DANHMUCVATTUs.Where(p => p.MAVT == items.MAVT).ToList();
                                            if (acc_chiphisx.Count > 0)
                                            {
                                                var asset = acc_chiphisx.FirstOrDefault().ASSET;
                                                var checkasset = asset?.Substring(0, 3) ?? "0";
                                                Acc_Coa = (checkasset == "153") ? "24212" : "24218";  // - Nếu account Item là 153 --> vào 24211 -Còn lại-- > vào 24218
                                            }
                                        }

                                        #region IDMACOSO
                                        var idmacs = vt_.TBLDMCAPCOSOes.Where(p => p.MABP == items.MaBP).ToList();
                                        var f_idmacs = idmacs.Count < 1 ? "" : idmacs.FirstOrDefault().IDMaCS;
                                        var checkidmacs = (f_idmacs == "" ?
                                        (department.Count < 1 ? "" : department.FirstOrDefault().externalid) : f_idmacs);
                                        var IAD_detail_department = nstt_.Donvi_mapping_Department.Where(p => p.idmacs == checkidmacs
                                        ).ToList();
                                        var f_IAD_detail_department = IAD_detail_department.Count < 1 ? (f_department_nhan) : IAD_detail_department.FirstOrDefault().externalid;
                                        #endregion

                                        var customform = "152"; // Điều chỉnh tồn                        
                                                                //var customform = "169"; // Điều chỉnh tồn                        
                                                                // var cls = nstt_.Classes.FirstOrDefault(p => p.externalid == "BAN_PTVT");
                                        var int_coa = nstt_.Chart_of_Accounts.Where(p => p.Number_TT == Acc_Coa)?.FirstOrDefault().InternalID_NS;
                                        if (int_coa == null)
                                        {
                                            return Json(new { status = -1, title = "Error", text = "Không có Account Đơn vị , Vui lòng liên hệ ITC", obj = "" }, JsonRequestBehavior.AllowGet);
                                        }
                                        var taikhoanketoan = int_coa; /// Chờ sửa lại
                                        var ex_location = kXuat.FirstOrDefault().externalid;
                                        #endregion

                                        #region InventoryAdjustment

                                        InventoryAdjustment InAdj = new InventoryAdjustment();
                                        CustomFieldRef[] cusfield = new CustomFieldRef[100];

                                        RecordRef cusfRec = new RecordRef();
                                        cusfRec.internalId = customform;
                                        InAdj.customForm = cusfRec;

                                        InAdj.externalId = externlaid_IAD;
                                        InAdj.tranId = externlaid_IAD;

                                        RecordRef account = new RecordRef();
                                        account.internalId = taikhoanketoan;
                                        InAdj.account = account;

                                        InAdj.tranDateSpecified = true;
                                        InAdj.tranDate = Convert.ToDateTime(dmxuatnhap.NGAY);

                                        RecordRef khoxuat = new RecordRef();
                                        khoxuat.externalId = ex_location;
                                        InAdj.adjLocation = khoxuat;
                                        InAdj.memo = dmxuatnhap.GHICHU;

                                        if (dmxuatnhap.IDMADVNHAN != "30")
                                        {
                                            RecordRef mabp = new RecordRef();
                                            mabp.externalId = f_department_nhan;
                                            InAdj.department = mabp;
                                        }
                                        if (dmxuatnhap.IDMADVNHAN == "30")
                                        {
                                            RecordRef mabp = new RecordRef();
                                            mabp.externalId = f_IAD_detail_department;
                                            InAdj.department = mabp;
                                        }
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
                                        CustomFieldRef[] cusDetail = new CustomFieldRef[100];
                                        InventoryAdjustmentInventory line_IA = new InventoryAdjustmentInventory();

                                        RecordRef refItem = new RecordRef();
                                        refItem.externalId = items.MAVT;
                                        line_IA.item = refItem;
                                        line_IA.memo = items.GHICHU;


                                        line_IA.adjustQtyBySpecified = true;
                                        line_IA.adjustQtyBy = Convert.ToDouble(-items.SoLuongTT);

                                        RecordRef reflocation = new RecordRef();
                                        // reflocation.externalId = kNhan.FirstOrDefault().externalid;
                                        reflocation.externalId = ex_location;
                                        line_IA.location = reflocation;

                                        RecordRef refdepartment = new RecordRef();
                                        // reflocation.externalId = kNhan.FirstOrDefault().externalid;
                                        refdepartment.externalId = f_IAD_detail_department;
                                        line_IA.department = refdepartment;
                                        orderItems_IA.Add(line_IA);


                                        orderItemList_IA.inventory = orderItems_IA.ToArray();
                                        InAdj.inventoryList = orderItemList_IA;

                                        WriteResponse response_IA = ns.Service.upsert(InAdj);
                                        #endregion
                                        #endregion end InventoryAdjustment
                                        if (response_IA.status.isSuccess == false)
                                        {
                                            var messerror = ((ToolsApp.com.netsuite.webservices.Status)response_IA.status).statusDetail;
                                            var messdetail = messerror.FirstOrDefault().message;
                                            return Json(new { status = 0, title = "Error", text = messdetail, obj = "" }, JsonRequestBehavior.AllowGet);
                                        }
                                        #endregion end Xuất sản xuất
                                    }
                                }
                                if (xuat3buoc.LOAIXN == "X")
                                {
                                    var assets = vt_.VATTU2026_SPLOAD_XUATNHAP_X_ASSETACCOUNT(SOCTXN).ToList(); // lấy số account number asset trùng lặp trong chi tiết
                                    var stt = 1;
                                    foreach (var asset in assets)
                                    {
                                        var ItemCTxuatnhap = vt_.VATTU2026_SPLOAD_XUATNHAP_X_ASSETACCOUNT_MAVT(SOCTXN, asset.ASSET).ToList(); // lấy mavt cùng account asset vào 1 phiếu

                                        #region Xuất sử dụng
                                        #region Call data  

                                        var int_Adj_Account = "";
                                        var Acc_Coa = "";

                                        // Xuất đơn vị sử dụng , lấy account chi phí của đơn vị đó, danh mục NetsuiteTT..department

                                        var Adj_Account = department.FirstOrDefault().Account_Donvi_Xsudung;
                                        if (Adj_Account == null)
                                        {
                                            return Json(new { status = 1, title = "Error", text = "Không có Account Đơn vị , Vui lòng liên hệ ITC", obj = "" }, JsonRequestBehavior.AllowGet);
                                        }
                                        int_Adj_Account = nstt_.Chart_of_Accounts.FirstOrDefault(p => p.Number_TT == Adj_Account).InternalID_NS;
                                        if (int_Adj_Account == null)
                                        {
                                            return Json(new { status = -1, title = "Error", text = "Không có Account Đơn vị , Vui lòng liên hệ ITC", obj = "" }, JsonRequestBehavior.AllowGet);
                                        }

                                        Acc_Coa = int_Adj_Account;

                                        var externlaid_IAD = dmxuatnhap.Externalid_IAD + "_" + stt; stt++;


                                        var customform = "152"; // Điều chỉnh tồn                        
                                                                //var customform = "169"; // Điều chỉnh tồn                        
                                                                // var cls = nstt_.Classes.FirstOrDefault(p => p.externalid == "BAN_PTVT");

                                        var ex_location = kXuat.FirstOrDefault().externalid;
                                        #endregion

                                        #region InventoryAdjustment
                                        InventoryAdjustment InAdj = new InventoryAdjustment();
                                        CustomFieldRef[] cusfield = new CustomFieldRef[100];

                                        RecordRef cusfRec = new RecordRef();
                                        cusfRec.internalId = customform;
                                        InAdj.customForm = cusfRec;

                                        InAdj.externalId = externlaid_IAD;
                                        InAdj.tranId = externlaid_IAD;

                                        RecordRef account = new RecordRef();
                                        account.internalId = int_Adj_Account;
                                        InAdj.account = account;

                                        InAdj.tranDateSpecified = true;
                                        InAdj.tranDate = Convert.ToDateTime(dmxuatnhap.NGAY);

                                        RecordRef khoxuat = new RecordRef();
                                        // khoxuat.externalId = kNhan.Count < 1 ? "" : kNhan.FirstOrDefault().externalid;
                                        khoxuat.externalId = ex_location;
                                        InAdj.adjLocation = khoxuat;
                                        InAdj.memo = dmxuatnhap.GHICHU;

                                        if (dmxuatnhap.IDMADVNHAN != "30")
                                        {
                                            RecordRef mabp = new RecordRef();
                                            mabp.externalId = f_department_nhan;
                                            InAdj.department = mabp;
                                        }

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
                                        foreach (var items in ItemCTxuatnhap)
                                        {

                                            var idmacs = vt_.TBLDMCAPCOSOes.Where(p => p.MABP == items.MaBP).ToList();
                                            var f_idmacs = idmacs.Count < 1 ? "" : idmacs.FirstOrDefault().IDMaCS;
                                            var checkidmacs = (f_idmacs == "" ?
                                            (department.Count < 1 ? "" : department.FirstOrDefault().externalid) : f_idmacs);
                                            var IAD_detail_department = nstt_.Donvi_mapping_Department.Where(p => p.idmacs == checkidmacs
                                            ).ToList();
                                            var f_IAD_detail_department = IAD_detail_department.Count < 1 ? (f_department_nhan) : IAD_detail_department.FirstOrDefault().externalid;

                                            CustomFieldRef[] cusDetail = new CustomFieldRef[100];
                                            InventoryAdjustmentInventory line_IA = new InventoryAdjustmentInventory();

                                            RecordRef refItem = new RecordRef();
                                            refItem.externalId = items.MAVT;
                                            line_IA.item = refItem;
                                            line_IA.memo = items.GHICHU;

                                            line_IA.adjustQtyBySpecified = true;
                                            line_IA.adjustQtyBy = Convert.ToDouble(-items.SoLuongTT);

                                            RecordRef reflocation = new RecordRef();
                                            reflocation.externalId = ex_location;
                                            line_IA.location = reflocation;

                                            RecordRef refdepartment = new RecordRef();
                                            refdepartment.externalId = f_IAD_detail_department;
                                            line_IA.department = refdepartment;
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
                            }
                            else
                            {
                                #region Xuất sử dụng
                                #region Call data  
                                var int_Adj_Account = "";
                                var Acc_Coa = "";

                                // Xuất đơn vị sử dụng , lấy account chi phí của đơn vị đó, danh mục NetsuiteTT..department
                                if (dmxuatnhap.LOAIXN == "X_MAU" || dmxuatnhap.LOAIXN == "XTN" || dmxuatnhap.LOAIXN == "XUAT_TIEUHAO")
                                {
                                    var Adj_Account = department.FirstOrDefault().Account_Donvi_Xsudung;
                                    if (Adj_Account == null)
                                    {
                                        return Json(new { status = 1, title = "Error", text = "Không có Account Đơn vị , Vui lòng liên hệ ITC", obj = "" }, JsonRequestBehavior.AllowGet);
                                    }
                                    int_Adj_Account = nstt_.Chart_of_Accounts.FirstOrDefault(p => p.Number_TT == Adj_Account).InternalID_NS;
                                    if (int_Adj_Account == null)
                                    {
                                        return Json(new { status = -1, title = "Error", text = "Không có Account Đơn vị , Vui lòng liên hệ ITC", obj = "" }, JsonRequestBehavior.AllowGet);
                                    }

                                    Acc_Coa = int_Adj_Account;
                                }
                                // Xuất điều chỉnh , lấy account theo danh mục wqlvattu..LoaiXN , có thể chỉnh sửa theo yêu cầu kế toán
                                if (dmxuatnhap.LOAIXN == "XTRAKH" || dmxuatnhap.LOAIXN == "XZZZ" || dmxuatnhap.LOAIXN == "XBAN" || dmxuatnhap.LOAIXN == "XGGC")
                                {
                                    var tk_iad = vt_.DMLOAIXNs.FirstOrDefault(p => p.LOAIXN == dmxuatnhap.LOAIXN).Account_IAD;
                                    Acc_Coa = nstt_.Chart_of_Accounts.FirstOrDefault(p => p.Number_TT == tk_iad).InternalID_NS;
                                }

                                if (dmxuatnhap.LOAIXN == "XHCMT")
                                {
                                    Acc_Coa = "2079";  // - number: 627221, int: 2079 , name: NVL G tiếp: Hóa chất - khác, gán mặc định ko đổi, chỉ lấy hóa chất
                                }

                                var customform = "152"; // Điều chỉnh tồn                        
                                                        //var customform = "169"; // Điều chỉnh tồn                        
                                                        // var cls = nstt_.Classes.FirstOrDefault(p => p.externalid == "BAN_PTVT");
                                var internalidAccount = Acc_Coa; /// Chờ sửa lại
                                var ex_location = kXuat.FirstOrDefault().externalid;
                                #endregion

                                #region InventoryAdjustment
                                InventoryAdjustment InAdj = new InventoryAdjustment();
                                CustomFieldRef[] cusfield = new CustomFieldRef[100];

                                RecordRef cusfRec = new RecordRef();
                                cusfRec.internalId = customform;
                                InAdj.customForm = cusfRec;

                                InAdj.externalId = dmxuatnhap.Externalid_IAD;
                                InAdj.tranId = dmxuatnhap.Externalid_IAD;

                                RecordRef account = new RecordRef();
                                account.internalId = internalidAccount;
                                InAdj.account = account;

                                InAdj.tranDateSpecified = true;
                                InAdj.tranDate = Convert.ToDateTime(dmxuatnhap.NGAY);

                                RecordRef khoxuat = new RecordRef();
                                // khoxuat.externalId = kNhan.Count < 1 ? "" : kNhan.FirstOrDefault().externalid;
                                khoxuat.externalId = ex_location;
                                InAdj.adjLocation = khoxuat;
                                InAdj.memo = dmxuatnhap.GHICHU;

                                if (dmxuatnhap.IDMADVNHAN != "30")
                                {
                                    RecordRef mabp = new RecordRef();
                                    mabp.externalId = f_department_nhan;
                                    InAdj.department = mabp;
                                }
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

                                    var idmacs = vt_.TBLDMCAPCOSOes.Where(p => p.MABP == items.MaBP).ToList();
                                    var f_idmacs = idmacs.Count < 1 ? "" : idmacs.FirstOrDefault().IDMaCS;
                                    var checkidmacs = (f_idmacs == "" ?
                                    (department.Count < 1 ? "" : department.FirstOrDefault().externalid) : f_idmacs);
                                    var IAD_detail_department = nstt_.Donvi_mapping_Department.Where(p => p.idmacs == checkidmacs
                                    ).ToList();
                                    var f_IAD_detail_department = IAD_detail_department.Count < 1 ? (f_department_nhan) : IAD_detail_department.FirstOrDefault().externalid;

                                    CustomFieldRef[] cusDetail = new CustomFieldRef[100];
                                    InventoryAdjustmentInventory line_IA = new InventoryAdjustmentInventory();

                                    RecordRef refItem = new RecordRef();
                                    refItem.externalId = items.MAVT;
                                    line_IA.item = refItem;
                                    line_IA.memo = items.GHICHU;

                                    line_IA.adjustQtyBySpecified = true;
                                    line_IA.adjustQtyBy = Convert.ToDouble(-items.SoLuongTT);

                                    RecordRef reflocation = new RecordRef();
                                    reflocation.externalId = ex_location;
                                    line_IA.location = reflocation;

                                    RecordRef refdepartment = new RecordRef();
                                    refdepartment.externalId = f_IAD_detail_department;
                                    line_IA.department = refdepartment;
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
                            } //ĐÓNG 

                        }
                        else
                        if (xuat3buoc.Xuat1buoc3buoc == "1") // xuất 1 bước
                        {
                            #region Xuất 1 bước 

                            #region Gọi thông tin data đã mapping

                            #endregion

                            #region Xử lý add vào Netsuite 423
                            //var manetsuite = db_.SP_API_Load_MaNetSuiteYC().FirstOrDefault().MaNetSuiteYC;
                            InventoryTransfer IT_PhieuXkho = new InventoryTransfer();
                            //Date
                            IT_PhieuXkho.tranDate = Convert.ToDateTime(dmxuatnhap.NGAY);
                            IT_PhieuXkho.tranDateSpecified = true;

                            //Số phiếu// Ref.No
                            IT_PhieuXkho.tranId = dmxuatnhap.SOCTXN; //-- bị chặn trên netsuite
                            IT_PhieuXkho.externalId = dmxuatnhap.SOCTXN;

                            //From location

                            RecordRef FromLocation = new RecordRef();
                            // FromLocation.externalId = "NS-A1.00";
                            FromLocation.externalId = kXuat.Count < 1 ? "" : kXuat.FirstOrDefault().externalid;
                            IT_PhieuXkho.location = FromLocation;

                            //To TransLocation 

                            RecordRef ToLocation = new RecordRef();
                            // ToLocation.externalId = "NS-A1.03";
                            ToLocation.externalId = kNhan.Count < 1 ? "" : kNhan.FirstOrDefault().externalid;
                            IT_PhieuXkho.transferLocation = ToLocation;

                            //Memo / Ghi chú
                            IT_PhieuXkho.memo = dmxuatnhap.GHICHU == null ? "" : dmxuatnhap.GHICHU;

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

                            StringCustomFieldRef allow_conversion = new StringCustomFieldRef();
                            allow_conversion.scriptId = "custbody_btm_allow_conversion";
                            allow_conversion.value = "T";
                            Cus_PhieuXkho[1] = allow_conversion;


                            //StringCustomFieldRef REF_SOCTXN = new StringCustomFieldRef();
                            //REF_SOCTXN.scriptId = "custbody_btm_tt_ma_don_yeu_cau";
                            //REF_SOCTXN.value = dmxuatnhap.SOCTXN.ToString();
                            //Cus_PhieuXkho[1] = REF_SOCTXN;    

                            //// CHI TIẾT PHIẾU YC

                            //var CTphieu = db_.XUATNHAPs.Where(p => p.SoCTXN == SOCTXN && p.DaXuatKho == 1).ToList();
                            InventoryTransferInventoryList orderItemList = new InventoryTransferInventoryList();
                            List<InventoryTransferInventory> orderItems = new List<InventoryTransferInventory>();

                            foreach (var ctitem in ctxuatnhap)
                            {
                                InventoryTransferInventory item_PhieuXkho = new InventoryTransferInventory();

                                //Item // Mặt hàng// MAVT
                                RecordRef Item_rec = new RecordRef();
                                Item_rec.externalId = ctitem.MAVT;
                                //Item_rec.type = RecordType.serializedInventoryItem;
                                //Item_rec.typeSpecified = true;
                                item_PhieuXkho.item = Item_rec;
                                item_PhieuXkho.adjustQtyBy = Convert.ToDouble(ctitem.SoLuongTT);
                                item_PhieuXkho.adjustQtyBySpecified = true;

                                orderItems.Add(item_PhieuXkho);
                            }

                            orderItemList.inventory = orderItems.ToArray();

                            IT_PhieuXkho.customFieldList = Cus_PhieuXkho;
                            IT_PhieuXkho.inventoryList = orderItemList;

                            WriteResponse response = ns.Service.upsert(IT_PhieuXkho);

                            #endregion

                            if (response.status.isSuccess == false) // fix
                            {
                                var messerror = ((ToolsApp.com.netsuite.webservices.Status)response.status).statusDetail;
                                var messdetail = messerror.FirstOrDefault().message;
                                return Json(new { status = 1, title = "Error", text = messdetail, obj = "" }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            { // khi xuất kho, đồng thời insert NNB 
                                var kyhieu_NNB = vt_.sp_Load_NLKiHieuDV(dmxuatnhap.MaNVYC).FirstOrDefault();
                                var SOCTXN_NNB = vt_.VATTU2024_SP_TAOSCT_NHAPKHO(kyhieu_NNB.ToUpper(), "NNB").FirstOrDefault().SOCTXN;
                                var KTRATONTAI = vt_.DM_XUATNHAP.Where(p => p.KETHUATUSOCT == dmxuatnhap.SOCTXN).ToList();
                                if (KTRATONTAI.Count > 0)
                                {
                                    return Json(new { status = -1, title = "", text = "Phiếu đã xuất kho hoặc đã kế thừa, vui lòng kiểm tra lại", obj = "" }, JsonRequestBehavior.AllowGet);
                                }

                                var tb = vt_.DM_XUATNHAP.Where(p => p.SOCTXN == SOCTXN_NNB).ToList();
                                if (tb.Count < 1)
                                {
                                    #region Insert DM_XUATNHAP
                                    var model_copy = new EntityFramework.VatTu.DM_XUATNHAP();
                                    model_copy.SOCTXN = SOCTXN_NNB.ToUpper().Trim();
                                    model_copy.LOAIXN = "NNB";
                                    model_copy.SOPO = dmxuatnhap.SOPO;
                                    model_copy.NGAY = dmxuatnhap.NGAY;
                                    model_copy.NGUOINHAN = dmxuatnhap.NGUOINHAN;
                                    model_copy.IDMADVNHAN = dmxuatnhap.IDMADVNHAN;
                                    model_copy.IDMADVXUAT = dmxuatnhap.IDMADVXUAT;
                                    model_copy.MAKHOXUAT = dmxuatnhap.MAKHOXUAT.Trim();
                                    model_copy.MAKHONHAN = dmxuatnhap.MAKHONHAN.Trim();
                                    model_copy.GHICHU = (dmxuatnhap.GHICHU == null || dmxuatnhap.GHICHU == "") ? "" : dmxuatnhap.GHICHU;
                                    model_copy.MaNVYC = User.UserName;
                                    model_copy.MANVCN = User.UserName;
                                    model_copy.NGAYCN = DateTime.Now;
                                    vt_.DM_XUATNHAP.Add(model_copy);
                                    vt_.SaveChanges();
                                    #endregion
                                    var kt_chitiet = vt_.DM_XUATNHAP.Where(p => p.SOCTXN == SOCTXN_NNB).ToList();
                                    if (kt_chitiet.Count > 0)
                                    {
                                        foreach (var model in ctxuatnhap)
                                        {
                                            var dmnhanvien = vt_.DMNHANVIENs.Where(p => p.MANV == User.UserName).FirstOrDefault();
                                            var model_item = new EntityFramework.VatTu.XUATNHAP();
                                            model_item.SoCTXN = SOCTXN_NNB.ToUpper().Trim();
                                            model_item.LoaiXN = "NNB";
                                            model_item.DonViYeuCau = dmnhanvien.MADV;
                                            model_item.MaBP = dmnhanvien.MABP;
                                            model_item.MaPhieuPD = model.MaPhieuPD;
                                            model_item.MAVT = model.MAVT;
                                            model_item.MaVTTam = model.MAVT;
                                            model_item.SoLuongYC = Convert.ToDecimal(model.SoLuongYC);
                                            model_item.SoLuongTT = Convert.ToDecimal(model.SoLuongTT);
                                            model_item.TongGia = Convert.ToDecimal(0.00);
                                            model_item.MaKhoXuat = dmxuatnhap.MAKHOXUAT.Trim();
                                            model_item.MaKho = dmxuatnhap.MAKHONHAN.Trim();
                                            model_item.GHICHU = (model.GHICHU == null || model.GHICHU == "") ? "Kế thừa từ " + dmxuatnhap.SOCTXN : model.GHICHU;
                                            model_item.MaNVYC = dmxuatnhap.MaNVYC;
                                            model_item.MANVCN = dmxuatnhap.MANVCN;
                                            model_item.MADH_CN = model.MADH_CN;
                                            model_item.Ngay = DateTime.Now;
                                            model_item.NgayKeToan = model.NgayKeToan;
                                            model_item.NGAYCN = DateTime.Now;
                                            var KeyKhoa = Guid.NewGuid();
                                            model_item.KHOAKEYXN = "NBB_KeyKhoa_" + KeyKhoa;
                                            vt_.XUATNHAPs.Add(model_item);                                        
                                        }
                                    }
                                    vt_.SaveChanges();
                                    vt_.VATTU2024_UPDATE_STATUS_DM_XUATNHAP(dmxuatnhap.SOCTXN, SOCTXN_NNB.ToUpper().Trim());
                                }
                            }
                            #endregion
                        }
                        else
                        if (xuat3buoc.Xuat1buoc3buoc == "4")
                        {
                            // var kyhieu_NNB = vt_.sp_Load_NLKiHieuDV(dmxuatnhap.MaNVYC).FirstOrDefault();
                            var kyhieu_NNB = vt_.DMDONVIs.FirstOrDefault(p => p.IDMaDV == dmxuatnhap.IDMADVNHAN)?.KiHieu;
                            var SOCTXN_NNB = vt_.VATTU2024_SP_TAOSCT_NHAPKHO(kyhieu_NNB.ToUpper(), "NNB").FirstOrDefault().SOCTXN;
                            var tb = vt_.DM_XUATNHAP.Where(p => p.SOCTXN == SOCTXN_NNB).ToList();
                            if (tb.Count < 1)
                            {
                                #region Insert DM_XUATNHAP
                                var model_copy = new EntityFramework.VatTu.DM_XUATNHAP();
                                model_copy.SOCTXN = SOCTXN_NNB.ToUpper().Trim();
                                model_copy.LOAIXN = "NNB";
                                // model_copy.SOPO = dmxuatnhap.SOPO;
                                model_copy.NGAY = dmxuatnhap.NGAY;
                                model_copy.NGUOINHAN = dmxuatnhap.NGUOINHAN;
                                model_copy.IDMADVNHAN = dmxuatnhap.IDMADVNHAN;
                                model_copy.IDMADVXUAT = dmxuatnhap.IDMADVXUAT;
                                model_copy.MAKHOXUAT = dmxuatnhap.MAKHOXUAT.Trim();
                                model_copy.MAKHONHAN = dmxuatnhap.MAKHONHAN.Trim();
                                model_copy.GHICHU = (dmxuatnhap.GHICHU == null || dmxuatnhap.GHICHU == "") ? "" : dmxuatnhap.GHICHU;
                                model_copy.MaNVYC = User.UserName;
                                model_copy.MANVCN = User.UserName;
                                model_copy.NGAYCN = DateTime.Now;
                                vt_.DM_XUATNHAP.Add(model_copy);
                                vt_.SaveChanges();
                                #endregion
                                var kt_chitiet = vt_.DM_XUATNHAP.Where(p => p.SOCTXN == SOCTXN_NNB).ToList();
                                if (kt_chitiet.Count > 0)
                                {
                                    foreach (var model in ctxuatnhap)
                                    {
                                        var dmnhanvien = vt_.DMNHANVIENs.Where(p => p.MANV == User.UserName).FirstOrDefault();
                                        var model_item = new EntityFramework.VatTu.XUATNHAP();
                                        model_item.SoCTXN = SOCTXN_NNB.ToUpper().Trim();
                                        model_item.LoaiXN = "NNB";
                                        model_item.DonViYeuCau = dmnhanvien.MADV;
                                        model_item.MaBP = dmnhanvien.MABP;
                                        model_item.MaPhieuPD = kyhieu_NNB.ToUpper() + "_" + model.MaPhieuPD;
                                        model_item.MAVT = model.MAVT;
                                        model_item.MaVTTam = model.MAVT;
                                        model_item.SoLuongYC = Convert.ToDecimal(model.SoLuongYC);
                                        model_item.SoLuongTT = Convert.ToDecimal(model.SoLuongTT);
                                        model_item.TongGia = Convert.ToDecimal(0.00);
                                        model_item.MaKhoXuat = dmxuatnhap.MAKHOXUAT.Trim();
                                        model_item.MaKho = dmxuatnhap.MAKHONHAN.Trim();
                                        model_item.GHICHU = (model.GHICHU == null || model.GHICHU == "") ? "Kế thừa từ " + dmxuatnhap.SOCTXN : model.GHICHU;
                                        model_item.MaNVYC = dmxuatnhap.MaNVYC;
                                        model_item.MANVCN = dmxuatnhap.MANVCN;
                                        model_item.Ngay = DateTime.Now;
                                        model_item.NgayKeToan = searchType == "DK2" ? NgayKeToan_ : model.NgayKeToan;
                                        model_item.NGAYCN = DateTime.Now;
                                        var KeyKhoa = Guid.NewGuid();
                                        model_item.KHOAKEYXN = "XDCNB_KeyKhoa_" + KeyKhoa;
                                        vt_.XUATNHAPs.Add(model_item);
                                        vt_.SaveChanges();
                                    }
                                }
                                vt_.VATTU2024_UPDATE_STATUS_DM_XUATNHAP(dmxuatnhap.SOCTXN, SOCTXN_NNB.ToUpper().Trim());
                            }
                        }
                        else if (xuat3buoc.Xuat1buoc3buoc == "5")
                        {
                            if (xuat3buoc.LOAIXN == "XBAN")
                            {
                                bool iff = Services.NetSuiteItemFulfillmentService.CreateSalesOrderFulfillment(dmxuatnhap.InternalidPO, dmxuatnhap.SOCTXN, dmxuatnhap.NGAY.Value);
                                if (iff == false)
                                {
                                    return Json(new { status = -1, title = "", text = "Xuất bán thất bại", obj = "" }, JsonRequestBehavior.AllowGet);
                                }
                            }
                        }
                    }
                    #endregion

                    #region Update đã xuất kho thành côngt
                    var check_dm = db_.DM_XUATNHAP.Where(a => a.SOCTXN == SOCTXN).FirstOrDefault();
                    check_dm.DAXUATKHO_ = true;
                    check_dm.GUIAPI = "1";
                    check_dm.NGAY = searchType == "DK2" ? NgayKeToan_ : check_dm.NGAY;
                    check_dm.NGAYGUIAPI = DateTime.Now;
                    db_.Entry(check_dm).State = EntityState.Modified;

                    var check = db_.XUATNHAPs.Where(a => a.SoCTXN == SOCTXN).ToList();
                    for (int i = 0; i < check.Count(); i++)
                    {
                        string khoa = check[i].KHOAKEYXN;
                        var dataUpdate = db_.XUATNHAPs.Where(a => a.KHOAKEYXN == khoa).FirstOrDefault();
                        dataUpdate.DaXuatKho = Convert.ToByte(IsXuatKho == "0" ? 0 : 1);
                        dataUpdate.MANVXuat = User.UserName;
                        dataUpdate.modified = DateTime.Now;
                        if (searchType == "DK2")
                        {
                            dataUpdate.NgayKeToan = (NgayKeToan_ == null) ? check_dm.NGAY : NgayKeToan_;
                        }
                        db_.Entry(dataUpdate).State = EntityState.Modified;
                    }
                    db_.SaveChanges();
                    #endregion

                    return Json(new { status = 1, title = "", text = "Cập nhật thành công", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = -1, title = "", text = "Mã kho không tồn tại", obj = "" }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new { status = -1, title = "", text = "Chưa chọn ngày kế toán", obj = "" }, JsonRequestBehavior.AllowGet);
            }



            //}
            //catch (Exception ex)
            //{
            //    return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            //}
        }
        public JsonResult Action_Update(string Khoa, string SoLuongConLai)
        {
            try
            {

                var dataUpdate = db_.XUATNHAPs.Where(a => a.KHOAKEYXN == Khoa).FirstOrDefault();
                if (dataUpdate == null)
                {
                    return Json(new { status = -1, title = "", text = "Cập nhật thất bại", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    dataUpdate.SoLuongTT = Convert.ToDecimal(SoLuongConLai);
                    dataUpdate.modified = DateTime.Now;
                    db_.Entry(dataUpdate).State = EntityState.Modified;
                    db_.SaveChanges();
                    return Json(new { status = 1, title = "", text = "Cập nhật thành công", obj = "" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}