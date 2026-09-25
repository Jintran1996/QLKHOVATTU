
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Spreadsheet;
using Newtonsoft.Json;
using NSClient;
using SpreadsheetLight;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using ToolsApp.Authentication;
using ToolsApp.com.netsuite.webservices;
using ToolsApp.EntityFramework.KhoSoi;
using ToolsApp.EntityFramework.NetsuiteTT;
using ToolsApp.EntityFramework.SPMAY;
using ToolsApp.EntityFramework.VatTu;
using ToolsApp.Models;

namespace ToolsApp.Controllers
{
    [Authorize]
    public class DMVattuController : BaseController
    {
        private wqlvattuEntities db_ = new wqlvattuEntities();
        private wqlkhosoiEntities soi_ = new wqlkhosoiEntities();
        private NetsuiteTTEntities1 nstt_ = new NetsuiteTTEntities1();
        private QLSANPHAMMAY2023Entities SPMAY = new QLSANPHAMMAY2023Entities();

        #region Index
        public ActionResult Index()
        {
            var istrangthai = db_.VATTU2024_CHECK_QUYENMANV_UPLOAD_EXCEL_DANHMUCVATTU(User.UserName, "1").Where(p => p.MANV == User.UserName).ToList();
            dynamic istrue = "0";

            var dmloaivt = db_.VATTU2024_SPLOAD_LOAIVT_COA().ToList();
            ViewBag.dmloaivt = dmloaivt;
            if (istrangthai.Count > 0)
            {
                istrue = "1";
            }  

            ViewBag.User = User.UserName;
            ViewBag.istrue = istrue;
            return View();
        }
        #endregion

        public ActionResult _Upload()
        {
            return PartialView();
        }

        #region _GetList
        public ActionResult _GetList(string MAVTSearch = null, string TENVTSearch = null, string LoaiVTSearch = null)
        {

            var model = db_.DANHMUCVATTUs.Where(p =>
                ((MAVTSearch == null || MAVTSearch == "" || p.MAVT.Contains(MAVTSearch)) &&
                (TENVTSearch == null || TENVTSearch == "" || p.TenVT.Contains(TENVTSearch)) &&
                (LoaiVTSearch == null || LoaiVTSearch == "" || p.LoaiVT.Contains(LoaiVTSearch))
                )).OrderByDescending(d => d.NGAYCN).ToList();
            ViewBag.List = model;
            ViewBag.Username = User.UserName;

            return PartialView();
        }
        #endregion

        #region form Upload Image
        public ActionResult _frmUpImage(string MAVT = null)
        {
            ViewBag.MAVT = MAVT;
            return PartialView();
        }
        #endregion

        #region form insert
        public ActionResult _DetailForEdit(string MAVT = null)
        {

            ViewBag.dmdvt = db_.DANHMUCDONVITINHs.ToList();
            ViewBag.dmdonvi = db_.DMDONVIs.ToList();
            var dmloaivt = db_.VATTU2024_SPLOAD_LOAIVT_COA().ToList();
            ViewBag.dmloaivt = dmloaivt;
            ViewBag.nhomvt = db_.VATTU2024_SPLOAD_MANHOM_BYLOAIVT_COA(dmloaivt.FirstOrDefault().LoaiVT, User.UserName).ToList();
            ViewBag.maChiPhi = db_.SP_LOAD_MACP_MAVATTU("").ToList();
            if (MAVT == null || MAVT == string.Empty)
            {
                return PartialView("_Insert", new DMVATTUViewModels { MAVT = MAVT });
            }
            else
            {
                var model = db_.DANHMUCVATTUs.FirstOrDefault(c => c.MAVT == MAVT);
                ViewBag.data = model;
                return PartialView("_Update", new DMVATTUViewModels { MAVT = model.MAVT, LoaiVT = model.LoaiVT, TenVT = model.TenVT, DVT = model.DVT, HieuLuc = model.HieuLuc, DonViSuDungtac = model.DonViSuDungtac, MACP = model.MACP });
            }

        }
        #endregion

        #region Thêm dm mã hàng + chi tiết

        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _InsertFun(DMVATTUViewModels model)
        {
            try
            {
                if (db_.DANHMUCVATTUs.FirstOrDefault(c => c.MAVT == model.MAVT) != null)
                {
                    return Json(new { status = -2, title = "", text = "Thêm không thành công. MAVT đã tồn tại. Kiểm tra lại.", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                if (model.MAVT == null)
                {
                    return Json(new { status = -1, title = "", text = "Mã vật tư không được để trống.", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                if (model.MAVT.Length != 11 && (model.LoaiVT == "TM-VAIMOC-NK" || model.LoaiVT == "TM-VAIMOC-ND"))
                {
                    return Json(new { status = -1, title = "", text = "Mã vật tư không phù hợp, Vui lòng tạo mã vải mộc đúng qui định.", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                if (model.MAVT.Length != 11 && (model.LoaiVT == "VATTU" || model.LoaiVT == "BAOBI_NHANMAC" || model.LoaiVT == "CONGCU_DUNGCU" || model.LoaiVT == "HOACHAT" || model.LoaiVT == "NHIENLIEU" || model.LoaiVT == "NLCHINH"
                || model.LoaiVT == "NLMAY" || model.LoaiVT == "PHUTUNG" || model.LoaiVT == "PHUTUNG_PHUCHOI" || model.LoaiVT == "PLMAY"
                ))
                {
                    return Json(new { status = -1, title = "", text = "Mã vật tư phải 11 ký tự, Vui lòng tạo mã đúng qui định.", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                var check_ = db_.VATTU2024_SPLOAD_CHECK_KYTU_LOAIVT_COA(model.LoaiVT, model.Manhom, model.MAVT).FirstOrDefault();
                if (check_.kq == "0")
                {
                    return Json(new { status = -1, title = "", text = "Mã vật tư " + model.MAVT.Trim().ToUpper() + " không phù hợp, " + check_.kiemtra, obj = "" }, JsonRequestBehavior.AllowGet);
                }

                var COA = db_.TBL_DMLOAIVT_COA.Where(p => p.LoaiVT == model.LoaiVT && p.ManhomVT == model.Manhom).ToList();
                var f_COA = COA.FirstOrDefault();

                var model_copy = new DANHMUCVATTU();
                model_copy.MAVT = model.MAVT;
                model_copy.Manhom = model.Manhom;
                model_copy.TenVT = model.TenVT;
                model_copy.LoaiVT = model.LoaiVT;
                model_copy.DVT = model.DVT;
                model_copy.DonViSuDungtac = model.DonViSuDungtac;
                model_copy.Ghichu = model.Ghichu;
                model_copy.HieuLuc = true;
                model_copy.MANVCAPNHAT = User.UserName;
                model_copy.NGAYCN = DateTime.Now;
                model_copy.MACP = f_COA.MACP;
                model_copy.COGS = f_COA.COGS;
                model_copy.ASSET = f_COA.ASSET;
                model_copy.INCOME = f_COA.INCOME;
                model_copy.TK_ChiPhi_SX = f_COA.TK_PhiSX;
                db_.DANHMUCVATTUs.Add(model_copy);
                db_.SaveChanges();

                var result = _IsDongBo(model.MAVT);
                var json = JsonConvert.SerializeObject(result.Data);
                var resultData = JsonConvert.DeserializeObject<SOAPResultViewModel>(json);               
                if (resultData.status < 1)
                {
                    return Json(new { status = -1, title = "", text = resultData.text, obj = "" }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { status = 1, title = "", text = "Thêm mới thành công", obj = "" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }

        }
        #endregion

        #region Sửa dm mã hàng + chi tiết

        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _EditFun(DMVATTUViewModels model)
        {
            try
            {
                var mavt = db_.DANHMUCVATTUs.Find(model.MAVT);
                if (mavt != null)
                {
                    #region gán dữ liệu

                    mavt.TenVT = model.TenVT;
                    mavt.Manhom = model.NhomVT;
                    mavt.LoaiVT = model.LoaiVT;
                    mavt.DVT = model.DVT;
                    mavt.DonViSuDungtac = model.DonViSuDungtac;
                    mavt.HieuLuc = model.HieuLuc;
                    mavt.MANVMODIFY = User.UserName;
                    mavt.NGAYMODIFY = DateTime.Now;
                    mavt.MACP = model.MACP;
                    #endregion

                    db_.SaveChanges();

                    return Json(new { status = 1, title = "", text = "Update thành công", obj = "" }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { status = -1, title = "", text = "Update không thành công", obj = "" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }

        }
        #endregion

        #region onchange 
        public JsonResult _Onchange_NhomVT_ByLoaiVT(string LoaiVT)
        {
            var nhomvt = db_.VATTU2024_SPLOAD_MANHOM_BYLOAIVT_COA(LoaiVT, User.UserName).ToList();
            return Json(new { status = 1, text = "", obj = nhomvt }, JsonRequestBehavior.AllowGet);
        }


        #endregion

        #region Insert all VATTU

        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _UpsertMAVTNS()
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var mavt = db_.SP_GUIAPI_Load_DMVATTU("").ToList();
                    if (mavt.Count > 0)
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

                                foreach (var items in mavt)
                                {
                                    var LOAIVTs = "";
                                    if (items.LOAIVT == "SOI")
                                    {
                                        LOAIVTs = "SOI";
                                    }
                                    else
                                    {
                                        LOAIVTs = db_.DANHMUCVATTUs.Where(p => p.MAVT == items.MAVT).FirstOrDefault().LoaiVT;
                                    }
                                    var LoaiSanPhams = nstt_.LoaiSanPhams.ToList();
                                    var PhanNhomSanPhams = nstt_.PhanNhomSanPhams.ToList();
                                    var LoaiSanPhamChiTiets = nstt_.LoaiSanPhamChiTiets.ToList();

                                    var item_PhanNhomSanPhams = PhanNhomSanPhams.Where(p => p.LoaiVT_TT == LOAIVTs.ToString()).ToList();
                                    var item_LoaiSanPhams = LoaiSanPhams.Where(p => p.externalid == (item_PhanNhomSanPhams.Count < 1 ? "VATTU"
                                                                          : item_PhanNhomSanPhams.FirstOrDefault().MaLoaiSP_TT))
                                                                          .FirstOrDefault().externalid;
                                    if (items.LOAIVT == "MAY")
                                    {
                                        item_LoaiSanPhams = LoaiSanPhams.Where(p => p.externalid == "MAY").FirstOrDefault().externalid;
                                    }
                                    if (items.LOAIVT == "VAI")
                                    {
                                        item_LoaiSanPhams = LoaiSanPhams.Where(p => p.externalid == "VAITHANHPHAM").FirstOrDefault().externalid;
                                    }

                                    var dvt = db_.VATTU2024_SpLoad_ConvertDVT(items.MAVT).FirstOrDefault().DVT_Convert;
                                    var item_LoaiSanPhamChiTiets = LoaiSanPhamChiTiets.Where(p => p.Kytu1 == items.MAVT[0].ToString()
                                                                                          && p.MaLoaiSPCT == items.MAVT[1].ToString()).ToList();

                                    var exiddvt = nstt_.Table_Mapping_Mavattu_DVT.Where(c => c.DVT == dvt).FirstOrDefault().Internalid;
                                    // var exiddvt = "3";
                                    dynamic item = "";
                                    // dynamic int_form = "67";
                                    var int_form = db_.TBL_DMLOAIVT_COA.FirstOrDefault(p => p.LoaiVT == items.LOAIVT).CustomForm;
                                    var Soi_GC = soi_.TBLDMNGUYENLIEUx.FirstOrDefault(p => p.MAVATTU == items.MAVT);
                                    switch (items.LOAIVT)
                                    {
                                        case "SOI":
                                            switch (Soi_GC.LoaiSoi)
                                            {
                                                case "MUANGOAI":
                                                    item = new LotNumberedInventoryItem();
                                                    int_form = "67"; // Form TT Vật Tư 
                                                    break;
                                                default:
                                                    item = new LotNumberedAssemblyItem();
                                                    int_form = "44";
                                                    break;
                                            }
                                            // item = new LotNumberedInventoryItem();
                                            break;
                                        case "MAY":
                                            item = new LotNumberedInventoryItem();
                                            break;
                                        case "VAI":
                                            item = new LotNumberedInventoryItem();
                                            break;
                                        case "VAI_TT_DI_GC":
                                            item = new LotNumberedInventoryItem();
                                            break;
                                        case "MOC":
                                            item = new LotNumberedInventoryItem();
                                            break;
                                        case "NHUONGQUYEN":
                                            item = new LotNumberedInventoryItem();
                                            break;
                                        default:
                                            item = new InventoryItem();
                                            break;
                                    }
                                    //InventoryItem item = new InventoryItem();
                                    dynamic f_coa_mavt = "";
                                    if (items.LOAIVT == "SOI")
                                    {
                                        f_coa_mavt = soi_.TBLDMNGUYENLIEUx.FirstOrDefault(p => p.MAVATTU == items.MAVT);
                                    }
                                    else
                                    {
                                        f_coa_mavt = db_.DANHMUCVATTUs.FirstOrDefault(p => p.MAVT == items.MAVT);
                                    }


                                    var taxcode = nstt_.Table_TAX_CODE.Where(p => p.HieuLuc == true).FirstOrDefault().Internalid_taxcode;
                                    var LOAISPCT = nstt_.VATTU2024_LOAD_LOAISPCHITIET(items.MAVT).ToList();
                                    var f_LOAISPCT = LOAISPCT.Count < 1 ? "" : LOAISPCT.FirstOrDefault().externalid;

                                    RecordRef form = new RecordRef();
                                    form.internalId = int_form;
                                    item.customForm = form;
                                    item.pricesIncludeTax = true;
                                    item.pricesIncludeTaxSpecified = true;

                                    RecordRef salesTax = new RecordRef();
                                    salesTax.internalId = taxcode;
                                    item.salesTaxCode = salesTax;

                                    dynamic tt_asset = f_coa_mavt.ASSET;
                                    var tt_cogs = f_coa_mavt.COGS;
                                    var tt_income = f_coa_mavt.INCOME;
                                    var COA = nstt_.Chart_of_Accounts.ToList();
                                    var intid_asset = COA.FirstOrDefault(p => p.Number_TT == tt_asset).InternalID_NS;
                                    var intid_cogs = COA.FirstOrDefault(p => p.Number_TT == tt_cogs).InternalID_NS;
                                    var intid_income = COA.FirstOrDefault(p => p.Number_TT == tt_income).InternalID_NS;

                                    RecordRef asset = new RecordRef();
                                    asset.internalId = intid_asset;
                                    item.assetAccount = asset;

                                    RecordRef cogs = new RecordRef();
                                    cogs.internalId = intid_cogs;
                                    item.cogsAccount = cogs;

                                    RecordRef income = new RecordRef();
                                    income.internalId = intid_income;
                                    item.incomeAccount = income;


                                    RecordRef unittype = new RecordRef();
                                    unittype.internalId = exiddvt;
                                    item.unitsType = unittype;


                                    item.externalId = items.MAVT; 
                                    item.itemId = items.MAVT;
                                    item.displayName = items.TenVT == null ? "" : items.TenVT;
                                    item.salesDescription = (items.TenVT == null ? "" : (items.MAVT + "_" + items.TenVT));

                                    CustomFieldRef[] a = new CustomFieldRef[99];

                                    SelectCustomFieldRef cust_loaisp = new SelectCustomFieldRef();
                                    ListOrRecordRef List_loaisp = new ListOrRecordRef();
                                    List_loaisp.externalId = item_LoaiSanPhams;
                                    cust_loaisp.scriptId = "custitem_btm_tt_loai_san_pham";
                                    cust_loaisp.value = List_loaisp;
                                    a[1] = cust_loaisp;

                                    SelectCustomFieldRef cust_phannhomsp = new SelectCustomFieldRef();
                                    ListOrRecordRef List_phannhomsp = new ListOrRecordRef();
                                    List_phannhomsp.externalId = item_PhanNhomSanPhams.Count < 1 ? "" : item_PhanNhomSanPhams.FirstOrDefault().externalid;
                                    cust_phannhomsp.scriptId = "custitem_btm_tt_nhom_sp_may";
                                    cust_phannhomsp.value = List_phannhomsp;
                                    a[2] = cust_phannhomsp;

                                    var LIST_WIPSTATUS = db_.TBL_DMLOAIVT_COA.Where(p => p.LoaiVT == LOAIVTs).ToList();
                                    var WIPSTATUS = LIST_WIPSTATUS.Count < 1 ? "2" : LIST_WIPSTATUS.FirstOrDefault().WIPSTATUS == "WIP" ? "1" : "2";

                                    SelectCustomFieldRef cust_statuswip = new SelectCustomFieldRef();
                                    ListOrRecordRef List_statuswip = new ListOrRecordRef();
                                    List_statuswip.internalId = WIPSTATUS;
                                    cust_statuswip.scriptId = "custitem_btm_mc_wip_status";
                                    cust_statuswip.value = List_statuswip;
                                    a[3] = cust_statuswip;


                                    if (items.LOAIVT == "SOI")
                                    {
                                        StringCustomFieldRef chisosoi = new StringCustomFieldRef();
                                        chisosoi.scriptId = "custitem_btm_tt_chi_so_soi";
                                        chisosoi.value = f_coa_mavt.CS1;
                                        StringCustomFieldRef Filament = new StringCustomFieldRef();
                                        Filament.scriptId = "custitem_btm_tt_chi_so_filament";
                                        Filament.value = f_coa_mavt.CS2;
                                        a[3] = chisosoi;
                                        a[4] = Filament;


                                        /**
             * 1	Base Price
             * 9	Giá Bán Chi Nhánh
             * 8	Giá Bán Lẻ
             * 3	Giá Bán Đại Lý Miền Bắc
             * 2	Giá Bán Đại Lý Miền Nam
             * 4	Giá Bán Đại Lý Miền Trung
             * 10	Giá Nhà May
             * 6	Giá Sỉ
             * 7	Giá lưu động
             * 11	Giá Đại Lý
             */

                                        string[] initializedArray = new string[] { "1", "9", "8", "7", "11" };
                                        PricingMatrix pm = new PricingMatrix();
                                        Pricing[] pricing = new Pricing[9];
                                        int o = 0;
                                        foreach (string iw in initializedArray)
                                        {

                                            Price[] prices = new Price[1];
                                            prices[0] = new Price()
                                            {
                                                quantity = 0,
                                                quantitySpecified = true,
                                                value = 1.0,
                                                valueSpecified = true

                                            };
                                            Pricing price0 = new Pricing()
                                            {
                                                currency = new RecordRef()
                                                {
                                                    type = RecordType.currency,
                                                    internalId = "1" //VND
                                                },
                                                priceLevel = new RecordRef()
                                                {
                                                    type = RecordType.priceLevel,
                                                    internalId = iw,
                                                },
                                                priceList = prices
                                            };

                                            pricing[o] = price0;

                                            pm.pricing = pricing;

                                            o++;
                                        }
                                        item.pricingMatrix = pm;
                                    }

                                    // LotNumberedInventoryItem itemsa = new LotNumberedInventoryItem();

                                    item.customFieldList = a;

                                    WriteResponse addmahang = ns.Service.upsert(item);

                                    var messerror = ((ToolsApp.com.netsuite.webservices.Status)addmahang.status).statusDetail;
                                    var messdetail = messerror.FirstOrDefault().message;
                                    var check = ((ToolsApp.com.netsuite.webservices.RecordRef)addmahang.baseRef).internalId;
                                    var status = addmahang.status.isSuccess;
                                    if (status == true)
                                    {
                                        if (items.LOAIVT == "SOI")
                                        {
                                            var dmvt = soi_.SP_UPDATE_GUIAPI_TBLDMNGUYENLIEU(items.MAVT);
                                        }
                                        else
                                        {
                                            var dmvt = db_.DANHMUCVATTUs.Where(p => p.MAVT == items.MAVT).FirstOrDefault();

                                            dmvt.GuiAPI = "1";
                                            dmvt.NgayGuiAPI = DateTime.Now;
                                            db_.Entry(dmvt).State = EntityState.Modified;
                                            db_.SaveChanges();

                                            if (dmvt.LoaiVT == "QUANAO" || dmvt.LoaiVT == "PHUTRANG" || dmvt.LoaiVT == "QUANAO_GC" || dmvt.LoaiVT == "PHUTRANG_GC"   || dmvt.LoaiVT == "SPMAYKHAC" || dmvt.LoaiVT == "PHUKIEN" || dmvt.LoaiVT == "NHUONGQUYEN")
                                            {
                                                SPMAY.INSERT_FROM_DANHMUCVATTU_TO_DMMAHANG_SPM2023_HANGMUANGOAI(items.MAVT);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        GhiLog_DMVattu_ErrorAPI_NS log = new GhiLog_DMVattu_ErrorAPI_NS();
                                        log.HanhDong = "Error btnDongboMAVT DANHMUC VATTU _ " + items.LOAIVT;
                                        log.id = Guid.NewGuid();
                                        log.Externalid = items.MAVT;
                                        log.Internalid = check;
                                        log.MAVT = items.MAVT;
                                        log.TenVT = items.TenVT;
                                        log.DVT = items.DVT;
                                        log.LogNS = messdetail == null ? "" : messdetail.ToString();
                                        log.MANV = User.UserName;
                                        log.Ngaylog = DateTime.Now;
                                        nstt_.GhiLog_DMVattu_ErrorAPI_NS.Add(log);
                                        nstt_.SaveChanges();
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                return Json(new { status = 0, title = "Error", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
                            }
                            return Json(new { status = 1, title = "", text = "Thêm thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
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

        #region Đồng bộ VATTU

        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _btnDongBo(string MAVT)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var mavt = db_.DANHMUCVATTUs.Where(p => p.MAVT == MAVT).ToList();

                    if (mavt.Count > 0)
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

                                foreach (var items in mavt)
                                {
                                    var LOAIVTs = db_.DANHMUCVATTUs.Where(p => p.MAVT == items.MAVT).FirstOrDefault().LoaiVT;
                                    var LoaiSanPhams = nstt_.LoaiSanPhams.ToList();
                                    var PhanNhomSanPhams = nstt_.PhanNhomSanPhams.ToList();
                                    var LoaiSanPhamChiTiets = nstt_.LoaiSanPhamChiTiets.ToList();
                                    var item_PhanNhomSanPhams = PhanNhomSanPhams.Where(p => p.LoaiVT_TT == LOAIVTs.ToString()).ToList();
                                    if (LOAIVTs == "PT-VT")
                                    {
                                        item_PhanNhomSanPhams = PhanNhomSanPhams.Where(p => p.LoaiVT_TT == LOAIVTs.ToString() && items.MAVT[0].ToString() == p.KyTu).ToList();
                                    }
                                    var item_LoaiSanPhams = LoaiSanPhams.Where(p => p.externalid == (item_PhanNhomSanPhams.Count < 1 ? "VATTU"
                                                                          : item_PhanNhomSanPhams.FirstOrDefault().MaLoaiSP_TT))
                                                                          .FirstOrDefault().externalid;
                                    if (items.LoaiVT == "PTMUANGOAI")
                                    {
                                        item_LoaiSanPhams = LoaiSanPhams.Where(p => p.externalid == "MAY").FirstOrDefault().externalid;
                                    }
                                    if (items.LoaiVT == "VAI" || items.LoaiVT == "VAI_TT_DI_GC")
                                    {
                                        item_LoaiSanPhams = LoaiSanPhams.Where(p => p.externalid == "VAITHANHPHAM").FirstOrDefault().externalid;
                                    }

                                    var item_LoaiSanPhamChiTiets = LoaiSanPhamChiTiets.Where(p => p.Kytu1 == items.MAVT[0].ToString()
                                                                                          && p.MaLoaiSPCT == items.MAVT[1].ToString()).ToList();
                                    var dvt = db_.VATTU2024_SpLoad_ConvertDVT(items.MAVT).FirstOrDefault().DVT_Convert;
                                    var exiddvt = nstt_.Table_Mapping_Mavattu_DVT.Where(c => c.DVT == dvt).FirstOrDefault().Internalid;
                                    dynamic item = "";
                                    // dynamic int_form = "67"; // customform - 67 = TT vật tư / inventory
                                    var int_form = db_.TBL_DMLOAIVT_COA.FirstOrDefault(p => p.LoaiVT == items.LoaiVT).CustomForm;

                                    if (items.LoaiVT == "MOC" || items.LoaiVT == "VAI" || items.LoaiVT == "QUANAO" || items.LoaiVT == "VAI_TT_DI_GC" || items.LoaiVT == "MOC_TT_DI_GC"
                                        || items.LoaiVT == "PHUTRANG" || items.LoaiVT == "QUANAO_GC" || items.LoaiVT == "PHUTRANG_GC" || items.LoaiVT == "PHUKIEN" || items.LoaiVT == "SPMAYKHAC" || items.LoaiVT == "NHUONGQUYEN")
                                    {
                                        item = new LotNumberedInventoryItem();
                                    }
                                    else
                                    {
                                        item = new InventoryItem();
                                    }


                                    var taxcode = nstt_.Table_TAX_CODE.Where(p => p.HieuLuc == true).FirstOrDefault().Internalid_taxcode;
                                    var LOAISPCT = nstt_.VATTU2024_LOAD_LOAISPCHITIET(items.MAVT).ToList();
                                    var f_LOAISPCT = LOAISPCT.Count < 1 ? "" : LOAISPCT.FirstOrDefault().externalid;

                                    //   InventoryItem itemsa = new InventoryItem();

                                    RecordRef form = new RecordRef();
                                    form.internalId = int_form;
                                    item.customForm = form;
                                    item.pricesIncludeTax = true;
                                    item.pricesIncludeTaxSpecified = true;

                                    RecordRef purchaseTaxCode = new RecordRef();
                                    purchaseTaxCode.internalId = taxcode;
                                    item.purchaseTaxCode = purchaseTaxCode;

                                    RecordRef salesTax = new RecordRef();
                                    salesTax.internalId = taxcode;
                                    item.salesTaxCode = salesTax;


                                    RecordRef unittype = new RecordRef();
                                    unittype.internalId = exiddvt;
                                    item.unitsType = unittype;


                                    item.externalId = items.MAVT;
                                    item.itemId = items.MAVT;
                                    item.upcCode = items.MAVT;
                                    item.displayName = items.TenVT == null ? "" : items.TenVT;
                                    item.salesDescription = (items.TenVT == null ? "" : (items.MAVT + "_" + items.TenVT));

                                    var f_coa_mavt = db_.DANHMUCVATTUs.FirstOrDefault(p => p.MAVT == items.MAVT);
                                    var tt_asset = f_coa_mavt.ASSET;
                                    var tt_cogs = f_coa_mavt.COGS;
                                    var tt_income = f_coa_mavt.INCOME;
                                    var COA = nstt_.Chart_of_Accounts.ToList();
                                    var intid_asset = tt_asset == "" ? "" : COA.FirstOrDefault(p => p.Number_TT == tt_asset).InternalID_NS;
                                    var intid_cogs = tt_cogs == "" ? "" : COA.FirstOrDefault(p => p.Number_TT == tt_cogs).InternalID_NS;
                                    var intid_income = tt_income == "" ? "" : COA.FirstOrDefault(p => p.Number_TT == tt_income).InternalID_NS;

                                    RecordRef asset = new RecordRef();
                                    asset.internalId = intid_asset;
                                    item.assetAccount = asset;

                                    RecordRef cogs = new RecordRef();
                                    cogs.internalId = intid_cogs;
                                    item.cogsAccount = cogs;

                                    RecordRef income = new RecordRef();
                                    income.internalId = intid_income;
                                    item.incomeAccount = income;

                                    CustomFieldRef[] a = new CustomFieldRef[99];

                                    SelectCustomFieldRef cust_loaisp = new SelectCustomFieldRef();
                                    ListOrRecordRef List_loaisp = new ListOrRecordRef();
                                    List_loaisp.externalId = item_LoaiSanPhams;
                                    cust_loaisp.scriptId = "custitem_btm_tt_loai_san_pham";
                                    cust_loaisp.value = List_loaisp;
                                    a[1] = cust_loaisp;

                                    SelectCustomFieldRef cust_phannhomsp = new SelectCustomFieldRef();
                                    ListOrRecordRef List_phannhomsp = new ListOrRecordRef();
                                    List_phannhomsp.externalId = item_PhanNhomSanPhams.Count < 1 ? "" : item_PhanNhomSanPhams.FirstOrDefault().externalid;
                                    cust_phannhomsp.scriptId = "custitem_btm_tt_nhom_sp_may";
                                    cust_phannhomsp.value = List_phannhomsp;
                                    a[2] = cust_phannhomsp;

                                    var LIST_WIPSTATUS = db_.TBL_DMLOAIVT_COA.Where(p => p.LoaiVT == LOAIVTs).ToList();
                                    var WIPSTATUS = LIST_WIPSTATUS.Count < 1 ? "2" : LIST_WIPSTATUS.FirstOrDefault().WIPSTATUS == "WIP" ? "1" : "2";

                                    SelectCustomFieldRef cust_statuswip = new SelectCustomFieldRef();
                                    ListOrRecordRef List_statuswip = new ListOrRecordRef();
                                    List_statuswip.internalId = WIPSTATUS;
                                    cust_statuswip.scriptId = "custitem_btm_mc_wip_status";
                                    cust_statuswip.value = List_statuswip;
                                    a[3] = cust_statuswip;

                                    SelectCustomFieldRef cust_phamvi = new SelectCustomFieldRef();
                                    ListOrRecordRef List_phamvi = new ListOrRecordRef();
                                    List_phamvi.internalId = "2";
                                    cust_phamvi.scriptId = "cseg_btm_tt_pv_bh";
                                    cust_phamvi.value = List_phamvi;
                                    a[4] = cust_phamvi;

                                    if (items.LoaiVT == "MOC" || items.LoaiVT == "VAI" || items.LoaiVT == "QUANAO" || items.LoaiVT == "VAI_TT_DI_GC" || items.LoaiVT == "MOC_TT_DI_GC"
                                    || items.LoaiVT == "PHUTRANG" || items.LoaiVT == "QUANAO_GC" || items.LoaiVT == "PHUTRANG_GC"
                                    || items.LoaiVT == "PHUKIEN" || items.LoaiVT == "SPMAYKHAC" || items.LoaiVT == "NHUONGQUYEN")
                                    {
                                        StringCustomFieldRef cust_dichdanhLOT = new StringCustomFieldRef();
                                        cust_dichdanhLOT.scriptId = "custitem_btm_mc_dich_danh_lot";
                                        cust_dichdanhLOT.value = "T";
                                        a[5] = cust_dichdanhLOT;
                                    }

                                    item.customFieldList = a;
                                    WriteResponse addmahang = ns.Service.upsert(item);
                                    var messerror = ((ToolsApp.com.netsuite.webservices.Status)addmahang.status).statusDetail;
                                    var messdetail = messerror.FirstOrDefault().message;
                                    var check = ((ToolsApp.com.netsuite.webservices.RecordRef)addmahang.baseRef).internalId;
                                    var status = addmahang.status.isSuccess;
                                    if (status == true)
                                    {

                                        var dmvt = db_.DANHMUCVATTUs.Where(p => p.MAVT == items.MAVT).FirstOrDefault();
                                        dmvt.GuiAPI = "1";
                                        dmvt.HieuLuc = true;
                                        dmvt.NgayGuiAPI = DateTime.Now;
                                        db_.Entry(dmvt).State = EntityState.Modified;
                                        db_.SaveChanges();
                                        if (dmvt.LoaiVT == "QUANAO" || dmvt.LoaiVT == "PHUTRANG" || dmvt.LoaiVT == "QUANAO_GC" || dmvt.LoaiVT == "PHUTRANG_GC" || dmvt.LoaiVT == "SPMAYKHAC" || dmvt.LoaiVT == "PHUKIEN" || dmvt.LoaiVT == "NHUONGQUYEN")
                                        {
                                            SPMAY.INSERT_FROM_DANHMUCVATTU_TO_DMMAHANG_SPM2023_HANGMUANGOAI(items.MAVT);
                                        }
                                    }
                                    else
                                    {
                                        GhiLog_DMVattu_ErrorAPI_NS log = new GhiLog_DMVattu_ErrorAPI_NS();
                                        log.HanhDong = "btnDongboMAVT DANHMUC VATTU _ " + items.LoaiVT;
                                        log.id = Guid.NewGuid();
                                        log.Externalid = items.MAVT;
                                        log.Internalid = check;
                                        log.MAVT = items.MAVT;
                                        log.TenVT = items.TenVT;
                                        log.DVT = items.DVT;
                                        log.LogNS = messdetail == null ? "" : messdetail.ToString();
                                        log.MANV = User.UserName;
                                        log.Ngaylog = DateTime.Now;
                                        nstt_.GhiLog_DMVattu_ErrorAPI_NS.Add(log);
                                        nstt_.SaveChanges();

                                        return Json(new { status = -1, title = "", text = "Thêm không thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
                                    }

                                }
                                return Json(new { status = 1, title = "", text = "Thêm thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
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

        #region Insert VATTU _NQTH

        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _UpsertMAVTNS_NQTH(string MAVTSearch = null, string TENVTSearch = null, string LoaiVTSearch = null)
        {
            if (ModelState.IsValid)
            {
                if (LoaiVTSearch == "" || LoaiVTSearch == null)
                {
                    return Json(new { status = -1, title = "", text = "Vui lòng chọn Loại VT.", obj = "" }, JsonRequestBehavior.AllowGet);
                }

                try
                {
                    var listmavt = db_.SP_GUIAPI_Load_DMVATTU_ALLNHUONGQUYEN_PHUKIEN().Where(p =>
            ((MAVTSearch == null || MAVTSearch == "" || p.MAVT.Contains(MAVTSearch)) &&
            (TENVTSearch == null || TENVTSearch == "" || p.TenVT.Contains(TENVTSearch)) &&
            (LoaiVTSearch == null || LoaiVTSearch == "" || p.LoaiVT.Contains(LoaiVTSearch)))).ToList();

                    if (listmavt.Count > 0)
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

                                foreach (var mavt in listmavt)
                                {
                                    var items = db_.DANHMUCVATTUs.Where(p => p.MAVT == mavt.MAVT).FirstOrDefault();
                                    var LOAIVTs = items.LoaiVT;
                                    var LIST_WIPSTATUS = db_.TBL_DMLOAIVT_COA.Where(p => p.LoaiVT == LOAIVTs).ToList();
                                    var WIPSTATUS = LIST_WIPSTATUS.Count < 1 ? "2" : LIST_WIPSTATUS.FirstOrDefault().WIPSTATUS == "WIP" ? "1" : "2";
                                    var LoaiSanPhams = nstt_.LoaiSanPhams.ToList();
                                    var PhanNhomSanPhams = nstt_.PhanNhomSanPhams.ToList();
                                    var LoaiSanPhamChiTiets = nstt_.LoaiSanPhamChiTiets.ToList();
                                    var item_PhanNhomSanPhams = PhanNhomSanPhams.Where(p => p.LoaiVT_TT == LOAIVTs.ToString()).ToList();
                                    if (LOAIVTs == "PT-VT")
                                    {
                                        item_PhanNhomSanPhams = PhanNhomSanPhams.Where(p => p.LoaiVT_TT == LOAIVTs.ToString() && items.MAVT[0].ToString() == p.KyTu).ToList();
                                    }
                                    var item_LoaiSanPhams = LoaiSanPhams.Where(p => p.externalid == (item_PhanNhomSanPhams.Count < 1 ? "VATTU"
                                                                          : item_PhanNhomSanPhams.FirstOrDefault().MaLoaiSP_TT))
                                                                          .FirstOrDefault().externalid;
                                    if (items.LoaiVT == "PTMUANGOAI")
                                    {
                                        item_LoaiSanPhams = LoaiSanPhams.Where(p => p.externalid == "MAY").FirstOrDefault().externalid;
                                    }
                                    if (items.LoaiVT == "VAI" || items.LoaiVT == "VAI_TT_DI_GC")
                                    {
                                        item_LoaiSanPhams = LoaiSanPhams.Where(p => p.externalid == "VAITHANHPHAM").FirstOrDefault().externalid;
                                    }

                                    var item_LoaiSanPhamChiTiets = LoaiSanPhamChiTiets.Where(p => p.Kytu1 == items.MAVT[0].ToString()
                                                                                          && p.MaLoaiSPCT == items.MAVT[1].ToString()).ToList();
                                    var dvt = db_.VATTU2024_SpLoad_ConvertDVT(items.MAVT).FirstOrDefault().DVT_Convert;
                                    var exiddvt = nstt_.Table_Mapping_Mavattu_DVT.Where(c => c.DVT == dvt).FirstOrDefault().Internalid;
                                    dynamic item = "";
                                    // dynamic int_form = "67"; // customform - 67 = TT vật tư / inventory
                                    var int_form = db_.TBL_DMLOAIVT_COA.FirstOrDefault(p => p.LoaiVT == items.LoaiVT).CustomForm;

                                    if (items.LoaiVT == "MOC" || items.LoaiVT == "VAI" || items.LoaiVT == "QUANAO" || items.LoaiVT == "VAI_TT_DI_GC" || items.LoaiVT == "MOC_TT_DI_GC"
                                        || items.LoaiVT == "PHUTRANG" || items.LoaiVT == "QUANAO_GC" || items.LoaiVT == "PHUTRANG_GC"
                                        || items.LoaiVT == "PHUKIEN" || items.LoaiVT == "SPMAYKHAC" || items.LoaiVT == "NHUONGQUYEN")
                                    {
                                        item = new LotNumberedInventoryItem();
                                    }
                                    else
                                    {
                                        item = new InventoryItem();
                                    }

                                    var taxcode = nstt_.Table_TAX_CODE.Where(p => p.HieuLuc == true).FirstOrDefault().Internalid_taxcode;
                                    var LOAISPCT = nstt_.VATTU2024_LOAD_LOAISPCHITIET(items.MAVT).ToList();
                                    var f_LOAISPCT = LOAISPCT.Count < 1 ? "" : LOAISPCT.FirstOrDefault().externalid;

                                    InventoryItem itemsa = new InventoryItem();

                                    RecordRef form = new RecordRef();
                                    form.internalId = int_form;
                                    item.customForm = form;
                                    item.pricesIncludeTax = true;
                                    item.pricesIncludeTaxSpecified = true;                                    

                                    RecordRef purchaseTaxCode = new RecordRef();
                                    purchaseTaxCode.internalId = taxcode;
                                    item.purchaseTaxCode = purchaseTaxCode;

                                    RecordRef salesTax = new RecordRef();
                                    salesTax.internalId = taxcode;
                                    item.salesTaxCode = salesTax;

                                    RecordRef unittype = new RecordRef();
                                    unittype.internalId = exiddvt;
                                    item.unitsType = unittype;

                                    item.externalId = items.MAVT;
                                    item.itemId = items.MAVT;
                                    item.upcCode = items.MAVT;
                                    item.displayName = items.TenVT == null ? "" : items.TenVT;
                                    item.salesDescription = (items.TenVT == null ? "" : (items.MAVT + "_" + items.TenVT));

                                    var f_coa_mavt = db_.DANHMUCVATTUs.FirstOrDefault(p => p.MAVT == items.MAVT);
                                    var tt_asset = f_coa_mavt.ASSET;
                                    var tt_cogs = f_coa_mavt.COGS;
                                    var tt_income = f_coa_mavt.INCOME;

                                    var COA = nstt_.Chart_of_Accounts.ToList();
                                    var intid_asset = tt_asset == "" ? "" : COA.FirstOrDefault(p => p.Number_TT == tt_asset).InternalID_NS;
                                    var intid_cogs = tt_cogs == "" ? "" : COA.FirstOrDefault(p => p.Number_TT == tt_cogs).InternalID_NS;
                                    var intid_income = tt_income == "" ? "" : COA.FirstOrDefault(p => p.Number_TT == tt_income).InternalID_NS;

                                    RecordRef asset = new RecordRef();
                                    asset.internalId = intid_asset;
                                    item.assetAccount = asset;

                                    RecordRef cogs = new RecordRef();
                                    cogs.internalId = intid_cogs;
                                    item.cogsAccount = cogs;

                                    RecordRef income = new RecordRef();
                                    income.internalId = intid_income;
                                    item.incomeAccount = income;

                                    CustomFieldRef[] a = new CustomFieldRef[99];

                                    SelectCustomFieldRef cust_loaisp = new SelectCustomFieldRef();
                                    ListOrRecordRef List_loaisp = new ListOrRecordRef();
                                    List_loaisp.externalId = item_LoaiSanPhams;
                                    cust_loaisp.scriptId = "custitem_btm_tt_loai_san_pham";
                                    cust_loaisp.value = List_loaisp;
                                    a[1] = cust_loaisp;

                                    SelectCustomFieldRef cust_phannhomsp = new SelectCustomFieldRef();
                                    ListOrRecordRef List_phannhomsp = new ListOrRecordRef();
                                    List_phannhomsp.externalId = item_PhanNhomSanPhams.Count < 1 ? "" : item_PhanNhomSanPhams.FirstOrDefault().externalid;
                                    cust_phannhomsp.scriptId = "custitem_btm_tt_nhom_sp_may";
                                    cust_phannhomsp.value = List_phannhomsp;
                                    a[2] = cust_phannhomsp;

                                    SelectCustomFieldRef cust_statuswip = new SelectCustomFieldRef();
                                    ListOrRecordRef List_statuswip = new ListOrRecordRef();
                                    List_statuswip.internalId = WIPSTATUS;
                                    cust_statuswip.scriptId = "custitem_btm_mc_wip_status";
                                    cust_statuswip.value = List_statuswip;
                                    a[3] = cust_statuswip;

                                    if (items.LoaiVT == "MOC" || items.LoaiVT == "VAI" || items.LoaiVT == "QUANAO" || items.LoaiVT == "VAI_TT_DI_GC" || items.LoaiVT == "MOC_TT_DI_GC"
                               || items.LoaiVT == "PHUTRANG" || items.LoaiVT == "QUANAO_GC" || items.LoaiVT == "PHUTRANG_GC"
                               || items.LoaiVT == "PHUKIEN" || items.LoaiVT == "SPMAYKHAC" || items.LoaiVT == "NHUONGQUYEN")
                                    {
                                        StringCustomFieldRef cust_dichdanhLOT = new StringCustomFieldRef();
                                        cust_dichdanhLOT.scriptId = "custitem_btm_mc_dich_danh_lot";
                                        cust_dichdanhLOT.value = "T";
                                        a[5] = cust_dichdanhLOT;
                                    }


                                    item.customFieldList = a;
                                    WriteResponse addmahang = ns.Service.upsert(item);
                                    var messerror = ((ToolsApp.com.netsuite.webservices.Status)addmahang.status).statusDetail;
                                    var messdetail = messerror.FirstOrDefault().message;
                                    var check = ((ToolsApp.com.netsuite.webservices.RecordRef)addmahang.baseRef).internalId;
                                    var status = addmahang.status.isSuccess;
                                    if (status == true)
                                    {
                                        var dmvt = db_.DANHMUCVATTUs.Where(p => p.MAVT == items.MAVT).FirstOrDefault();
                                        dmvt.GuiAPI = "1";
                                        dmvt.HieuLuc = true;
                                        dmvt.NgayGuiAPI = DateTime.Now;
                                        db_.Entry(dmvt).State = EntityState.Modified;
                                        db_.SaveChanges();

                                        if (dmvt.LoaiVT == "QUANAO" || dmvt.LoaiVT == "PHUTRANG" ||  dmvt.LoaiVT == "QUANAO_GC" || dmvt.LoaiVT == "PHUTRANG_GC" || dmvt.LoaiVT == "SPMAYKHAC" || dmvt.LoaiVT == "PHUKIEN" || dmvt.LoaiVT == "NHUONGQUYEN")
                                        {
                                            SPMAY.INSERT_FROM_DANHMUCVATTU_TO_DMMAHANG_SPM2023_HANGMUANGOAI(items.MAVT);
                                        }
                                    }
                                    else
                                    {
                                        GhiLog_DMVattu_ErrorAPI_NS log = new GhiLog_DMVattu_ErrorAPI_NS();
                                        log.HanhDong = "btnDongboMAVT DANHMUC VATTU _ " + items.LoaiVT;
                                        log.id = Guid.NewGuid();
                                        log.Externalid = items.MAVT;
                                        log.Internalid = check;
                                        log.MAVT = items.MAVT;
                                        log.TenVT = items.TenVT;
                                        log.DVT = items.DVT;
                                        log.LogNS = messdetail == null ? "" : messdetail.ToString();
                                        log.MANV = User.UserName;
                                        log.Ngaylog = DateTime.Now;
                                        nstt_.GhiLog_DMVattu_ErrorAPI_NS.Add(log);
                                        nstt_.SaveChanges();
                                    }

                                }
                                return Json(new { status = 1, title = "", text = "Thêm thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
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

        #region Delete 
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _DeleteFun(string MAVT)
        {
            try
            {
                var item = db_.DANHMUCVATTUs.FirstOrDefault(p => p.MAVT == MAVT);
                db_.DANHMUCVATTUs.Remove(item);
                db_.SaveChanges();
                return Json(new { status = 1, title = "", text = "Xóa thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

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

                    var list_DANHMUCVATTU = new List<DANHMUCVATTU>();
                    #region Read
                    for (int i = 2; i <= stats.EndRowIndex; i++)
                    {
                        var MAVT = sl.GetCellValueAsString(i, 1);
                        var TenVT = sl.GetCellValueAsString(i, 2);
                        var LoaiVT = sl.GetCellValueAsString(i, 3).ToUpper().Trim();
                        var Manhom = sl.GetCellValueAsString(i, 4).ToUpper().Trim();
                        var DVT = sl.GetCellValueAsString(i, 5).ToUpper().Trim();
                        var DonViSuDungtac = sl.GetCellValueAsString(i, 6).ToUpper().Trim();

                        var COA = db_.TBL_DMLOAIVT_COA.Where(p => p.LoaiVT == LoaiVT && p.ManhomVT == Manhom).ToList();
                        if (COA.Count < 1)
                        {
                            return Json(new { status = -1, title = "", text = "LOẠI VT và MÃ NHÓM không hợp lệ", obj = "" }, JsonRequestBehavior.AllowGet);
                        }
                        var f_COA = COA.FirstOrDefault();
                        if (!string.IsNullOrEmpty(MAVT))
                        {
                            #region Kiểm tra dữ liệu
                            if (string.IsNullOrEmpty(MAVT))
                            {
                                return Json((new { status = -1, title = "", text = string.Format("Mã vật tư dòng {0} : " + MAVT + " không được bỏ trống.", numRecord), obj = "" }), JsonRequestBehavior.AllowGet);
                            }
          
                            if (string.IsNullOrEmpty(TenVT))
                            {
                                return Json((new { status = -1, title = "", text = string.Format("Tên vật tư dòng {0} : " + TenVT + " không được bỏ trống.", numRecord), obj = "" }), JsonRequestBehavior.AllowGet);
                            }
                            if (string.IsNullOrEmpty(LoaiVT))
                            {
                                return Json((new { status = -1, title = "", text = string.Format("Loại vật tư dòng {0} : " + LoaiVT + " không được bỏ trống.", numRecord), obj = "" }), JsonRequestBehavior.AllowGet);
                            }
                            if (string.IsNullOrEmpty(Manhom))
                            {
                                return Json((new { status = -1, title = "", text = string.Format("Mã nhóm dòng {0} : " + Manhom + " không được bỏ trống.", numRecord), obj = "" }), JsonRequestBehavior.AllowGet);
                            }
                            if (string.IsNullOrEmpty(DVT))
                            {
                                return Json((new { status = -1, title = "", text = string.Format("DVT dòng {0} : " + DVT + " không được bỏ trống.", numRecord), obj = "" }), JsonRequestBehavior.AllowGet);
                            }
                            if (string.IsNullOrEmpty(DonViSuDungtac))
                            {
                                return Json((new { status = -1, title = "", text = string.Format("Đơn vị sử dụng dòng {0} : " + DonViSuDungtac + " không được bỏ trống.", numRecord), obj = "" }), JsonRequestBehavior.AllowGet);
                            }
                            //if (LoaiVT != "NHUONGQUYEN" && LoaiVT != "PHUKIEN")
                            //{
                            //    return Json((new { status = -1, title = "", text = string.Format("Loại VT phải là NHUONGQUYEN hoặc PHUKIEN. Kiểm tra lại", numRecord), obj = "" }), JsonRequestBehavior.AllowGet);
                            //}
                            #endregion
                            #region Kiểm tra trùng trong excel
                            if (list_DANHMUCVATTU.FirstOrDefault(p => (p.MAVT == MAVT)) != null)
                            {
                                return Json((new { status = -1, title = "", text = string.Format(" MAVT dòng {0} : " + MAVT + " trùng trong file. Vui lòng kiểm tra lại.", numRecord), obj = "" }), JsonRequestBehavior.AllowGet);
                            }
                            #endregion
                            #region Kiểm tra cập nhật hay insert

                            if (db_.DANHMUCVATTUs.FirstOrDefault(c => c.MAVT == MAVT) == null)                     
                            {
                                    var model_copy = new DANHMUCVATTU();
                                    {
                                        model_copy.MAVT = MAVT;
                                        model_copy.Manhom = Manhom;
                                        model_copy.TenVT = TenVT;
                                        model_copy.LoaiVT = LoaiVT;
                                        model_copy.DVT = DVT;
                                        model_copy.DonViSuDungtac = DonViSuDungtac;
                                        model_copy.HieuLuc = true;
                                        model_copy.MANVCAPNHAT = User.UserName;
                                        model_copy.NGAYCN = DateTime.Now;
                                        model_copy.MACP = f_COA.MACP;
                                        model_copy.COGS = f_COA.COGS;
                                        model_copy.ASSET = f_COA.ASSET;
                                        model_copy.INCOME = f_COA.INCOME;
                                        model_copy.TK_ChiPhi_SX = f_COA.TK_PhiSX;
                                    };
                                    list_DANHMUCVATTU.Add(model_copy);                                
                            }
                            #endregion
                            isHaveData = true;
                            numRecord++;
                        }
                    }
                    if (isHaveData)
                    {
                        db_.DANHMUCVATTUs.AddRange(list_DANHMUCVATTU);
                        db_.SaveChanges();
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

        #region Xuất excel 

        [HttpPost]
        [ValidateInput(false)]
        public JsonResult _ExportExcelFun(string MAVTSearch = null, string TENVTSearch = null, string LoaiVTSearch = null)
        {
            var models = db_.DANHMUCVATTUs.Where(p =>
                ((MAVTSearch == null || MAVTSearch == "" || p.MAVT.Contains(MAVTSearch)) &&
                (TENVTSearch == null || TENVTSearch == "" || p.TenVT.Contains(TENVTSearch)) &&
                (LoaiVTSearch == null || LoaiVTSearch == "" || p.LoaiVT.Contains(LoaiVTSearch))
                )).OrderByDescending(d => d.NGAYCN).ToList();

            if (models.Count > 0)
            {
                #region xóa file export cũ
                string folderPath = Server.MapPath("~/Userfiles/Download/EXPORT_DANHMUCVATTU.xlsx");
                if (Directory.Exists(folderPath))
                {
                    Directory.Delete(folderPath, true); // Xóa cả thư mục và file bên trong
                }
                #endregion

                //   string fileIn = Request.PhysicalApplicationPath + @"UserFiles\Template\" + "EXPORT_DANHMUCVATTU.xlsx";
                string fileIn = Server.MapPath("~/Userfiles/Template/EXPORT_DANHMUCVATTU.xlsx");

                string fileOutName = "EXPORT_DANHMUCVATTU.xlsx";

                SLDocument sl = new SLDocument(fileIn);

                sl.SelectWorksheet(sl.GetSheetNames()[0]);

                SLWorksheetStatistics stats = sl.GetWorksheetStatistics();

                var row = 2;

                var stt = 1;

                #region style
                SLStyle style = sl.CreateStyle();
                style.Border.TopBorder.BorderStyle = BorderStyleValues.Thin;
                style.Border.BottomBorder.BorderStyle = BorderStyleValues.Thin;
                style.Border.LeftBorder.BorderStyle = BorderStyleValues.Thin;
                style.Border.RightBorder.BorderStyle = BorderStyleValues.Thin;
                style.Border.TopBorder.Color = System.Drawing.Color.Black;
                style.Border.BottomBorder.Color = System.Drawing.Color.Black;
                style.Border.LeftBorder.Color = System.Drawing.Color.Black;
                style.Border.RightBorder.Color = System.Drawing.Color.Black;
                #endregion

                foreach (var item in models)
                {
                    sl.SetCellValue("A" + row.ToString(), stt);
                    sl.SetCellValue("B" + row.ToString(), item.MAVT);
                    sl.SetCellValue("C" + row.ToString(), item.TenVT);
                    sl.SetCellValue("D" + row.ToString(), item.DVT);
                    sl.SetCellValue("E" + row.ToString(), item.LoaiVT);
                    sl.SetCellValue("F" + row.ToString(), item.Manhom);
                    sl.SetCellValue("G" + row.ToString(), item.DonViSuDungtac);
                    sl.SetCellValue("H" + row.ToString(), item.ASSET);
                    sl.SetCellValue("I" + row.ToString(), item.COGS);
                    sl.SetCellValue("J" + row.ToString(), item.INCOME);
                    sl.SetCellValue("K" + row.ToString(), item.TK_ChiPhi_SX);
                    sl.SetCellValue("L" + row.ToString(), (item.HieuLuc == true) ? "1" : "0");
                    sl.SetCellValue("M" + row.ToString(), item.GuiAPI == "1" ? "1" : "0");
                    row++;
                    stt++;
                }

                sl.SetCellStyle("A2", "M" + (row - 1), style);

                sl.SaveAs(Request.PhysicalApplicationPath + @"UserFiles\Download\" + fileOutName);

                return Json((new { status = 1, title = "", text = "Exported.", obj = ToolsApp.Utilities.AppParameters.FolderDownload + fileOutName }), JsonRequestBehavior.DenyGet);
            }
            else
            {
                return Json((new { status = -1, title = "", text = "Nothing to export.", obj = "" }), JsonRequestBehavior.DenyGet);
            }



        }
        #endregion


        #region Insert Image//ok
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _UploadImage(DMVATTUViewModels model)
        {
            try
            {
                if (model.MAVT == null)
                {
                    return Json(new { status = -1, title = "", text = "MAVT không được đẽ trống. Vui lòng kiểm tra lại.", obj = "" }, JsonRequestBehavior.AllowGet);
                }
               
                    #region                    

                    string subPath = "~/materials_images/";

                    bool exists = System.IO.Directory.Exists(Server.MapPath(subPath));

                    if (!exists)
                    {
                        System.IO.Directory.CreateDirectory(Server.MapPath(subPath));
                    }
                    #endregion
                    var uploadPath = Server.MapPath(subPath);
                    #region Hình Ảnh  
                    var files = Request.Files;
                    List<string> imagePaths = new List<string>();
                    for (int i = 0; i < files.Count; i++)
                    {
                        var file = files[i];

                        if (file != null && file.ContentLength > 0 && file.ContentType.StartsWith("image/"))
                        {
                         var fileName = Path.GetFileName(file.FileName);
                         // Lấy tên và phần mở rộng
                         // var name = Path.GetFileNameWithoutExtension(fileName);
                        var name = model.MAVT;
                        var ext = Path.GetExtension(fileName);
                        fileName = $"{name}{ext}";
                        if (!Directory.Exists(uploadPath))
                             Directory.CreateDirectory(uploadPath);

                        var fullPath = Path.Combine(uploadPath, fileName);

                        int count = 1;
                        while (System.IO.File.Exists(fullPath))
                        {
                            fileName = $"{name}_{count}{ext}";
                            fullPath = Path.Combine(uploadPath, fileName);
                            count++;
                        }
                        file.SaveAs(fullPath);
                        imagePaths.Add(fileName);
                        }
                    }

                    model.Images = string.Join(";", imagePaths);
                    var HINHANHs = model.Images;
                    #endregion
                    #region
                    var dmvt = db_.DANHMUCVATTUs.FirstOrDefault(p => p.MAVT == model.MAVT);
                    dmvt.Images_v2 = HINHANHs;
                    db_.Entry(dmvt).State = EntityState.Modified;
                    db_.SaveChanges();
                    #endregion

                return Json(new { status = 1, title = "", text = "Thêm thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion


        private JsonResult _IsDongBo(string MAVT)
        {
            if (!ModelState.IsValid)
                return Json(new { status = -2, text = "Lưu không thành công." }, JsonRequestBehavior.AllowGet);

            try
            {
                var listVT = db_.DANHMUCVATTUs
                                .Where(x => x.MAVT == MAVT)
                                .ToList();

                if (!listVT.Any())
                    return Json(new { status = -1, text = "Không có dữ liệu hoặc đã được đẩy API." }, JsonRequestBehavior.AllowGet);

                // ===== Init NetSuite =====
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback += (a, b, c, d) => true;

                var ns = new NSClient.NSClient();
                NSBase.Client = ns;

                // ===== Cache data (TRÁNH query trong loop) =====
                var loaiSPs = nstt_.LoaiSanPhams.ToList();
                var phanNhomSPs = nstt_.PhanNhomSanPhams.ToList();
                var loaiSPCTs = nstt_.LoaiSanPhamChiTiets.ToList();
                var coaList = nstt_.Chart_of_Accounts.ToList();
                var taxcode = nstt_.Table_TAX_CODE.FirstOrDefault(x => x.HieuLuc == true)?.Internalid_taxcode;

                foreach (var items in listVT)
                {
                    try
                    {
                       

                        var LOAIVTs = db_.DANHMUCVATTUs.Where(p => p.MAVT == items.MAVT).FirstOrDefault().LoaiVT;
                        var LoaiSanPhams = nstt_.LoaiSanPhams.ToList();
                        var PhanNhomSanPhams = nstt_.PhanNhomSanPhams.ToList();
                        var LoaiSanPhamChiTiets = nstt_.LoaiSanPhamChiTiets.ToList();
                        var item_PhanNhomSanPhams = PhanNhomSanPhams.Where(p => p.LoaiVT_TT == LOAIVTs.ToString()).ToList();
                        if (LOAIVTs == "PT-VT")
                        {
                            item_PhanNhomSanPhams = PhanNhomSanPhams.Where(p => p.LoaiVT_TT == LOAIVTs.ToString() && items.MAVT[0].ToString() == p.KyTu).ToList();
                        }
                        var item_LoaiSanPhams = LoaiSanPhams.Where(p => p.externalid == (item_PhanNhomSanPhams.Count < 1 ? "VATTU"
                                                              : item_PhanNhomSanPhams.FirstOrDefault().MaLoaiSP_TT))
                                                              .FirstOrDefault().externalid;
                        if (items.LoaiVT == "PTMUANGOAI")
                        {
                            item_LoaiSanPhams = LoaiSanPhams.Where(p => p.externalid == "MAY").FirstOrDefault().externalid;
                        }
                        if (items.LoaiVT == "VAI" || items.LoaiVT == "VAI_TT_DI_GC")
                        {
                            item_LoaiSanPhams = LoaiSanPhams.Where(p => p.externalid == "VAITHANHPHAM").FirstOrDefault().externalid;
                        }

                        var item_LoaiSanPhamChiTiets = LoaiSanPhamChiTiets.Where(p => p.Kytu1 == items.MAVT[0].ToString()
                                                                              && p.MaLoaiSPCT == items.MAVT[1].ToString()).ToList();
                        var dvt = db_.VATTU2024_SpLoad_ConvertDVT(items.MAVT).FirstOrDefault().DVT_Convert;
                        var exiddvt = nstt_.Table_Mapping_Mavattu_DVT.Where(c => c.DVT == dvt).FirstOrDefault().Internalid;
                        dynamic item = "";
                        // dynamic int_form = "67"; // customform - 67 = TT vật tư / inventory
                        var int_form = db_.TBL_DMLOAIVT_COA.FirstOrDefault(p => p.LoaiVT == items.LoaiVT).CustomForm;

                        if (items.LoaiVT == "MOC" || items.LoaiVT == "VAI" || items.LoaiVT == "QUANAO"
                            || items.LoaiVT == "QUANAO_GC"  || items.LoaiVT == "VAI_TT_DI_GC" 
                            || items.LoaiVT == "MOC_TT_DI_GC"
                            || items.LoaiVT == "PHUTRANG"  
                            || items.LoaiVT == "PHUTRANG_GC"  || items.LoaiVT == "PHUKIEN" 
                            || items.LoaiVT == "SPMAYKHAC" || items.LoaiVT == "NHUONGQUYEN")
                        {
                            item = new LotNumberedInventoryItem();
                        }
                        else
                        {
                            item = new InventoryItem();
                        }


                    
                        var LOAISPCT =  nstt_.VATTU2024_LOAD_LOAISPCHITIET(items.MAVT).ToList();
                        var f_LOAISPCT = LOAISPCT.Count < 1 ? "" : LOAISPCT.FirstOrDefault().externalid;

                        //   InventoryItem itemsa = new InventoryItem();

                        RecordRef form = new RecordRef();
                        form.internalId = int_form;
                        item.customForm = form;
                        item.pricesIncludeTax = true;
                        item.pricesIncludeTaxSpecified = true;

                        RecordRef purchaseTaxCode = new RecordRef();
                        purchaseTaxCode.internalId = taxcode;
                        item.purchaseTaxCode = purchaseTaxCode;

                        RecordRef salesTax = new RecordRef();
                        salesTax.internalId = taxcode;
                        item.salesTaxCode = salesTax;


                        RecordRef unittype = new RecordRef();
                        unittype.internalId = exiddvt;
                        item.unitsType = unittype;


                        item.externalId = items.MAVT;
                        item.itemId = items.MAVT;
                        item.upcCode = items.MAVT;
                        item.displayName = items.TenVT == null ? "" : items.TenVT;
                        item.salesDescription = (items.TenVT == null ? "" : (items.MAVT + "_" + items.TenVT));

                        var f_coa_mavt = db_.DANHMUCVATTUs.FirstOrDefault(p => p.MAVT == items.MAVT);
                        var tt_asset = f_coa_mavt.ASSET;
                        var tt_cogs = f_coa_mavt.COGS;
                        var tt_income = f_coa_mavt.INCOME;
                        var COA = nstt_.Chart_of_Accounts.ToList();
                        var intid_asset = tt_asset == "" ? "" : COA.FirstOrDefault(p => p.Number_TT == tt_asset).InternalID_NS;
                        var intid_cogs = tt_cogs == "" ? "" : COA.FirstOrDefault(p => p.Number_TT == tt_cogs).InternalID_NS;
                        var intid_income = tt_income == "" ? "" : COA.FirstOrDefault(p => p.Number_TT == tt_income).InternalID_NS;

                        RecordRef asset = new RecordRef();
                        asset.internalId = intid_asset;
                        item.assetAccount = asset;

                        RecordRef cogs = new RecordRef();
                        cogs.internalId = intid_cogs;
                        item.cogsAccount = cogs;

                        RecordRef income = new RecordRef();
                        income.internalId = intid_income;
                        item.incomeAccount = income;

                        CustomFieldRef[] a = new CustomFieldRef[99];

                        SelectCustomFieldRef cust_loaisp = new SelectCustomFieldRef();
                        ListOrRecordRef List_loaisp = new ListOrRecordRef();
                        List_loaisp.externalId = item_LoaiSanPhams;
                        cust_loaisp.scriptId = "custitem_btm_tt_loai_san_pham";
                        cust_loaisp.value = List_loaisp;
                        a[1] = cust_loaisp;

                        SelectCustomFieldRef cust_phannhomsp = new SelectCustomFieldRef();
                        ListOrRecordRef List_phannhomsp = new ListOrRecordRef();
                        List_phannhomsp.externalId = item_PhanNhomSanPhams.Count < 1 ? "" : item_PhanNhomSanPhams.FirstOrDefault().externalid;
                        cust_phannhomsp.scriptId = "custitem_btm_tt_nhom_sp_may";
                        cust_phannhomsp.value = List_phannhomsp;
                        a[2] = cust_phannhomsp;

                        var LIST_WIPSTATUS = db_.TBL_DMLOAIVT_COA.Where(p => p.LoaiVT == LOAIVTs).ToList();
                        var WIPSTATUS = LIST_WIPSTATUS.Count < 1 ? "2" : LIST_WIPSTATUS.FirstOrDefault().WIPSTATUS == "WIP" ? "1" : "2";

                        SelectCustomFieldRef cust_statuswip = new SelectCustomFieldRef();
                        ListOrRecordRef List_statuswip = new ListOrRecordRef();
                        List_statuswip.internalId = WIPSTATUS;
                        cust_statuswip.scriptId = "custitem_btm_mc_wip_status";
                        cust_statuswip.value = List_statuswip;
                        a[3] = cust_statuswip;

                        SelectCustomFieldRef cust_phamvi = new SelectCustomFieldRef();
                        ListOrRecordRef List_phamvi = new ListOrRecordRef();
                        List_phamvi.internalId = "2";
                        cust_phamvi.scriptId = "cseg_btm_tt_pv_bh";
                        cust_phamvi.value = List_phamvi;
                        a[4] = cust_phamvi;

                        if (items.LoaiVT == "MOC" || items.LoaiVT == "VAI" || items.LoaiVT == "QUANAO" || items.LoaiVT == "VAI_TT_DI_GC" || items.LoaiVT == "MOC_TT_DI_GC"
                        || items.LoaiVT == "PHUTRANG" || items.LoaiVT == "QUANAO_GC" || items.LoaiVT == "PHUTRANG_GC"
                        || items.LoaiVT == "PHUKIEN" || items.LoaiVT == "SPMAYKHAC" || items.LoaiVT == "NHUONGQUYEN")
                        {
                            StringCustomFieldRef cust_dichdanhLOT = new StringCustomFieldRef();
                            cust_dichdanhLOT.scriptId = "custitem_btm_mc_dich_danh_lot";
                            cust_dichdanhLOT.value = "T";
                            a[5] = cust_dichdanhLOT;
                        }

                        item.customFieldList = a;
                        WriteResponse addmahang = ns.Service.upsert(item);
                        var messerror = ((ToolsApp.com.netsuite.webservices.Status)addmahang.status).statusDetail;
                        var messdetail = messerror.FirstOrDefault().message;
                        var check = ((ToolsApp.com.netsuite.webservices.RecordRef)addmahang.baseRef).internalId;
                        var status = addmahang.status.isSuccess;
                        if (status == true)
                        {

                            var dmvt = db_.DANHMUCVATTUs.Where(p => p.MAVT == items.MAVT).FirstOrDefault();
                            dmvt.GuiAPI = "1";
                            dmvt.HieuLuc = true;
                            dmvt.NgayGuiAPI = DateTime.Now;
                            db_.Entry(dmvt).State = EntityState.Modified;
                            db_.SaveChanges();
                            if (dmvt.LoaiVT == "QUANAO" || dmvt.LoaiVT == "PHUTRANG" || dmvt.LoaiVT == "QUANAO_GC" || dmvt.LoaiVT == "PHUTRANG_GC" ||  dmvt.LoaiVT == "SPMAYKHAC" || dmvt.LoaiVT == "PHUKIEN" || dmvt.LoaiVT == "NHUONGQUYEN")
                            {
                                SPMAY.INSERT_FROM_DANHMUCVATTU_TO_DMMAHANG_SPM2023_HANGMUANGOAI(items.MAVT);
                            }
                        }
                        else
                        {
                            GhiLog_DMVattu_ErrorAPI_NS log = new GhiLog_DMVattu_ErrorAPI_NS();
                            log.HanhDong = "btnDongboMAVT DANHMUC VATTU _ " + items.LoaiVT;
                            log.id = Guid.NewGuid();
                            log.Externalid = items.MAVT;
                            log.Internalid = check;
                            log.MAVT = items.MAVT;
                            log.TenVT = items.TenVT;
                            log.DVT = items.DVT;
                            log.LogNS = messdetail == null ? "" : messdetail.ToString();
                            log.MANV = User.UserName;
                            log.Ngaylog = DateTime.Now;
                            nstt_.GhiLog_DMVattu_ErrorAPI_NS.Add(log);
                            nstt_.SaveChanges();

                            return Json(new { status = -1, text = "Thêm không thành công" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    catch (Exception)
                    {
                        // log từng item để không crash cả batch
                        continue;
                    }
                }

                return Json(new { status = 1, text = "Đồng bộ thành công" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { status = 0, text = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }



    }
}