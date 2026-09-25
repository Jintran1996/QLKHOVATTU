using AutoMapper;
using ToolsApp.Models;
using ToolsApp.EntityFramework.KhoSoi;
using ToolsApp.EntityFramework.VatTu;
using System.Collections.Generic;

namespace ToolsApp.App_Start
{
    public static class Mapper
    {
        private static IMapper _mapper;
        public static void RegisterMappings()
        {
            var mapperConfiguration = new MapperConfiguration(cfg =>
            {
                #region User
                cfg.CreateMap<QLKhoSoi_User, HomeUserViewModel>()
                                .ForMember(dto => dto.Id, opt => opt.MapFrom(src => src.Id))
                                .ForMember(dto => dto.Username, opt => opt.MapFrom(src => src.Username))
                                .ForMember(dto => dto.SalesRepCode, opt => opt.MapFrom(src => src.SalesRepCode))
                                .ForMember(dto => dto.Fullname, opt => opt.MapFrom(src => src.Fullname))
                                .ForMember(dto => dto.Email, opt => opt.MapFrom(src => src.Email))
                                .ForMember(dto => dto.UserCreate, opt => opt.MapFrom(src => src.UserCreate))
                                .ForMember(dto => dto.UserFullnameCreate, opt => opt.MapFrom(src => src.UserFullnameCreate))
                                .ForMember(dto => dto.DatetimeCreate, opt => opt.MapFrom(src => src.DatetimeCreate))
                                .ForMember(dto => dto.UserUpdate, opt => opt.MapFrom(src => src.UserUpdate))
                                .ForMember(dto => dto.UserFullnameUpdate, opt => opt.MapFrom(src => src.UserFullnameUpdate))
                                .ForMember(dto => dto.DatetimeUpdate, opt => opt.MapFrom(src => src.DatetimeUpdate))
                                .ForMember(dto => dto.isDelete, opt => opt.MapFrom(src => src.isDelete))
                                .ForMember(dto => dto.UserDelete, opt => opt.MapFrom(src => src.UserDelete))
                                .ForMember(dto => dto.UserFullnameDelete, opt => opt.MapFrom(src => src.UserFullnameDelete))
                                .ForMember(dto => dto.DatetimeDelete, opt => opt.MapFrom(src => src.DatetimeDelete));

                cfg.CreateMap<HomeUserViewModel, QLKhoSoi_User>()
                                .ForMember(dto => dto.Id, opt => opt.MapFrom(src => src.Id))
                                .ForMember(dto => dto.Username, opt => opt.MapFrom(src => src.Username))
                                .ForMember(dto => dto.SalesRepCode, opt => opt.MapFrom(src => src.SalesRepCode))
                                .ForMember(dto => dto.Fullname, opt => opt.MapFrom(src => src.Fullname))
                                .ForMember(dto => dto.Email, opt => opt.MapFrom(src => src.Email))
                                .ForMember(dto => dto.UserCreate, opt => opt.MapFrom(src => src.UserCreate))
                                .ForMember(dto => dto.UserFullnameCreate, opt => opt.MapFrom(src => src.UserFullnameCreate))
                                .ForMember(dto => dto.DatetimeCreate, opt => opt.MapFrom(src => src.DatetimeCreate))
                                .ForMember(dto => dto.UserUpdate, opt => opt.MapFrom(src => src.UserUpdate))
                                .ForMember(dto => dto.UserFullnameUpdate, opt => opt.MapFrom(src => src.UserFullnameUpdate))
                                .ForMember(dto => dto.DatetimeUpdate, opt => opt.MapFrom(src => src.DatetimeUpdate))
                                .ForMember(dto => dto.isDelete, opt => opt.MapFrom(src => src.isDelete))
                                .ForMember(dto => dto.UserDelete, opt => opt.MapFrom(src => src.UserDelete))
                                .ForMember(dto => dto.UserFullnameDelete, opt => opt.MapFrom(src => src.UserFullnameDelete))
                                .ForMember(dto => dto.DatetimeDelete, opt => opt.MapFrom(src => src.DatetimeDelete));
                #endregion

                #region Page
                cfg.CreateMap<QLKhoSoi_Page, HomePageViewModel>()
                                .ForMember(dto => dto.Id, opt => opt.MapFrom(src => src.Id))
                                .ForMember(dto => dto.Controler, opt => opt.MapFrom(src => src.Controler))
                                .ForMember(dto => dto.Action, opt => opt.MapFrom(src => src.Action))
                                .ForMember(dto => dto.Code, opt => opt.MapFrom(src => src.Code))
                                .ForMember(dto => dto.Info, opt => opt.MapFrom(src => src.Info))
                                .ForMember(dto => dto.UserCreate, opt => opt.MapFrom(src => src.UserCreate))
                                .ForMember(dto => dto.UserFullnameCreate, opt => opt.MapFrom(src => src.UserFullnameCreate))
                                .ForMember(dto => dto.DatetimeCreate, opt => opt.MapFrom(src => src.DatetimeCreate))
                                .ForMember(dto => dto.UserUpdate, opt => opt.MapFrom(src => src.UserUpdate))
                                .ForMember(dto => dto.UserFullnameUpdate, opt => opt.MapFrom(src => src.UserFullnameUpdate))
                                .ForMember(dto => dto.DatetimeUpdate, opt => opt.MapFrom(src => src.DatetimeUpdate))
                                .ForMember(dto => dto.isDelete, opt => opt.MapFrom(src => src.isDelete))
                                .ForMember(dto => dto.UserDelete, opt => opt.MapFrom(src => src.UserDelete))
                                .ForMember(dto => dto.UserFullnameDelete, opt => opt.MapFrom(src => src.UserFullnameDelete))
                                .ForMember(dto => dto.DatetimeDelete, opt => opt.MapFrom(src => src.DatetimeDelete));

                cfg.CreateMap<HomePageViewModel, QLKhoSoi_Page>()
                                .ForMember(dto => dto.Id, opt => opt.MapFrom(src => src.Id))
                                .ForMember(dto => dto.Controler, opt => opt.MapFrom(src => src.Controler))
                                .ForMember(dto => dto.Action, opt => opt.MapFrom(src => src.Action))
                                .ForMember(dto => dto.Code, opt => opt.MapFrom(src => src.Code))
                                .ForMember(dto => dto.Info, opt => opt.MapFrom(src => src.Info))
                                .ForMember(dto => dto.UserCreate, opt => opt.MapFrom(src => src.UserCreate))
                                .ForMember(dto => dto.UserFullnameCreate, opt => opt.MapFrom(src => src.UserFullnameCreate))
                                .ForMember(dto => dto.DatetimeCreate, opt => opt.MapFrom(src => src.DatetimeCreate))
                                .ForMember(dto => dto.UserUpdate, opt => opt.MapFrom(src => src.UserUpdate))
                                .ForMember(dto => dto.UserFullnameUpdate, opt => opt.MapFrom(src => src.UserFullnameUpdate))
                                .ForMember(dto => dto.DatetimeUpdate, opt => opt.MapFrom(src => src.DatetimeUpdate))
                                .ForMember(dto => dto.isDelete, opt => opt.MapFrom(src => src.isDelete))
                                .ForMember(dto => dto.UserDelete, opt => opt.MapFrom(src => src.UserDelete))
                                .ForMember(dto => dto.UserFullnameDelete, opt => opt.MapFrom(src => src.UserFullnameDelete))
                                .ForMember(dto => dto.DatetimeDelete, opt => opt.MapFrom(src => src.DatetimeDelete));
                #endregion

                #region Role
                cfg.CreateMap<QLKhoSoi_Role, HomeRoleViewModel>()
                                .ForMember(dto => dto.Id, opt => opt.MapFrom(src => src.Id))
                                .ForMember(dto => dto.RoleName, opt => opt.MapFrom(src => src.RoleName))
                                .ForMember(dto => dto.Code, opt => opt.MapFrom(src => src.Code))
                                .ForMember(dto => dto.UserCreate, opt => opt.MapFrom(src => src.UserCreate))
                                .ForMember(dto => dto.UserFullnameCreate, opt => opt.MapFrom(src => src.UserFullnameCreate))
                                .ForMember(dto => dto.DatetimeCreate, opt => opt.MapFrom(src => src.DatetimeCreate))
                                .ForMember(dto => dto.UserUpdate, opt => opt.MapFrom(src => src.UserUpdate))
                                .ForMember(dto => dto.UserFullnameUpdate, opt => opt.MapFrom(src => src.UserFullnameUpdate))
                                .ForMember(dto => dto.DatetimeUpdate, opt => opt.MapFrom(src => src.DatetimeUpdate))
                                .ForMember(dto => dto.isDelete, opt => opt.MapFrom(src => src.isDelete))
                                .ForMember(dto => dto.UserDelete, opt => opt.MapFrom(src => src.UserDelete))
                                .ForMember(dto => dto.UserFullnameDelete, opt => opt.MapFrom(src => src.UserFullnameDelete))
                                .ForMember(dto => dto.DatetimeDelete, opt => opt.MapFrom(src => src.DatetimeDelete));

                cfg.CreateMap<HomeRoleViewModel, QLKhoSoi_Role>()
                                .ForMember(dto => dto.Id, opt => opt.MapFrom(src => src.Id))
                                .ForMember(dto => dto.RoleName, opt => opt.MapFrom(src => src.RoleName))
                                .ForMember(dto => dto.Code, opt => opt.MapFrom(src => src.Code))
                                .ForMember(dto => dto.UserCreate, opt => opt.MapFrom(src => src.UserCreate))
                                .ForMember(dto => dto.UserFullnameCreate, opt => opt.MapFrom(src => src.UserFullnameCreate))
                                .ForMember(dto => dto.DatetimeCreate, opt => opt.MapFrom(src => src.DatetimeCreate))
                                .ForMember(dto => dto.UserUpdate, opt => opt.MapFrom(src => src.UserUpdate))
                                .ForMember(dto => dto.UserFullnameUpdate, opt => opt.MapFrom(src => src.UserFullnameUpdate))
                                .ForMember(dto => dto.DatetimeUpdate, opt => opt.MapFrom(src => src.DatetimeUpdate))
                                .ForMember(dto => dto.isDelete, opt => opt.MapFrom(src => src.isDelete))
                                .ForMember(dto => dto.UserDelete, opt => opt.MapFrom(src => src.UserDelete))
                                .ForMember(dto => dto.UserFullnameDelete, opt => opt.MapFrom(src => src.UserFullnameDelete))
                                .ForMember(dto => dto.DatetimeDelete, opt => opt.MapFrom(src => src.DatetimeDelete));
                #endregion

                #region Menu
                cfg.CreateMap<QLKhoSoi_Menu, HomeMenuViewModel>()
                                .ForMember(dto => dto.Id, opt => opt.MapFrom(src => src.Id))
                                .ForMember(dto => dto.MenuName, opt => opt.MapFrom(src => src.MenuName))
                                .ForMember(dto => dto.ParentId, opt => opt.MapFrom(src => src.ParentId))
                                .ForMember(dto => dto.PageId, opt => opt.MapFrom(src => src.PageId))
                                .ForMember(dto => dto.Icon, opt => opt.MapFrom(src => src.Icon))
                                .ForMember(dto => dto.OrderNo, opt => opt.MapFrom(src => src.OrderNo))
                                .ForMember(dto => dto.UserCreate, opt => opt.MapFrom(src => src.UserCreate))
                                .ForMember(dto => dto.UserFullnameCreate, opt => opt.MapFrom(src => src.UserFullnameCreate))
                                .ForMember(dto => dto.DatetimeCreate, opt => opt.MapFrom(src => src.DatetimeCreate))
                                .ForMember(dto => dto.UserUpdate, opt => opt.MapFrom(src => src.UserUpdate))
                                .ForMember(dto => dto.UserFullnameUpdate, opt => opt.MapFrom(src => src.UserFullnameUpdate))
                                .ForMember(dto => dto.DatetimeUpdate, opt => opt.MapFrom(src => src.DatetimeUpdate))
                                .ForMember(dto => dto.isDelete, opt => opt.MapFrom(src => src.isDelete))
                                .ForMember(dto => dto.UserDelete, opt => opt.MapFrom(src => src.UserDelete))
                                .ForMember(dto => dto.UserFullnameDelete, opt => opt.MapFrom(src => src.UserFullnameDelete))
                                .ForMember(dto => dto.DatetimeDelete, opt => opt.MapFrom(src => src.DatetimeDelete));

                cfg.CreateMap<HomeMenuViewModel, QLKhoSoi_Menu>()
                                .ForMember(dto => dto.Id, opt => opt.MapFrom(src => src.Id))
                                .ForMember(dto => dto.MenuName, opt => opt.MapFrom(src => src.MenuName))
                                .ForMember(dto => dto.ParentId, opt => opt.MapFrom(src => src.ParentId))
                                .ForMember(dto => dto.PageId, opt => opt.MapFrom(src => src.PageId))
                                .ForMember(dto => dto.Icon, opt => opt.MapFrom(src => src.Icon))
                                .ForMember(dto => dto.OrderNo, opt => opt.MapFrom(src => src.OrderNo))
                                .ForMember(dto => dto.UserCreate, opt => opt.MapFrom(src => src.UserCreate))
                                .ForMember(dto => dto.UserFullnameCreate, opt => opt.MapFrom(src => src.UserFullnameCreate))
                                .ForMember(dto => dto.DatetimeCreate, opt => opt.MapFrom(src => src.DatetimeCreate))
                                .ForMember(dto => dto.UserUpdate, opt => opt.MapFrom(src => src.UserUpdate))
                                .ForMember(dto => dto.UserFullnameUpdate, opt => opt.MapFrom(src => src.UserFullnameUpdate))
                                .ForMember(dto => dto.DatetimeUpdate, opt => opt.MapFrom(src => src.DatetimeUpdate))
                                .ForMember(dto => dto.isDelete, opt => opt.MapFrom(src => src.isDelete))
                                .ForMember(dto => dto.UserDelete, opt => opt.MapFrom(src => src.UserDelete))
                                .ForMember(dto => dto.UserFullnameDelete, opt => opt.MapFrom(src => src.UserFullnameDelete))
                                .ForMember(dto => dto.DatetimeDelete, opt => opt.MapFrom(src => src.DatetimeDelete));
                #endregion


                #region DMPHIEU
                cfg.CreateMap<DMPHIEU, DMPHIEUViewModels>()
                                      .ForMember(dto => dto.MAPHIEU, opt => opt.MapFrom(src => src.MAPHIEU))
                                      .ForMember(dto => dto.NguoiPD, opt => opt.MapFrom(src => src.NguoiPD))
                                      .ForMember(dto => dto.NguoiXX, opt => opt.MapFrom(src => src.NguoiXX))
                                      .ForMember(dto => dto.NguoiLapPhieu, opt => opt.MapFrom(src => src.NguoiLapPhieu))
                                      .ForMember(dto => dto.LyDoSD, opt => opt.MapFrom(src => src.LyDoSD))
                                      .ForMember(dto => dto.NguoiTiepNhan, opt => opt.MapFrom(src => src.NguoiTiepNhan))
                                      .ForMember(dto => dto.Nam, opt => opt.MapFrom(src => src.Nam))
                                      .ForMember(dto => dto.LoaiVT, opt => opt.MapFrom(src => src.LoaiVT))
                                      .ForMember(dto => dto.LOAIPHIEU, opt => opt.MapFrom(src => src.LOAIPHIEU));

                cfg.CreateMap<DMPHIEUViewModels, DMPHIEU>()
                                       .ForMember(dto => dto.MAPHIEU, opt => opt.MapFrom(src => src.MAPHIEU))
                                      .ForMember(dto => dto.NguoiPD, opt => opt.MapFrom(src => src.NguoiPD))
                                      .ForMember(dto => dto.NguoiXX, opt => opt.MapFrom(src => src.NguoiXX))
                                      .ForMember(dto => dto.NguoiLapPhieu, opt => opt.MapFrom(src => src.NguoiLapPhieu))
                                      .ForMember(dto => dto.LyDoSD, opt => opt.MapFrom(src => src.LyDoSD))
                                      .ForMember(dto => dto.NguoiTiepNhan, opt => opt.MapFrom(src => src.NguoiTiepNhan))
                                      .ForMember(dto => dto.Nam, opt => opt.MapFrom(src => src.Nam))
                                      .ForMember(dto => dto.LoaiVT, opt => opt.MapFrom(src => src.LoaiVT))
                                      .ForMember(dto => dto.LOAIPHIEU, opt => opt.MapFrom(src => src.LOAIPHIEU));
                #endregion

                #region CTPHIEU
                cfg.CreateMap<CTPHIEU, CTPHIEUViewModels>()
                                      .ForMember(dto => dto.MAPHIEU, opt => opt.MapFrom(src => src.MAPHIEU))
                                      .ForMember(dto => dto.MASOCP, opt => opt.MapFrom(src => src.MASOCP))
                                      .ForMember(dto => dto.IDKHOA, opt => opt.MapFrom(src => src.IDKHOA))
                                      .ForMember(dto => dto.GHICHU, opt => opt.MapFrom(src => src.GHICHU))
                                      .ForMember(dto => dto.DACTINHKYTHUAT, opt => opt.MapFrom(src => src.DACTINHKYTHUAT))
                                      .ForMember(dto => dto.SLYCAU, opt => opt.MapFrom(src => src.SLYCAU))
                                      .ForMember(dto => dto.MAVT, opt => opt.MapFrom(src => src.MAVT))
                                      .ForMember(dto => dto.THOIDIEMSD, opt => opt.MapFrom(src => src.THOIDIEMSD))
                                      .ForMember(dto => dto.STT, opt => opt.MapFrom(src => src.STT))
                                      .ForMember(dto => dto.SLPD, opt => opt.MapFrom(src => src.SLPD))
                                      .ForMember(dto => dto.SLTONDVI, opt => opt.MapFrom(src => src.SLTONDVI))
                                      .ForMember(dto => dto.TINHTRANG, opt => opt.MapFrom(src => src.TINHTRANG))
                                      .ForMember(dto => dto.THOIDIEMDAPUNG, opt => opt.MapFrom(src => src.THOIDIEMDAPUNG))
                                      .ForMember(dto => dto.YKIENLANHDAO, opt => opt.MapFrom(src => src.SLXXET))
                                      .ForMember(dto => dto.MaphieuchuyenNB, opt => opt.MapFrom(src => src.MaphieuchuyenNB))
                                      .ForMember(dto => dto.MADH, opt => opt.MapFrom(src => src.MADH))
                                      .ForMember(dto => dto.IsLOT, opt => opt.MapFrom(src => src.IsLOT))
                                      ;

                cfg.CreateMap<CTPHIEUViewModels, CTPHIEU>()
                                      .ForMember(dto => dto.MAPHIEU, opt => opt.MapFrom(src => src.MAPHIEU))
                                      .ForMember(dto => dto.MASOCP, opt => opt.MapFrom(src => src.MASOCP))
                                      .ForMember(dto => dto.IDKHOA, opt => opt.MapFrom(src => src.IDKHOA))
                                      .ForMember(dto => dto.GHICHU, opt => opt.MapFrom(src => src.GHICHU))
                                      .ForMember(dto => dto.DACTINHKYTHUAT, opt => opt.MapFrom(src => src.DACTINHKYTHUAT))
                                      .ForMember(dto => dto.SLYCAU, opt => opt.MapFrom(src => src.SLYCAU))
                                      .ForMember(dto => dto.MAVT, opt => opt.MapFrom(src => src.MAVT))
                                      .ForMember(dto => dto.THOIDIEMSD, opt => opt.MapFrom(src => src.THOIDIEMSD))
                                      .ForMember(dto => dto.STT, opt => opt.MapFrom(src => src.STT))
                                      .ForMember(dto => dto.SLPD, opt => opt.MapFrom(src => src.SLPD))
                                      .ForMember(dto => dto.SLTONDVI, opt => opt.MapFrom(src => src.SLTONDVI))
                                      .ForMember(dto => dto.TINHTRANG, opt => opt.MapFrom(src => src.TINHTRANG))
                                      .ForMember(dto => dto.THOIDIEMDAPUNG, opt => opt.MapFrom(src => src.THOIDIEMDAPUNG))
                                      .ForMember(dto => dto.YKIENLANHDAO, opt => opt.MapFrom(src => src.SLXXET))
                                      .ForMember(dto => dto.MaphieuchuyenNB, opt => opt.MapFrom(src => src.MaphieuchuyenNB))
                                      .ForMember(dto => dto.MADH, opt => opt.MapFrom(src => src.MADH))
                                      .ForMember(dto => dto.IsLOT, opt => opt.MapFrom(src => src.IsLOT));
                #endregion

                #region SP_CHIPHI_28062016
                cfg.CreateMap<SP_CHIPHI_28062016_Result, SP_CHIPHI_28062016ViewModels>()
                                      .ForMember(dto => dto.MSCHIPHI, opt => opt.MapFrom(src => src.MSCHIPHI))
                                      .ForMember(dto => dto.TENCHIPHI, opt => opt.MapFrom(src => src.TENCHIPHI))
                                      ;

                cfg.CreateMap<SP_CHIPHI_28062016ViewModels, SP_CHIPHI_28062016_Result>()
                                      .ForMember(dto => dto.MSCHIPHI, opt => opt.MapFrom(src => src.MSCHIPHI))
                                      .ForMember(dto => dto.TENCHIPHI, opt => opt.MapFrom(src => src.TENCHIPHI))
                                      ;
                #endregion

                cfg.CreateMap<XuatHCTNTheotrucDto, LOADXUATHCTNDto>().ReverseMap();
            });

            _mapper = mapperConfiguration.CreateMapper();
        }

        public static LOADXUATHCTNDto MapFrom(XuatHCTNTheotrucDto data)
        {
            return _mapper.Map<XuatHCTNTheotrucDto, LOADXUATHCTNDto>(data);
        }
        public static List<LOADXUATHCTNDto> MapListFrom(List<XuatHCTNTheotrucDto> data)
        {
            return _mapper.Map<List<XuatHCTNTheotrucDto>, List<LOADXUATHCTNDto>>(data);
        }

        #region User
        public static HomeUserViewModel MapFrom(QLKhoSoi_User data)
        {
            return _mapper.Map<QLKhoSoi_User, HomeUserViewModel>(data);
        }
        public static QLKhoSoi_User MapFrom(HomeUserViewModel data)
        {
            return _mapper.Map<HomeUserViewModel, QLKhoSoi_User>(data);
        }
        public static QLKhoSoi_User MapFrom(HomeUserViewModel data_view, QLKhoSoi_User data_entity)
        {
            return _mapper.Map(data_view, data_entity);
        }
        #endregion

        #region Page
        public static HomePageViewModel MapFrom(QLKhoSoi_Page data)
        {
            return _mapper.Map<QLKhoSoi_Page, HomePageViewModel>(data);
        }
        public static QLKhoSoi_Page MapFrom(HomePageViewModel data)
        {
            return _mapper.Map<HomePageViewModel, QLKhoSoi_Page>(data);
        }
        public static QLKhoSoi_Page MapFrom(HomePageViewModel data_view, QLKhoSoi_Page data_entity)
        {
            return _mapper.Map(data_view, data_entity);
        }
        #endregion

        #region Role
        public static HomeRoleViewModel MapFrom(QLKhoSoi_Role data)
        {
            return _mapper.Map<QLKhoSoi_Role, HomeRoleViewModel>(data);
        }
        public static QLKhoSoi_Role MapFrom(HomeRoleViewModel data)
        {
            return _mapper.Map<HomeRoleViewModel, QLKhoSoi_Role>(data);
        }
        public static QLKhoSoi_Role MapFrom(HomeRoleViewModel data_view, QLKhoSoi_Role data_entity)
        {
            return _mapper.Map(data_view, data_entity);
        }
        #endregion

        #region Menu
        public static HomeMenuViewModel MapFrom(QLKhoSoi_Menu data)
        {
            return _mapper.Map<QLKhoSoi_Menu, HomeMenuViewModel>(data);
        }
        public static QLKhoSoi_Menu MapFrom(HomeMenuViewModel data)
        {
            return _mapper.Map<HomeMenuViewModel, QLKhoSoi_Menu>(data);
        }
        public static QLKhoSoi_Menu MapFrom(HomeMenuViewModel data_view, QLKhoSoi_Menu data_entity)
        {
            return _mapper.Map(data_view, data_entity);
        }
        #endregion

        #region DM Phiếu YC Mua hàng
        public static DMPHIEUViewModels MapFrom(DMPHIEU data)
        {
            return _mapper.Map<DMPHIEU, DMPHIEUViewModels>(data);
        }
        public static DMPHIEU MapFrom(DMPHIEUViewModels data)
        {
            return _mapper.Map<DMPHIEUViewModels, DMPHIEU>(data);
        }
        public static DMPHIEU MapFrom(DMPHIEUViewModels data_view, DMPHIEU data_entity)
        {
            return _mapper.Map(data_view, data_entity);
        }
        #endregion


        #region CT Phiếu YC Mua hàng
        public static CTPHIEUViewModels MapFrom(CTPHIEU data)
        {
            return _mapper.Map<CTPHIEU, CTPHIEUViewModels>(data);
        }
        public static CTPHIEU MapFrom(CTPHIEUViewModels data)
        {
            return _mapper.Map<CTPHIEUViewModels, CTPHIEU>(data);
        }
        public static CTPHIEU MapFrom(CTPHIEUViewModels data_view, CTPHIEU data_entity)
        {
            return _mapper.Map(data_view, data_entity);
        }
        #endregion

        #region SP_CHIPHI_28062016
        public static SP_CHIPHI_28062016ViewModels MapFrom(SP_CHIPHI_28062016_Result data)
        {
            return _mapper.Map<SP_CHIPHI_28062016_Result, SP_CHIPHI_28062016ViewModels>(data);
        }
        public static SP_CHIPHI_28062016_Result MapFrom(SP_CHIPHI_28062016ViewModels data)
        {
            return _mapper.Map<SP_CHIPHI_28062016ViewModels, SP_CHIPHI_28062016_Result>(data);
        }
        public static SP_CHIPHI_28062016_Result MapFrom(SP_CHIPHI_28062016ViewModels data_view, SP_CHIPHI_28062016_Result data_entity)
        {
            return _mapper.Map(data_view, data_entity);
        }
        #endregion
    }
}