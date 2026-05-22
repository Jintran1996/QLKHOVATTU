using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ToolsApp.Utilities
{
    public static class UtilsNetsuite
    {
        public static class CustomForm
        {
            public static class InventoryAdjustment
            {
                public static string TT_PNK_MuaHangGiaCong { get { return "169"; } }
                public static string TT_PXKTH_MuaHangGiaCong { get { return "171"; } }
                public static string TT_DieuChinhChuyenMa { get { return "142"; } }
            }
            public static class TransferOrder
            {
                public static string TT_VanChuyenNoiBo { get { return "110"; } }
            }

            public static class Phieu
            {
                public static string PhieuMuaHang { get { return "-1242"; } }
            }
        }
        public static class CustomFieldRef
        {
            public static string custrecord_btm_tt_don_yc_m_parent { get { return "custrecord_btm_tt_don_yc_m_parent"; } }
            public static string custrecord_btm_tt_dyc_gcs_dinh_muc { get { return "custrecord_btm_tt_dyc_gcs_dinh_muc"; } }
            public static string custrecord_btm_tt_dyc_gcs_ngay_xem_xet { get { return "custrecord_btm_tt_dyc_gcs_ngay_xem_xet"; } }
            public static string custrecord_btm_tt_dyc_gcs_mnvdv_thuc_hie { get { return "custrecord_btm_tt_dyc_gcs_mnvdv_thuc_hie"; } }
            public static string custrecord_btm_tt_dyc_gcs_ngay_xac_nhan { get { return "custrecord_btm_tt_dyc_gcs_ngay_xac_nhan"; } }
            public static string custrecord_btm_tt_dyc_gcs_hieu_luc_xn { get { return "custrecord_btm_tt_dyc_gcs_hieu_luc_xn"; } }
            public static string custrecord_btm_tt_dyc_gcs_hieu_luc_xem_x { get { return "custrecord_btm_tt_dyc_gcs_hieu_luc_xem_x"; } }
            public static string custrecord_btm_tt_dyc_gcs_m_nv_ycsx { get { return "custrecord_btm_tt_dyc_gcs_m_nv_ycsx"; } }
            public static string custrecord_btm_tt_dyc_gcs_ngay_cap_nhat { get { return "custrecord_btm_tt_dyc_gcs_ngay_cap_nhat"; } }
            public static string custrecord_btm_tt_dyc_gcs_ma_nhan_vien { get { return "custrecord_btm_tt_dyc_gcs_ma_nhan_vien"; } }
            public static string custrecord_btm_tt_dyc_gcs_sl_yc { get { return "custrecord_btm_tt_dyc_gcs_sl_yc"; } }
            public static string custrecord_btm_tt_dyc_gcs_mhtp { get { return "custrecord_btm_tt_dyc_gcs_mhtp"; } }
            public static string custrecord_btm_tt_dyc_gcs_parent { get { return "custrecord_btm_tt_dyc_gcs_parent"; } }
            public static string custrecord_btm_tt_dyc_gcs_loai_dh { get { return "custrecord_btm_tt_dyc_gcs_loai_dh"; } }
            public static string custrecord_btm_tt_status_manu { get { return "custrecord_btm_tt_status_manu"; } }
            public static string custrecord_btm_tt_bill_of_materials_revi { get { return "custrecord_btm_tt_bill_of_materials_revi"; } }
            public static string custrecord_btm_tt_bill_of_materials { get { return "custrecord_btm_tt_bill_of_materials"; } }
            public static string custrecord_btm_tt_ma_nhom_soi_bom { get { return "custrecord_btm_tt_ma_nhom_soi_bom"; } }
            public static string custrecord_btm_tt_kl_hao_hut { get { return "custrecord_btm_tt_kl_hao_hut"; } }
            public static string custrecord_btm_tt_kl_khong_hao_hut { get { return "custrecord_btm_tt_kl_khong_hao_hut"; } }
            public static string custrecord_btm_tt_hs_at { get { return "custrecord_btm_tt_hs_at"; } }
            public static string custbody_btm_tt_loai_sp { get { return "custbody_btm_tt_loai_sp"; } }
            public static string custrecord_btm_tt_bpsx { get { return "custrecord_btm_tt_bpsx"; } }
            public static string custrecord_btm_tt_mnsx { get { return "custrecord_btm_tt_mnsx"; } }
            public static string custrecordcustrecord_btm_tt_don_yc_gcsoi { get { return "custrecordcustrecord_btm_tt_don_yc_gcsoi"; } }
            public static string custrecord_btm_tt_ma_loai_vai { get { return "custrecord_btm_tt_ma_loai_vai"; } }
            public static string custrecord_btm_tt_gia_thanh_kh_tam { get { return "custrecord_btm_tt_gia_thanh_kh_tam"; } }
            public static string custrecord_btm_tt_loi_dau_tam { get { return "custrecord_btm_tt_loi_dau_tam"; } }
            public static string custrecord_btm_tt_so_bien_ban_tam { get { return "custrecord_btm_tt_so_bien_ban_tam"; } }
            public static string custrecord_btm_tt_raubien1_tam { get { return "custrecord_btm_tt_raubien1_tam"; } }
            public static string custrecord_btm_tt_raubien2_tam { get { return "custrecord_btm_tt_raubien2_tam"; } }
            public static string custrecord_btm_tt_nguoi_kiem_tam { get { return "custrecord_btm_tt_nguoi_kiem_tam"; } }
            public static string custrecord_btm_tt_sl_yard_tam { get { return "custrecord_btm_tt_sl_yard_tam"; } }
            public static string custrecord_btm_tt_sl_cat_mau_tam { get { return "custrecord_btm_tt_sl_cat_mau_tam"; } }
            public static string custrecord_btm_tt_ma_chat_luong_tam { get { return "custrecord_btm_tt_ma_chat_luong_tam"; } }
            public static string custrecord_btm_tt_sl_thanh_pham { get { return "custrecord_btm_tt_sl_thanh_pham"; } }
            public static string custrecord_btm_tt_ngay_sx { get { return "custrecord_btm_tt_ngay_sx"; } }
            public static string custrecord_btm_tt_gio_bat_dau { get { return "custrecord_btm_tt_gio_bat_dau"; } }
            public static string custrecord_btm_tt_gio_ket_thuc { get { return "custrecord_btm_tt_gio_ket_thuc"; } }
            public static string custrecord_btm_tt_kt_cay_moc { get { return "custrecord_btm_tt_kt_cay_moc"; } }
            public static string custrecord_btm_tt_mh_tp { get { return "custrecord_btm_tt_mh_tp"; } }
            public static string custrecord_btm_tt_lot_serial_number { get { return "custrecord_btm_tt_lot_serial_number"; } }
            public static string custrecord_btm_tt_so_phieu_nhap { get { return "custrecord_btm_tt_so_phieu_nhap"; } }
            public static string custrecord_btm_tt_m_dh_yc_sx { get { return "custrecord_btm_tt_m_dh_yc_sx"; } }
            public static string custrecord_btm_tt_dyc_m_bpsx { get { return "custrecord_btm_tt_dyc_m_bpsx"; } }
            public static string custrecord_btm_tt_dyc_m_mnsx { get { return "custrecord_btm_tt_dyc_m_mnsx"; } }
            public static string custrecord_btm_tt_dyc_m_qcdh { get { return "custrecord_btm_tt_dyc_m_qcdh"; } }
            public static string custrecord_btm_tt_dyc_m_loai_bao_bi { get { return "custrecord_btm_tt_dyc_m_loai_bao_bi"; } }
            public static string custrecord_btm_tt_dyc_m_nhom_dong_goi { get { return "custrecord_btm_tt_dyc_m_nhom_dong_goi"; } }
            public static string custrecord_btm_tt_dyc_m_ngay_cap_nhat { get { return "custrecord_btm_tt_dyc_m_ngay_cap_nhat"; } }
            public static string custrecord_btm_tt_dyc_m_loai_yc { get { return "custrecord_btm_tt_dyc_m_loai_yc"; } }
            public static string custrecord_btm_tt_dyc_m_kho_may_det { get { return "custrecord_btm_tt_dyc_m_kho_may_det"; } }
            public static string custrecord_btm_tt_dyc_m_hang_tp { get { return "custrecord_btm_tt_dyc_m_hang_tp"; } }
            public static string custrecord_btm_tt_dyc_m_ma_nv_cap_nhat { get { return "custrecord_btm_tt_dyc_m_ma_nv_cap_nhat"; } }
            public static string custrecord_btm_tt_dyc_m_loai_bong { get { return "custrecord_btm_tt_dyc_m_loai_bong"; } }
            public static string custrecord_btm_tt_dyc_m_loai_may_det { get { return "custrecord_btm_tt_dyc_m_loai_may_det"; } }
            public static string custrecord_btm_tt_dyc_m_do_co_nhuom { get { return "custrecord_btm_tt_dyc_m_do_co_nhuom"; } }
            public static string custrecord_btm_tt_dyc_m_mn { get { return "custrecord_btm_tt_dyc_m_mn"; } }
            public static string custrecord_btm_tt_dyc_m_sl_yc { get { return "custrecord_btm_tt_dyc_m_sl_yc"; } }
            public static string custrecord_btm_tt_dyc_m_sl_dieu_bo { get { return "custrecord_btm_tt_dyc_m_sl_dieu_bo"; } }
            public static string custrecord_btm_tt_dyc_m_ma_hang_kd { get { return "custrecord_btm_tt_dyc_m_ma_hang_kd"; } }
            public static string custrecord_btm_tt_dyc_m_ma_dinh_muc { get { return "custrecord_btm_tt_dyc_m_ma_dinh_muc"; } }
            public static string custbody_btm_tt_phan_loai_dieu_chinh { get { return "custbody_btm_tt_phan_loai_dieu_chinh"; } }
            public static string cseg_btm_tt_pv_bh { get { return "cseg_btm_tt_pv_bh"; } }
            public static string custbody_btm_tt_nguon_goc { get { return "custbody_btm_tt_nguon_goc"; } }
            public static string custbody_btm_tt_nguon_goc_hang { get { return "custbody_btm_tt_nguon_goc_hang"; } }
            public static string custbody_btm_tt_loai_don_hang_gia_cong { get { return "custbody_btm_tt_loai_don_hang_gia_cong"; } }
            public static string custitemnumber_btm_tt_ngay_sx { get { return "custitemnumber_btm_tt_ngay_sx"; } }
            public static string custitemnumber_btm_tt_loai_vai_luu_dong { get { return "custitemnumber_btm_tt_loai_vai_luu_dong"; } }
            public static string custitemnumber_btm_tt_ma_qc { get { return "custitemnumber_btm_tt_ma_qc"; } }
            public static string custitemnumber_btm_tt_loai_chat_luong { get { return "custitemnumber_btm_tt_loai_chat_luong"; } }
            public static string custitemnumber_btm_tt_ma_loi { get { return "custitemnumber_btm_tt_ma_loi"; } }
            public static string custitemnumber_btm_tt_loi_chi_tiet { get { return "custitemnumber_btm_tt_loi_chi_tiet"; } }
            public static string custbody_btm_tt_loai_phieu_van_chuyen { get { return "custbody_btm_tt_loai_phieu_van_chuyen"; } }
            public static string custbody_btm_tt_chung_tu_tham_chieu { get { return "custbody_btm_tt_chung_tu_tham_chieu"; } }
            public static string custcol_btm_tt_don_yc_nhuom { get { return "custcol_btm_tt_don_yc_nhuom"; } }
            public static string custrecord_btm_tt_ma_mau_danh_muc { get { return "custrecord_btm_tt_ma_mau_danh_muc"; } }
            public static string custrecord_btm_tt_nhom_sp_ma { get { return "custrecord_btm_tt_nhom_sp_ma"; } }
            public static string custrecord_btm_tt_nhom_sp_phan_loai { get { return "custrecord_btm_tt_nhom_sp_phan_loai"; } }
            public static string custitem_btm_tt_nguon_goc_hang { get { return "custitem_btm_tt_nguon_goc_hang"; } }
            public static string custitem_btm_tt_bong_in { get { return "custitem_btm_tt_bong_in"; } }
            public static string custitem_btm_tt_nhom_hoa_van_det_in { get { return "custitem_btm_tt_nhom_hoa_van_det_in"; } }
            public static string custitem_btm_tt_nhom_trang_phuc { get { return "custitem_btm_tt_nhom_trang_phuc"; } }
            public static string custitem_btm_tt_nhom_san_pham { get { return "custitem_btm_tt_nhom_san_pham"; } }
            public static string custitem_btm_tt_loai_san_pham { get { return "custitem_btm_tt_loai_san_pham"; } }
            public static string custitem_btm_tt_bong_nen_vai { get { return "custitem_btm_tt_bong_nen_vai"; } }
            public static string custitem_btm_tt_loai_may_det { get { return "custitem_btm_tt_loai_may_det"; } }
            public static string custitem_btm_tt_cong_nghe_det { get { return "custitem_btm_tt_cong_nghe_det"; } }
            public static string custitem_btm_tt_nhom_cong_nghe { get { return "custitem_btm_tt_nhom_cong_nghe"; } }
            public static string custitem_btm_tt_cap_mau_nhuom_in { get { return "custitem_btm_tt_cap_mau_nhuom_in"; } }
            public static string custitem_btm_tt_nhom_sp_kinh_doanh { get { return "custitem_btm_tt_nhom_sp_kinh_doanh"; } }
            public static string custitem_btm_tt_kho_vai_moc { get { return "custitem_btm_tt_kho_vai_moc"; } }
            public static string custitem_btm_tt_ma_mau { get { return "custitem_btm_tt_ma_mau"; } }
            public static string custitem_btm_tt_loai_vai { get { return "custitem_btm_tt_loai_vai"; } }
            public static string custitem_btm_tt_ten_sp_hop_quy { get { return "custitem_btm_tt_ten_sp_hop_quy"; } }
            public static string custitem_btm_tt_kl_gm2 { get { return "custitem_btm_tt_kl_gm2"; } }
            public static string custitem_btm_tt_kho_thuc_te { get { return "custitem_btm_tt_kho_thuc_te"; } }
            public static string custbody_btm_po_gia_cong { get { return "custbody_btm_po_gia_cong"; } }
            public static string custrecord_btm_tt_don_yc_loai { get { return "custrecord_btm_tt_don_yc_loai"; } }
            public static string custrecord_btm_tt_don_yc_bo_phan { get { return "custrecord_btm_tt_don_yc_bo_phan"; } }
            public static string custrecord_btm_tt_don_yc_nguoi { get { return "custrecord_btm_tt_don_yc_nguoi"; } }
            public static string custrecord_btm_tt_don_yc_ngay { get { return "custrecord_btm_tt_don_yc_ngay"; } }
            public static string custrecord_btm_tt_don_yc_bo_phan_sx { get { return "custrecord_btm_tt_don_yc_bo_phan_sx"; } }
            public static string custrecord_btm_tt_don_yc_nha_may_sx { get { return "custrecord_btm_tt_don_yc_nha_may_sx"; } }
            public static string custrecord_btm_tt_don_yc_ma_dh { get { return "custrecord_btm_tt_don_yc_ma_dh"; } }
            public static string custrecord_btm_tt_don_yc_tt_don_hang { get { return "custrecord_btm_tt_don_yc_tt_don_hang"; } }
            public static string custrecord_btm_tt_don_yc_thi_truong { get { return "custrecord_btm_tt_don_yc_thi_truong"; } }
            public static string custrecord_btm_tt_don_yc_date_start { get { return "custrecord_btm_tt_don_yc_date_start"; } }
            public static string custrecord_btm_tt_don_yc_date_end { get { return "custrecord_btm_tt_don_yc_date_end"; } }
            public static string custrecord_btm_tt_don_yc_in_parent { get { return "custrecord_btm_tt_don_yc_in_parent"; } }
            public static string custrecord_btm_tt_don_yc_in_sl_yc { get { return "custrecord_btm_tt_don_yc_in_sl_yc"; } }
            public static string custrecord_btm_tt_don_yc_in_ma_mat_hang { get { return "custrecord_btm_tt_don_yc_in_ma_mat_hang"; } }
            public static string custrecord_btm_tt_don_yc_in_loai_hang { get { return "custrecord_btm_tt_don_yc_in_loai_hang"; } }
            public static string custrecord_btm_tt_don_yc_in_ms_quy_trinh { get { return "custrecord_btm_tt_don_yc_in_ms_quy_trinh"; } }
            public static string custrecord_btm_tt_don_yc_in_ma_mau { get { return "custrecord_btm_tt_don_yc_in_ma_mau"; } }
            public static string custrecord_btm_tt_don_yc_in_mm_kinh_doan { get { return "custrecord_btm_tt_don_yc_in_mm_kinh_doan"; } }
            public static string custrecord_btm_tt_don_yc_in_mk_gia_thanh { get { return "custrecord_btm_tt_don_yc_in_mk_gia_thanh"; } }
            public static string custrecord_btm_tt_don_yc_in_mnxn_khoa { get { return "custrecord_btm_tt_don_yc_in_mnxn_khoa"; } }
            public static string custrecord_btm_tt_don_yc_in_sl_da_xem_x { get { return "custrecord_btm_tt_don_yc_in_sl_da_xem_x"; } }
            public static string custrecord_btm_tt_don_yc_in_date_start { get { return "custrecord_btm_tt_don_yc_in_date_start"; } }
            public static string custrecord_btm_tt_don_yc_in_date_end { get { return "custrecord_btm_tt_don_yc_in_date_end"; } }
            public static string custrecord_btm_tt_don_yc_in_loai_dh { get { return "custrecord_btm_tt_don_yc_in_loai_dh"; } }
            public static string custrecord_btm_tt_don_yc_in_ma_cn_in { get { return "custrecord_btm_tt_don_yc_in_ma_cn_in"; } }
            public static string custrecord_btm_tt_don_yc_in_chuyen_kho { get { return "custrecord_btm_tt_don_yc_in_chuyen_kho"; } }
            public static string custrecord_btm_tt_don_yc_vtp_parent { get { return "custrecord_btm_tt_don_yc_vtp_parent"; } }
            public static string custrecord_btm_tt_don_yc_loai_sx { get { return "custrecord_btm_tt_don_yc_loai_sx"; } }
            public static string custrecord_btm_tt_don_yc_vtp_loai_hang { get { return "custrecord_btm_tt_don_yc_vtp_loai_hang"; } }
            public static string custrecord_btm_tt_don_yc_vtp_ma_kh_dat { get { return "custrecord_btm_tt_don_yc_vtp_ma_kh_dat"; } }
            public static string custrecord_btm_tt_don_yc_vtp_ms_quy_trin { get { return "custrecord_btm_tt_don_yc_vtp_ms_quy_trin"; } }
            public static string custrecord_btm_tt_don_yc_vtp_ma_mau { get { return "custrecord_btm_tt_don_yc_vtp_ma_mau"; } }
            public static string custrecord_btm_tt_don_yc_vtp_mm_kd { get { return "custrecord_btm_tt_don_yc_vtp_mm_kd"; } }
            public static string custrecord_btm_tt_don_yc_vtp_mk_gia_than { get { return "custrecord_btm_tt_don_yc_vtp_mk_gia_than"; } }
            public static string custrecord_btm_tt_don_yc_vtp_mnv_xn_khoa { get { return "custrecord_btm_tt_don_yc_vtp_mnv_xn_khoa"; } }
            public static string custrecord_btm_tt_don_yc_vtp_sl_yc { get { return "custrecord_btm_tt_don_yc_vtp_sl_yc"; } }
            public static string custrecord_btm_tt_don_yc_vtp_sl_da_xem_x { get { return "custrecord_btm_tt_don_yc_vtp_sl_da_xem_x"; } }
            public static string custrecord_btm_tt_don_yc_vtp_loai_dh { get { return "custrecord_btm_tt_don_yc_vtp_loai_dh"; } }
            public static string custrecord_btm_tt_don_yc_vtp_mcn_nhuom { get { return "custrecord_btm_tt_don_yc_vtp_mcn_nhuom"; } }
            public static string custrecord_btm_tt_don_yc_vtp_chuyen_kho { get { return "custrecord_btm_tt_don_yc_vtp_chuyen_kho"; } }
            public static string custrecord_btm_tt_ma_khoa { get { return "custrecord_btm_tt_ma_khoa"; } }
            public static string custrecord_btm_tt_loai_khoa { get { return "custrecord_btm_tt_loai_khoa"; } }
        }
        public static class CustomRecord
        {
            public static string DonYeuCauSanXuat { get { return "291"; } }
            public static string DonHangXKNhuom { get { return "155"; } }
            public static string ThanhPhamSanXuat { get { return "273"; } }
            public static string YeuCauIn { get { return "561"; } }
            public static string YeuCauMoc { get { return "223"; } }
            public static string MaDinhMuc { get { return "318"; } }
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
            public static string GiaoGiaCong { get { return "3"; } }
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



    }
}