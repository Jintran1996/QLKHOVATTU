using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ToolsApp.Models
{
    public class SearchHoSoNhanVienModel
    {
        public string EmpCodeSearch { get; set; }
        public Nullable<System.Guid> GuidNhanVienIdSearch { get; set; }
        public string SAPNoSearch { get; set; }
        public string HoTenSearch { get; set; }
        public string SoDienThoaiSearch { get; set; }
        public string CMNDSearch { get; set; }
        public string MaTinhThanhSearch { get; set; }
        public string MaQuanHuyenSearch { get; set; }
        public string MaPhuongXaSearch { get; set; }
        public string TheBHYTeSearch { get; set; }
        public int? page { get; set; }
    }

    public class SearchDMPhieuYCMuaHang
    {
        public string LoaiVTSearch { get; set; }
        public string MaphieuPDSearch { get; set; }
        public string MaphieuSearch { get; set; }
        public string TuNgaySearch { get; set; }
        public string DenNgaySearch { get; set; }

        public DateTime? TuNgaySearch_ { get; set; }
        public DateTime? DenNgaySearch_ { get; set; }

        public int? page { get; set; }
    }

    public class SearchDanhMucKho
    {
        public string maKhoSearch { get; set; }
        public string tenKhoSearch { get; set; }
        public string maKhoKTSearch { get; set; }
        public int? page { get; set; }
    }

    public class SearchPYCXuatSoi
    {
        public string MaphieuSearch { get; set; }
        public int? page { get; set; }
    }

    public class SearchPhieuMuaHang
    {
        public string SOPHIEUSEARCH { get; set; }
        public string SHOWROOMSEARCH { get; set; }
        public List<string> SHOWROOMSEARCH_ { get; set; }
        public int? page { get; set; }
    }
    public class SearchTraCuuXNKho
    {
        public List<string> MAKHOSEARCH_ { get; set; }
        public string MAKHOSEARCH { get; set; }
        public string MAVTSearch { get; set; }
        public DateTime? TUNGAY_ { get; set; }
        public string TUNGAY { get; set; }
        public DateTime? DENNGAY_ { get; set; }
        public string DENNGAY { get; set; }
        public int? page { get; set; }
    }

    public class SearchTraCuuTheoDoiTienDoMuaHang
    {
     //   public List<string> MAPHIEUPDSearch { get; set; }
        public string MAPHIEUPDSearch { get; set; }
        public string MAKHOSEARCH { get; set; }
        public string MAVTSearch { get; set; }
        public DateTime? TUNGAY_ { get; set; }
        public string TUNGAYs { get; set; }
        public DateTime? DENNGAY_ { get; set; }
        public string DENNGAYs { get; set; }
        public string searchType { get; set; }
        public string weekValue { get; set; }
    }
}
