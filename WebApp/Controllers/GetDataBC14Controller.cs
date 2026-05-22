using NSClient;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ToolsApp.Authentication;
using ToolsApp.com.netsuite.webservices;
using ToolsApp.EntityFramework.VatTu;
using ToolsApp.EntityFramework.NetsuiteTT;
using ToolsApp.Helper;
using Newtonsoft.Json;

namespace ToolsApp.Controllers
{
    public class GetDataBC14Controller : BaseController
    {
        #region Database
        NSClient.NSClient ns = new NSClient.NSClient();
        private NetsuiteTTEntities1 dbns = new NetsuiteTTEntities1();
        private wqlvattuEntities dbvt = new wqlvattuEntities();
        #endregion

        #region Index
        public ActionResult Index()
        {
            return View();
        }
        #endregion

        #region Danh sách phiếu gần nhất
        public ActionResult GetList()
        {
            var list = dbvt.DMPHIEUx.OrderByDescending(p => p.NgayYC).Take(3000).ToList();
            if (list.Count() > 0)
            {
                ViewBag.listPhieu = list;
            }
            return PartialView();
        }
        #endregion

        #region Lấy trạng thái Đơn đề nghị mua hàng
        public JsonResult Getdata_(string fromdate = null, string todate = null)
        {
            #region NetSuite Client Initialization
            // NSClient.NSClient ns = new NSClient.NSClient();
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
            var fromdate_ = fromdate;
            var todate_ = todate;
            #region Xử lý ngày
            CultureInfo cul = CultureInfo.GetCultureInfo("en-GB");

            DateTime fromdate_convert = DateTime.ParseExact(fromdate_, "dd/MM/yyyy", cul);
            DateTime todate_convert = DateTime.ParseExact(todate_, "dd/MM/yyyy", cul);

            #endregion


            // Define your model
            TEMPMODELS_BCChiPhiVattu_558 model = new TEMPMODELS_BCChiPhiVattu_558();

            // Set up the search with your saved search ID
            var transactionSearch = new TransactionSearchAdvanced
            {
                savedSearchId = "558" // Báo Cáo Chi Phí Vật Tư: Results

            };
            // Set up the criteria for the search
            TransactionSearch criteriaSearch = new TransactionSearch();
            criteriaSearch.basic = new TransactionSearchBasic();
            criteriaSearch.basic.tranDate = new SearchDateField()
            {
                operatorSpecified = true,
                @operator = SearchDateFieldOperator.within,
                searchValue = fromdate_convert,       // Start date
                searchValueSpecified = true,
                searchValue2 = todate_convert,   // End date
                searchValue2Specified = true,
            };
            transactionSearch.criteria = criteriaSearch;
            transactionSearch.criteria.basic.tranDate = criteriaSearch.basic.tranDate;

            // Execute the search
            try
            {
                var searchResult = ns.Service.search(transactionSearch);
                if (searchResult.status.isSuccess)
                {
                    for (var i = (searchResult.pageIndex - 1); i < searchResult.totalPages; i++)
                    {
                        searchResult = ns.Service.searchMoreWithId(searchResult.searchId, i + 1);
                        SearchRow[] searchList = searchResult.searchRowList;
                        var searchList_ = searchResult.searchRowList;
                        if (searchList != null)
                        {
                            foreach (var r in searchList)
                            {
                                var row = r as TransactionSearchRow;
                                if (row?.accountJoin?.externalId != null && row.accountJoin.externalId?[0].searchValue.externalId.Length > 0)
                                {

                                    string jsonResult = JsonConvert.SerializeObject(searchList_);
                                    TransactionSearch transSearch_ = new TransactionSearch();
                                    SearchStringField customerEntityID = new SearchStringField();
                                    customerEntityID.@operator = SearchStringFieldOperator.contains;
                                    customerEntityID.operatorSpecified = true;
                                    customerEntityID.searchValue = row.basic.tranId[0].searchValue;

                                    TransactionSearchBasic custBasic_ = new TransactionSearchBasic();
                                    custBasic_.tranId = customerEntityID;

                                    transSearch_.basic = custBasic_;
                                    SearchResult res_ = ns.Service.search(transSearch_);
                                    Record[] recordList_;
                                    recordList_ = res_.recordList;

                                    var accountmain_ = "";
                                    var Type = "";
                                    if (recordList_ != null)
                                    {
                                        foreach (Record record in recordList_)
                                        {
                                            if (record is InventoryAdjustment)
                                            {
                                                List<InventoryAdjustment> IADList = new List<InventoryAdjustment>();
                                                IADList.Add((InventoryAdjustment)record);
                                                accountmain_ = IADList.Count < 1 ? "" : IADList.FirstOrDefault().account.name;
                                                Type = "InventoryAdjustment";
                                            }
                                            else if (record is AssemblyBuild assemblyBuild)
                                            {
                                                //  accountmain_ = assemblyBuild.account?.name ?? "";
                                                Type = "AssemblyBuild";
                                            }
                                        }
                                    }
                                        

                                    double sotienco_ = row.basic.creditAmount?[0].searchValue ?? 0;

                                    TBL_Data_BCChiPhiVatTu_BC14 data = new TBL_Data_BCChiPhiVatTu_BC14();
                                    data.Item = row.itemJoin?.externalId?[0].searchValue.externalId ?? null;
                                    data.ItemName = row.itemJoin?.displayName?[0].searchValue ?? null;
                                    data.SoPhieu = row.basic.tranId?[0].searchValue ?? "";
                                    data.NgayPhieu = row.basic.tranDate?[0].searchValue ?? null;
                                    data.Quantity = Convert.ToDecimal(row.basic.quantity?[0].searchValue ?? 0);
                                    data.Location = row.locationJoin.externalId?[0].searchValue.externalId ?? "";
                                    data.SoTienCo = Convert.ToDecimal(row.basic.creditAmount?[0].searchValue ?? 0);
                                    data.SoTienNo = Convert.ToDecimal(row.basic.debitAmount?[0].searchValue ?? 0);
                                    data.Account = row.accountJoin.externalId?[0].searchValue.externalId ?? "";
                                    data.AccountMain = accountmain_;
                                    data.Memo = row.basic.memo?[0].searchValue ?? "";
                                    data.PhongBan = row.basic.department?[0].searchValue.externalId ?? "";
                                    data.MANVCN = User.UserName == null ? "" : User.UserName;
                                    data.NgayCN = DateTime.Now;
                                    data.Type = Type;
                                    data.ID_ = Guid.NewGuid();
                                    dbns.TBL_Data_BCChiPhiVatTu_BC14.Add(data);
                                    dbns.SaveChanges();
                                }
                            }
                        }
                    } 
                    return Json(new { status = 1, title = "", text = "Tải dữ liệu thành công", obj = model }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = 0, title = "Error", text = "Tìm kiếm thất bại" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = 0, title = "Exception", text = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }
        #endregion


        #region Lấy trạng thái Đơn đề nghị mua hàng
        public JsonResult Updata_()
        {
            #region NetSuite Client Initialization
            // NSClient.NSClient ns = new NSClient.NSClient();
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

            // Define your model
            TEMPMODELS_BCChiPhiVattu_558 model = new TEMPMODELS_BCChiPhiVattu_558();

            // Set up the search with your saved search ID
            try
            {

                string fullName = "Trần Lê Hựu Quốc Công"; // Chuỗi chứa họ và tên
                string[] nameParts = fullName.Split(' '); // Tách chuỗi dựa trên dấu cách

                // Kiểm tra nếu đúng định dạng họ và tên
                if (nameParts.Length >= 2)
                {
                    // Lấy họ
                    string lastName = nameParts[0];

                    // Lấy tên
                    string firstName = string.Join(" ", nameParts, 1, nameParts.Length - 1);

                    // In ra kết quả
                    Console.WriteLine("Họ: " + lastName);
                    Console.WriteLine("Tên: " + firstName);
                }

                Employee employeeRecord = new Employee
                {
                    // Gán các thuộc tính cần thiết
                    externalId = "itc9996",
                    entityId = "itc9996",                
                    salutation = "Vinh",

                    // Thêm các thuộc tính khác nếu cần
                };
                WriteResponse rp = ns.Service.upsert(employeeRecord);
                var kq = "0";
                if (!rp.status.isSuccess)
                {
                    kq = "0";
                    return Json(new { status = -1, title = "", text = "Tải dữ liệu thất bại", obj = kq }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    kq = "1";
                    return Json(new { status = 1, title = "", text = "Tải dữ liệu thành công", obj = kq }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    status = 0,
                    title = "Exception",
                    text = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }


        }



        #endregion
    }
}