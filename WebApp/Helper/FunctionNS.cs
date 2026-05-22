using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ToolsApp.com.netsuite.webservices;
using NSClient;
using System.Net;
using ToolsApp.EntityFramework.VatTu;

namespace ToolsApp.Helper
{
    public class ENUM_ID
    {

        #region TT Phân loại điều chỉnh Inventory Adjustment
        public static class PhanLoaiDieuChinh
        {
            public const string DieuChinhTon = "6";

        }
        #endregion

        #region TINHTRANGDONHANG
        public static class TINHTRANGDONHANG
        {
            /// <summary>
            /// 1: Đơn hàng mới
            /// 2: Đang sản xuất
            /// 3: Kết thúc
            /// </summary>
            /// <param name="HL_TBP"></param>
            /// <param name="HL_DP"></param>
            /// <param name="date"></param>
            /// <returns></returns>
            public static String TinhTrangDH(bool HL_TBP, bool HL_DP, DateTime? date)
            {
                string tinhtrang = "";
                if (!HL_TBP && !HL_DP)
                {
                    tinhtrang = "1";
                }
                if (HL_TBP && !HL_DP)
                {
                    tinhtrang = "1";
                }
                if (HL_TBP && !HL_DP)
                {
                    tinhtrang = "1";
                }
                if (HL_TBP && HL_DP)
                {
                    tinhtrang = "2";
                    if (date.Value.AddDays(3) >= DateTime.Now)
                    {
                        tinhtrang = "3";
                    }
                }
                return tinhtrang;
            }
        }
        #endregion

        #region NHIỀU TABLE
        public static class LOAIYEUCAUSANXUAT
        {
            public const string GIACONG = "2";
            public const string SANXUAT = "1";
        }


        public static class LOAIYEUCAU_BASEVN
        {
            public const string GIACONG = "2";
            public const string SANXUAT = "1";
        }
        public static class CustomRecord
        {
            public static string DonYeuCauSanXuat { get { return "550"; } }
            public static string ThanhPhamSanXuat { get { return "570"; } }
            public static string YeuCauIn { get { return "561"; } }
            public static string YeuCauMoc { get { return "551"; } }
            public static string YeuCauSoi { get { return "573"; } }
            public static string PhieuMuaHangSoi { get { return "542"; } }
        }
        public static class LoaiYeuCauSanXuat
        {
            public static string In { get { return "4"; } }
            public static string May { get { return "5"; } }
            public static string Moc { get { return "1"; } }
            public static string Nhuom { get { return "2"; } }
            public static string VaiThanhPham { get { return "3"; } }
            public static string DinhDa { get { return "6"; } }
            public static string GiatUi { get { return "7"; } }
            public static string GCSoi { get { return "8"; } }
        }
        public static class LoaiSanXuat
        {
            public static string GiaCong { get { return "2"; } }
            public static string SanXuat { get { return "1"; } }
        }
        public static class TrangThaiDonYeuCauSX
        {
            public static string BatDau { get { return "1"; } }
            public static string DangSanXuat { get { return "2"; } }
            public static string KetThuc { get { return "3"; } }
        }
        public static class ThiTruong
        {
            public static string ND { get { return "1"; } }
            public static string KXD { get { return "2"; } }
            public static string XK { get { return "3"; } }
        }
        public static class Defaul_Location_Department_Moc
        {
            public static string ex_NMD1 { get { return "W01"; } }
            public static string ex_Khomoc { get { return "MS-A1.03"; } }

        }
        public static class LoaiKhoaCuaThaiTuan
        {
            public static string KeyHH { get { return "1"; } }
            public static string KhoaTinhVatTu { get { return "2"; } }
            public static string IdKhoaGiaThanh { get { return "3"; } }

        }
        public static class LoaiDonHang
        {
            public static string INKH { get { return "1"; } }
            public static string OUTKH { get { return "2"; } }


        }
        public static class LoaiVT
        {
            public static string VATTU { get { return "VATTU"; } }
            public static string SOI { get { return "SOI"; } }

        }
        public static class NGUONGOC
        {
            public static string MUANGOAIND { get { return "106"; } }
            public static string MUANGOAINK { get { return "107"; } }

        }
        public static class NGUONGOCHANG
        {
            public static string MUANGOAI { get { return "3"; } }
            public static string TUSANXUAT { get { return "2"; } }

        }

        #endregion

        #region LOAIKHOATHAITUAN
        public static class LOAIKHOATHAITUAN
        {
            public const string KeyHH = "1";
            public const string KhoaTinhVatTu = "2";
            public const string IdKhoaGiaThanh = "3";
        }
        #endregion

        #region UNITSOFMEASURE
        public static class UNITSOFMEASURE
        {
            public const string MET = "50";
        }
        #endregion

        #region TAXCODE
        public static class TAXCODE
        {
            public const string Z_VN_XK_Cty_KD_CH = "64980";
            public const string Z_VN_NĐ_ĐN = "20";
            public const string Z_VN_NĐ_HN = "19";
            public const string Z_VN_NĐ_Cty_KD_CH = "11";
            public const string Z_VN_NK_Cty_KD_CH = "64981";
            public const string S_VN_NĐ_ĐN8_per = "64875";
            public const string S_VN_NĐ_ĐN10_per = "17";
            public const string S_VN_NĐ_HN8_per = "64876";
            public const string S_VN_NĐ_HN10_per = "16";
            public const string S_VN_NĐ_Cty_KD_CH8_per = "64877";
            public const string S_VN_NĐ_Cty_KD_CH10_per = "8";
            public const string S_VN_NK_Cty_KD_CH8_per = "64982";
            public const string S_VN_NK_Cty_KD_CH10_per = "64983";
            public const string S_FA_VN_8_per = "13";
            public const string S_FA_VN_10_per = "64879";
            public const string E_VN_NĐ_ĐN = "24";
            public const string E_VN_NĐ_HN = "22";
            public const string E_VN_NĐ_Cty_KD_CH = "14";
            public const string E_VN_NK_Cty_KD_CH = "64984";

        }
        #endregion

        #region LOẠI ĐƠN HÀNG MMXK
        public static class LOAIDONHANGMMXK
        {
            public const string FOB = "1";
            public const string CM = "2";
        }
        #endregion

        #region GỬI API STATUS
        public static class GUIAPI
        {
            public const string MOI = "1";
            public const string CAPNHAT = "2";
            public const string HUYBO = "3";
        }
        #endregion

        #region DUNG SAI 
        public static class DUNGSAI
        {
            public const string FIVEPERCENT = "1";
            public const string TENPERCENT = "2";
        }
        #endregion

        #region PHẠM VI
        public static class PHAMVI
        {
            public const string XK = "1";
            public const string ND = "2";
        }
        #endregion

        #region NGUỒN GỐC VẢI
        public static class NGUONGOCVAI
        {
            public const string CONGTY = "1";
            public const string KHACHHANG = "2";
        }
        #endregion


    }



    public class CUSTOMSEARCH
    {
        public static string SEARCH_CUSTOMER_INTERNALID(string name_search, string externalid)
        {
            NSClient.NSClient ns = new NSClient.NSClient();

            string customerId = "";
            CustomerSearch customerSearch = new CustomerSearch();
            customerSearch.basic = new CustomerSearchBasic
            {
                entityId = new SearchStringField
                {
                    @operator = SearchStringFieldOperator.contains,
                    operatorSpecified = true,
                    searchValue = name_search
                }
            };

            // Execute the search
            SearchResult result = ns.Service.search(customerSearch);
            if (result.status.isSuccess)
            {
                if (result.recordList.Count() > 0)
                {
                    Customer record = (Customer)result.recordList[0];
                    customerId = record.internalId;
                }
                else
                {
                    //15302	CÔNG TY CP TẬP ĐOÀN THÁI TUẤN
                    customerId = "15302";
                }
            }
            else
            {
                customerId = "15302";
            }

            return customerId;
        }

        #region ITEM SEARCH
        public static TEMPMODELS SEARCH_ITEM_INTERNALID(string name_search, string location)
        {
            #region Netsuite
            NSClient.NSClient ns = new NSClient.NSClient();

            #endregion

            TEMPMODELS model = new TEMPMODELS();

            var b = new ItemSearchAdvanced() { savedSearchId = "374" }; //ITC TT TRA CUU TON FOR API
            #region Criteria
            ItemSearch criteria_search = new ItemSearch();
            criteria_search.basic = new ItemSearchBasic();
            criteria_search.inventoryLocationJoin = new LocationSearchBasic()
            {
                externalId = new SearchMultiSelectField
                {
                    @operator = SearchMultiSelectFieldOperator.anyOf,
                    operatorSpecified = true,
                    searchValue = new RecordRef[] { new RecordRef { externalId = location } }
                },
            };
            criteria_search.basic.itemId = new SearchStringField()
            {
                @operator = SearchStringFieldOperator.@is,
                operatorSpecified = true,
                searchValue = name_search
            };
            b.criteria = criteria_search;
            b.criteria.inventoryLocationJoin = criteria_search.inventoryLocationJoin;
            b.criteria.basic.itemId = criteria_search.basic.itemId;
            #endregion

            try
            {
                var searchResult = ns.Service.search(b);
                if (searchResult.status.isSuccess)
                {
                    var searchlist = searchResult.searchRowList;
                    double quantityonhand_ = 0;
                    double sumquantityonhand = 0;
                    foreach (var r in searchlist)
                    {   
                        var row = r as ItemSearchRow;
                        if (row != null)
                        {
                            var qty = row.basic.locationQuantityOnHand?.FirstOrDefault()?.searchValue ?? 0; // tồn kho vt
                            quantityonhand_ += qty; 
                            sumquantityonhand = row.basic.quantityOnHand?.FirstOrDefault()?.searchValue ?? 0; // tổng tồn mã vật tư
                            //  quantityonhand_ = quantityonhand_ + (row.basic.locationQuantityOnHand[0]?.searchValue ?? 0);
                            model.itemname = row.basic.itemId[0].searchValue;                            
                            model.location = row.inventoryLocationJoin.externalId[0].searchValue.externalId;                            
                        }
                    }
                    model.quantityonhand = quantityonhand_;
                    model.sumquantityonhand = sumquantityonhand;
                    return model;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return model;
        }
        #endregion

        #region Item Search MAVT - tất cả kho
        #endregion
    }



    public class CUSTOMRECORD
    {
        #region FUNC_SelectCustomFieldRef
        public static SelectCustomFieldRef FUNC_SelectCustomFieldRef(string scriptid, string invalue)
        {
            SelectCustomFieldRef selectCustomFieldRef = new SelectCustomFieldRef();
            ListOrRecordRef listOrRecordRef = new ListOrRecordRef();
            listOrRecordRef.internalId = invalue;
            selectCustomFieldRef.scriptId = scriptid;
            selectCustomFieldRef.value = listOrRecordRef;
            return selectCustomFieldRef;
        }
        public static SelectCustomFieldRef FUNC_SelectCustomFieldRef(string scriptid, string invalue, string exvalue)
        {
            SelectCustomFieldRef selectCustomFieldRef = new SelectCustomFieldRef();
            ListOrRecordRef listOrRecordRef = new ListOrRecordRef();
            listOrRecordRef.externalId = exvalue;
            selectCustomFieldRef.scriptId = scriptid;
            selectCustomFieldRef.value = listOrRecordRef;
            return selectCustomFieldRef;
        }
        #endregion

        #region FUNC_StringCustomFieldRef
        public static StringCustomFieldRef FUNC_StringCustomFieldRef(string sciptid, string value)
        {
            StringCustomFieldRef stringCustomFieldRef = new StringCustomFieldRef();
            stringCustomFieldRef.scriptId = sciptid;
            stringCustomFieldRef.value = value;
            return stringCustomFieldRef;
        }
        #endregion

        #region FUNC_DoubleCustomFieldRef
        public static DoubleCustomFieldRef FUNC_DoubleCustomFieldRef(string sciptid, double value)
        {
            DoubleCustomFieldRef doubleCustomFieldRef = new DoubleCustomFieldRef();
            doubleCustomFieldRef.scriptId = sciptid;
            doubleCustomFieldRef.value = value;
            return doubleCustomFieldRef;
        }
        #endregion

        #region FUNC_DateCustomFieldRef
        public static DateCustomFieldRef FUNC_DateCustomFieldRef(string sciptid, string value)
        {
            DateCustomFieldRef dateCustomFieldRef = new DateCustomFieldRef();
            dateCustomFieldRef.scriptId = sciptid;
            dateCustomFieldRef.value = Convert.ToDateTime(value);
            return dateCustomFieldRef;
        }
        #endregion
    }

    public class BASERECORD
    {
        public static void FUNC_ADD_ASSEMBLIES_ITEM_TO_BOM(string bomid, string itemid)
        {
            NSClient.NSClient ns = new NSClient.NSClient();

            try
            {
                LotNumberedAssemblyItem assemblyItem = new LotNumberedAssemblyItem() { externalId = itemid };
                LotNumberedAssemblyItemBillOfMaterialsList lotNumberedAssemblyItemBillOfMaterialsList = new LotNumberedAssemblyItemBillOfMaterialsList();
                LotNumberedAssemblyItemBillOfMaterials assemblyItemBillOfMaterials = new LotNumberedAssemblyItemBillOfMaterials() { billOfMaterials = new RecordRef() { type = RecordType.bom, externalId = bomid } };
                lotNumberedAssemblyItemBillOfMaterialsList.lotNumberedAssemblyItemBillOfMaterials = new LotNumberedAssemblyItemBillOfMaterials[] { assemblyItemBillOfMaterials };
                assemblyItem.billOfMaterialsList = (lotNumberedAssemblyItemBillOfMaterialsList);
                WriteResponse response = ns.Service.upsert(assemblyItem);
                if (response.status.isSuccess == false)
                {
                    var error = response.status.statusDetail.FirstOrDefault().message;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
    #region
    public class SEARCH_EMPLPOYEE
    {
        public static string GetEmployee(string MANV)
        {
            NSClient.NSClient ns = new NSClient.NSClient();
            wqlvattuEntities vt_ = new wqlvattuEntities();

            var dmnhanvien = vt_.DMNHANVIENs.Where(p => p.MANV == MANV).ToList();

            string fullName = dmnhanvien.FirstOrDefault().HOTEN; // Chuỗi chứa họ và tên
            string[] nameParts = fullName.Split(' '); // Tách chuỗi dựa trên dấu cách
            string lastName = "";
            string firstName = "";
            string Ten = "";

            // Kiểm tra nếu đúng định dạng họ và tên
            if (nameParts.Length >= 2)
            {
                // Lấy họ
                lastName = nameParts[0];
                
                // Lấy tên
                firstName = string.Join(" ", nameParts, 1, nameParts.Length - 1);
                if (nameParts.Length >= 3)
                {
                    Ten = string.Join(" ", nameParts, (nameParts.Length - 1), nameParts.Length - (nameParts.Length - 1));
                }
                // In ra kết quả    
            }
            else
            {
                lastName = fullName;
            }

            Employee employeeRecord = new Employee
            {
                // Gán các thuộc tính cần thiết
                externalId = MANV,
                entityId = MANV,
                salutation = Ten,
                firstName = firstName,
                lastName = lastName,

                // Thêm các thuộc tính khác nếu cần
            };
            WriteResponse responseEm = ns.Service.add(employeeRecord);
            var kq = "0";
            if (!responseEm.status.isSuccess)
            {
                kq = "0";
                return kq;
            }
            else
            {
                kq = "1";
                return kq;
            }
        }        
    }
    #endregion


    public class UPSERTDANHMUC
    {
        #region Upsert Danh mục 
        public static byte FUNC_UPSERT_CustomRecord(string recordid, string value)
        {


            NSClient.NSClient ns = new NSClient.NSClient();
            // NSBase.Client = ns;

            CustomRecord customRecord = new CustomRecord();

            RecordRef recordRef = new RecordRef();
            recordRef.internalId = recordid;
            customRecord.recType = recordRef;
            customRecord.name = value;
            customRecord.externalId = value;
            var response = ns.Service.upsert(customRecord);
            if (response.status.isSuccess == false)
            {
                return 0;
            }
            return 1;
        }
        #endregion
    }

    public class TEMPMODELS
    {
        public string itemname { get; set; }
        public double quantityonhand { get; set; }
        public double sumquantityonhand { get; set; }
        public string location { get; set; }
    }

    public class TEMPMODELS_BCChiPhiVattu_558
    {
        public string internalid { get; set; }
        public string item { get; set; }
        public string itemname { get; set; }
        public string sophieu { get; set; }
        public string ngayphieu { get; set; }
        public string memo { get; set; }
        public string locaiton { get; set; }
        public double quantity { get; set; }
        public double sotien_co { get; set; }
        public double sotien_no { get; set; }
        public string accout { get; set; }
        public string accoutmain { get; set; }
        public string department { get; set; }
        public string ketoankiemtra { get; set; }

    }
}