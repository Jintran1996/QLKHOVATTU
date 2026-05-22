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
using ToolsApp.EntityFramework.KhoTP2023;
using ToolsApp.Utilities;
using System.Data.Entity;
using SpreadsheetLight;
using DocumentFormat.OpenXml.Spreadsheet;

namespace ToolsApp.Controllers
{
    [Authorize]
    public class StartUpdateRateCurrencyController : BaseController
    {
        private wqlvattuEntities vt_ = new wqlvattuEntities();
        private QUANLYKHOTP2023Entities tp_ = new QUANLYKHOTP2023Entities();
        private NetsuiteTTEntities1 ns_ = new NetsuiteTTEntities1();
        // GET: XemXetAllVatTu
        public ActionResult Index()
        {
            return View();
        }



        #region Xuất excel IAA_ID

        [HttpPost]
        [ValidateInput(false)]
        public JsonResult _UPDATE_FUN(string KEY)
        {
            StartUpdateRateCurrency(KEY);
            return Json(new { status = 1, text = "", obj = "" }, JsonRequestBehavior.AllowGet);
        }
        #endregion


        private void StartUpdateRateCurrency(string KEY)
        {
            #region Netsuite
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

            if (KEY == "VATTU")
            {
                if (ns != null)
                {
                    using (wqlvattuEntities db_ = new wqlvattuEntities())
                    {
                        var data = (from t in db_.DM_XUATNHAP
                                    join b in db_.XUATNHAPs on t.SOCTXN equals b.SoCTXN
                                    where t.LOAIXN == "N"
                                       //  && (b.RatePO == null || b.RatePO == 0)
                                       && (b.DVT_TIEN == null || b.DVT_TIEN == "")
                                       && (t.InternalidPO != "NOT_FOUND" || t.InternalidPO == null)
                                       && (t.SOPO != null)
                                    group new { t, b } by t.SOCTXN into g
                                    select new
                                    {
                                        SOCTXN = g.Key,
                                        LOAIXN = g.FirstOrDefault().t.LOAIXN,
                                        SOPO = g.FirstOrDefault().t.SOPO,
                                        NgayCT = g.FirstOrDefault().t.NGAY,
                                    }).OrderByDescending(c => c.NgayCT)
                                    .ToList();
                        if (data.Any())
                        {
                            foreach (var tblPhieuNhap in data)
                            {
                                var tranId = tblPhieuNhap.SOPO.Trim();
                                bool checkOK = false;
                                string maphieuInternalID = "";

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
                                    //if(result.recordList[0].)

                                    var po = (PurchaseOrder)result.recordList[0];

                                    if (po.approvalStatus.name == "Approved")
                                    {
                                        InitializeRef initializeRef = new InitializeRef()
                                        {
                                            internalId = po.internalId,
                                            type = InitializeRefType.purchaseOrder,
                                            typeSpecified = true,
                                        };




                                        //InitializeRecord initializeRecord = new InitializeRecord() { reference = initializeRef, type = InitializeType.itemReceipt };
                                        //ReadResponse read = ns.Service.initialize(initializeRecord);
                                        //if (read.status.isSuccess)
                                        //{
                                        //    ItemReceipt ItemReceipt = (ItemReceipt)read.record;

                                        maphieuInternalID = po.internalId;
                                        var Currency = po.currency.name;
                                        RecordRef purchaseOrderRef = new RecordRef();
                                        purchaseOrderRef.internalId = maphieuInternalID;
                                        purchaseOrderRef.type = RecordType.purchaseOrder;
                                        purchaseOrderRef.typeSpecified = true;

                                        // Lấy thông tin chi tiết của đơn đặt hàng mua hàng
                                        ReadResponse purchaseOrderRecords = ns.Service.get(purchaseOrderRef);


                                        if (!purchaseOrderRecords.status.isSuccess)
                                        {
                                        }
                                        else
                                        {

                                            PurchaseOrder record = (PurchaseOrder)purchaseOrderRecords.record;

                                            PurchaseOrderItemList ItemReceipt = (PurchaseOrderItemList)record.itemList;// list field trong 1 record

                                            var ctPhieu = db_.XUATNHAPs.Where(p => p.SoCTXN == tblPhieuNhap.SOCTXN).ToList();
                                            for (int v = 0; v < ItemReceipt.item.Length; v++)
                                            {
                                                var POname = ItemReceipt.item[v].item.name;
                                                //  var currency = ItemReceipt.item[v].currency;
                                                var rate = ItemReceipt.item[v].rate;
                                                var ctphieulist = ctPhieu.Where(p => p.MAVT == POname).ToList();

                                                var ctPhieuMH = db_.XUATNHAPs.Where(p => p.MAVT == POname && p.SoCTXN == tblPhieuNhap.SOCTXN).ToList();
                                                // Cập nhật giá trị mới
                                                ctPhieuMH.ForEach(p =>
                                                {
                                                    p.DVT_TIEN = Currency;
                                                    p.RatePO = Convert.ToDecimal(rate);
                                                });
                                                db_.SaveChanges();
                                                checkOK = true;

                                            }
                                        }

                                    }
                                    else
                                    {
                                        var phieu = db_.DM_XUATNHAP.FirstOrDefault(p => p.SOCTXN == tblPhieuNhap.SOCTXN);
                                        if (phieu != null)
                                        {
                                            phieu.InternalidPO = "NOT_FOUND";
                                            db_.SaveChanges();

                                            checkOK = false;
                                        }
                                    }
                                }
                                else
                                {
                                    var phieu = db_.DM_XUATNHAP.FirstOrDefault(p => p.SOCTXN == tblPhieuNhap.SOCTXN);
                                    if (phieu != null)
                                    {
                                        phieu.InternalidPO = "NOT_FOUND";
                                        db_.SaveChanges();

                                        checkOK = false;
                                    }
                                }

                                if (checkOK)
                                {
                                    var phieuNhap = db_.DM_XUATNHAP.FirstOrDefault(p => p.SOCTXN == tblPhieuNhap.SOCTXN);
                                    if (phieuNhap != null)
                                    {
                                        phieuNhap.InternalidPO = maphieuInternalID;
                                        db_.SaveChanges();
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (KEY == "SOI")
            {
                if (ns != null)
                {
                    using (wqlkhosoiEntities db_ = new wqlkhosoiEntities())
                    {
                        var data = (from t in db_.NL_DMPHIEUXN
                                    join b in db_.NL_CTXUATNHAP on t.SOCTXN equals b.SOCTXN
                                    where t.MAHTHUC == "NK"
                                     //  && (b.RatePO == null || b.RatePO == 0)
                                       && (b.DVT_TIEN == null || b.DVT_TIEN == "")
                                      // && (t.InternalidPO != "NOT_FOUND" || t.InternalidPO == null)
                                       && (t.SOPO != null)
                                    group new { t, b } by t.SOCTXN into g
                                    select new
                                    {
                                        SOCTXN = g.Key,
                                        LOAIXN = g.FirstOrDefault().t.MAHTHUC,
                                        SOPO = g.FirstOrDefault().t.SOPO,
                                        NgayCT = g.FirstOrDefault().t.NGAY,
                                    }).OrderByDescending(c => c.NgayCT)
                                    .ToList();
                        if (data.Any())
                        {
                            foreach (var tblPhieuNhap in data)
                            {
                                var tranId = tblPhieuNhap.SOPO.Trim();
                                bool checkOK = false;
                                string maphieuInternalID = "";
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
                                    //if(result.recordList[0].)

                                    var po = (PurchaseOrder)result.recordList[0];

                                    if (po.approvalStatus.name == "Approved")
                                    {
                                        InitializeRef initializeRef = new InitializeRef()
                                        {
                                            internalId = po.internalId,
                                            type = InitializeRefType.purchaseOrder,
                                            typeSpecified = true,
                                        };
                                 
                                        maphieuInternalID = po.internalId;
                                        var Currency = po.currency.name;
                                        RecordRef purchaseOrderRef = new RecordRef();
                                        purchaseOrderRef.internalId = maphieuInternalID;
                                        purchaseOrderRef.type = RecordType.purchaseOrder;
                                        purchaseOrderRef.typeSpecified = true;

                                        // Lấy thông tin chi tiết của đơn đặt hàng mua hàng
                                        ReadResponse purchaseOrderRecords = ns.Service.get(purchaseOrderRef);
                                        if (!purchaseOrderRecords.status.isSuccess)
                                        {
                                        }
                                        else
                                        {

                                            PurchaseOrder record = (PurchaseOrder)purchaseOrderRecords.record;

                                            PurchaseOrderItemList ItemReceipt = (PurchaseOrderItemList)record.itemList;// list field trong 1 record

                                            var ctPhieu = db_.NL_CTXUATNHAP.Where(p => p.SOCTXN == tblPhieuNhap.SOCTXN).ToList();
                                            for (int v = 0; v < ItemReceipt.item.Length; v++)
                                            {
                                                var POname = ItemReceipt.item[v].item.name;
                                                var rate = ItemReceipt.item[v].rate;
                                                var ctphieulist = ctPhieu.Where(p => p.MAVT == POname).ToList();
                                                var ctPhieuMH = db_.NL_CTXUATNHAP.Where(p => p.MAVT == POname && p.SOCTXN == tblPhieuNhap.SOCTXN).ToList();
                                                // Cập nhật giá trị mới
                                                ctPhieuMH.ForEach(p =>
                                                {
                                                    p.DVT_TIEN = Currency;
                                                    p.RatePO = Convert.ToDecimal(rate);
                                                });
                                                db_.SaveChanges();
                                                checkOK = true;

                                            }
                                        }

                                    }
                                    else
                                    {
                                        var phieu = db_.NL_DMPHIEUXN.FirstOrDefault(p => p.SOCTXN == tblPhieuNhap.SOCTXN);
                                        if (phieu != null)
                                        {
                                            phieu.InternalidPO = "NOT_FOUND";
                                            db_.SaveChanges();

                                            checkOK = false;
                                        }
                                    }
                                }
                                else
                                {
                                    var phieu = db_.NL_DMPHIEUXN.FirstOrDefault(p => p.SOCTXN == tblPhieuNhap.SOCTXN);
                                    if (phieu != null)
                                    {
                                        phieu.InternalidPO = "NOT_FOUND";
                                        db_.SaveChanges();

                                        checkOK = false;
                                    }
                                }

                                if (checkOK)
                                {
                                    var phieuNhap = db_.NL_DMPHIEUXN.FirstOrDefault(p => p.SOCTXN == tblPhieuNhap.SOCTXN);
                                    if (phieuNhap != null)
                                    {
                                        phieuNhap.InternalidPO = maphieuInternalID;
                                        db_.SaveChanges();
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

    }
}