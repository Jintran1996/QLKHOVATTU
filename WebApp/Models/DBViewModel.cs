using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ToolsApp.Models
{
    #region ChangePasswordViewModel
    public partial class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "UserName is required")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "ResetCode is required")]
        public string ResetCode { get; set; }
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8}$", ErrorMessage = "Password must meet requirements")]
        [StringLength(255, ErrorMessage = "Must be between 8 and 255 characters", MinimumLength = 8)]
        public string Password { get; set; }
        [Required(ErrorMessage = "Confirm Password is required")]
        [StringLength(255, ErrorMessage = "Must be between 8 and 255 characters", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }

    }
    #endregion

    #region HomeUserViewModel
    public partial class HomeUserViewModel
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string LoaiKethua { get; set; }
        public string UserKeThua { get; set; }
        public string SalesRepCode { get; set; }
        public string Fullname { get; set; }
        public string Email { get; set; }
        public string UserCreate { get; set; }
        public string UserFullnameCreate { get; set; }
        public Nullable<System.DateTime> DatetimeCreate { get; set; }
        public string UserUpdate { get; set; }
        public string UserFullnameUpdate { get; set; }
        public Nullable<System.DateTime> DatetimeUpdate { get; set; }
        public Nullable<bool> isDelete { get; set; }
        public string UserDelete { get; set; }
        public string UserFullnameDelete { get; set; }
        public Nullable<System.DateTime> DatetimeDelete { get; set; }
        public string Roles { get; set; }
        public string Pages { get; set; }
        public string Softwares { get; set; }
        public string PrincipalGroups { get; set; }
    }
    #endregion

    #region HomePageViewModel
    public partial class HomePageViewModel
    {
        [Display(Name = "Id")]
        [Required(ErrorMessage = "The field is required.")]
        public int Id { get; set; }
        [Display(Name = "Code")]
        [Required(ErrorMessage = "The field is required.")]
        public string Code { get; set; }
        [Display(Name = "Info")]
        public string Info { get; set; }
        [Display(Name = "Controler")]
        [Required(ErrorMessage = "The field is required.")]
        public string Controler { get; set; }
        [Display(Name = "Action")]
        [Required(ErrorMessage = "The field is required.")]
        public string Action { get; set; }
        public string UserCreate { get; set; }
        public string UserFullnameCreate { get; set; }
        public Nullable<System.DateTime> DatetimeCreate { get; set; }
        public string UserUpdate { get; set; }
        public string UserFullnameUpdate { get; set; }
        public Nullable<System.DateTime> DatetimeUpdate { get; set; }
        public Nullable<bool> isDelete { get; set; }
        public string UserDelete { get; set; }
        public string UserFullnameDelete { get; set; }
        public Nullable<System.DateTime> DatetimeDelete { get; set; }
    }
    #endregion

    #region HomeRoleViewModel
    public partial class HomeRoleViewModel
    {
        [Display(Name = "Id")]
        [Required(ErrorMessage = "The field Id is required.")]
        public int Id { get; set; }
        [Display(Name = "Code")]
        [Required(ErrorMessage = "The field Code is required.")]
        public string Code { get; set; }
        [Display(Name = "Role name")]
        [Required(ErrorMessage = "The field RoleName is required.")]
        public string RoleName { get; set; }
        public string UserCreate { get; set; }
        public string UserFullnameCreate { get; set; }
        public Nullable<System.DateTime> DatetimeCreate { get; set; }
        public string UserUpdate { get; set; }
        public string UserFullnameUpdate { get; set; }
        public Nullable<System.DateTime> DatetimeUpdate { get; set; }
        public Nullable<bool> isDelete { get; set; }
        public string UserDelete { get; set; }
        public string UserFullnameDelete { get; set; }
        public Nullable<System.DateTime> DatetimeDelete { get; set; }
    }
    #endregion

    #region HomeMenuViewModel
    public partial class HomeMenuViewModel
    {
        [Display(Name = "Id")]
        [Required(ErrorMessage = "The field Id is required.")]
        public int Id { get; set; }
        [Display(Name = "Menu Name")]
        [Required(ErrorMessage = "The field MenuName is required.")]
        public string MenuName { get; set; }
        [Display(Name = "Parent Id")]
        public Nullable<int> ParentId { get; set; }
        [Display(Name = "Page Id")]
        public Nullable<int> PageId { get; set; }
        [Display(Name = "Icon")]
        public string Icon { get; set; }
        [Display(Name = "Order No")]
        [Required(ErrorMessage = "The field OrderNo is required.")]
        public Nullable<int> OrderNo { get; set; }
        public string UserCreate { get; set; }
        public string UserFullnameCreate { get; set; }
        public Nullable<System.DateTime> DatetimeCreate { get; set; }
        public string UserUpdate { get; set; }
        public string UserFullnameUpdate { get; set; }
        public Nullable<System.DateTime> DatetimeUpdate { get; set; }
        public Nullable<bool> isDelete { get; set; }
        public string UserDelete { get; set; }
        public string UserFullnameDelete { get; set; }
        public Nullable<System.DateTime> DatetimeDelete { get; set; }
    }
    #endregion

    #region HomeSoftwareGroupViewModel
    public partial class HomeSoftwareGroupViewModel
    {
        [Display(Name = "Id")]
        [Required(ErrorMessage = "The field Id is required.")]
        public int Id { get; set; }
        [Display(Name = "Title")]
        [Required(ErrorMessage = "The field Title is required.")]
        public string Title { get; set; }
        public string Domain { get; set; }
        public string Code { get; set; }
        public string Icon { get; set; }
        public Nullable<int> OrderNo { get; set; }
        public string UserCreate { get; set; }
        public string UserFullnameCreate { get; set; }
        public Nullable<System.DateTime> DatetimeCreate { get; set; }
        public string UserUpdate { get; set; }
        public string UserFullnameUpdate { get; set; }
        public Nullable<System.DateTime> DatetimeUpdate { get; set; }
        public Nullable<bool> isDelete { get; set; }
        public string UserDelete { get; set; }
        public string UserFullnameDelete { get; set; }
        public Nullable<System.DateTime> DatetimeDelete { get; set; }
        public string ColorCode { get; set; }
    }
    #endregion

    #region HomeSoftwareViewModel
    public partial class HomeSoftwareViewModel
    {
        [Display(Name = "Id")]
        [Required(ErrorMessage = "The field Id is required.")]
        public int Id { get; set; }
        [Display(Name = "Title")]
        [Required(ErrorMessage = "The field Title is required.")]
        public string Title { get; set; }
        public Nullable<int> GroupId { get; set; }
        public string Domain { get; set; }
        public string Code { get; set; }
        public string Icon { get; set; }
        public Nullable<int> OrderNo { get; set; }
        public string UserCreate { get; set; }
        public string UserFullnameCreate { get; set; }
        public Nullable<System.DateTime> DatetimeCreate { get; set; }
        public string UserUpdate { get; set; }
        public string UserFullnameUpdate { get; set; }
        public Nullable<System.DateTime> DatetimeUpdate { get; set; }
        public Nullable<bool> isDelete { get; set; }
        public string UserDelete { get; set; }
        public string UserFullnameDelete { get; set; }
        public Nullable<System.DateTime> DatetimeDelete { get; set; }
        public string Sites { get; set; }
    }
    #endregion

    #region HomeUserBlackListViewModel
    public partial class HomeUserBlackListViewModel
    {
        public long Id { get; set; }
        public string BlackListUser { get; set; }
        public string BlackListFullname { get; set; }
        public string Email { get; set; }
        public string PrincipalId { get; set; }
        public string PrincipalName { get; set; }
        public string Ticket { get; set; }
        public string Reason { get; set; }
        public string UserCreate { get; set; }
        public string UserFullnameCreate { get; set; }
        public Nullable<System.DateTime> DatetimeCreate { get; set; }
        public string UserUpdate { get; set; }
        public string UserFullnameUpdate { get; set; }
        public Nullable<System.DateTime> DatetimeUpdate { get; set; }
        public Nullable<bool> isDelete { get; set; }
        public string UserDelete { get; set; }
        public string UserFullnameDelete { get; set; }
        public Nullable<System.DateTime> DatetimeDelete { get; set; }
    }
    #endregion

    #region SitesViewModel
    public partial class SitesViewModel
    {
        [Display(Name = "Site Code")]
        [Required(ErrorMessage = "The field SiteCode is required.")]
        [StringLength(3, ErrorMessage = "Must be between 1 and 3 characters", MinimumLength = 1)]
        public string SiteCode { get; set; }
        [Display(Name = "Site Name")]
        [Required(ErrorMessage = "The field SiteName is required.")]
        public string SiteName { get; set; }
    }
    #endregion

    #region TaiKhoanViewModels
    public partial class TaiKhoanViewModels
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string FullName { get; set; }
        public Nullable<bool> IsHieuLuc { get; set; }
        public string NguoiTao { get; set; }
        public Nullable<System.DateTime> NgayTao { get; set; }
        public string NguoiCapNhat { get; set; }
        public Nullable<System.DateTime> NgayCapNhat { get; set; }
    }
    #endregion

    #region PhieuYCMuaHangViewModels
    public partial class PhieuYCMuaHangViewModels
    {
        public PhieuYCMuaHangViewModels()
        {
            this.Detail = new List<CTPhieuYCMuaHangViewModels>();
        }
        public decimal STT { get; set; }
        public string KYHIEU { get; set; }
        public string MAPHIEU { get; set; }
        public Nullable<System.DateTime> NGAYYC { get; set; }
        public string NGUOIPD { get; set; }
        public string NGUOIXX { get; set; }
        public string NGUOITIEPNHAN { get; set; }
        public string NGUOILAPPHIEU { get; set; }
        public string LYDOSD { get; set; }
        public string MAPHIEUPD { get; set; }
        public string LOAIPHIEU { get; set; }
        public Nullable<byte> DX { get; set; }
        public Nullable<byte> NK { get; set; }
        public string ND { get; set; }
        public string KH { get; set; }
        public Nullable<bool> HIEULUC { get; set; }
        public Nullable<System.DateTime> NGAYXX { get; set; }
        public Nullable<System.DateTime> NGAYPD { get; set; }
        public Nullable<System.DateTime> NGAYTN { get; set; }
        public Nullable<byte> XXET { get; set; }
        public Nullable<byte> PDUYET { get; set; }
        public Nullable<byte> TNHANYC { get; set; }
        public string LOAIDONHANG { get; set; }
        public string GHICHU { get; set; }
        public List<CTPhieuYCMuaHangViewModels> Detail { get; set; }
    }
    #endregion

    #region CTPhieuYCMuaHangViewModels
    public partial class CTPhieuYCMuaHangViewModels
    {
        public string MASOCP { get; set; }
        public string MAPHIEU { get; set; }
        public string MAVT { get; set; }
        public Nullable<decimal> SLYCAU { get; set; }
        public Nullable<decimal> SLXXET { get; set; }
        public Nullable<decimal> SLPD { get; set; }
        public Nullable<decimal> SLTONDVI { get; set; }
        public string TINHTRANG { get; set; }
        public string THOIDIEMSD_ { get; set; }
        public Nullable<System.DateTime> THOIDIEMSD { get; set; }
        public string GHICHU { get; set; }
        public string LO { get; set; }
        public string HIEU { get; set; }
        public Nullable<System.DateTime> NGAYDAPUNG { get; set; }
        public string KHOATHAMCHIEU { get; set; }
        public bool NK_STATIC { get; set; }
        public string IDMaDV_YCMUA { get; set; }
        public string LOAIYEUCAU { get; set; }
        public string GhiChucuaLanhDao { get; set; }
        public string MADH { get; set; }
        public string DonviTH { get; set; }
        public string Thang { get; set; }
        public string Nam { get; set; }
        public List<PhieuYCMuaHangViewModels> NL_DMPHIEU { get; set; }
        public Nullable<bool> Delete { get; set; }
    }
    #endregion

    #region Phieu_YC_Duyet_model
    public partial class Phieu_YC_Duyet_model
    {
        public string MaVt { get; set; }
        public string TenVT { get; set; }
        public string MaPhieuPD { get; set; }
        public Nullable<decimal> SLTONTHEOMAPHIEUPB { get; set; }
        public Nullable<decimal> SLPD { get; set; }
        public Nullable<decimal> SLXK { get; set; }
        public string SoChungTu { get; set; }
        public string MaKho { get; set; }
        public string LoaiXuatNhap { get; set; }
        public string MANVYC { get; set; }
        public string GhiChu { get; set; }
    }
    #endregion

    #region Phieu_Xuat kho dv
    public partial class xuatkho_DV_model
    {
        public string SOCTXN_DV { get; set; }
        public string MaVT_DV { get; set; }
        public string MaKho_DV { get; set; }
        public string GhiChu_DV { get; set; }
        public string LoaiXuatNhap_DV { get; set; }
        public string KhoaKeyXN_DV { get; set; }
        public decimal Soluong_DV { get; set; }

    }
    #endregion

    #region Phieu_Xuat kho GCN
    public partial class xuatkho_GCN_model
    {
        public string SOCTXN_GCN { get; set; }
        public string MaVT_GCN { get; set; }
        public string MaKho_GCN { get; set; }
        public string GhiChu_GCN { get; set; }
        public string LoaiXuatNhap_GCN { get; set; }
        public string LoaiCapPhat_GCN { get; set; }
        public string KhoaKeyXN_GCN { get; set; }
        public decimal Soluong_GCN { get; set; }
        public decimal SL_PheDuyet_GCN { get; set; }
        public string SoCTXN_PhieuPD_GCN { get; set; }

    }
    #endregion

    #region Phieu_Xuat kho TTKD
    public partial class xuatkho_TTKD_model
    {
        public string SOCTXN_TTKD { get; set; }
        public string MaVT_TTKD { get; set; }
        public string MaKho_TTKD { get; set; }
        public string GhiChu_TTKD { get; set; }
        public string LoaiXuatNhap_TTKD { get; set; }
        public string XuatCho_TTKD { get; set; }
        public string KhoaKeyXN_TTKD { get; set; }
        public string MaBP_TTKD { get; set; }
        public decimal Soluong_TTKD { get; set; }

    }
    #endregion

    #region Phieu_Xuat GGC
    public partial class xuatkho_GGC_model
    {
        public string SOCTXN { get; set; }
        public string MaVT { get; set; }
        public string MaKho { get; set; }
        public string SOCTKT { get; set; }
        public string SoLuongXuat { get; set; }
        public string GhiChu { get; set; }
        public string LoaiXuatNhap { get; set; }
        public string KhoNhan { get; set; }
        public string MaNVYC { get; set; }
        public string DonHangCongNghiep { get; set; }

    }
    #endregion
    public partial class DMPHIEUViewModels
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public DMPHIEUViewModels()
        {
            this.Detail = new List<CTPHIEUViewModels>();
        }

        public Nullable<decimal> STT { get; set; }
        public string MAPHIEU { get; set; }
        public Nullable<System.DateTime> NgayYC { get; set; }
        public string NguoiPD { get; set; }
        public string NguoiXX { get; set; }
        public string NguoiTiepNhan { get; set; }
        public string NguoiLapPhieu { get; set; }
        public string LyDoSD { get; set; }
        public string Nam { get; set; }
        public Nullable<decimal> STTPD { get; set; }
        public string MaPhieuPD { get; set; }
        public string LOAIPHIEU { get; set; }
        public string DX { get; set; }
        public string NK { get; set; }
        public string ND { get; set; }
        public string KH { get; set; }
        public Nullable<byte> HieuLuc { get; set; }
        public string KyHieu { get; set; }
        public Nullable<System.DateTime> NgayXX { get; set; }
        public Nullable<System.DateTime> NgayPD { get; set; }
        public Nullable<System.DateTime> NgayTN { get; set; }
        public Nullable<byte> XXet { get; set; }
        public Nullable<byte> PDuyet { get; set; }
        public Nullable<byte> TNhanYC { get; set; }
        public Nullable<System.DateTime> KEHOACHTHANG { get; set; }
        public string LoaiVT { get; set; }
        public string locations { get; set; }

        public List<CTPHIEUViewModels> Detail { get; set; }
    }
    public partial class CTPHIEUViewModels
    {
        public Nullable<decimal> STT { get; set; }
        public string MASOCP { get; set; }
        public string LoaiVT_2 { get; set; }
        public string MAPHIEU { get; set; }
        public string MAVT { get; set; }
        public string MAVT_TUONGDUONG { get; set; }
        public List<string> MAVT_TUONGDUONG_ { get; set; }
        public Nullable<decimal> SLYCAU { get; set; }
        public Nullable<decimal> SLXXET { get; set; }
        public Nullable<decimal> SLPD { get; set; }
        public Nullable<decimal> SLTONDVI { get; set; }
        public string SLTONDVI_ { get; set; }
        public Nullable<decimal> SLTONCTY { get; set; }
        public string SLTONCTY_ { get; set; }
        public string TINHTRANG { get; set; }
        public string THOIDIEMSD_ { get; set; }
        public Nullable<System.DateTime> THOIDIEMSD { get; set; }
        public string THOIDIEMSD_String { get; set; }
        public string GHICHU { get; set; }
        public List<string> GHICHU_ { get; set; }
        public Nullable<System.DateTime> THOIDIEMDAPUNG { get; set; }
        public string YKIENLANHDAO { get; set; }
        public string MaphieuchuyenNB { get; set; }
        public string MADH { get; set; }
        public string DACTINHKYTHUAT { get; set; }
        public int IDKHOA { get; set; }
        public Nullable<bool> IsLOT { get; set; }
        public List<DMPHIEUViewModels> DMPhieu { get; set; }
        public Nullable<bool> Delete { get; set; }
    }
    public partial class SP_CHIPHI_28062016ViewModels
    {
        public string MSCHIPHI { get; set; }
        public string TENCHIPHI { get; set; }
    }
    public partial class DMVATTUViewModels
    {
        public string MAVT { get; set; }
        public string TenVT { get; set; }
        public string LoaiVT { get; set; }
        public string NhomVT { get; set; }
        public string DVT { get; set; }
        public string DonViSuDungtac { get; set; }
        public string ViTriDe { get; set; }
        public Nullable<decimal> DinhMucToiUu { get; set; }
        public Nullable<bool> HieuLuc { get; set; }
        public string MaNGL { get; set; }
        public string Ghichu { get; set; }
        public string CS1 { get; set; }
        public string CS2 { get; set; }
        public string MA2 { get; set; }
        public string Manhom { get; set; }
        public string TenVTat { get; set; }
        public string Images { get; set; }
        public Nullable<int> HANDAPUNG { get; set; }
        public string MACP { get; set; }
        public Nullable<System.DateTime> NGAYCN { get; set; }
        public string NHOM_GC { get; set; }
        public string MANVCAPNHAT { get; set; }
        public string MANVMODIFY { get; set; }
        public Nullable<System.DateTime> NGAYMODIFY { get; set; }
        public string GuiAPI { get; set; }
        public Nullable<System.DateTime> NgayGuiAPI { get; set; }
    }

    public partial class DMSOIViewModels
    {
        public decimal MAID { get; set; }
        public string MAVATTU { get; set; }
        public string Tenvattu { get; set; }
        public string MaNGL { get; set; }
        public string Ghichu { get; set; }
        public string CS1 { get; set; }
        public string CS2 { get; set; }
        public string DVT { get; set; }
        public string LoaiNguyenLieu { get; set; }
        public Nullable<bool> isHieuluc { get; set; }
        public string MA2 { get; set; }
        public string Manhomvt { get; set; }
        public Nullable<decimal> Dongia { get; set; }
        public string TenVTat { get; set; }
        public Nullable<System.DateTime> Ngaycn { get; set; }
        public string MANGL1 { get; set; }
        public Nullable<decimal> Tile1 { get; set; }
        public string MANGL2 { get; set; }
        public Nullable<decimal> Tile2 { get; set; }
        public string MANGL3 { get; set; }
        public Nullable<decimal> Tile3 { get; set; }
        public string MANGL4 { get; set; }
        public Nullable<decimal> Tile4 { get; set; }
        public string MANGL5 { get; set; }
        public Nullable<decimal> Tile5 { get; set; }
        public string Tenvattu_CUGC { get; set; }
        public string GuiAPI { get; set; }
        public Nullable<System.DateTime> NgayGuiAPI { get; set; }
    }
    public partial class DanhMucKhoViewModels
    {
        public string maKho { get; set; }
        public string tenKho { get; set; }
        public string maKhoKT { get; set; }
        public string NHOM_OGSTM { get; set; }
        public string DV_OGSTM { get; set; }
        public string diaChi { get; set; }
        public string khoNhan { get; set; }
        public string thuKho { get; set; }
        public string Loaiphieu { get; set; }
        public Nullable<bool> xuatNB { get; set; }
        public Nullable<bool> hieuluc { get; set; }
    }
    public partial class DanhMucQuyenViewModels
    {
        public string maKho { get; set; }
        public string maNV { get; set; }
        public string kyHieuDV { get; set; }

    }
    public partial class tblKiHieuNhapKhoViewModels
    {
        public string maKho { get; set; }
        public string kyHieuDV { get; set; }

    }
    public partial class Table_Tam_PhieuMuaHang_MAVT
    {
        public string MAVT { get; set; }
        public string TENVT { get; set; }
        public decimal TONKHODONVI { get; set; }
        public string VT { get; set; }
    }
    public partial class NL_DMPHIEUXNViewModels
    {
        public int STT { get; set; }
        public string SOCTXN { get; set; }
        public string MADH { get; set; }
        public Nullable<System.DateTime> NGAY { get; set; }
        public string MAHTHUC { get; set; }
        public string MaNVYC { get; set; }
        public string MANVXUAT { get; set; }
        public string MABP { get; set; }
        public string MAKHO { get; set; }
        public string IDMADVNHAN { get; set; }
        public string IDMADVXUAT { get; set; }
        public string MAKHOXUAT { get; set; }
        public string MANVNHAN { get; set; }
        public string MAKHONHAN { get; set; }
        public string GHICHU { get; set; }
        public string NOIDEN { get; set; }
        public string KETHUA { get; set; }
        public string SOPO { get; set; }
        public string InternalidPO { get; set; }
        public string Externalid_IAD { get; set; }
        public string NGAYCT_ { get; set; }
        public string eNGAYCT_ { get; set; }
        public Nullable<System.DateTime> NGAYCT { get; set; }

    }
    public partial class DM_XUATNHAPViewModels
    {
      
        public string SOPO { get; set; }
        public string SOCTXN { get; set; }
        public string MADH { get; set; }
        public Nullable<System.DateTime> NGAY { get; set; }
        public string LOAIXN { get; set; }
        public string MaNVYC { get; set; }
        public string MANVXUAT { get; set; }
        public string IDMADVXUAT { get; set; }
        public string IDMADVNHAN { get; set; }
        public string MAKHONHAN { get; set; }
        public string NGUOINHAN { get; set; }
        public string GHICHU { get; set; }
        public string KH_NCC { get; set; }
        public string MADVXUAT { get; set; }
        public string MAKHOXUAT { get; set; }
        public string NGUOIXUAT { get; set; }
        public string KETHUATUSOCT { get; set; }
        public Nullable<bool> DAXUATKHO_ { get; set; }
        public string GUIAPI { get; set; }
        public Nullable<System.DateTime> NGAYGUIAPI { get; set; }
        public string NGAYCT_ { get; set; }
        public string eNGAYCT_ { get; set; }
        public Nullable<System.DateTime> NGAYCT { get; set; }
    }
    public partial class XUATNHAPViewModels
    {

        public string NGAYCT_ { get; set; }
        public string eNGAYCT_ { get; set; }
        public Nullable<System.DateTime> NGAYCT { get; set; } 
        public string SoCTXN { get; set; }
        public string SOCTXN_xuatsx_kethua { get; set; }
        public Nullable<System.DateTime> Ngay { get; set; }
        public string LoaiXN { get; set; }
        public string MaNVYC { get; set; }
        public string MANVXuat { get; set; }
        public string DonViYeuCau { get; set; }
        public string MaBP { get; set; }
        public string MaKho { get; set; }
        public string MAVT { get; set; }
        public Nullable<decimal> SoLuongYC { get; set; }
        public Nullable<decimal> SoLuongTT { get; set; }
        public string MaPhieuPD { get; set; }
        public string GHICHU { get; set; }
        public string LoaiCapPhat { get; set; }
        public string SoCTKeToan { get; set; }
        public Nullable<System.DateTime> NgayKeToan { get; set; }
        public string MaVTTam { get; set; }
        public Nullable<System.DateTime> modified { get; set; }
        public string MaTaiSan { get; set; }
        public Nullable<decimal> TongGia { get; set; }
        public Nullable<byte> DaXuatKho { get; set; }
        public string KHOAKEYXN { get; set; }
        public string MaTruongDV { get; set; }
        public Nullable<byte> XacNhanTDV { get; set; }
        public Nullable<System.DateTime> NgayXacNhan { get; set; }
        public string TenVT_NCC { get; set; }
        public string DVT_NCC { get; set; }
        public Nullable<decimal> SOLUONGTT_NCC { get; set; }
        public Nullable<int> VAT { get; set; }
        public string Ghichugia { get; set; }
        public string MAKH { get; set; }
        public string MaphieuchuyenNB { get; set; }
        public string MaKhoXuat { get; set; }
        public string MADH_CN { get; set; }
     

    }
    public partial class NL_CTXUATNHAPViewModels
    {
        public string KHOAKEYXN { get; set; }
        public string MAVT { get; set; }
        public string HIEU { get; set; }
        public string LO { get; set; }
       
        public string MADH { get; set; }
        public string LANXN { get; set; }
        public decimal SOLUONGYC { get; set; }
        public decimal SOLUONGTT { get; set; }
        public decimal SOLUONGTON { get; set; }
        public string MAPHIEUPD { get; set; }
        public string GHICHU { get; set; }
        public string MADINHMUC { get; set; }
        public string MAKHO { get; set; }
        public string MAHTHUC { get; set; }
        public string LOAICAPPHAT { get; set; }
        public string SOCTXN { get; set; }
        public string SOCTXN_Nhap { get; set; }
        public string SOCTXN_Xuat { get; set; }
        public string MASOCP { get; set; }
        public Nullable<System.DateTime> NGAYKETOAN { get; set; }
        public string NGAYKETOAN_ { get; set; }
        public string MAVTTAM { get; set; }
        public string MaTaiSan { get; set; }
        public Nullable<decimal> DonGia { get; set; }
        public byte DAXUATKHO { get; set; }
        public Nullable<System.DateTime> MODIFIED { get; set; }
        public string SOHOADON { get; set; }
        public string KHOATHAMCHIEU { get; set; }
        public bool NK_STATIC { get; set; }
        public string LOAIDONHANG { get; set; }

        public Nullable<System.Guid> ID_XN { get; set; }
        public string GUIAPI { get; set; }
        public Nullable<System.DateTime> NGAYGUIAPI { get; set; }

        public string NGAYCT_ { get; set; }
        public string eNGAYCT_ { get; set; }
        public Nullable<System.DateTime> NGAYCT { get; set; }
        public Nullable<System.DateTime> NGAY { get; set; }

    }
    public partial class NL_DistinctMaPhieuPDViewModels
    {
        public string MAPHIEUPD { get; set; }
    
    }
    #region DM Khách hàng
    public partial class KhachHangViewModels
    {
        public string LoaiDT { get; set; }
        public string MaKH { get; set; }
        public string TenKH { get; set; }
        public string DiaChi { get; set; }
        public string MaSothue { get; set; }
        public string DienThoai { get; set; }
        public string Email { get; set; }
        public string Manvcapnhat { get; set; }
        public Nullable<System.DateTime> Ngaycapnhat { get; set; }
        public string Manvmodify { get; set; }
        public Nullable<System.DateTime> Ngaymodify { get; set; }
    }
    #endregion
    #region NL Vật tư xuất chuyền
    public partial class NL_VATTUXUATCHUYENViewModels
    {
        public string MADH { get; set; }
        public string MAHANGSX { get; set; }
        public Nullable<decimal> SOLUONGVAIMOCSX { get; set; }
        public string KEYHH { get; set; }
        public string MAVATTU { get; set; }
        public Nullable<decimal> SOLUONGDINHMUC { get; set; }
        public Nullable<decimal> SOLUONGSOIXUATCHUYEN { get; set; }
        public Nullable<decimal> SLTONTRENCHUYEN { get; set; }
        public System.Guid ID_XUATCHUYEN { get; set; }
    }
    #endregion
    public partial class NL_DMPHIEUNHAP_CHUYENViewModels
    {
        public string SOCTNHAP { get; set; }
        public string SOCTXUAT { get; set; }
        public string SOCTXN_XD { get; set; }
        public string MADH { get; set; }
        public string MAHTHUC { get; set; }
        public string MAKHOXUAT { get; set; }
        public string MAKHONHAN { get; set; }
        public string MANVXUAT { get; set; }
        public string MANVNHAN { get; set; }
        public string NOINHAN { get; set; }
        public string NOIDEN { get; set; }
        public string GHICHU { get; set; }
        public Nullable<System.DateTime> NGAYCN { get; set; }
    }

    public partial class NL_CTPHIEUNHAP_CHUYENModelsView
    {
        public string SOCTXUAT { get; set; }
        public string SOCTNHAP { get; set; }
        public string MAHTHUC { get; set; }
        public string MAVT { get; set; }
        public string HIEU { get; set; }
        public string LO { get; set; }
        public string MADH { get; set; }
        public string LANXN { get; set; }
        public Nullable<decimal> SOLUONGXUAT { get; set; }
        public string GHICHU { get; set; }
        public Nullable<System.DateTime> NGAYCN { get; set; }
        public string MANVCN { get; set; }
        public System.Guid id_kHOA { get; set; }
    }
    #region TBL Kiểm tra 
    public partial class ITBLKIEMTRACLViewModels
    {
        public string SOCT { get; set; }
        public string MAVT { get; set; }
        public decimal THUC { get; set; }
        public double PL { get; set; }
        public string KHOKIEMTRA { get; set; }
        public string DANGLOI { get; set; }
        public int KEQUA { get; set; }
        public string NGUOIKT { get; set; }
        public string LOAI { get; set; }
        public string NHACC { get; set; }
        public string MAPHIEUPD { get; set; }
        public ITBLKIEMTRACLViewModels()
        {
            this.Detail = new List<ITBLKIEMTRACLViewModels>();
        }
        public List<ITBLKIEMTRACLViewModels> Detail { get; set; }
    }
    #endregion
    #region NL Chất lượng kiểm tra
    public partial class NL_CHATLUONGKIEMTRAViewModels
    {
        public string SOCTXN { get; set; }
        public string MAVT { get; set; }
        public decimal THUC { get; set; }
        public string LO { get; set; }
        public string KHOKIEMTRA { get; set; }
        public string DANGLOI { get; set; }
        public string HIEU { get; set; }
        public string DONVIKIEMTRA { get; set; }
        public string LOAI { get; set; }
        public string NHACC { get; set; }
        public string MANVKHO { get; set; }
        public string MANVDONVI { get; set; }
        public string MANHACC { get; set; }
        public string MAPHIEUPD { get; set; }
        public NL_CHATLUONGKIEMTRAViewModels()
        {
            this.Detail = new List<NL_CHATLUONGKIEMTRAViewModels>();
        }
        public List<NL_CHATLUONGKIEMTRAViewModels> Detail { get; set; }
    }
    #endregion
    #region Chất lượng
    public partial class CHATLUONGViewModels
    {
        public string SOCT { get; set; }
        public Nullable<System.DateTime> NGAYKHO { get; set; }
        public Nullable<System.DateTime> NGAYKT { get; set; }
        public string THUKHO { get; set; }
        public string GDDV { get; set; }
        public Nullable<bool> KHOXN { get; set; }
        public Nullable<bool> DVXN { get; set; }
        public Nullable<bool> DaIn { get; set; }
        public byte FlagXoa { get; set; }
        public Nullable<bool> DVKT { get; set; }
        public CHATLUONGViewModels()
        {
            this.Detail = new List<CHATLUONGViewModels>();
        }
        public List<CHATLUONGViewModels> Detail { get; set; }
    }
    #endregion
    public partial class DONHANGGCSOIModels
    {
        public DONHANGGCSOIModels()
        {
            this.Detail = new List<DONHANGGCSOI_CTModels>();
        }
        public List<DONHANGGCSOI_CTModels> Detail { get; set; }
        public string MADH_GCSOI { get; set; }
        public string LOAIGIACONG { get; set; }
        public string IDMADV_YC { get; set; }
        public string IDMADV_TH { get; set; }
        public string MANVCN { get; set; }
        public Nullable<System.DateTime> NGAYCAPNHAT { get; set; }
        public string MANV_DONVIYC_XX { get; set; }
        public Nullable<System.DateTime> NGAYXEMXET { get; set; }
        public Nullable<bool> HIEULUCXX { get; set; }
        public string MANV_DONVITH_XACNHAN { get; set; }
        public Nullable<System.DateTime> NGAYXACNHAN { get; set; }
        public Nullable<bool> HIEULUCXACNHAN { get; set; }
    }
    public partial class DONHANGGCSOI_CTModels
    {
        public string MADH_GCSOI { get; set; }
        public string Loaidh { get; set; }
        public Nullable<System.DateTime> NGAYYCAU { get; set; }
        public Nullable<System.DateTime> NGAYKETTHUC { get; set; }
        public string DIENGIAI { get; set; }
        public Nullable<decimal> SOLUONGYEUCAU { get; set; }
        public string MASOITHANHPHAM { get; set; }
        public System.Guid KHOA_ID { get; set; }
        public string NGAYYEUCAU_ { get; set; }
        public string NGAYKETTHUC_ { get; set; }
        public bool Delete { get; set; }
    }
    public partial class GC_DMPHIEU_BANGKENHAPSOIModels
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public GC_DMPHIEU_BANGKENHAPSOIModels()
        {
            this.Detail = new List<GC_CTPHIEUXUATGCModels>();
        }

        public string SOPHIEU { get; set; }
        public string MADV { get; set; }
        public Nullable<int> SO { get; set; }
        public Nullable<int> NAM { get; set; }
        public Nullable<int> THANG { get; set; }
        public string MAKH { get; set; }
        public string NHAPKHO { get; set; }
        public string LYDOSANXUAT { get; set; }
        public string MALOAICONGDOAN { get; set; }
        public string CONGDOAN { get; set; }
        public string MANV { get; set; }
        public string LoaiYCSX { get; set; }
        public Nullable<System.DateTime> NGAYLAP { get; set; }
        public string MANV_XX { get; set; }
        public Nullable<bool> HIEULUC_XX { get; set; }
        public Nullable<System.DateTime> NGAY_XX { get; set; }
        public string MANV_KT { get; set; }
        public Nullable<bool> HIEULUC_KT { get; set; }
        public Nullable<System.DateTime> NGAY_KT { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<GC_CTPHIEUXUATGCModels> Detail { get; set; }
    }
    public partial class GC_CTPHIEUXUATGCModels
    {
        public string SOPHIEU { get; set; }
        public Nullable<int> STT { get; set; }
        public string KHOACTHUC { get; set; }
        public string MADH_GCSOI { get; set; }
        public string SOCTXN { get; set; }
        public string MASOI { get; set; }
        public string HIEU { get; set; }
        public string LO { get; set; }
        public string MASOIHOANTAT { get; set; }
        public string MASOICHAP { get; set; }
        public Nullable<decimal> SOLUONGNhap { get; set; }
        public Nullable<decimal> SOLUONGXuat { get; set; }
        public Nullable<decimal> SLHaoHutDM { get; set; }
        public string DVT { get; set; }
        public Nullable<int> SOTRUC { get; set; }
        public Nullable<decimal> SOCANH { get; set; }
        public Nullable<decimal> KHOTRUC { get; set; }
        public Nullable<decimal> CHIEUDAI { get; set; }
        public Nullable<decimal> DONGIA { get; set; }
        public Nullable<decimal> GIATRI { get; set; }
        public string GHICHU { get; set; }
        public string MACONGDOAN { get; set; }
        public string DANGCONGDOAN { get; set; }
        public string Khoa_CTphieuGC { get; set; }
        public bool Delete { get; set; }

        public virtual GC_DMPHIEU_BANGKENHAPSOIModels GC_DMPHIEU_BANGKENHAPSOI { get; set; }
    }

    #region Invoice
    public partial class InvoiceViewModel
    {
        public InvoiceViewModel()
        {
            this.Detail = new List<CTInvoiceViewModel>();
        }
        public List<CTInvoiceViewModel> Detail { get; set; }
        public string MAPHIEU { get; set; }
        public string MAPHIEUPD { get; set; }
        public string SOHOPDONG { get; set; }
        public string MAVATTU { get; set; }
        public string TENVT { get; set; }
        public string INVOICE1 { get; set; }
        public string TOKHAI { get; set; }
        public Nullable<decimal> GIANHAP { get; set; }
        public string QUOCGIA { get; set; }
        public string NGAYNHAPHANG_ { get; set; }
        public Nullable<System.DateTime> NGAYNHAPHANG { get; set; }
        public string MASPKHACH { get; set; }
        public string MAKH { get; set; }
        public string MANVCAPNHAT { get; set; }
        public Nullable<System.DateTime> NGAYCAPNHAT { get; set; }
        public string MANVMODIFY { get; set; }
        public Nullable<System.DateTime> NGAYMODIFY { get; set; }
        public System.Guid khoa_id { get; set; }
    }
    public partial class CTInvoiceViewModel
    {
        public string MAPHIEU { get; set; }
        public string MAPHIEUPD { get; set; }
        public string SOHOPDONG { get; set; }
        public string MAVATTU { get; set; }
        public string TENVT { get; set; }
        public string INVOICE1 { get; set; }
        public string TOKHAI { get; set; }
        public Nullable<decimal> GIANHAP { get; set; }
        public string QUOCGIA { get; set; }
        public string NGAYNHAPHANG_ { get; set; }
        public Nullable<System.DateTime> NGAYNHAPHANG { get; set; }
        public string MASPKHACH { get; set; }
        public string MAKH { get; set; }
        public string MANVCAPNHAT { get; set; }
        public Nullable<System.DateTime> NGAYCAPNHAT { get; set; }
        public string MANVMODIFY { get; set; }
        public Nullable<System.DateTime> NGAYMODIFY { get; set; }
        public System.Guid khoa_id { get; set; }
    }
    #endregion

    public partial class NHAPDIENGIAIViewModels
    {
        public string KEYNHAP { get; set; }
        public string SOCTXN { get; set; }
        public string SOCTKETOAN { get; set; }
        public string SOHOADON { get; set; }
        public string MAVT { get; set; }
        public string TENVATTU_NCC { get; set; }
        public decimal SOLUONG_NCC { get; set; }
        public string DVT_NCC { get; set; }
        public Nullable<System.DateTime> NGAY_NCC { get; set; }
        public string MAPHIEUPD { get; set; }
        public string KHOAKEYXN { get; set; }
        public string NGAYNCC_ { get; set; }
        public string NGAY_ { get; set; }
        public Nullable<System.DateTime> NGAYNCC { get; set; }
    }

    public partial class Ticket
    {
        public string type { get; set; }
        public string hid { get; set; }
        public string token { get; set; }
        public string root_id { get; set; }
    }

    public partial class RootObject
    {
        public int code { get; set; }
        public string message { get; set; }
        public Ticket ticket { get; set; }
    }



    public partial class ChangePasswordViewModel
    {
        public string ChangePwd_MANV { get; set; }
        public string ChangePwd_Password { get; set; }
        public string ReChangePwd_Password { get; set; }
    }

    public partial class BangKeModel
    {
        public string MAPHIEUPD { get; set; }
        public string MAVT { get; set; }
        public string TENVT { get; set; }
        public Decimal SLPD { get; set; }
        public string DVT { get; set; }
        public string GHICHU { get; set; }
        public DateTime THOIDIEMDAPUNG { get; set; }
    }


    public partial class TABLE_LUUTRU_SOHOADONViewModel
    {
        public int ID_SOCTXN { get; set; }
        public List<string> SOCTXN { get; set; }   // Cho phép chọn nhiều
        public string PO { get; set; }
        public string PR { get; set; }
        [Required(ErrorMessage = "Bạn phải nhập ký hiệu hóa đơn!")]
        public string kyhieuhoadon { get; set; }
        [Required(ErrorMessage = "Bạn phải nhập số hóa đơn!")]
        public string sohoadon { get; set; }
        public string LinkHD { get; set; }
        public string TenHinhAnh { get; set; }
        public byte[] HinhAnh { get; set; }
        public string HinhAnhBase64 { get; set; }
        public string FileNamePDF { get; set; }
        public string ContentTypePDF { get; set; }
        // Dùng khi nhận từ giao diện dạng base64
        public string FileDataPDFBase64 { get; set; }
        // Dùng khi lưu vào DB
        public byte[] FileDataPDF { get; set; }
        public Nullable<int> THANG { get; set; }
        public Nullable<int> NAM { get; set; }
        public string LoaiPhieu { get; set; }
        public string Thumuc { get; set; }
        public Nullable<System.DateTime> NGAYHD { get; set; }
        public string NGAYHD_ { get; set; }

        public string MANVCN { get; set; }
        public string GhiChu { get; set; }

        public Nullable<System.DateTime> NGAYKHAI { get; set; }
        public string NGAYKHAI_ { get; set; }
        
        public string TOKHAI { get; set; }
        public string MST { get; set; }
        public string XUATXU { get; set; }
        public string NgoaiTe { get; set; }
        public Nullable<decimal> TyGia_NgoaiTe { get; set; }
        public Nullable<System.DateTime> NGAYCN { get; set; }

        public HttpPostedFileBase fileHinhAnh { get; set; }
        public HttpPostedFileBase fileHinhAnhs { get; set; }



    }
    public class FileUpload
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public byte[] FileData { get; set; }
    }

    public class NetsuiteToken
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public string token_type { get; set; }
    }

    public class NetsuiteStockDto
    {
        public string internalId { get; set; }
        public string item { get; set; }
        public decimal quantityOnHand { get; set; }
        public string location { get; set; }
        public string quality { get; set; }
    }
    public class SOAPResultViewModel
    {
        public int status { get; set; }
        public string text { get; set; }

    }

}