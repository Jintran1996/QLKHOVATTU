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
using ToolsApp.EntityFramework.NetsuiteTT;
using ToolsApp.Utilities;
using System.Data.Entity;
using ToolsApp.Helper;

namespace ToolsApp.Controllers
{
    [Authorize]
    public class XemXetAllVatTuController : BaseController
    {
        private wqlvattuEntities dbvt = new wqlvattuEntities();
        private NetsuiteTTEntities1 nstt = new NetsuiteTTEntities1();
        // GET: XemXetAllVatTu
        public ActionResult Index()
        {
            #region Mã phiếu
            ViewBag.MaPhieu = dbvt.SP_2023_LOADPHIEUXX(User.UserName, "all").ToList();
            #endregion

            #region lấy ký hiệu đơn vị user
            var kyhieudv = "itc";
            var thongtinuser = dbvt.sp_kyhieudonvi_by_manv(User.UserName).FirstOrDefault();
            if (thongtinuser != null)
            {
                kyhieudv = thongtinuser.KiHieu;
            }           

            #region lấy tên người tiếp nhận
            ViewBag.NguoiTN = dbvt.A23_LoadNguoiTiepNhan_ByMaPhieu(kyhieudv).ToList();      
            #endregion

            #region Người phê duyệt 
            ViewBag.NguoiPD = dbvt.SP_LOADNGUOIPHEDUYET_BY_KIHIEU(kyhieudv).ToList();   
            #endregion
            return View();
            #endregion
        }


        #region getlist
        public ActionResult _GetList05(string MaPhieu)
        {
            var list = dbvt.VATTU2026_A23_LoadChiTietBM05(MaPhieu).ToList();
            ViewBag.List = list;
            ViewBag.MaPhieu = MaPhieu;
            return PartialView();
        }

        public ActionResult _GetList03(string MaPhieu)
        {
            var list = dbvt.SP_SEARCHCTPHIEUBM03(MaPhieu).ToList();
            ViewBag.List = list;
            ViewBag.MaPhieu = MaPhieu;
            return PartialView();
        }

        #endregion

        #region Update SLXXET
        public JsonResult _UpdateSLXXET(string mavt, string maphieu, string slxx = null, string GHICHU = null)
        {
            try
            {
                decimal slxemxet = (slxx == null) ? 0 : Convert.ToDecimal(slxx);
                var item = dbvt.CTPHIEUx.FirstOrDefault(p => p.MAPHIEU == maphieu && p.MAVT == mavt);
                if (item == null)
                {
                    return Json(new { status = -1, title = "", text = "Không tìm thấy chi tiết phiếu.", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var check_PheDuyet = dbvt.DMPHIEUx.Where(a => a.PDuyet == 1 && a.XXet == 1 && a.MAPHIEU == maphieu).FirstOrDefault();
                    if (check_PheDuyet == null)
                    {
                        if (slxemxet < 0)
                        {
                            return Json(new { status = -1, title = "", text = "Số lượng xem xét phải lớn hơn hoặc bằng không.", obj = "" }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            var mphieu = item.MAPHIEU;
                            item.SLXXET = Convert.ToDecimal(slxemxet);
                            item.GHICHU = GHICHU;
                            dbvt.Entry(item).State = EntityState.Modified;
                            dbvt.SaveChanges();
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

        #region Update SLXXET 05
        public JsonResult _UpdateSLXXET05(string mavt, string maphieu, string slxx = null , string GHICHU = null, int idkhoa = 0)
        {
            try
            {
                decimal slxemxet = (slxx == null) ? 0 : Convert.ToDecimal(slxx);
                var item = dbvt.CTPHIEUx.FirstOrDefault(p => p.MAPHIEU == maphieu && p.MAVT == mavt && p.IDKHOA  == idkhoa);
                if (item == null)
                {
                    return Json(new { status = -1, title = "", text = "Không tìm thấy chi tiết phiếu.", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var check_PheDuyet = dbvt.DMPHIEUx.Where(a => a.PDuyet == 1 && a.XXet == 1 && a.MAPHIEU == maphieu).FirstOrDefault();
                    if (check_PheDuyet == null)
                    {
                        if (slxemxet < 0)
                        {
                            return Json(new { status = -1, title = "", text = "Số lượng xem xét phải lớn hơn hoặc bằng không.", obj = "" }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            var mphieu = item.MAPHIEU;
                            item.SLXXET = Convert.ToDecimal(slxemxet);
                            item.GHICHU = GHICHU;
                            dbvt.Entry(item).State = EntityState.Modified;
                            dbvt.SaveChanges();
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
        public JsonResult _Save(string maphieu, string NguoiPD, string NguoiTN)
        {
            #region UPDATE XX

            var update_nguoitn = dbvt.DMPHIEUx.FirstOrDefault(c => c.MAPHIEU == maphieu);
            if (update_nguoitn != null)
            {
               
                update_nguoitn.NguoiTiepNhan = NguoiTN;             
               
                dbvt.Entry(update_nguoitn).State = System.Data.Entity.EntityState.Modified;
            }
            dbvt.SaveChanges();

            #endregion
          
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
            var LoaiPhieu = dbvt.SP_SearchLoaiPhieu(maphieu).FirstOrDefault().LoaiPhieu;
            var item = dbvt.Sp_API_Load_NLDMPhieu_vattu(maphieu).FirstOrDefault();
            var ctitem_check = dbvt.CTPHIEUx.Where(c => c.MAPHIEU == maphieu).ToList();
            dynamic f_department = "";
            if (item.IDMaDV == "30")
            {
                var department = nstt.Donvi_mapping_Department.Where(p => p.idmacs == item.IDMaDV).ToList();
                f_department = department.FirstOrDefault().externalid;
                if (department.Count < 1)
                {
                    return Json(new { status = -1, text = "Chưa mapping IDMADV cho Netsuite, Vui lòng liên hệ ITC để cập nhật, IDMADV: " + item.IDMaDV, obj = "" }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                var i_idmadv = item.IDMaDV == "83" ? "84" : item.IDMaDV;
                var department = nstt.Donvi_mapping_Department.Where(p => p.idmadv == i_idmadv).ToList();
                if(department.Count < 1)
                {
                    return Json(new { status = -1, text = "Chưa mapping IDMADV cho Netsuite, Vui lòng liên hệ ITC để cập nhật, IDMADV: "+ i_idmadv, obj = "" }, JsonRequestBehavior.AllowGet);
                }

                f_department = department.FirstOrDefault().externalid;
            }

            System.Collections.ArrayList cfArrayList = new System.Collections.ArrayList();

            CustomRecord phieumuahang = new CustomRecord();

            RecordRef RecordRef = new RecordRef();
            RecordRef.internalId = "242"; // ID record Phiếu đề nghị mua hàng

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
            lydo.value = item.LyDoSD == null ? "" : item.LyDoSD;
            a[2] = lydo;


            DateCustomFieldRef date = new DateCustomFieldRef();
            date.scriptId = "custrecord_tt_ngay_yeu_cau";
            date.value = item.NgayYC == null ? DateTime.Now : new DateTime(item.NgayYC.Value.Year, item.NgayYC.Value.Month, item.NgayYC.Value.Day);
            //  date.value = DateTime.Now; 
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

            // var testdepd = "SRS-THO-000"; //KDND
            SelectCustomFieldRef depart = new SelectCustomFieldRef();
            ListOrRecordRef listdepart = new ListOrRecordRef();
            listdepart.externalId = (f_department == null || f_department == "") ? "" : f_department;
            //listdepart.externalId = deparment.Count > 0 ? testdepd : "";
            depart.scriptId = "custrecord_tt_department";
            depart.value = listdepart;
            a[7] = depart;
        
            var upnv = SEARCH_EMPLPOYEE.GetEmployee(item.NguoiLapPhieu);
            SelectCustomFieldRef manvyc = new SelectCustomFieldRef();
            ListOrRecordRef listmanvyc = new ListOrRecordRef();
            listmanvyc.externalId = (item.NguoiLapPhieu == null || item.NguoiLapPhieu == "") ? "" : item.NguoiLapPhieu;
            manvyc.scriptId = "custrecord_tt_nguoi_yeu_cau";
            manvyc.value = listmanvyc;
            a[8] = manvyc;

            var hoten = dbvt.DMNHANVIENs.FirstOrDefault(p => p.MANV == item.NguoiXX).HOTEN;
            StringCustomFieldRef manvxx = new StringCustomFieldRef();
            manvxx.scriptId = "custrecord_tt_nguoi_xem_xet";
            manvxx.value = item.NguoiXX + "|" + hoten;
            a[9] = manvxx;

            SelectCustomFieldRef loaiphieu = new SelectCustomFieldRef();
            ListOrRecordRef listloaiphieu = new ListOrRecordRef();
            listloaiphieu.internalId = LoaiPhieu == "05" ? "3" : LoaiPhieu == "04" ? "2" : "1";
            loaiphieu.scriptId = "custrecord_tt_loai_phieu_yeu_cau";
            loaiphieu.value = listloaiphieu;
            a[11] = loaiphieu;

            var locationdmphieu = dbvt.DMPHIEUx.FirstOrDefault(p => p.MAPHIEU == maphieu).locations;
            SelectCustomFieldRef location = new SelectCustomFieldRef();
            ListOrRecordRef locations = new ListOrRecordRef();
            locations.externalId = locationdmphieu == null ? "NS-A4.01" : locationdmphieu;
            location.scriptId = "custrecord_tt_location";
            location.value = locations;
            a[12] = location;

            var LoaiSanPhams = nstt.LoaiSanPhams.ToList();
            var PhanNhomSanPhams = nstt.PhanNhomSanPhams.ToList();
            var item_PhanNhomSanPhams = PhanNhomSanPhams.Where(p => p.LoaiVT_TT == item.LoaiVT.ToString()).ToList();
            var item_LoaiSanPhams = LoaiSanPhams.Where(p => p.externalid == (item_PhanNhomSanPhams.Count < 1 ? "VATTU" 
                                                                            : item_PhanNhomSanPhams.FirstOrDefault().MaLoaiSP_TT)).FirstOrDefault().externalid;

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

            var manvpd_ = nstt.TaiKhoanNhanVien_Netsuite.Where(p => p.name == NguoiPD).ToList();
            if (manvpd_.Count < 1)
            {
                return Json(new { status = -1, text = "Vui lòng cập nhật người PD", obj = "" }, JsonRequestBehavior.AllowGet);
            }


            phieumuahang.customFieldList = a;
            var response = ns.Service.upsert(phieumuahang);

            var jsonString = "{ \"resultStatus\":" + Newtonsoft.Json.JsonConvert.SerializeObject(response.status) + ",";
                                        jsonString += "\"resultData\":" + Newtonsoft.Json.JsonConvert.SerializeObject(phieumuahang) + "}";

            var check = ((ToolsApp.com.netsuite.webservices.CustomRecordRef)response.baseRef).internalId;
            if (check != null)
            {
                var ctitems = dbvt.CTPHIEUx.Where(c => c.MAPHIEU == maphieu).ToList();
               
                foreach (var ctitem in ctitems)
                {
                    var dvt = dbvt.VATTU2024_SpLoad_ConvertDVT(ctitem.MAVT.Trim()).FirstOrDefault().DVT_Convert;
                  //  var dvt = dbvt.DANHMUCVATTUs.Where(p => p.MAVT == ctitem.MAVT).Select(p => p.DVT).FirstOrDefault();
                  
                    var Internalid_ = nstt.Table_Mapping_Mavattu_DVT.Where(p => p.DVT == dvt).ToList();
                    if(Internalid_.Count < 1)
                    {
                       return Json(new { status = -1, text = "Chưa mapping DVT cho Netsuite, Vui lòng liên hệ ITC để cập nhật, DVT: " + dvt, obj = "" }, JsonRequestBehavior.AllowGet);
                    }
                    var Internaliddvt = Internalid_.FirstOrDefault().Internalid;
                    var namedvt = Internalid_.FirstOrDefault().TenDVT;
                    var Externaliddvt = Internalid_.FirstOrDefault().Externalid;

                    #region add Mã  hàng
                    CustomRecord mh = new CustomRecord();

                    RecordRef mhRecordRef = new RecordRef();
                    mhRecordRef.internalId = "270"; // id record mã hàng(270) của phiếu đề nghị mua hàng (242)
                    mh.recType = mhRecordRef;
                    mh.externalId = ctitem.MAPHIEU + ctitem.IDKHOA;
                    mh.name = ctitem.MAVT;
                    

                    CustomFieldRef[] b = new CustomFieldRef[99];

                    SelectCustomFieldRef custmh = new SelectCustomFieldRef();
                    ListOrRecordRef itemRef = new ListOrRecordRef();
                    //  itemRef.externalId = ctitem.FirstOrDefault().MAVT;
                    itemRef.externalId = ctitem.MAVT;
                    custmh.scriptId = "custrecord_tt_itm_mat_hang";
                    custmh.value = itemRef;
                    b[0] = custmh;


                    StringCustomFieldRef slyc = new StringCustomFieldRef();
                    slyc.scriptId = "custrecord_tt_itm_so_luong_yeu_cau";
                    slyc.value = ctitem.SLXXET.ToString();
                    b[1] = slyc;
                  

                    StringCustomFieldRef pr = new StringCustomFieldRef();
                    pr.scriptId = "custrecord_tt_itm_pr_parent";
                    pr.value = ((ToolsApp.com.netsuite.webservices.CustomRecordRef)response.baseRef).internalId;
                    b[2] = pr;

                    DateCustomFieldRef dateitem = new DateCustomFieldRef();
                    dateitem.scriptId = "custrecord_tt_itm_ngay_can_hang";
                    dateitem.value = ctitem.THOIDIEMSD == null ? DateTime.Now : (DateTime)ctitem.THOIDIEMSD;
                    b[3] = dateitem;

                    StringCustomFieldRef mota = new StringCustomFieldRef();
                    mota.scriptId = "custrecord_tt_itm_mo_ta";
                   // mota.value = (ctitem.TINHTRANG == null ? "" : (ctitem.TINHTRANG )) + " " + (ctitem.GHICHU == null ? "" : ctitem.GHICHU);
                    mota.value =
                                " Tình trạng: "+(ctitem.TINHTRANG ?? "")  +
                                " | Ghi chú: " +(ctitem.GHICHU ?? "") +
                                " | MAVT Tương đương: " + (ctitem.MAVT_TUONGDUONG ?? "") +
                                " | Tồn ĐV: "  +(ctitem.SLTONDVI != null ? ctitem.SLTONDVI.ToString() : "0.00") +
                                " | Tồn CTY: " +(ctitem.SLTONCTY != null ? ctitem.SLTONCTY.ToString() : "0.00");
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
                    hieu.value = "";
                    b[6] = hieu;

                    StringCustomFieldRef idkhoa = new StringCustomFieldRef();
                    idkhoa.scriptId = "custrecordttg_ctphieu_id_khoa";
                    idkhoa.value = ctitem.IDKHOA.ToString();
                    b[7] = idkhoa;

                    mh.customFieldList = b;
                    WriteResponse responsemh = ns.Service.upsert(mh);

                   // var jsonStringct = "{ \"resultStatus\":" + Newtonsoft.Json.JsonConvert.SerializeObject(responsemh.status) + ",";
                   // jsonStringct += "\"resultData\":" + Newtonsoft.Json.JsonConvert.SerializeObject(mh) + "}";

                    #endregion                
                }
            }

            var messerror = ((ToolsApp.com.netsuite.webservices.Status)response.status).statusDetail;
            var messdetail = messerror.FirstOrDefault().message;

            if (messdetail == "" || messdetail == null)
            {
                #region UPDATE XX

                var Phieu = dbvt.DMPHIEUx.FirstOrDefault(c => c.MAPHIEU == maphieu);
                if (Phieu != null)
                {
                    Phieu.NguoiPD = NguoiPD;
                    Phieu.NguoiTiepNhan = NguoiTN;
                    Phieu.XXet = 1;
                    Phieu.NgayXX = DateTime.Now;                    
                    dbvt.Entry(Phieu).State = System.Data.Entity.EntityState.Modified;
                }
                dbvt.SaveChanges();

                #endregion
            }
            else
            {
                return Json(new { status = -1, text = messdetail, obj = messdetail }, JsonRequestBehavior.AllowGet);
            }
            #endregion
            return Json(new { status = 2, text = "Xem xét hoàn tất", obj = response }, JsonRequestBehavior.AllowGet);

        }
        #endregion

        #region Gửi mail yêu cầu phê duyệt
        [HttpPost]
        public string openOutlookemailbox(string MaPhieu, string NguoiPD, string LoaiPhieu = "01")
        {
            #region Lấy địa chỉ mail của người pd

            var Email = dbvt.DMNHANVIENs.FirstOrDefault(p => p.MANV == NguoiPD).EMail;
            #endregion

            MailMessage mail = new MailMessage();
            mail.To.Add(new MailAddress(Email));
            mail.IsBodyHtml = true;
            mail.Subject = MaPhieu;
            mail.Body = MaPhieu;
            var outlookmail = "mailto:" + mail.To.ToString()
                + "?subject=" + "FW: Trinh duyet ma phieu: " + mail.Subject.ToString() + " tu chuong trinh QLVT "
                + "&body=" + "Kính gửi : Ban TGD. %0D%0A" + "Trinh duyet Loai phieu: BM" + LoaiPhieu + "(" + mail.Body.ToString() + ")%0D%0A" + "Trân trọng!";
            return outlookmail;
        }
        #endregion

        #region onchange 
        public JsonResult NguoiLapPhieu(string MaPhieu)
        {
            var NguoiLapPhieu = dbvt.SP_NguoiLapPhieu(MaPhieu).ToList();
            return Json(new { status = 1, text = "", obj = NguoiLapPhieu }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult LoaiPhieu(string MaPhieu)
        {
            var LoaiPhieu = dbvt.SP_SearchLoaiPhieu(MaPhieu).FirstOrDefault().LoaiPhieu;
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
                var CTPhieu = dbvt.SP_REPHIEUMUAHANGTHEOBM(MaPhieu).ToList();
              
                //ViewData["ChukyPD"] = image;
                ViewBag.List = CTPhieu;
            }
            return PartialView();
        }
        #endregion
    }
}