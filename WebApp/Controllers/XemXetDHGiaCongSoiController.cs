using NSClient;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using ToolsApp.Authentication;
using ToolsApp.com.netsuite.webservices;
using ToolsApp.EntityFramework.KhoSoi;
using ToolsApp.EntityFramework.NetsuiteTT;
using ToolsApp.EntityFramework.VatTu;
using ToolsApp.Models;
using ToolsApp.Utilities;

namespace ToolsApp.Controllers
{
    public class XemXetDHGiaCongSoiController : BaseController
    {
        private wqlvattuEntities vt_ = new wqlvattuEntities();
        private wqlkhosoiEntities ks_ = new wqlkhosoiEntities();
        NetsuiteTTEntities1 ns_ = new NetsuiteTTEntities1();


        // GET: XemXetDHGiaCongSoi
        public ActionResult Index()
        {
            var madh = ks_.SP_GC_MADHGCSOI_DONHANGGCSOI().ToList();
            ViewBag.MADH = madh;
            ViewBag.MADHXX = ks_.SP_GC_MADHGCSOI_DONHANGGCSOI_XNDVYC().ToList();
            return View();
        }
        #region Load View
        public ActionResult _GetList(string MADH_GCSOI)
        {
            ViewBag.DATA = ks_.SP_GC_LOAD_DONHANGGCSOI_CT_BYMADH(MADH_GCSOI).ToList();
            return PartialView();
        }
        public ActionResult _GetListXN(string MADH_GCSOI)
        {
            ViewBag.DATA = ks_.SP_GC_LOAD_DONHANGGCSOI_CT_BYMADH(MADH_GCSOI).ToList();
            return PartialView();
        }
        #endregion
        #region onchange 
        public JsonResult _LoadNguoiXX(string MADH_GCSOI)
        {
            var data = ks_.SP_GC_MADHGCSOI_DONHANGGCSOI().Where(p => p.MADH_GCSOI == MADH_GCSOI).FirstOrDefault();
            var nguoixx = vt_.DMNHANVIENs.Where(p => p.MANV == data.MANV_DONVIYC_XX).ToList();
            return Json(new { status = 1, text = "", obj = nguoixx }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult _LoadNguoiXN(string MADH_GCSOI)
        {
            var data = ks_.SP_GC_MADHGCSOI_DONHANGGCSOI_XNDVYC().Where(p => p.MADH_GCSOI == MADH_GCSOI).FirstOrDefault();
            var nguoixx = vt_.DMNHANVIENs.Where(p => p.MANV == data.MANV_DONVITH_XACNHAN).ToList();
            return Json(new { status = 1, text = "", obj = nguoixx }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult _LoadMADH_GCSOI_XN()
        {
            var data = ks_.SP_GC_MADHGCSOI_DONHANGGCSOI_XNDVYC().ToList();
            return Json(new { status = 1, text = "", obj = data }, JsonRequestBehavior.AllowGet);
        }

        #endregion
        #region Sửa đơn hàng gia công
        public ActionResult _Edit(Guid KHOA_ID)
        {
            ViewBag.CT = ks_.DONHANGGCSOI_CT.Where(p => p.KHOA_ID == KHOA_ID).FirstOrDefault();
            return PartialView();
        }
        #endregion
        #region Sửa đơn hàng được thực hiện bởi GD Thực hiện
        public ActionResult _UpdateEdit(DONHANGGCSOI_CTModels model)
        {
            var data = ks_.SP_GC_MADHGCSOI_DONHANGGCSOI_XNDVYC().Where(p => p.MADH_GCSOI == model.MADH_GCSOI).FirstOrDefault();
            if (data.HIEULUCXACNHAN == true)
            {
                return Json(new { status = -1, text = "Đơn hàng này đã được xác nhận không thể chỉnh sửa", obj = "" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var madvi = vt_.sp_kyhieudonvi_by_manv(User.UserName).FirstOrDefault();
                var nguoix = vt_.DMNHANVIENs.Where(p => p.IDMaDV == madvi.IDMaDV && p.XemXet == 1 && p.MANV == data.MANV_DONVITH_XACNHAN).FirstOrDefault();
                if (nguoix != null)
                {
                    var ctDonHang = ks_.DONHANGGCSOI_CT.Where(p => p.KHOA_ID == model.KHOA_ID).FirstOrDefault();
                    if (ctDonHang != null)
                    {
                        ctDonHang.SOLUONGYEUCAU = model.SOLUONGYEUCAU;
                        ks_.Entry(ctDonHang).State = EntityState.Modified;
                        ks_.SaveChanges();
                        return Json(new { status = 1, text = "Đã sửa số lượng thành công", obj = ctDonHang }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { status = -1, text = "Không có thông tin đơn hàng", obj = "" }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new { status = -1, text = "Chỉ GĐ đơn vị thực hiện mới được quyền sửa số lượng", obj = "" }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        #endregion
        #region Lưu Xem Xét
        public ActionResult Luu_click(string MADH_GCSOI)
        {
            try
            {
                var data = ks_.SP_GC_MADHGCSOI_DONHANGGCSOI().Where(p => p.MADH_GCSOI == MADH_GCSOI).FirstOrDefault();
                var madvi = vt_.sp_kyhieudonvi_by_manv(User.UserName).FirstOrDefault();
                var nguoix = vt_.DMNHANVIENs.Where(p => p.IDMaDV == madvi.IDMaDV && p.XemXet == 1 && p.MANV == data.MANV_DONVIYC_XX).FirstOrDefault();
                if (nguoix != null)
                {
                    var luutt = ks_.SP_GC_UPDATE_DONHANGGCSOI_XX(MADH_GCSOI, DateTime.Now);
                    return Json(new { status = 1, text = "Đã xem xét thành công", obj = luutt }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = -1, text = "Không có quyền xem xét", obj = "" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, text = "", obj = ex }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
        #region Lưu Xác Nhận
        public ActionResult Luu_click_Xn(string MADH_GCSOI, string DVTHXN)
        {
            try
            {
                var data = ks_.SP_GC_MADHGCSOI_DONHANGGCSOI_XNDVYC().Where(p => p.MADH_GCSOI == MADH_GCSOI).FirstOrDefault();
                var madvi = vt_.sp_kyhieudonvi_by_manv(User.UserName).FirstOrDefault();
                var nguoix = vt_.DMNHANVIENs.Where(p => p.IDMaDV == madvi.IDMaDV && p.XemXet == 1 && p.MANV == data.MANV_DONVITH_XACNHAN).FirstOrDefault();
                var tb = ks_.DONHANGGCSOIs.Where(p => p.MADH_GCSOI == MADH_GCSOI).ToList();
                var tb_ct = ks_.DONHANGGCSOI_CT.Where(p => p.MADH_GCSOI == MADH_GCSOI).ToList();
                var f_tb = tb.FirstOrDefault();
                #region Kết nối thư viện Netsuite
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
                if (DVTHXN == "XACNHAN")
                {
                    if (nguoix != null)
                    {

                        if (ns != null)
                        {

                            #region Call data

                            // var dvyc = vattu_.DMDONVIs.Where(p => p.KiHieu == f_tb.MADH.Substring(0, 3)).FirstOrDefault();
                            var IDMADV_TH = ns_.Donvi_mapping_Department.Where(p => p.idmadv == f_tb.IDMADV_TH).ToList();
                            var IDMADV_YC = ns_.Donvi_mapping_Department.Where(p => p.idmadv == f_tb.IDMADV_YC).ToList();
                            var f_IDMADV_TH = IDMADV_TH.Count < 1 ? "" : IDMADV_TH.FirstOrDefault().externalid;
                            var f_IDMADV_YC = IDMADV_YC.Count < 1 ? "" : IDMADV_YC.FirstOrDefault().externalid;
                            #endregion

                            CustomRecord customRec = new CustomRecord();

                            RecordRef RecordRef = new RecordRef();
                            RecordRef.internalId = UtilsNetsuite.CustomRecord.DonYeuCauSanXuat;
                            customRec.recType = RecordRef; // Chọn CustomRecord
                            customRec.name = f_tb.MADH_GCSOI; // gán mã đơn hàng 
                            customRec.externalId = f_tb.MADH_GCSOI; // gán ext cho đơn hàng
                            customRec.isInactive = false; // ...bật hiệu lực cho đơn hàng 

                            CustomFieldRef[] fields = new CustomFieldRef[99];

                            SelectCustomFieldRef f_donyc_loai = new SelectCustomFieldRef()
                            {
                                scriptId = UtilsNetsuite.CustomFieldRef.custrecord_btm_tt_don_yc_loai,
                                value = new ListOrRecordRef()
                                {
                                    internalId = UtilsNetsuite.LoaiYeuCauSanXuat.GCSoi
                                }
                            };
                            SelectCustomFieldRef f_BoPhanyc = new SelectCustomFieldRef()
                            {
                                scriptId = UtilsNetsuite.CustomFieldRef.custrecord_btm_tt_don_yc_bo_phan,
                                value = new ListOrRecordRef()
                                {
                                    externalId = f_IDMADV_YC
                                }
                            };
                            StringCustomFieldRef f_MANVYC = new StringCustomFieldRef()
                            {
                                scriptId = UtilsNetsuite.CustomFieldRef.custrecord_btm_tt_don_yc_nguoi,
                                value = f_tb.MANVCN
                            };
                            DateCustomFieldRef f_Ngay = new DateCustomFieldRef()
                            {
                                scriptId = UtilsNetsuite.CustomFieldRef.custrecord_btm_tt_don_yc_ngay,
                                value = (DateTime)DateTime.Now,
                            };
                            SelectCustomFieldRef f_Loaisx = new SelectCustomFieldRef()
                            {
                                scriptId = UtilsNetsuite.CustomFieldRef.custrecord_btm_tt_don_yc_loai_sx,
                                value = new ListOrRecordRef()
                                {
                                    internalId = UtilsNetsuite.LoaiSanXuat.SanXuat
                                }
                            };
                            SelectCustomFieldRef f_Thitruong = new SelectCustomFieldRef()
                            {
                                scriptId = UtilsNetsuite.CustomFieldRef.custrecord_btm_tt_don_yc_thi_truong,
                                value = new ListOrRecordRef()
                                {
                                    internalId = UtilsNetsuite.ThiTruong.ND
                                }
                            };
                            SelectCustomFieldRef f_Trangthai = new SelectCustomFieldRef()
                            {
                                scriptId = UtilsNetsuite.CustomFieldRef.custrecord_btm_tt_don_yc_tt_don_hang,
                                value = new ListOrRecordRef()
                                {
                                    internalId = UtilsNetsuite.TrangThaiDonYeuCauSX.BatDau
                                }
                            };
                            DateCustomFieldRef f_date_start = new DateCustomFieldRef()
                            {
                                scriptId = UtilsNetsuite.CustomFieldRef.custrecord_btm_tt_don_yc_date_start,
                                value = (DateTime)DateTime.Now,
                            };
                            DateCustomFieldRef f_date_end = new DateCustomFieldRef()
                            {
                                scriptId = UtilsNetsuite.CustomFieldRef.custrecord_btm_tt_don_yc_date_end,
                                value = (DateTime)DateTime.Now.AddDays(30),
                            };


                            fields[1] = f_donyc_loai;
                            fields[2] = f_BoPhanyc;
                            fields[3] = f_Ngay;
                            fields[4] = f_Loaisx;
                            fields[5] = f_Thitruong;
                            fields[6] = f_Trangthai;
                            fields[7] = f_date_start;
                            fields[8] = f_date_end;
                            fields[9] = f_MANVYC;

                            customRec.customFieldList = fields;
                            WriteResponse response = ns.Service.upsert(customRec);

                            if (response.status.isSuccess == false)
                            {
                                return Json(new { status = -1, title = "", text = "API Netsuite thất bại", obj = response }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                var parent = ((ToolsApp.com.netsuite.webservices.CustomRecordRef)response.baseRef).internalId;
                                

                                foreach (var items in tb_ct)
                                {
                                    #region customRec_pageDinhmuc -- 509  // add form định mức
                                    var item_dinhmuc = f_tb.MADH_GCSOI + "_" + items.MASOITHANHPHAM;
                                    CustomRecord customRec_pageDinhmuc = new CustomRecord();
                                    RecordRef DataRec = new RecordRef();
                                    DataRec.internalId = "509";
                                    customRec_pageDinhmuc.recType = DataRec;
                                    customRec_pageDinhmuc.name = item_dinhmuc;
                                    customRec_pageDinhmuc.externalId = item_dinhmuc;
                                    WriteResponse response_pageDinhmuc = ns.Service.upsert(customRec_pageDinhmuc);
                                    #endregion


                                    #region Add  CT
                                    CustomRecord customRec_item = new CustomRecord();

                                    RecordRef RecordRef_item = new RecordRef();
                                  //  RecordRef_item.internalId = UtilsNetsuite.CustomRecord.YeuCauSoi;
                                    customRec_item.recType = RecordRef_item; // Chọn RecordRef_item                                     
                                    customRec_item.externalId = items.KHOA_ID.ToString(); // gán ext cho đơn hàng

                                    CustomFieldRef[] fields_items = new CustomFieldRef[99];

                                    SelectCustomFieldRef fi_parent = new SelectCustomFieldRef()
                                    {
                                        scriptId = UtilsNetsuite.CustomFieldRef.custrecord_btm_tt_dyc_gcs_parent,
                                        value = new ListOrRecordRef()
                                        {
                                            internalId = parent
                                        }
                                    };
                                    SelectCustomFieldRef fi_Loaidh = new SelectCustomFieldRef()
                                    {
                                        scriptId = UtilsNetsuite.CustomFieldRef.custrecord_btm_tt_dyc_gcs_loai_dh,
                                        value = new ListOrRecordRef()
                                        {
                                            internalId = UtilsNetsuite.LoaiDonHang.INKH
                                        }
                                    };
                                    SelectCustomFieldRef fi_mahangthanhpham = new SelectCustomFieldRef()
                                    {
                                        scriptId = UtilsNetsuite.CustomFieldRef.custrecord_btm_tt_dyc_gcs_mhtp,
                                        value = new ListOrRecordRef()
                                        {
                                            externalId = items.MASOITHANHPHAM
                                        }
                                    };
                                    SelectCustomFieldRef fi_dinhmucsoi = new SelectCustomFieldRef()
                                    {
                                        scriptId = UtilsNetsuite.CustomFieldRef.custrecord_btm_tt_dyc_gcs_dinh_muc,
                                        value = new ListOrRecordRef()
                                        {
                                            externalId = item_dinhmuc
                                        }
                                    };
                                    StringCustomFieldRef fi_SLYC = new StringCustomFieldRef()
                                    {
                                        scriptId = UtilsNetsuite.CustomFieldRef.custrecord_btm_tt_dyc_gcs_sl_yc,
                                        value = items.SOLUONGYEUCAU == null ? "0" : Convert.ToString(items.SOLUONGYEUCAU)
                                    };
                                    StringCustomFieldRef fi_MANVYC = new StringCustomFieldRef()
                                    {
                                        scriptId = UtilsNetsuite.CustomFieldRef.custrecord_btm_tt_dyc_gcs_ma_nhan_vien,
                                        value = f_tb.MANVCN == null ? "" : Convert.ToString(f_tb.MANVCN)
                                    };
                                    DateCustomFieldRef fi_NgayYC = new DateCustomFieldRef()
                                    {
                                        scriptId = UtilsNetsuite.CustomFieldRef.custrecord_btm_tt_dyc_gcs_ngay_cap_nhat,
                                        value = (DateTime)f_tb.NGAYCAPNHAT,
                                    };
                                    StringCustomFieldRef fi_MANVXX = new StringCustomFieldRef()
                                    {
                                        scriptId = UtilsNetsuite.CustomFieldRef.custrecord_btm_tt_dyc_gcs_m_nv_ycsx,
                                        value = f_tb.MANV_DONVIYC_XX == null ? "" : Convert.ToString(f_tb.MANV_DONVIYC_XX)
                                    };
                                    DateCustomFieldRef fi_NgayXX = new DateCustomFieldRef()
                                    {
                                        scriptId = UtilsNetsuite.CustomFieldRef.custrecord_btm_tt_dyc_gcs_ngay_xem_xet,
                                        value = (DateTime)f_tb.NGAYXEMXET,
                                    };
                                    StringCustomFieldRef fi_HLXX = new StringCustomFieldRef()
                                    {
                                        scriptId = UtilsNetsuite.CustomFieldRef.custrecord_btm_tt_dyc_gcs_hieu_luc_xem_x,
                                        value = f_tb.HIEULUCXX == null ? "F" : "T"
                                    };
                                    StringCustomFieldRef fi_MANVXACNHAN = new StringCustomFieldRef()
                                    {
                                        scriptId = UtilsNetsuite.CustomFieldRef.custrecord_btm_tt_dyc_gcs_mnvdv_thuc_hie,
                                        value = f_tb.MANV_DONVITH_XACNHAN == null ? "" : Convert.ToString(f_tb.MANV_DONVITH_XACNHAN)
                                    };
                                    DateCustomFieldRef fi_NgayXACNHAN = new DateCustomFieldRef()
                                    {
                                        scriptId = UtilsNetsuite.CustomFieldRef.custrecord_btm_tt_dyc_gcs_ngay_xac_nhan,
                                        value = (DateTime)f_tb.NGAYXACNHAN,
                                    };
                                    StringCustomFieldRef fi_HLXACNHAN = new StringCustomFieldRef()
                                    {
                                        scriptId = UtilsNetsuite.CustomFieldRef.custrecord_btm_tt_dyc_gcs_hieu_luc_xn,
                                        value = f_tb.HIEULUCXACNHAN == null ? "F" : "T"
                                    };

                                    fields_items[0] = fi_parent;
                                    fields_items[1] = fi_Loaidh;
                                    fields_items[2] = fi_mahangthanhpham;
                                    fields_items[3] = fi_SLYC;
                                    fields_items[4] = fi_MANVYC;
                                    fields_items[5] = fi_NgayYC;
                                    fields_items[6] = fi_MANVXX;
                                    fields_items[7] = fi_NgayXX;
                                    fields_items[8] = fi_HLXX;
                                    fields_items[9] = fi_MANVXACNHAN;
                                    fields_items[10] = fi_NgayXACNHAN;
                                    fields_items[11] = fi_HLXACNHAN;
                                    fields_items[12] = fi_dinhmucsoi;


                                    customRec_item.customFieldList = fields_items;

                                    WriteResponse response_item = ns.Service.upsert(customRec_item);
                                    #endregion

                                    #region response_item

                                    if (response_item.status.isSuccess == false)
                                    {
                                        return Json(new { status = -1, title = "", text = "Thêm thất bại", obj = response_item }, JsonRequestBehavior.AllowGet);
                                    }
                                    #endregion
                                }

                            }


                            //    var luutt = ks_.SP_GC_UPDATE_DONHANGGCSOI_XN(MADH_GCSOI, DateTime.Now);




                            return Json(new { status = 1, text = "Đã xác nhận thành công", obj = "" }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(new { status = -1, text = "Xác nhận thất bại", obj = "" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        return Json(new { status = -1, text = "Người Xác nhận không hợp lệ", obj = "" }, JsonRequestBehavior.AllowGet);
                    }


                }
                else
                {
                    if (nguoix != null)
                    {
                        CustomRecord customRec = new CustomRecord();

                        RecordRef RecordRef = new RecordRef();
                        RecordRef.internalId = UtilsNetsuite.CustomRecord.DonYeuCauSanXuat;
                        customRec.recType = RecordRef; // Chọn CustomRecord
                        customRec.name = f_tb.MADH_GCSOI; // gán mã đơn hàng 
                        customRec.externalId = f_tb.MADH_GCSOI; // gán ext cho đơn hàng
                        customRec.isInactive = true; // ...bật hiệu lực cho đơn hàng 
                        WriteResponse response_boxn = ns.Service.upsert(customRec);

                        if (response_boxn.status.isSuccess == false)
                        {
                            return Json(new { status = -1, title = "", text = "Thêm thất bại", obj = response_boxn }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            var luutt = ks_.SP_GC_UPDATE_DONHANGGCSOI_BOXN(MADH_GCSOI, DateTime.Now);
                            return Json(new { status = 1, text = "Đã bỏ xác nhận thành công", obj = luutt }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        return Json(new { status = 1, text = "Người Bỏ xác nhận không hợp lệ", obj = "" }, JsonRequestBehavior.AllowGet);

                    }    
                }

            }
            catch (Exception ex)
            {
                return Json(new { status = -1, text = "", obj = ex }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion


    }
}