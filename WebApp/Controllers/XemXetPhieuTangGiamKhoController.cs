using NSClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using ToolsApp.Authentication;
using ToolsApp.com.netsuite.webservices;
using ToolsApp.EntityFramework.VatTu;
using ToolsApp.EntityFramework.KhoSoi;
using ToolsApp.EntityFramework.NetsuiteTT;
using ToolsApp.Utilities;
using System.Data.Entity;
using ToolsApp.Helper;

namespace ToolsApp.Controllers
{
    [Authorize]
    public class XemXetPhieuTangGiamKhoController : BaseController
    {
        private wqlvattuEntities vt_ = new wqlvattuEntities();
        private wqlkhosoiEntities soi_ = new wqlkhosoiEntities();
        private NetsuiteTTEntities1 nstt_ = new NetsuiteTTEntities1();
        // GET: XemXetAllVatTu
        public ActionResult Index()
        {
            #region Mã phiếu
            ViewBag.SOCTXN = vt_.VATTU2025_SPLOAD_SOCTXN_TANGGIAMKHO(User.UserName).ToList();
            #endregion

            return View();

        }


        #region getlist
        public ActionResult _GetList_Vattu(string SOCTXN)
        {
            var list = vt_.VATTU2025_SPLOAD_SOCTXN_TANGGIAMKHO_XUATNHAP(SOCTXN).ToList();
            ViewBag.List = list;
            ViewBag.SOCTXN = SOCTXN;
            return PartialView();
        }
        public ActionResult _GetList_Soi(string SOCTXN)
        {
            var list = soi_.SOI2025_SPLOAD_SOCTXN_TANGGIAMKHO_NL_CTXUATNHAP(SOCTXN).ToList();
            ViewBag.List = list;
            ViewBag.SOCTXN = SOCTXN;
            return PartialView();
        }



        #endregion

        #region Update SLXXET
        public JsonResult _UpdateSLXXET(string mavt, string maphieu, Double slxx = 0, string GHICHU = null)
        {
            try
            {
                var item = vt_.CTPHIEUx.FirstOrDefault(p => p.MAPHIEU == maphieu && p.MAVT == mavt);
                if (item == null)
                {
                    return Json(new { status = -1, title = "", text = "Không tìm thấy chi tiết phiếu.", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var check_PheDuyet = vt_.DMPHIEUx.Where(a => a.PDuyet == 1 && a.XXet == 1 && a.MAPHIEU == maphieu).FirstOrDefault();
                    if (check_PheDuyet == null)
                    {
                        if (slxx < 0)
                        {
                            return Json(new { status = -1, title = "", text = "Số lượng xem xét phải lớn hơn hoặc bằng không.", obj = "" }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            var mphieu = item.MAPHIEU;
                            item.SLXXET = Convert.ToDecimal(slxx);
                            item.GHICHU = GHICHU;
                            vt_.Entry(item).State = EntityState.Modified;
                            vt_.SaveChanges();
                            return Json(new { status = 1, title = "", text = "Cập nhật thành công.", obj = mphieu }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        return Json(new { status = -1, title = "", text = "Phiếu đã được phê duyệt.", obj = "" }, JsonRequestBehavior.AllowGet);
                    }
                }

            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Update -- Gửi APIs Netsuite
        public JsonResult _Save(string SOCTXN, string Loaiphieu)
        {
            #region Khai báo data TTG
            var dmxuatnhap = vt_.DM_XUATNHAP.FirstOrDefault(p => p.SOCTXN == SOCTXN);
            var ctxuatnhap = vt_.XUATNHAPs.Where(p => p.SoCTXN == SOCTXN).ToList();

            var dmphieuxn = soi_.NL_DMPHIEUXN.Where(p => p.SOCTXN == SOCTXN).ToList();
            var nl_ctxuatnhap = soi_.NL_CTXUATNHAP.Where(p => p.SOCTXN == SOCTXN).ToList();

            #endregion

            #region CALL NSCLIENT

            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
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

            if (dmxuatnhap != null && ctxuatnhap.Any())
            {
                #region Nhập điều chỉnh
                #region Call data                   
                var xuat3buoc = vt_.DMLOAIXNs.FirstOrDefault(p => p.LOAIXN == dmxuatnhap.LOAIXN);
                var department = nstt_.Donvi_mapping_Department.Where(p => p.idmadv == dmxuatnhap.IDMADVNHAN).ToList();
                if (dmxuatnhap.IDMADVNHAN == "30" || dmxuatnhap.IDMADVNHAN == "92")
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


                var loaixn = dmxuatnhap?.LOAIXN;
                var customform = "152"; // Điều chỉnh tồn                        
                                        //var customform = "169"; // Điều chỉnh tồn                        
                                        // var cls = nstt_.Classes.FirstOrDefault(p => p.externalid == "BAN_PTVT");
                var tk_iad = vt_.DMLOAIXNs.FirstOrDefault(p => p.LOAIXN == loaixn).Account_IAD;
                var int_iad = nstt_.Chart_of_Accounts.FirstOrDefault(p => p.Number_TT == tk_iad).InternalID_NS;
                var taikhoanketoan = int_iad; // Account: 1571 - Hàng gửi bán: Vật tư
                                              // Account: 1578  - Hàng gửi bán: Chờ xử lý


                var ex_location = kNhan.FirstOrDefault().externalid;
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
                account.internalId = taikhoanketoan;
                InAdj.account = account;

                InAdj.tranDateSpecified = true;
                InAdj.tranDate = Convert.ToDateTime(dmxuatnhap.NGAY);

                RecordRef khonhan = new RecordRef();
                khonhan.externalId = ex_location;
                InAdj.adjLocation = khonhan;
                InAdj.memo = dmxuatnhap.GHICHU;

                if (dmxuatnhap.IDMADVNHAN != "30")
                {
                    RecordRef mabp = new RecordRef();
                    mabp.externalId = department.Count < 1 ? "" : department.FirstOrDefault().externalid;
                    InAdj.department = mabp;
                }

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
                    var checkidmacs = (f_idmacs == "" ? (department.Count < 1 ? "" : department.FirstOrDefault().externalid) : f_idmacs);
                    var IAD_detail_department = nstt_.Donvi_mapping_Department.Where(p => p.idmacs == checkidmacs).ToList();
                    var f_IAD_detail_department = IAD_detail_department.Count < 1 ? (department.Count < 1 ? "" : department.FirstOrDefault().externalid) : IAD_detail_department.FirstOrDefault().externalid;

                    CustomFieldRef[] cusDetail = new CustomFieldRef[100];
                    InventoryAdjustmentInventory line_IA = new InventoryAdjustmentInventory();
                    InventoryAdjustment IA = new InventoryAdjustment();


                    // line_IA.GetType()
                    RecordRef refItem = new RecordRef();
                    refItem.externalId = items.MAVT;
                    line_IA.item = refItem;
                    line_IA.memo = items.GHICHU;

                    RecordRef location_item = new RecordRef();
                    location_item.externalId = kNhan.FirstOrDefault().externalid; // Kho nhận
                    line_IA.location = location_item;

                    line_IA.adjustQtyBySpecified = true;
                    line_IA.adjustQtyBy = Convert.ToDouble(items.SoLuongTT);

                    RecordRef reflocation = new RecordRef();
                    // reflocation.externalId = kNhan.FirstOrDefault().externalid;
                    reflocation.externalId = ex_location;
                    line_IA.location = reflocation;

                    RecordRef refdepartment = new RecordRef();
                    // reflocation.externalId = kNhan.FirstOrDefault().externalid;
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

                #region
                try
                {
                    dmxuatnhap.GUIAPI = "1";
                    dmxuatnhap.NGAYGUIAPI = DateTime.Now;
                    vt_.Entry(dmxuatnhap).State = EntityState.Modified;
                    vt_.SaveChanges();
                }
                catch (Exception ex)
                {
                    string message = ex.Message;
                    string exmessage = ex.InnerException.InnerException.Message;
                    string errormessage = exmessage == null || exmessage == "" ? message : exmessage;
                    return Json(new { status = -1, title = "", text = errormessage, obj = "" }, JsonRequestBehavior.AllowGet);
                }
      
                #endregion

                return Json(new { status = 1, title = "", text = "Xem xét thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
            }
            if (dmphieuxn.Any() && nl_ctxuatnhap.Any())
            {
                #region Xử lý dữ liệu 

                var f_dmphieuxn = dmphieuxn.FirstOrDefault();
                var ct_phieuxn = soi_.NL_CTXUATNHAP.Where(p => p.SOCTXN == SOCTXN).ToList();
                var MAHINHTHUC = f_dmphieuxn?.MAHTHUC;
                var bophansx = nstt_.Donvi_mapping_Department.Where(p => p.mabp == f_dmphieuxn.MABP).ToList();
                var f_bophansx = bophansx.Count < 1 ? "" : bophansx.FirstOrDefault().externalid;
                var nhamaysx = nstt_.Khoes.Where(p => p.makho == f_dmphieuxn.MAKHONHAN).ToList();
                var f_nhamaysx = nhamaysx.Count < 1 ? "" : nhamaysx.FirstOrDefault().externalid;
                var key = soi_.NL_DMHINHTHUC.Where(p => p.MAHTHUC == MAHINHTHUC).FirstOrDefault();
                var xuat3buoc = soi_.NL_DMHINHTHUC.FirstOrDefault(p => p.MAHTHUC == MAHINHTHUC).XUAT3BUOC;

                var department = nstt_.Donvi_mapping_Department.Where(p => p.idmadv == f_dmphieuxn.IDMADVNHAN).ToList();
                if (f_dmphieuxn.IDMADVNHAN == "30" || f_dmphieuxn.IDMADVNHAN == "92")
                {
                    department = nstt_.Donvi_mapping_Department.Where(p => p.idmacs == f_dmphieuxn.IDMADVNHAN).ToList();
                }

                var kNhan = nstt_.Khoes.Where(p => p.makho == f_dmphieuxn.MAKHONHAN).ToList();
                var kXuat = nstt_.Khoes.Where(p => p.makho == f_dmphieuxn.MAKHOXUAT).ToList();

                if (kNhan.Count < 1)
                {
                    kNhan = nstt_.Khoes.Where(p => p.externalid == f_dmphieuxn.MAKHONHAN).ToList();
                }
                if (kXuat.Count < 1)
                {
                    kXuat = nstt_.Khoes.Where(p => p.externalid == f_dmphieuxn.MAKHOXUAT).ToList();
                }

                #endregion

                if (xuat3buoc == "0") // (nhập kho NZZZ phiên mã vải)
                {
                    #region Nhập điều chỉnh
                    #region Call data                     

                    var customform = "152"; // Điều chỉnh tồn                        
                                            //var customform = "169"; // Điều chỉnh tồn                        
                                            // var cls = nstt_.Classes.FirstOrDefault(p => p.externalid == "BAN_PTVT");
                    var tk_iad = soi_.NL_DMHINHTHUC.FirstOrDefault(p => p.MAHTHUC == MAHINHTHUC).Account_KT;
                    var int_iad = nstt_.Chart_of_Accounts.FirstOrDefault(p => p.Number_TT == tk_iad).InternalID_NS;
                    var taikhoanketoan = int_iad; // Account: 1571
                    var ex_location = kNhan.FirstOrDefault().externalid;
                    #endregion

                    #region InventoryAdjustment
                    InventoryAdjustment InAdj = new InventoryAdjustment();
                    CustomFieldRef[] cusfield = new CustomFieldRef[100];

                    RecordRef cusfRec = new RecordRef();
                    cusfRec.internalId = customform;
                    InAdj.customForm = cusfRec;

                    InAdj.externalId = f_dmphieuxn.SOCTXN + "_IAD";
                    InAdj.tranId = f_dmphieuxn.SOCTXN + "_IAD";

                    RecordRef account = new RecordRef();
                    account.internalId = taikhoanketoan;
                    InAdj.account = account;

                    InAdj.tranDateSpecified = true;
                    InAdj.tranDate = Convert.ToDateTime(f_dmphieuxn.NGAY);

                    RecordRef khonhan = new RecordRef();
                    // khoxuat.externalId = kNhan.Count < 1 ? "" : kNhan.FirstOrDefault().externalid;
                    khonhan.externalId = ex_location;
                    InAdj.adjLocation = khonhan;
                    InAdj.memo = f_dmphieuxn.GHICHU;

                    if (f_dmphieuxn.IDMADVNHAN != "30")
                    {
                        RecordRef mabp = new RecordRef();
                        mabp.externalId = department.Count < 1 ? "" : department.FirstOrDefault().externalid;
                        InAdj.department = mabp;
                    }

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
                    foreach (var items in ct_phieuxn)
                    {

                        var IAD_detail_department = nstt_.Donvi_mapping_Department.Where(p => p.idmadv == f_dmphieuxn.IDMADVNHAN).ToList();
                        var f_IAD_detail_department = IAD_detail_department.Count < 1 ? "" : IAD_detail_department.FirstOrDefault().externalid;

                        CustomFieldRef[] cusDetail = new CustomFieldRef[100];
                        InventoryAdjustmentInventory line_IA = new InventoryAdjustmentInventory();

                        RecordRef refItem = new RecordRef();
                        refItem.externalId = items.MAVT;
                        line_IA.item = refItem;
                        line_IA.memo = items.GHICHU;

                        RecordRef location_item = new RecordRef();
                        location_item.externalId = kNhan.FirstOrDefault().externalid; // Kho nhận
                        line_IA.location = location_item;
                        line_IA.adjustQtyBySpecified = true;
                        line_IA.adjustQtyBy = Convert.ToDouble(items.SOLUONGTT);

                        RecordRef mabp = new RecordRef();
                        mabp.externalId = f_IAD_detail_department;
                        line_IA.department = mabp;

                        var chkquantity = Convert.ToDouble(items.SOLUONGTT);
                        var LOT = items.HIEU + ((items.LO == null || items.LO == "") ? "" : ("-" + items.LO));

                        InventoryDetail inventoryDetail = new InventoryDetail();

                        InventoryAssignmentList inventoryAssignmentList = new InventoryAssignmentList();

                        List<InventoryAssignment> List_inventoryAssignment = new List<InventoryAssignment>();

                        InventoryAssignment inventoryAssignment = new InventoryAssignment();
                        if (chkquantity > 0)
                        {
                            RecordRef lotRecord = new RecordRef();
                            // lotRecord.externalId = "AA030001002_HAND";
                            inventoryAssignment.receiptInventoryNumber = LOT;
                            inventoryAssignment.issueInventoryNumber = lotRecord;
                            inventoryAssignment.quantity = Convert.ToDouble(items.SOLUONGTT);
                            inventoryAssignment.quantitySpecified = true;
                            List_inventoryAssignment.Add(inventoryAssignment);
                            inventoryAssignmentList.inventoryAssignment = List_inventoryAssignment.ToArray();
                            inventoryDetail.inventoryAssignmentList = inventoryAssignmentList;
                            line_IA.inventoryDetail = inventoryDetail;
                            orderItems_IA.Add(line_IA);
                        }
                        else
                        {
                            RecordRef lotRecord = new RecordRef();
                            // lotRecord.externalId = "AA030001002_HAND";
                            lotRecord.externalId = items.MAVT + "_" + LOT;

                            inventoryAssignment.issueInventoryNumber = lotRecord;
                            inventoryAssignment.quantity = Convert.ToDouble(items.SOLUONGTT);
                            inventoryAssignment.quantitySpecified = true;
                            List_inventoryAssignment.Add(inventoryAssignment);
                            inventoryAssignmentList.inventoryAssignment = List_inventoryAssignment.ToArray();
                            inventoryDetail.inventoryAssignmentList = inventoryAssignmentList;
                            line_IA.inventoryDetail = inventoryDetail;
                            orderItems_IA.Add(line_IA);
                        }
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

                    #region update trạng thái API   

                    var up_dmphieuxn = soi_.NL_DMPHIEUXN.Where(p => p.SOCTXN == SOCTXN).FirstOrDefault();
                    up_dmphieuxn.GUIAPI = "1";
                    up_dmphieuxn.Externalid_IAD = f_dmphieuxn.SOCTXN + "_IAD";
                    up_dmphieuxn.NGAYGUIAPI = DateTime.Now;
                    soi_.Entry(up_dmphieuxn).State = EntityState.Modified;
                    soi_.SaveChanges();
                    #endregion

                    return Json(new { status = 1, title = "", text = "Xem xét hoàn tất.", obj = "" }, JsonRequestBehavior.AllowGet);
                }

            }
            return Json(new { status = 2, text = "Xem xét hoàn tất", obj = "" }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Gửi mail yêu cầu phê duyệt
        [HttpPost]
        public string openOutlookemailbox(string SOCTXN)
        {
            // Danh sách email người phê duyệt
            var emailList = new List<string>
                {
                    "bichlien@thaituan.com.vn",
                    "maithi@thaituan.com.vn",
                    "ktvattu2@thaituan.com.vn",
                    "kschiphi@thaituan.com.vn",
                };
            // Gộp danh sách email bằng dấu ';'
            string toEmails = string.Join(";", emailList);

            // Tạo tiêu đề và nội dung
            string subject = Uri.EscapeDataString("FW: Đơn vị xem xét tăng giảm tồn SOCTXN: " + SOCTXN + " từ chương trình QLVT");
            string body = Uri.EscapeDataString(
                "Kính gửi: Ban TGD.\r\n" +
                "Đơn vị xem xét tăng giảm tồn vật tư từ chương trình QLVT: SOCTXN (" + SOCTXN + ")\r\n" +
                "Trân trọng!"
            );

            // Tạo link mailto
            string outlookmail = $"mailto:{toEmails}?subject={subject}&body={body}";

            return outlookmail;
        }
        #endregion

        #region onchange 
        public JsonResult NguoiLapPhieu(string MaPhieu)
        {
            var NguoiLapPhieu = vt_.SP_NguoiLapPhieu(MaPhieu).ToList();
            return Json(new { status = 1, text = "", obj = NguoiLapPhieu }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult LoaiPhieu(string MaPhieu)
        {
            var LoaiPhieu = vt_.SP_SearchLoaiPhieu(MaPhieu).FirstOrDefault().LoaiPhieu;
            return Json(new { status = 1, text = "", obj = LoaiPhieu }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region In phiếu xem xét
        public ActionResult ReportPrint(string MaPhieu)
        {
            if (MaPhieu == null)
            {
                return Json(new { status = -1, text = "", obj = "" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var CTPhieu = vt_.SP_REPHIEUMUAHANGTHEOBM(MaPhieu).ToList();

                //ViewData["ChukyPD"] = image;
                ViewBag.List = CTPhieu;
            }
            return PartialView();
        }
        #endregion
    }
}