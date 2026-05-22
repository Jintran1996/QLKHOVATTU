
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

using System.Net;
using NSClient;
using ToolsApp.com.netsuite.webservices;
using ToolsApp.Models;
using System.Data.Entity;

namespace ToolsApp.Controllers
{
    [Authorize]
    public class DMSOIController : BaseController
    {
        private wqlvattuEntities db_ = new wqlvattuEntities();
        private wqlkhosoiEntities soi_ = new wqlkhosoiEntities();
        private NetsuiteTTEntities1 nstt_ = new NetsuiteTTEntities1();

        #region Index
        public ActionResult Index()
        {
            ViewBag.manv = User.UserName;

            return View();
        }
        #endregion

        #region _GetList
        public ActionResult _GetList(string MAVTSearch = null)
        {

            var model = soi_.TBLDMNGUYENLIEUx.Where(p =>
                ((MAVTSearch == null || MAVTSearch == "" || p.MAVATTU.Contains(MAVTSearch)))).OrderByDescending(d => d.NGAYCN).ToList();
            ViewBag.List = model;

            return PartialView();
        }
        #endregion

        #region form insert
        public ActionResult _DetailForEdit(string MAVT = null)
        {

            ViewBag.dmnhomngl = soi_.SP_NL_Load_NL_DMNHOMNGLIEU().ToList();
            ViewBag.nhomngl = soi_.NL_DMNHOMNGLIEU.ToList();
            //ViewBag.dmdonvi = db_.DMDONVIs.ToList();
            //ViewBag.dmloaivt = db_.DanhmucLoaiVTs.ToList(); 
            //ViewBag.maChiPhi = db_.SP_LOAD_MACP_MAVATTU("").ToList();

            if (MAVT == null || MAVT == string.Empty)
            {
                return PartialView("_Insert", new DMSOIViewModels { MAVATTU = MAVT });
            }
            else
            {
                var model = db_.DANHMUCVATTUs.FirstOrDefault(c => c.MAVT == MAVT);
                ViewBag.data = model;
                return PartialView("_Update", new DMSOIViewModels { MAVATTU = model.MAVT });
            }

        }
        #endregion

        #region Thêm dm mã hàng + chi tiết

        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _InsertFun(DMSOIViewModels model)
        {
            try
            {
                if (soi_.TBLDMNGUYENLIEUx.FirstOrDefault(c => c.MAVATTU == model.MAVATTU) != null)
                {
                    return Json(new { status = -2, title = "", text = "Thêm không thành công. MAVATTU đã tồn tại. Kiểm tra lại.", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                //if (model.MACP == null)
                //{
                //    return Json(new { status = -1, title = "", text = "Mã chi phí không được để trống.", obj = "" }, JsonRequestBehavior.AllowGet);
                //}
                if (model.MAVATTU == null)
                {
                    return Json(new { status = -1, title = "", text = "Mã vật tư không được để trống.", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                var MAID = soi_.TBLDMNGUYENLIEUx.OrderByDescending(P => P.MAID).FirstOrDefault().MAID;
                var COA = db_.TBL_DMLOAIVT_COA.Where(p => p.LoaiVT == "NLCHINH" && p.ManhomVT == "SOI").ToList();
                var f_COA = COA.FirstOrDefault();

                var model_copy = new TBLDMNGUYENLIEU();
                model_copy.MAID = MAID + 1;
                model_copy.MAVATTU = model.MAVATTU;
                model_copy.Tenvattu = model.Tenvattu;
                model_copy.CS1 = model.CS1;
                model_copy.CS2 = model.CS2;

                model_copy.DVT = "KG";
                model_copy.Ghichu = model.Ghichu;
                model_copy.isHieuluc = true;
                model_copy.MaNGL = model.MaNGL;
                model_copy.MANGL1 = model.MANGL1;
                model_copy.MANGL2 = model.MANGL2;
                model_copy.MANGL3 = model.MANGL3;
                model_copy.MANGL4 = model.MANGL4;
                model_copy.MANGL5 = model.MANGL5;
                model_copy.Tile1 = (model.Tile1 == null) ? (Decimal)0.00 : model.Tile1;
                model_copy.Tile2 = (model.Tile2 == null) ? (Decimal)0.00 : model.Tile2;
                model_copy.Tile3 = (model.Tile3 == null) ? (Decimal)0.00 : model.Tile3;
                model_copy.Tile4 = (model.Tile4 == null) ? (Decimal)0.00 : model.Tile4;
                model_copy.Tile5 = (model.Tile5 == null) ? (Decimal)0.00 : model.Tile5;
                model_copy.NGAYCN = DateTime.Now;
                model_copy.MANVCN = User.UserName;
                model_copy.COGS = f_COA.COGS;
                model_copy.ASSET = model.LoaiNguyenLieu == "MUANGOAI" ? f_COA.ASSET : "15517"; //15517 ACCOUNT ASSET MÃ SỢI THÀNH PHẨM
                model_copy.INCOME = f_COA.INCOME;
                model_copy.MACP = f_COA.TK_PhiSX;
                model_copy.LoaiSoi = model.LoaiNguyenLieu;
                soi_.TBLDMNGUYENLIEUx.Add(model_copy);

                soi_.SaveChanges();
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

        #region Onchange
        public JsonResult _btnTaoMaSoi(string MaNGL, string Chisosoi, string SoFilament, string MAVATTU)
        {
            try
            {
                if (MaNGL == null || MaNGL == "")
                {
                    return Json(new { status = -1, title = "", text = MaNGL + " không được trống", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                if ((Chisosoi == null || Chisosoi == "") && Chisosoi.Length != 3)
                {
                    return Json(new { status = -1, title = "", text = Chisosoi + " phải đủ 3 ký tự", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                if ((SoFilament == null || SoFilament == "") && SoFilament.Length != 3)
                {
                    return Json(new { status = -1, title = "", text = SoFilament + " phải đủ 3 ký tự", obj = "" }, JsonRequestBehavior.AllowGet);
                }

                var taoma = soi_.sp_Nl_TaoMaVT(MaNGL, Chisosoi, SoFilament).FirstOrDefault();
                var stt = taoma.Sothutu;
                var masoi = taoma.mavt;

                return Json(new { status = 1, title = "", text = "", obj = stt, obj1 = masoi }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { status = -1, title = "", text = e.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        //#region Insert all VATTU

        //[ValidateInput(false)]
        //[HttpPost]
        //public JsonResult _UpsertMAVTNS()
        //{
        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            var mavt = db_.SP_GUIAPI_Load_DMVATTU("").ToList();

        //            if (mavt.Count > 0)
        //            {
        //                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

        //                // In order to enable SOAPscope to work through SSL. Refer to FAQ for more details
        //                ServicePointManager.ServerCertificateValidationCallback += delegate (object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        //                {
        //                    return true;
        //                };

        //                NSClient.NSClient ns = null;
        //                try
        //                {
        //                    ns = new NSClient.NSClient();
        //                    NSBase.Client = ns;
        //                }
        //                catch (Exception ex)
        //                {
        //                    Console.WriteLine("Error while loading the application:" + ex.Message);
        //                    Console.WriteLine("Press Enter to quit ... ");
        //                    Console.ReadKey();

        //                }
        //                if (ns != null)
        //                {
        //                    try
        //                    {

        //                        foreach (var items in mavt)
        //                        {
        //                            var LoaiSanPhams = nstt_.LoaiSanPhams.ToList();
        //                            var PhanNhomSanPhams = nstt_.PhanNhomSanPhams.ToList();
        //                            var LoaiSanPhamChiTiets = nstt_.LoaiSanPhamChiTiets.ToList();

        //                            var item_LoaiSanPhams = LoaiSanPhams.Where(p => p.externalid == "VATTU").FirstOrDefault().externalid;
        //                            var item_PhanNhomSanPhams = PhanNhomSanPhams.Where(p => p.KyTu == items.MAVT[0].ToString()).ToList();
        //                            var item_LoaiSanPhamChiTiets = LoaiSanPhamChiTiets.Where(p => p.Kytu1 == items.MAVT[0].ToString() 
        //                                                                                  && p.MaLoaiSPCT == items.MAVT[1].ToString()).ToList();

        //                            var exiddvt = nstt_.Table_Mapping_Mavattu_DVT.Where(c => c.DVT == items.DVT).FirstOrDefault().Internalid;
        //                            dynamic item = "";
        //                            dynamic int_form = "123";

        //                           switch(items.LOAIVT)
        //                            {
        //                                case "SOI":
        //                                    switch (items.Soi_GC)
        //                                    {
        //                                        case "0":
        //                                            item = new LotNumberedInventoryItem();
        //                                            break;
        //                                        default:
        //                                            item = new LotNumberedAssemblyItem();
        //                                            int_form = "164";
        //                                            break;
        //                                    }
        //                                    // item = new LotNumberedInventoryItem();
        //                                    break;
        //                                default:
        //                                    item = new InventoryItem();
        //                                    break;
        //                            }    
        //                            //InventoryItem item = new InventoryItem();

        //                            RecordRef form = new RecordRef();
        //                            form.internalId = int_form;
        //                            item.customForm = form;

        //                            RecordRef salesTax = new RecordRef();
        //                            salesTax.internalId = "08";
        //                            item.salesTaxCode = salesTax;

        //                            RecordRef unittype = new RecordRef();
        //                            unittype.internalId = exiddvt;
        //                            item.unitsType = unittype;


        //                            item.externalId = items.MAVT;
        //                            item.itemId = items.MAVT;
        //                            item.displayName = items.TenVT == null ? "" : items.TenVT;



        //                            RecordRef itemRef = new RecordRef();
        //                            itemRef.internalId = "309";                                 
        //                            item.assetAccount = itemRef;

        //                            CustomFieldRef[] a = new CustomFieldRef[99];

        //                            SelectCustomFieldRef cust_loaisp = new SelectCustomFieldRef();
        //                            ListOrRecordRef List_loaisp = new ListOrRecordRef();
        //                            List_loaisp.externalId = item_LoaiSanPhams;
        //                            cust_loaisp.scriptId = "custitem_btm_tt_loai_san_pham";
        //                            cust_loaisp.value = List_loaisp;
        //                            a[1] = cust_loaisp;

        //                            SelectCustomFieldRef cust_phannhomsp = new SelectCustomFieldRef();
        //                            ListOrRecordRef List_phannhomsp = new ListOrRecordRef();
        //                            List_phannhomsp.externalId = item_PhanNhomSanPhams.Count < 1 ? "" : item_PhanNhomSanPhams.FirstOrDefault().externalid;
        //                            cust_phannhomsp.scriptId = "custitem_btm_tt_nhom_sp_may";
        //                            cust_phannhomsp.value = List_phannhomsp;
        //                            a[2] = cust_phannhomsp;

        //                            SelectCustomFieldRef cust_ctloaisp = new SelectCustomFieldRef();
        //                            ListOrRecordRef List_ctloaisp = new ListOrRecordRef();
        //                            List_ctloaisp.externalId = item_LoaiSanPhamChiTiets.Count < 1 ? "" : item_LoaiSanPhamChiTiets.FirstOrDefault().externalid;
        //                            cust_ctloaisp.scriptId = "custitem_btm_tt_loai_sp_chi_tiet";
        //                            cust_ctloaisp.value = List_ctloaisp;
        //                            if(int_form != "164")
        //                            {
        //                             a[3] = cust_ctloaisp;
        //                            }    


        //                            item.customFieldList = a;

        //                             WriteResponse addmahang = ns.Service.upsert(item);

        //                            var messerror = ((ToolsApp.com.netsuite.webservices.Status)addmahang.status).statusDetail;
        //                            var messdetail = messerror.FirstOrDefault().message;
        //                            var check = ((ToolsApp.com.netsuite.webservices.RecordRef)addmahang.baseRef).internalId;
        //                            var status = addmahang.status.isSuccess;
        //                            if (status == true)
        //                            {


        //                                if (items.LOAIVT == "SOI")
        //                                {
        //                                    var dmvt = soi_.SP_UPDATE_GUIAPI_TBLDMNGUYENLIEU(items.MAVT);
        //                                }
        //                                else
        //                                {
        //                                    var dmvt = db_.DANHMUCVATTUs.Where(p => p.MAVT == items.MAVT).FirstOrDefault();

        //                                    dmvt.GuiAPI = "1";
        //                                    dmvt.NgayGuiAPI = DateTime.Now;
        //                                    db_.Entry(dmvt).State = EntityState.Modified;
        //                                    db_.SaveChanges();

        //                                }    


        //                            }

        //                            GhiLog_DMVattu_ErrorAPI_NS log = new GhiLog_DMVattu_ErrorAPI_NS();
        //                            log.HanhDong = "btnDongboMAVT DANHMUC VATTU _ " + items.LOAIVT;
        //                            log.id = Guid.NewGuid();
        //                            log.Externalid = items.MAVT;
        //                            log.Internalid = check;
        //                            log.MAVT = items.MAVT;
        //                            log.TenVT = items.TenVT;
        //                            log.DVT = items.DVT;
        //                            log.LogNS = messdetail == null ? "" : messdetail.ToString();
        //                            log.MANV = User.UserName;
        //                            log.Ngaylog = DateTime.Now;
        //                            nstt_.GhiLog_DMVattu_ErrorAPI_NS.Add(log);
        //                            nstt_.SaveChanges();


        //                        }
        //                        return Json(new { status = 1, title = "", text = "Thêm thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        return Json(new { status = 0, title = "Error", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                return Json(new { status = -1, title = "", text = "Không có dữ liệu hoặc đã được đẩy API.", obj = "" }, JsonRequestBehavior.AllowGet);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
        //        }

        //        return Json(new { status = -2, title = "", text = "Thêm không thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
        //    }
        //    else
        //    {
        //        return Json(new { status = -2, title = "", text = "Lưu không thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
        //    }
        //}
        //#endregion

        #region đồng bộ

        [ValidateInput(false)]
        [HttpPost]
        public JsonResult _btnDongBo(string MAVT)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var mavt = soi_.TBLDMNGUYENLIEUx.Where(p => p.MAVATTU == MAVT).ToList();

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
                                    var LoaiSanPhams = nstt_.LoaiSanPhams.ToList();
                                    var PhanNhomSanPhams = nstt_.PhanNhomSanPhams.ToList();
                                    var LoaiSanPhamChiTiets = nstt_.LoaiSanPhamChiTiets.ToList();

                                    var item_LoaiSanPhams = LoaiSanPhams.Where(p => p.externalid == "VATTU").FirstOrDefault().externalid;
                                    var item_PhanNhomSanPhams = PhanNhomSanPhams.Where(p => p.KyTu == items.MAVATTU[0].ToString()).ToList();
                                    var item_LoaiSanPhamChiTiets = LoaiSanPhamChiTiets.Where(p => p.Kytu1 == items.MAVATTU[0].ToString()
                                                                                          && p.MaLoaiSPCT == items.MAVATTU[1].ToString()).ToList();

                                    var exiddvt = nstt_.Table_Mapping_Mavattu_DVT.Where(c => c.DVT == items.DVT).FirstOrDefault().Internalid;
                                    dynamic item = "";
                                    dynamic int_form = "67";
                                    var SOIGC = items.LoaiSoi;
                                    var f_mavt = mavt.FirstOrDefault();

                                    switch (SOIGC)
                                    {
                                        case "MUANGOAI":
                                            item = new LotNumberedInventoryItem();
                                            int_form = "67";
                                            break;
                                        default:
                                            item = new LotNumberedAssemblyItem();
                                            int_form = "44";
                                            break;
                                    }
                                    // item = new LotNumberedInventoryItem();

                                    // InventoryItem item = new InventoryItem();
                                    var tt_asset = items.ASSET;
                                    var tt_cogs = items.COGS;
                                    var tt_income = items.INCOME;
                                    var COA = nstt_.Chart_of_Accounts.ToList();
                                    var intid_asset = COA.FirstOrDefault(p => p.Number_TT == tt_asset).InternalID_NS;
                                    var intid_cogs = COA.FirstOrDefault(p => p.Number_TT == tt_cogs).InternalID_NS;
                                    var intid_income = COA.FirstOrDefault(p => p.Number_TT == tt_income).InternalID_NS;
                                    var taxcode = nstt_.Table_TAX_CODE.Where(p => p.HieuLuc == true).FirstOrDefault().Internalid_taxcode;



                                    RecordRef form = new RecordRef();
                                    form.internalId = int_form;
                                    item.customForm = form;
                                    item.pricesIncludeTax = true;
                                    item.pricesIncludeTaxSpecified = true;

                                    RecordRef salesTax = new RecordRef();
                                    salesTax.internalId = taxcode;
                                    item.salesTaxCode = salesTax;

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


                                    item.externalId = items.MAVATTU;
                                    item.itemId = items.MAVATTU;
                                    item.displayName = items.Tenvattu == null ? "" : items.Tenvattu;


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

                                    StringCustomFieldRef chisosoi = new StringCustomFieldRef();
                                    chisosoi.scriptId = "custitem_btm_tt_chi_so_soi";
                                    chisosoi.value = items.CS1;
                                    StringCustomFieldRef Filament = new StringCustomFieldRef();
                                    Filament.scriptId = "custitem_btm_tt_chi_so_filament";
                                    Filament.value = items.CS2;
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
                                    Pricing[] pricing = new Pricing[6];
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

                                    item.customFieldList = a;

                                    WriteResponse addmahang = ns.Service.upsert(item);

                                    var messerror = ((ToolsApp.com.netsuite.webservices.Status)addmahang.status).statusDetail;
                                    var messdetail = messerror.FirstOrDefault().message;
                                    var check = ((ToolsApp.com.netsuite.webservices.RecordRef)addmahang.baseRef).internalId;
                                    var status = addmahang.status.isSuccess;
                                    if (status == true)
                                    {
                                        var dmvt = soi_.TBLDMNGUYENLIEUx.Where(p => p.MAVATTU == items.MAVATTU).FirstOrDefault();

                                        dmvt.GuiAPI = "1";
                                        dmvt.NgayGuiAPI = DateTime.Now;
                                        soi_.Entry(dmvt).State = EntityState.Modified;
                                        soi_.SaveChanges();

                                        return Json(new { status = 1, title = "", text = "Thêm thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
                                    }
                                    else
                                    {
                                        GhiLog_DMVattu_ErrorAPI_NS log = new GhiLog_DMVattu_ErrorAPI_NS();
                                        log.HanhDong = "btnDongboMAVT DANHMUC VATTU _ " + "SOI";
                                        log.id = Guid.NewGuid();
                                        log.Externalid = items.MAVATTU;
                                        log.Internalid = check;
                                        log.MAVT = items.MAVATTU;
                                        log.TenVT = items.Tenvattu;
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
                var item = soi_.TBLDMNGUYENLIEUx.FirstOrDefault(p => p.MAVATTU == MAVT);
                soi_.TBLDMNGUYENLIEUx.Remove(item);
                soi_.SaveChanges();
                return Json(new { status = 1, title = "", text = "Xóa thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion


    }
}