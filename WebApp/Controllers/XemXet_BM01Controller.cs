
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using ToolsApp.Authentication;
using ToolsApp.EntityFramework.KhoSoi;
using ToolsApp.EntityFramework.VatTu;
using System.Net;
using NSClient;
using ToolsApp.com.netsuite.webservices;
using ToolsApp.EntityFramework.NetsuiteTT;
using ToolsApp.Utilities;
using System.Data.Entity;
using ToolsApp.Helper;

namespace ToolsApp.Controllers
{
    [Authorize]
    public class XemXet_BM01Controller : BaseController
    {
        private wqlkhosoiEntities db_ = new wqlkhosoiEntities();
        private wqlvattuEntities dbvt = new wqlvattuEntities();
        private NetsuiteTTEntities1 nstt_ = new NetsuiteTTEntities1();
        // GET: XemXet_BM01
        public ActionResult Index()
        {
            #region mã phiếu
            ViewBag.maphieu = db_.SP_LOAD_MAPHIEU_YEUCAU_XEMXET(User.UserName).ToList();
            #endregion

            #region lấy ký hiệu đơn vị user
            var kyhieudv = "itc";
            var thongtinuser = dbvt.sp_kyhieudonvi_by_manv(User.UserName).FirstOrDefault();
            if (thongtinuser != null)
            {
                kyhieudv = thongtinuser.KiHieu;
            }
            #endregion

            #region người phê duyệt
            ViewBag.nguoipd = dbvt.SP_LOADNGUOIPHEDUYET_BY_KIHIEU(kyhieudv).ToList();
            #endregion

            return View();
        }
        public ActionResult _GetList(string maphieu)
        {
            var userid = User.UserName;
            var list = db_.SP_NL_CTPHIEU_LOADBYMAPHIEU(maphieu, userid).ToList();
            ViewBag.List = list;
            return PartialView();
        }

        #region Update xem xét
        public JsonResult _SaveXemxet(string maphieu, string nguoipd)
        {
            var LoaiPhieu = "03";
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

            #region

            var item = db_.Sp_API_Load_NLDMPhieu(maphieu).FirstOrDefault();
            dynamic f_department = "";
            if (item.IDMaDV == "30")
            {
                var department = nstt_.Donvi_mapping_Department.Where(p => p.idmacs == item.IDMaDV).ToList();
                f_department = department.FirstOrDefault().externalid;
            }
            else
            {
                var department = nstt_.Donvi_mapping_Department.Where(p => p.idmadv == item.IDMaDV).ToList();
                f_department = department.FirstOrDefault().externalid;
            }

            System.Collections.ArrayList cfArrayList = new System.Collections.ArrayList();

            CustomRecord phieumuahang = new CustomRecord();

            RecordRef RecordRef = new RecordRef();
            RecordRef.internalId = "242"; // đề nghị mua hàng

            phieumuahang.recType = RecordRef;

            RecordRef form = new RecordRef();
            form.internalId = UtilsNetsuite.CustomForm.Phieu.PhieuMuaHang;
            phieumuahang.customForm = form;

            phieumuahang.name = maphieu;
            phieumuahang.externalId = maphieu;
            CustomFieldRef[] a = new CustomFieldRef[99];

            StringCustomFieldRef intType = new StringCustomFieldRef();
            intType.scriptId = "custrecord_tt_ma_phieu_phe_duyet";
            intType.value = maphieu;
            a[0] = intType;

            SelectCustomFieldRef listCF = new SelectCustomFieldRef();
            ListOrRecordRef listRef = new ListOrRecordRef();
            listRef.internalId = "1";
            listCF.scriptId = "custrecord_tt_loai_don_hang";
            listCF.value = listRef;
            a[1] = listCF;

            StringCustomFieldRef lydo = new StringCustomFieldRef();
            lydo.scriptId = "custrecord_tt_ly_do_su_dung";
            lydo.value = item.LYDOSD;
            a[2] = lydo;

            DateCustomFieldRef date = new DateCustomFieldRef();
            date.scriptId = "custrecord_tt_ngay_yeu_cau";
            date.value = Convert.ToDateTime(item.NGAYYC);
            a[3] = date;

            DateCustomFieldRef datexx = new DateCustomFieldRef();
            datexx.scriptId = "custrecord_tt_ngay_xem_xet";
            datexx.value = DateTime.Now;
            a[4] = datexx;

            StringCustomFieldRef kehoachthang = new StringCustomFieldRef();
            kehoachthang.scriptId = "custrecord_tt_ke_hoach_thang";
            kehoachthang.value = item.DX == 1 ? "DX" : "KH";

            a[5] = kehoachthang;

            StringCustomFieldRef emailnhantb = new StringCustomFieldRef();
            emailnhantb.scriptId = "custrecord_tt_email_nhan_thong_bao";
            emailnhantb.value = item.EMAILTHUKY;
            a[6] = emailnhantb;
       
            SelectCustomFieldRef depart = new SelectCustomFieldRef();
            ListOrRecordRef listdepart = new ListOrRecordRef();
            listdepart.externalId = (f_department == null || f_department == "") ? "" : f_department;     
            depart.scriptId = "custrecord_tt_department";
            depart.value = listdepart;
            a[7] = depart;

            var upnv = SEARCH_EMPLPOYEE.GetEmployee(item.NGUOILAPPHIEU); 
            SelectCustomFieldRef manvyc = new SelectCustomFieldRef();
            ListOrRecordRef listmanvyc = new ListOrRecordRef();
            listmanvyc.externalId = (item.NGUOILAPPHIEU == null || item.NGUOILAPPHIEU == "") ? "" : item.NGUOILAPPHIEU;
            manvyc.scriptId = "custrecord_tt_nguoi_yeu_cau";
            manvyc.value = listmanvyc;
            a[8] = manvyc;

            StringCustomFieldRef manvxx = new StringCustomFieldRef();
            manvxx.scriptId = "custrecord_tt_nguoi_xem_xet";
            manvxx.value = item.NGUOIXX;
            a[9] = manvxx;

            SelectCustomFieldRef loaiphieu = new SelectCustomFieldRef();
            ListOrRecordRef listloaiphieu = new ListOrRecordRef();
            listloaiphieu.internalId = LoaiPhieu == "05" ? "3" : LoaiPhieu == "04" ? "2" : "1";
            loaiphieu.scriptId = "custrecord_tt_loai_phieu_yeu_cau";
            loaiphieu.value = listloaiphieu;
            a[11] = loaiphieu;

            //var Location_soi = nstt_.Khoes.Where(p => p.makho == item. )

            SelectCustomFieldRef location = new SelectCustomFieldRef();
            ListOrRecordRef locations = new ListOrRecordRef();
            locations.externalId = "NS-A4.01";
            location.scriptId = "custrecord_tt_location";
            location.value = locations;
            a[12] = location;

            var LoaiSanPhams = nstt_.LoaiSanPhams.ToList();
            var PhanNhomSanPhams = nstt_.PhanNhomSanPhams.ToList();
            var item_PhanNhomSanPhams = PhanNhomSanPhams.Where(p => p.LoaiVT_TT == UtilsNetsuite.LoaiVT.SOI).ToList();
            var item_LoaiSanPhams = LoaiSanPhams.Where(p => p.externalid == (item_PhanNhomSanPhams.Count < 1 ? UtilsNetsuite.LoaiVT.SOI
                                                                             : item_PhanNhomSanPhams.FirstOrDefault().MaLoaiSP_TT))
                                                                             .FirstOrDefault().externalid;

            SelectCustomFieldRef cust_loaisp = new SelectCustomFieldRef();
            ListOrRecordRef List_loaisp = new ListOrRecordRef();
            List_loaisp.externalId = item_LoaiSanPhams;
            cust_loaisp.scriptId = "custrecord_tt_loai_san_pham";
            cust_loaisp.value = List_loaisp;
            a[13] = cust_loaisp;

            SelectCustomFieldRef cust_phannhomsp = new SelectCustomFieldRef();
            ListOrRecordRef List_phannhomsp = new ListOrRecordRef();
            List_phannhomsp.externalId = item_PhanNhomSanPhams.Count < 1 ? "" : item_PhanNhomSanPhams.FirstOrDefault().externalid;
            cust_phannhomsp.scriptId = "custrecord_tt_phan_nhom_san_pham";
            cust_phannhomsp.value = List_phannhomsp;
            a[14] = cust_phannhomsp;

            SelectCustomFieldRef cust_loaiyeucau = new SelectCustomFieldRef();
            ListOrRecordRef List_loaiyeucau = new ListOrRecordRef();
            List_loaiyeucau.internalId = "1";
            cust_loaiyeucau.scriptId = "custrecord_tt_loai_yeu_cau";
            cust_loaiyeucau.value = List_loaiyeucau;
            a[15] = cust_loaiyeucau;

            phieumuahang.customFieldList = a;
            var response = ns.Service.upsert(phieumuahang);

            var jsonString = "{ \"resultStatus\":" + Newtonsoft.Json.JsonConvert.SerializeObject(response.status) + ",";
            jsonString += "\"resultData\":" + Newtonsoft.Json.JsonConvert.SerializeObject(phieumuahang) + "}";

            var check = ((ToolsApp.com.netsuite.webservices.CustomRecordRef)response.baseRef).internalId;
            if (check != null)

            {
                var ctitems = db_.NL_CTPHIEU.Where(c => c.MAPHIEU == maphieu).ToList();               
                foreach (var ctitem in ctitems)
                {
                    var dvt = "KG";
                    var Internaliddvt = nstt_.Table_Mapping_Mavattu_DVT.Where(p => p.DVT == dvt).FirstOrDefault().Internalid;
                    var namedvt = nstt_.Table_Mapping_Mavattu_DVT.Where(p => p.DVT == dvt).FirstOrDefault().TenDVT;        
                    
                    #region add Mã  hàng
                    CustomRecord mh = new CustomRecord();

                    RecordRef mhRecordRef = new RecordRef();
                    mhRecordRef.internalId = "270";
                    mh.recType = mhRecordRef;
                    mh.externalId = ctitem.KHOATHAMCHIEU; //add externalid
                    mh.name = ctitem.MAVT;

                    CustomFieldRef[] b = new CustomFieldRef[99];

                    SelectCustomFieldRef custmh = new SelectCustomFieldRef();
                    ListOrRecordRef itemRef = new ListOrRecordRef();                 
                    itemRef.externalId = ctitem.MAVT;
                    custmh.scriptId = "custrecord_tt_itm_mat_hang";
                    custmh.value = itemRef;
                    b[0] = custmh;

                    StringCustomFieldRef slyc = new StringCustomFieldRef();
                    slyc.scriptId = "custrecord_tt_itm_so_luong_yeu_cau";
                    slyc.value = ctitem.SLYCAU.ToString();
                    b[1] = slyc;                 

                    StringCustomFieldRef pr = new StringCustomFieldRef();
                    pr.scriptId = "custrecord_tt_itm_pr_parent";
                    pr.value = ((ToolsApp.com.netsuite.webservices.CustomRecordRef)response.baseRef).internalId; // add internalid của số phiếu (khóa liên kết)
                    b[2] = pr;

                    DateCustomFieldRef dateitem = new DateCustomFieldRef();
                    dateitem.scriptId = "custrecord_tt_itm_ngay_can_hang";
                    dateitem.value = ctitem.THOIDIEMSD == null ? DateTime.Now : (DateTime)ctitem.THOIDIEMSD;
                    b[3] = dateitem;

                    StringCustomFieldRef mota = new StringCustomFieldRef();
                    mota.scriptId = "custrecord_tt_itm_mo_ta";
                    mota.value = ctitem.GHICHU == null ? "" : ctitem.GHICHU;
                    b[4] = mota;

                    SelectCustomFieldRef unit = new SelectCustomFieldRef();
                    ListOrRecordRef unitRef = new ListOrRecordRef();

                    #region Get DVT
                    RecordRef UnitType = new RecordRef();
                    UnitType.internalId = Internaliddvt;
                    UnitType.type = RecordType.unitsType;
                    UnitType.typeSpecified = true;

                    // Lấy thông tin 
                    ReadResponse UnitTypeReponse = ns.Service.get(UnitType);
                    UnitsType record = (UnitsType)UnitTypeReponse.record; // Lấy danh sách record                
                    UnitsTypeUomList fields = (UnitsTypeUomList)record.uomList;// list field trong 1 record
                    var UomInternalID = Internaliddvt;
                    for (int k = 0; k < fields.uom.Length; k++)
                    {
                        UnitsTypeUom itemfields = (UnitsTypeUom)fields.uom[k]; //lấy list uom trong units
                        if (itemfields.unitName == namedvt) // so sánh unitname = tên dvt
                        {
                            UomInternalID = itemfields.internalId; // lấy internalid Uom Units
                        }
                    }
                    #endregion                    
                    unitRef.internalId = UomInternalID;
                    unit.scriptId = "custrecord_tt_itm_don_vi_tinh";
                    unit.value = unitRef;
                    b[5] = unit;

                    StringCustomFieldRef hieu = new StringCustomFieldRef();
                    hieu.scriptId = "custrecord_tt_itm_hieu";
                    hieu.value = ctitem.HIEU;
                    b[6] = hieu;

                    mh.customFieldList = b;
                    WriteResponse responsemh = ns.Service.upsert(mh);
                  
                    #endregion                    
                }
            }

            var messerror = ((ToolsApp.com.netsuite.webservices.Status)response.status).statusDetail;
            var messdetail = messerror.FirstOrDefault().message;

            if (messdetail == "" || messdetail == null)

            {
                #region UPDATE XX

                var Phieu = db_.NL_DMPHIEU.FirstOrDefault(c => c.MAPHIEU == maphieu);
                if (Phieu != null)
                {
                    Phieu.NGUOIPD = nguoipd;
                    Phieu.NGUOITIEPNHAN = User.UserName;
                    Phieu.XXET = 1;
                    Phieu.NGAYXX = DateTime.Now;
                    Phieu.GuiAPI = true;
                    Phieu.NgayGuiAPI = DateTime.Now;
                    db_.Entry(Phieu).State = System.Data.Entity.EntityState.Modified;
                }
                db_.SaveChanges();

                #endregion

                return Json(new { status = 1, text = "Xem xét hoàn tất", obj = response }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { status = -1, text = messdetail, obj = messdetail }, JsonRequestBehavior.AllowGet);

            }
            #endregion

        }
        #endregion

        #region Gửi mail yêu cầu phê duyệt
  
        public string openOutlookemailbox(string maphieu, string nguoipd)
        {
            var maPhieu = db_.NL_DMPHIEU.Where(p => p.NGUOIXX == User.UserName && p.XXET == 1 && p.MAPHIEU == maphieu).FirstOrDefault();
            if(maPhieu == null)
            {
                throw new Exception("Mã phiếu " + maphieu + " chưa được xem xét không thể gởi mail");
            }    
            
            else
            {
                #region Lấy địa chỉ mail của người pd
                var Email = dbvt.DMNHANVIENs.FirstOrDefault(p => p.MANV == nguoipd).EMail;
                #endregion

                MailMessage mail = new MailMessage();
                mail.To.Add(new MailAddress(Email));
                mail.IsBodyHtml = true;
                mail.Subject = maphieu;
                mail.Body = "Tôi đã kiểm tra xong phiếu yêu cầu mua sợi từ chương trình Quản lý kho sợi. Mã phiếu: " + maphieu + " %0D%0A";
                var outlookmail = "mailto:" + mail.To.ToString()
                    + "?subject=" + "Đã kiểm tra phiếu yêu cầu mua từ chương trình Quản lý kho sợi - " + mail.Subject.ToString()
                    + "&body=" + "Kính gửi : Ban TGD. %0D%0A" + mail.Body.ToString() + "Trân trọng!";
                return outlookmail;
            }    
            
        }
        #endregion


        #region Search  API bỏ ko sử dụng
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _CapNhatPD(string maphieu = null)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (maphieu != null)
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
                                dynamic custfieldlist = "";
                                dynamic resulti = "";
                                var service = new NetSuiteService();
                                // Tạo đối tượng search
                                var search = new CustomRecordSearchBasic();
                                var searchr = new RecordRef();
                                searchr.internalId = "189";
                                // Thêm filter vào search object
                                // Điều kiện filter là internal ID của customrecord

                                search.recType = searchr;

                                // Thực hiện tìm kiếm
                                SearchResult result = ns.Service.search(search);

                                if (result.status.isSuccess == true)
                                {
                                    Record[] recordList;
                                    List<CustomRecord> customList = new List<CustomRecord>();
                                    for (int i = 0; i <= result.totalPages - 1; i++)
                                    {
                                        recordList = result.recordList;
                                        for (int j = 0; j <= recordList.Length - 1; j++)
                                        {
                                            customList.Add((CustomRecord)recordList[j]);
                                        }
                                        if (result.pageIndex < result.totalPages)
                                        {
                                            if (ns.UseTba)
                                            {
                                                ns.SetPreferences();
                                            }
                                            result = ns.Service.searchMoreWithId(result.searchId, result.pageIndex + 1);
                                        }
                                    }
                                    foreach (CustomRecord cust in customList)
                                    {
                                        SearchStringField tranid = null;
                                        
                                            tranid = new SearchStringField();
                                            tranid.@operator = SearchStringFieldOperator.contains;
                                            tranid.operatorSpecified = true;
                                            tranid.searchValue = "itc03032014-231_1";

                                        //var src = nstt_.Table_SearchTESTAPI.ToList();

                                        //Table_SearchTESTAPI api = new Table_SearchTESTAPI();
                                        //api.Column_1Name = cust.externalId;
                                        //api.InternalID = cust.internalId;
                                        //api.Column_7 = InventoryItem.externalId;
                                        //api.Column_2 = InventoryItem.displayName;

                                        //nstt_.Table_SearchTESTAPI.Add(api);
                                        //nstt_.SaveChanges();

                                    }


                                }    




                                //custrec.externalId = maphieu + "_1";
                                //ReadResponse response = ns.Service.get(recordRef);

                                //CustomRecord InventoryItem = (CustomRecord)response.record;



                                //CustomRecord custrec = new CustomRecord();

                                //    RecordRef recref = new RecordRef();
                                //    recref.internalId = "331";
                                //    custrec.recType = recref;
                                //recref.typeSpecified = true;
                                //custrec.customFieldList = recref;

                                //ReadResponse response = ns.Service.get(custrec);

                                //// Process response from get() operation


                                // CustomRecord InventoryItem = (CustomRecord)res.searchId;

                                //var src = nstt_.Table_SearchTESTAPI.ToList();

                                //Table_SearchTESTAPI api = new Table_SearchTESTAPI();
                                //api.Column_1Name = InventoryItem.recordList;
                                //api.InternalID = InventoryItem.internalId;
                                //api.Column_7 = InventoryItem.externalId;
                                //api.Column_2 = InventoryItem.displayName;

                                //nstt_.Table_SearchTESTAPI.Add(api);
                                //nstt_.SaveChanges();
                                return Json(new { status = 1, title = "", text = "Thêm thành công.", obj = resulti }, JsonRequestBehavior.AllowGet);
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