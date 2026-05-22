using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ToolsApp.Authentication;
using ToolsApp.com.netsuite.webservices;
using ToolsApp.EntityFramework.KhoSoi;
using ToolsApp.EntityFramework.VatTu;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using System.IO;
using System.Drawing;
using static System.Collections.Specialized.BitVector32;
using System.Web.UI.WebControls;
using ToolsApp.Models;
using System.Collections;
using System.Drawing.Imaging;
using Image = System.Drawing.Image;
using System.Configuration;

namespace ToolsApp.Controllers
{
    public class TraCuuPYCController : BaseController
    {
        #region Database
        NSClient.NSClient ns = new NSClient.NSClient();
        private wqlvattuEntities dbvt = new wqlvattuEntities();
        private wqlkhosoiEntities soi_ = new wqlkhosoiEntities();
        #endregion

        #region Index
        public ActionResult Index()
        {
            return View();
        }
        #endregion

        #region Load View _Details 
        public ActionResult _Details(string maphieu)
        {
            var List = dbvt.VATTU2025_LOAD_CTPHIEU_PR_(maphieu).ToList();
            ViewBag.List = List;
            return PartialView();
        }
        #endregion

        #region In phiếu 
        public ActionResult _ReportPrintCT(string maphieu)
        {
            if (maphieu == null)
            {
                return Json(new { status = -1, text = "Mã phiếu null vui lòng liện hệ ITC kiểm tra", obj = "" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var CTPhieu = dbvt.VATTU2025_SP_REPHIEUMUAHANGTHEOBM(maphieu).ToList();

                var chuKyBytes = CTPhieu.FirstOrDefault()?.CHUKYPD;
                if (chuKyBytes != null)
                {
                   // string base64Image = Convert.ToBase64String(chuKyBytes);
                  //  ViewBag.Base64Image = "data:image/png;base64," + base64Image;
                    ViewBag.Base64Image = ToBase64Image(chuKyBytes);

                }

                ViewBag.List = CTPhieu;
            }
            return PartialView();
        }
        #endregion

        #region In phiếu  Sợi
        public ActionResult _ReportPrintCT_SOI(string maphieu)
        {
            if (maphieu == null)
            {
                return Json(new { status = -1, text = "", obj = "" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var CTPhieu = soi_.VATTU2025_SP_RPT_PHIEUDENGHIMUAHANG_PRINT(maphieu,User.UserName).ToList();

                var first = CTPhieu.FirstOrDefault();
                if (first != null)
                {
                    ViewBag.Base64ImagePD = ToBase64Image(first.CHUKY_NGUOIPD);
                    ViewBag.Base64ImageXX = ToBase64Image(first.CHUKY_NGUOIXX);
                }

                ViewBag.List = CTPhieu;
            }
            return PartialView();
        }

        private string ToBase64Image(byte[] bytes)
        {
            if (bytes == null || bytes.Length < 10)
                return null;

            // PNG: 89 50 4E 47
            if (bytes[0] == 0x89 && bytes[1] == 0x50 &&
                bytes[2] == 0x4E && bytes[3] == 0x47)
            {
                return "data:image/png;base64," + Convert.ToBase64String(bytes);
            }

            // JPG: FF D8
            if (bytes[0] == 0xFF && bytes[1] == 0xD8)
            {
                return "data:image/jpeg;base64," + Convert.ToBase64String(bytes);
            }

            // ❌ Không phải ảnh web → bỏ
            return null;
        }
        #endregion

        #region Danh sách phiếu gần nhất
        public ActionResult GetList(string maphieu = "")
        {
            var list = dbvt.VATTU2024_TRACUUPHIEUMUAHANG(maphieu.Trim()).OrderByDescending(p => p.NgayYC).Take(3000).ToList();
            if (list.Count() > 0)
            {
                ViewBag.listPhieu = list;
            }
            return PartialView();
        }
        #endregion

        #region Lấy trạng thái Đơn đề nghị mua hàng
        public async Task<JsonResult> _Dongbo(string maphieu)
        {
            DateTime ngayPD = new DateTime();
            string trangThaiPD = "";
            string nguoiPD = "";

            CustomRecordRef recordRef = new CustomRecordRef();
            recordRef.scriptId = "customrecord_btm_tt_purchase_request";
            recordRef.externalId = maphieu;
            ReadResponse response = ns.Service.get(recordRef);
            if (!response.status.isSuccess)
            {
                return Json(new { status = -1, text = "Không tìm thấy đơn đề nghị mua hàng", obj = "" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                CustomRecord record = (CustomRecord)response.record;
                string maphieuInternalID = record.internalId;
                CustomFieldRef[] fields = record.customFieldList;// list field trong 1 record

                #region Lấy trạng thái PD, người PD, ngày PD
                for (int i = 0; i < fields.Length; i++)
                {
                    CustomFieldRef field = fields[i];

                    if (field.scriptId == "custrecord_tt_nguoi_duyet")
                    {
                        if (field.GetType() == typeof(MultiSelectCustomFieldRef))
                        {
                            MultiSelectCustomFieldRef valField = field as MultiSelectCustomFieldRef;
                            var inEm = valField.value.FirstOrDefault().internalId;
                            nguoiPD = GetEmployee(inEm);// ID người phê duyệt = mã nhân viên
                        }
                    }
                    if (field.scriptId == "custrecord_tt_trang_thai_phe_duyet")
                    {
                        if (field.GetType() == typeof(SelectCustomFieldRef))
                        {
                            SelectCustomFieldRef valField = field as SelectCustomFieldRef;
                            trangThaiPD = valField.value.internalId;
                        }
                    }
                    if (field.scriptId == "custrecord_tt_ngay_phe_duyet")
                    {
                        if (field.GetType() == typeof(DateCustomFieldRef))
                        {
                            DateCustomFieldRef valField = field as DateCustomFieldRef;
                            ngayPD = valField.value;
                        }
                    }
                }
                #endregion
                var dmphieu_vt = dbvt.DMPHIEUx.Where(p => p.MAPHIEU == maphieu).ToList();
                var dmphieu_soi = soi_.NL_DMPHIEU.Where(p => p.MAPHIEU == maphieu).ToList();
                if (dmphieu_vt.Count > 0)
                {
                    #region Lưu vào danh mục phiếu, chi tiết phiếu
                    if (trangThaiPD == "1")
                    {
                        var phieu = dbvt.DMPHIEUx.FirstOrDefault(p => p.MAPHIEU == maphieu);
                        if (phieu == null)
                        {
                            return Json(new { status = -1, text = "Không tìm thấy phiếu ở database để cập nhật!", obj = "" }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            phieu.PDuyet = 1;
                            phieu.NgayPD = ngayPD;
                            phieu.FinishCheck = true;
                            phieu.MaPhieuPD = maphieu;
                            dbvt.Entry(phieu).State = System.Data.Entity.EntityState.Modified;
                            dbvt.SaveChanges();
                            GetItemLine(maphieuInternalID, maphieu);//Lấy mã hàng, po của phiếu lưu chi tiết phiếu 
                            #region API BASEVN
                            return await _APIBASE(maphieu);
                            #endregion
                        }
                    }
                    if (trangThaiPD == "2")
                    {
                        var phieu = dbvt.DMPHIEUx.FirstOrDefault(p => p.MAPHIEU == maphieu);
                        if (phieu == null)
                        {
                            return Json(new { status = -1, text = "Không tìm thấy phiếu ở database để cập nhật!", obj = "" }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            phieu.PDuyet = 0;
                            phieu.FinishCheck = false;
                            dbvt.Entry(phieu).State = System.Data.Entity.EntityState.Modified;
                            dbvt.SaveChanges();
                        }
                    }
                    if (trangThaiPD == "4")
                    {
                        var phieu = dbvt.DMPHIEUx.FirstOrDefault(p => p.MAPHIEU == maphieu);
                        if (phieu == null)
                        {
                            return Json(new { status = -1, text = "Không tìm thấy phiếu ở database để cập nhật!", obj = "" }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            phieu.PDuyet = 0;
                            phieu.FinishCheck = true;
                            dbvt.Entry(phieu).State = System.Data.Entity.EntityState.Modified;
                            dbvt.SaveChanges();
                        }
                    }
                    #endregion
                }

                if (dmphieu_soi.Count > 0)
                {
                    var item = dmphieu_soi.FirstOrDefault();
                    #region Lưu vào danh mục phiếu, chi tiết phiếu
                    if (trangThaiPD == "1")
                    {
                        var phieu = soi_.NL_DMPHIEU.FirstOrDefault(p => p.MAPHIEU == item.MAPHIEU);
                        if (phieu != null)
                        {
                            //var info = dbvt.InfoNguoiPheDuyet(nguoiPD).FirstOrDefault();
                            phieu.PDUYET = 1;
                            phieu.NGAYPD = ngayPD;
                            phieu.FinishCheck = true;
                            phieu.MAPHIEUPD = item.MAPHIEU;
                            soi_.Entry(phieu).State = System.Data.Entity.EntityState.Modified;
                            soi_.SaveChanges();
                            //   GetItemLine(maphieuInternalID, item.MAPHIEU);//Lấy mã hàng, po của phiếu lưu chi tiết phiếu 

                            #region API BASEVN
                            return await _APIBASE(maphieu);
                            #endregion
                        }
                    }
                    if (trangThaiPD == "2")
                    {
                        var phieu = soi_.NL_DMPHIEU.FirstOrDefault(p => p.MAPHIEU == item.MAPHIEU);
                        if (phieu != null)
                        {
                            var numchk = (phieu.NumberCheck == 0) ? 0 : phieu.NumberCheck;
                            if (numchk < 31)
                            {
                                phieu.NumberCheck = numchk + 1;
                                phieu.FinishCheck = false;
                                phieu.PDUYET = 0;
                                soi_.Entry(phieu).State = System.Data.Entity.EntityState.Modified;
                                soi_.SaveChanges();
                            }
                            else
                            {
                                phieu.FinishCheck = true;
                                phieu.PDUYET = 0;
                                soi_.Entry(phieu).State = System.Data.Entity.EntityState.Modified;
                                soi_.SaveChanges();
                            }
                        }
                    }
                    if (trangThaiPD == "4")
                    {
                        var phieu = soi_.NL_DMPHIEU.FirstOrDefault(p => p.MAPHIEU == item.MAPHIEU);
                        phieu.PDUYET = 0;
                        phieu.FinishCheck = true;
                        soi_.Entry(phieu).State = System.Data.Entity.EntityState.Modified;
                        soi_.SaveChanges();
                    }
                    #endregion
                }
                return Json(new { status = 1, text = "Đã cập nhật thành công!", obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Lấy mã nhân viên
        public string GetEmployee(string employeeInternalID)
        {
            string kq = "";
            RecordRef recordRefEm = new RecordRef();
            recordRefEm.internalId = employeeInternalID;
            recordRefEm.type = RecordType.employee;
            recordRefEm.typeSpecified = true;

            ReadResponse responseEm = ns.Service.get(recordRefEm);
            if (!responseEm.status.isSuccess)
            {
                kq = "0";
                return kq;
            }
            else
            {
                Employee recordEm = (Employee)responseEm.record;
                kq = recordEm.entityId;

                return kq;
            }
        }
        #endregion 

        #region Lấy mã nhân viên
        public string GetNumPO(string poInternalID)
        {
            RecordRef recordRefPO = new RecordRef();
            recordRefPO.internalId = poInternalID;
            recordRefPO.type = RecordType.purchaseOrder;
            recordRefPO.typeSpecified = true;

            ReadResponse responsePO = ns.Service.get(recordRefPO);
            string kq = "";
            if (!responsePO.status.isSuccess)
            {
                kq = "0";
                return kq;
            }
            else
            {
                PurchaseOrder recordPO = (PurchaseOrder)responsePO.record;
                kq = recordPO.tranId;
                return kq;
            }
        }
        #endregion

        #region Lấy mặt hàng, PO
        public void GetItemLine(string maphieuInternalID, string maphieu)
        {

            string matHang = "";
            //string tenPO = "";
            string inPO = "";
            string soPO = "";
            string soluongpd = "";
            string idkhoact = "";

            CustomRecordSearchBasic searchObjectBasic = new CustomRecordSearchBasic();
            RecordRef recordRef = new RecordRef();
            recordRef.type = RecordType.customRecord;
            recordRef.internalId = "270"; //Đơn Đề Nghị Mua Hàng - Mặt Hàng
            recordRef.typeSpecified = true;
            searchObjectBasic.recType = recordRef;

            CustomRecordSearchAdvanced customRecordSearchAdvanced = new CustomRecordSearchAdvanced();
            CustomRecordSearch customRecordSearch = new CustomRecordSearch();
            customRecordSearch.basic = searchObjectBasic;
            
            #region Lọc theo phiếu yêu cầu mua hàng
            SearchMultiSelectCustomField phieuYC = new SearchMultiSelectCustomField();
            phieuYC.scriptId = "custrecord_tt_itm_pr_parent"; //PURCHASE REQUEST mã đơn đề nghị
            phieuYC.@operator = SearchMultiSelectFieldOperator.anyOf;
            phieuYC.operatorSpecified = true;

            ListOrRecordRef maPhieu = new ListOrRecordRef();
            maPhieu.typeId = "242";//Đơn Đề Nghị Mua Hàng
            maPhieu.internalId = maphieuInternalID;
            phieuYC.searchValue = new ListOrRecordRef[] { maPhieu };

            searchObjectBasic.customFieldList = new SearchCustomField[] { phieuYC };
            #endregion  

            customRecordSearchAdvanced.criteria = customRecordSearch;

            SearchPreferences searchPreferences = new SearchPreferences();
            searchPreferences.bodyFieldsOnly = false;
            searchPreferences.pageSize = 1000;
            searchPreferences.pageSizeSpecified = true;
            searchPreferences.returnSearchColumns = false;
            ns.Service.searchPreferences = searchPreferences;
            SearchResult searchResult = ns.Service.search(searchObjectBasic);

            foreach (var item in searchResult.recordList)
            {
                CustomRecord b = (CustomRecord)item;
                CustomFieldRef[] fields = b.customFieldList;
                foreach (var field in fields)
                {
                    if (field.scriptId == "custrecord_tt_itm_mat_hang")
                    {
                        if (field.GetType() == typeof(SelectCustomFieldRef))
                        {
                            SelectCustomFieldRef valField = field as SelectCustomFieldRef;
                            matHang = valField.value.name;
                        }
                    }

                    if (field.scriptId == "custrecord_tt_itm_so_luong_phe_duyet")
                    {
                        if (field.GetType() == typeof(DoubleCustomFieldRef))
                        {
                            DoubleCustomFieldRef valField = field as DoubleCustomFieldRef;
                            soluongpd = valField.value.ToString();
                        }
                    }
                    if (field.scriptId == "custrecordttg_ctphieu_id_khoa")
                    {
                        if (field.GetType() == typeof(StringCustomFieldRef))
                        {
                            StringCustomFieldRef valField = field as StringCustomFieldRef;
                            idkhoact = valField.value == null ? "" : valField.value.ToString();
                        }
                    }
                }
                if (matHang != "" && soluongpd != "")
                {
                    string ignoreKeyword = ConfigurationManager.AppSettings["IgnoreSLPDKeyword"];
                    if (idkhoact == null || idkhoact == "")
                    {
                        var itemVT = dbvt.CTPHIEUx.FirstOrDefault(p => p.MAPHIEU == maphieu && p.MAVT == matHang);
                        if (itemVT != null)
                        {
                            if (!maphieu.Contains(ignoreKeyword))
                            {
                                itemVT.SLPD = Convert.ToDecimal(soluongpd);
                            }
                            itemVT.SoPONetSuite = soPO;
                            itemVT.InternalIDNetSuite = inPO;
                            dbvt.Entry(itemVT).State = System.Data.Entity.EntityState.Modified;
                            dbvt.SaveChanges();
                        }

                    }
                    else
                    {
                        int int_idkhoact = Convert.ToInt32(idkhoact);
                        var itemVT = dbvt.CTPHIEUx.FirstOrDefault(p => p.MAPHIEU == maphieu && p.MAVT == matHang && p.IDKHOA == int_idkhoact);
                        if (itemVT != null)
                        {
                            itemVT.SLPD = Convert.ToDecimal(soluongpd);
                            itemVT.SoPONetSuite = soPO;
                            itemVT.InternalIDNetSuite = inPO;
                            dbvt.Entry(itemVT).State = System.Data.Entity.EntityState.Modified;
                            dbvt.SaveChanges();
                        }
                    }

                    matHang = "";
                    soluongpd = "";
                    idkhoact = "";

                }

            }
        }
        #endregion

        #region API BASE
        public async Task<JsonResult> _APIBASE(string MAPHIEU)
        {
            var apibase = dbvt.TTG_BASEVN_TOKEN_API.FirstOrDefault(p => p.namebase == "add_PR");
            if (apibase.isHieuluc == true)
            {
                var LOGPR = dbvt.TTG_BASEVN_LOG_PR.Where(p => p.MAPHIEUPR == MAPHIEU && p.isSTATUS == true).ToList();
                if (LOGPR.Count < 1)
                {
                    #region DATA
                    // var apibase = dbvt.TTG_BASEVN_TOKEN_API.FirstOrDefault(p => p.namebase == "add_PR");
                    var url = apibase.url;
                    var item = dbvt.DMPHIEUx?.FirstOrDefault(p => p.MAPHIEU == MAPHIEU && p.PDuyet == 1);
                    var item_soi = soi_.NL_DMPHIEU?.FirstOrDefault(p => p.MAPHIEU == MAPHIEU && p.PDUYET == 1);
                    var kihieu = MAPHIEU.Substring(0, 3);
                    var department = dbvt.TTG_BASEVN_DEPARTMENT.Where(p => p.KIHIEU == kihieu).ToList();
                    if (department.Count < 1)
                    {
                        department = dbvt.TTG_BASEVN_DEPARTMENT.Where(p => p.KIHIEU == "TTG").ToList();
                    }
                    #endregion
                    if (!string.IsNullOrEmpty(item?.MAPHIEU) || !string.IsNullOrEmpty(item_soi?.MAPHIEU))
                    {
                        using (var client = new HttpClient())
                        {
                            var bangKeListList = new ArrayList();
                            var itemlist = dbvt.VATTU2025_LOADDATA_ITEMLIST_PR_BASEVN(MAPHIEU).ToList();
                            if (itemlist.Count > 0)
                            {
                                foreach (var i in itemlist)
                                {
                                    var bangKeList = new ArrayList()
                                    {
                                        i.MAPHIEU, i.MAVT, i.TenVT, i.SLPD, i.DVT, i.GHICHU, i.THOIDIEMSD
                                    };
                                    bangKeListList.Add(bangKeList);
                                }
                            }

                            string bangKeJson = JsonConvert.SerializeObject(bangKeListList);
                            byte[] jsonBytes = Encoding.UTF8.GetBytes(bangKeJson);
                            var base64BangKe = Convert.ToBase64String(jsonBytes);
                            var values = new Dictionary<string, string>
                                {
                                    { "access_token",  apibase.access_token },
                                    { "service_id",apibase.service_id },
                                    { "block_id", apibase.block_id },
                                    { "name", MAPHIEU },
                                    { "username", apibase.username},
                                    { "root_content","Đề nghị mua hàng (PR) - " + MAPHIEU },
                                    { "service_ma_phieu_yc", MAPHIEU },
                                    { "service_don_vi_yeu_cau",  department.FirstOrDefault().BASE_DEPARTMENT},
                                    { "service_bang_ke", base64BangKe}
                                };

                            var content = new FormUrlEncodedContent(values);
                            var response = await client.PostAsync(url, content);

                            var responseString = await response.Content.ReadAsStringAsync();
                            var responseString_ = response.Content.ReadAsStringAsync();
                            var status = response.IsSuccessStatusCode;
                            var statuscode = response.StatusCode;
                            var root_id = responseString_.Result;
                            var json = JsonConvert.DeserializeObject<RootObject>(responseString);
                            var rootId = json?.ticket?.root_id;
                            var BASELOGPR = new EntityFramework.VatTu.TTG_BASEVN_LOG_PR()
                            {
                                MAPHIEUPR = MAPHIEU,
                                isSTATUS = status == true ? true : false,
                                Root_id = rootId,
                                STATUSCODE = statuscode.ToString(),
                                NGAYCN = DateTime.Now,
                                Department = department.FirstOrDefault().BASE_DEPARTMENT,
                                ERROR = json.message.ToString(),
                                GHICHU = base64BangKe,
                            };
                            dbvt.TTG_BASEVN_LOG_PR.Add(BASELOGPR);
                            dbvt.SaveChanges();
                        }
                    }
                }
            }
            //Console.WriteLine("Response: " + responseString);
            return Json(new { status = 1, text = "Đã cập nhật thành công!", obj = "" }, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }

}