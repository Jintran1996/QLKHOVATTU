using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using ToolsApp.Models;

namespace ToolsApp.Helper
{
    public class StoreDatabaseHelper
    {
        public static string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["QLSX_Nhuom_AnhHongConnectionString"].ToString();
        }
        public async Task<List<XuatHCTNTheotrucDto>> GetXuatHCTNTheoTrucAsync()
        {
            var result = new List<XuatHCTNTheotrucDto>();

            string connectionString =
                GetConnectionString();

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();

                string sql = @"
            SELECT *
            FROM [dbo].[XuatHCTN_theotruc] where isnull(Xacnhan_NMN,'') = '' and isnull(QCxacnhan,'') = ''";

                using (var cmd = new SqlCommand(sql, conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var item = new XuatHCTNTheotrucDto
                        {
                            Matruc = reader["Matruc"]?.ToString(),
                            Matbi = reader["Matbi"]?.ToString(),
                            Macd = reader["Macd"]?.ToString(),
                            Macongthuc = reader["Macongthuc"]?.ToString(),
                            LoaiSX = reader["LoaiSX"]?.ToString(),
                            Masoqt = reader["Masoqt"]?.ToString(),

                            Sometmoc = reader["Sometmoc"] == DBNull.Value
                                ? (decimal?)null
                                : Convert.ToDecimal(reader["Sometmoc"]),

                            ChieudaiQT = reader["ChieudaiQT"] == DBNull.Value
                                ? (decimal?)null
                                : Convert.ToDecimal(reader["ChieudaiQT"]),

                            MetTP_tinhtoan = reader["MetTP_tinhtoan"] == DBNull.Value
                                ? (decimal?)null
                                : Convert.ToDecimal(reader["MetTP_tinhtoan"]),

                            Doco_lythuyet = reader["Doco_lythuyet"] == DBNull.Value
                                ? (decimal?)null
                                : Convert.ToDecimal(reader["Doco_lythuyet"]),

                            MaDH = reader["MaDH"]?.ToString(),
                            MaMH = reader["MaMH"]?.ToString(),
                            MaHC = reader["MaHC"]?.ToString(),
                            TenHC = reader["TenHC"]?.ToString(),

                            Nongdo = reader["Nongdo"] == DBNull.Value
                                ? (decimal?)null
                                : Convert.ToDecimal(reader["Nongdo"]),

                            DVT = reader["DVT"]?.ToString(),

                            KL_1TrucMoc = reader["KL_1TrucMoc"] == DBNull.Value
                                ? (decimal?)null
                                : Convert.ToDecimal(reader["KL_1TrucMoc"]),

                            Thoigian_batdau = reader["Thoigian_batdau"] == DBNull.Value
                                ? (DateTime?)null
                                : Convert.ToDateTime(reader["Thoigian_batdau"]),

                            KhoaID = reader["KhoaID"]?.ToString(),
                            Nguoichinhsua = reader["Nguoichinhsua"]?.ToString(),

                            Ngaychinhsua = reader["Ngaychinhsua"] == DBNull.Value
                                ? (DateTime?)null
                                : Convert.ToDateTime(reader["Ngaychinhsua"]),

                            Noidungsua = reader["Noidungsua"]?.ToString(),

                            Xacnhan_NMN = reader["Xacnhan_NMN"] == DBNull.Value
                                ? (bool?)null
                                : Convert.ToBoolean(reader["Xacnhan_NMN"]),

                            Manvxacnhan = reader["Manvxacnhan"]?.ToString(),
                            LayHCtu = reader["LayHCtu"]?.ToString(),

                            Ngayxacnhan = reader["Ngayxacnhan"] == DBNull.Value
                                ? (DateTime?)null
                                : Convert.ToDateTime(reader["Ngayxacnhan"]),

                            QCxacnhan = reader["QCxacnhan"] == DBNull.Value
                                ? (bool?)null
                                : Convert.ToBoolean(reader["QCxacnhan"]),

                            MANVqc_xacnhan = reader["MANVqc_xacnhan"]?.ToString(),

                            NgayQC_xacnhan = reader["NgayQC_xacnhan"] == DBNull.Value
                                ? (DateTime?)null
                                : Convert.ToDateTime(reader["NgayQC_xacnhan"])
                        };

                        result.Add(item);
                    }
                }
            }

            return result;
        }
        public async Task<List<XuatHCTNTheotrucDto>> GetXuatHCTNTheoTrucByMaTrucAsync(string maTruc)
        {
            var result = new List<XuatHCTNTheotrucDto>();

            string connectionString =
                GetConnectionString();

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();

                string sql = @"
            SELECT *
            FROM [dbo].[XuatHCTN_theotruc] where matruc = '" +  maTruc + "'";

                using (var cmd = new SqlCommand(sql, conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var item = new XuatHCTNTheotrucDto
                        {
                            Matruc = reader["Matruc"]?.ToString(),
                            Matbi = reader["Matbi"]?.ToString(),
                            Macd = reader["Macd"]?.ToString(),
                            Macongthuc = reader["Macongthuc"]?.ToString(),
                            LoaiSX = reader["LoaiSX"]?.ToString(),
                            Masoqt = reader["Masoqt"]?.ToString(),

                            Sometmoc = reader["Sometmoc"] == DBNull.Value
                                ? (decimal?)null
                                : Convert.ToDecimal(reader["Sometmoc"]),

                            ChieudaiQT = reader["ChieudaiQT"] == DBNull.Value
                                ? (decimal?)null
                                : Convert.ToDecimal(reader["ChieudaiQT"]),

                            MetTP_tinhtoan = reader["MetTP_tinhtoan"] == DBNull.Value
                                ? (decimal?)null
                                : Convert.ToDecimal(reader["MetTP_tinhtoan"]),

                            Doco_lythuyet = reader["Doco_lythuyet"] == DBNull.Value
                                ? (decimal?)null
                                : Convert.ToDecimal(reader["Doco_lythuyet"]),

                            MaDH = reader["MaDH"]?.ToString(),
                            MaMH = reader["MaMH"]?.ToString(),
                            MaHC = reader["MaHC"]?.ToString(),
                            TenHC = reader["TenHC"]?.ToString(),

                            Nongdo = reader["Nongdo"] == DBNull.Value
                                ? (decimal?)null
                                : Convert.ToDecimal(reader["Nongdo"]),

                            DVT = reader["DVT"]?.ToString(),

                            KL_1TrucMoc = reader["KL_1TrucMoc"] == DBNull.Value
                                ? (decimal?)null
                                : Convert.ToDecimal(reader["KL_1TrucMoc"]),

                            Thoigian_batdau = reader["Thoigian_batdau"] == DBNull.Value
                                ? (DateTime?)null
                                : Convert.ToDateTime(reader["Thoigian_batdau"]),

                            KhoaID = reader["KhoaID"]?.ToString(),
                            Nguoichinhsua = reader["Nguoichinhsua"]?.ToString(),

                            Ngaychinhsua = reader["Ngaychinhsua"] == DBNull.Value
                                ? (DateTime?)null
                                : Convert.ToDateTime(reader["Ngaychinhsua"]),

                            Noidungsua = reader["Noidungsua"]?.ToString(),

                            Xacnhan_NMN = reader["Xacnhan_NMN"] == DBNull.Value
                                ? (bool?)null
                                : Convert.ToBoolean(reader["Xacnhan_NMN"]),

                            Manvxacnhan = reader["Manvxacnhan"]?.ToString(),
                            LayHCtu = reader["LayHCtu"]?.ToString(),

                            Ngayxacnhan = reader["Ngayxacnhan"] == DBNull.Value
                                ? (DateTime?)null
                                : Convert.ToDateTime(reader["Ngayxacnhan"]),

                            QCxacnhan = reader["QCxacnhan"] == DBNull.Value
                                ? (bool?)null
                                : Convert.ToBoolean(reader["QCxacnhan"]),

                            MANVqc_xacnhan = reader["MANVqc_xacnhan"]?.ToString(),

                            NgayQC_xacnhan = reader["NgayQC_xacnhan"] == DBNull.Value
                                ? (DateTime?)null
                                : Convert.ToDateTime(reader["NgayQC_xacnhan"])
                        };

                        result.Add(item);
                    }
                }
            }

            return result;
        }

        public async Task<XuatHCTNTheotrucDto> GetXuatHCTNTheoTrucByIDKhoaAsync(string Id)
        {
            var result = new XuatHCTNTheotrucDto();

            string connectionString =
                GetConnectionString();

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();

                string sql = @"
            SELECT *
            FROM [dbo].[XuatHCTN_theotruc] where KhoaID = '" + Id + "'";

                using (var cmd = new SqlCommand(sql, conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                         result = new XuatHCTNTheotrucDto
                        {
                            Matruc = reader["Matruc"]?.ToString(),
                            Matbi = reader["Matbi"]?.ToString(),
                            Macd = reader["Macd"]?.ToString(),
                            Macongthuc = reader["Macongthuc"]?.ToString(),
                            LoaiSX = reader["LoaiSX"]?.ToString(),
                            Masoqt = reader["Masoqt"]?.ToString(),

                            Sometmoc = reader["Sometmoc"] == DBNull.Value
                                ? (decimal?)null
                                : Convert.ToDecimal(reader["Sometmoc"]),

                            ChieudaiQT = reader["ChieudaiQT"] == DBNull.Value
                                ? (decimal?)null
                                : Convert.ToDecimal(reader["ChieudaiQT"]),

                            MetTP_tinhtoan = reader["MetTP_tinhtoan"] == DBNull.Value
                                ? (decimal?)null
                                : Convert.ToDecimal(reader["MetTP_tinhtoan"]),

                            Doco_lythuyet = reader["Doco_lythuyet"] == DBNull.Value
                                ? (decimal?)null
                                : Convert.ToDecimal(reader["Doco_lythuyet"]),

                            MaDH = reader["MaDH"]?.ToString(),
                            MaMH = reader["MaMH"]?.ToString(),
                            MaHC = reader["MaHC"]?.ToString(),
                            TenHC = reader["TenHC"]?.ToString(),

                            Nongdo = reader["Nongdo"] == DBNull.Value
                                ? (decimal?)null
                                : Convert.ToDecimal(reader["Nongdo"]),

                            DVT = reader["DVT"]?.ToString(),

                            KL_1TrucMoc = reader["KL_1TrucMoc"] == DBNull.Value
                                ? (decimal?)null
                                : Convert.ToDecimal(reader["KL_1TrucMoc"]),

                            Thoigian_batdau = reader["Thoigian_batdau"] == DBNull.Value
                                ? (DateTime?)null
                                : Convert.ToDateTime(reader["Thoigian_batdau"]),

                            KhoaID = reader["KhoaID"]?.ToString(),
                            Nguoichinhsua = reader["Nguoichinhsua"]?.ToString(),

                            Ngaychinhsua = reader["Ngaychinhsua"] == DBNull.Value
                                ? (DateTime?)null
                                : Convert.ToDateTime(reader["Ngaychinhsua"]),

                            Noidungsua = reader["Noidungsua"]?.ToString(),

                            Xacnhan_NMN = reader["Xacnhan_NMN"] == DBNull.Value
                                ? (bool?)null
                                : Convert.ToBoolean(reader["Xacnhan_NMN"]),

                            Manvxacnhan = reader["Manvxacnhan"]?.ToString(),
                            LayHCtu = reader["LayHCtu"]?.ToString(),

                            Ngayxacnhan = reader["Ngayxacnhan"] == DBNull.Value
                                ? (DateTime?)null
                                : Convert.ToDateTime(reader["Ngayxacnhan"]),

                            QCxacnhan = reader["QCxacnhan"] == DBNull.Value
                                ? (bool?)null
                                : Convert.ToBoolean(reader["QCxacnhan"]),

                            MANVqc_xacnhan = reader["MANVqc_xacnhan"]?.ToString(),

                            NgayQC_xacnhan = reader["NgayQC_xacnhan"] == DBNull.Value
                                ? (DateTime?)null
                                : Convert.ToDateTime(reader["NgayQC_xacnhan"])
                        };
                    }
                }
            }

            return result;
        }

        public async Task<List<LOADXUATHCTNDto>> GetLoadXuatHCTNAsync()
        {
            try
            {
                var list = new List<LOADXUATHCTNDto>();
                using (SqlConnection conn = new SqlConnection(
                    GetConnectionString()))
                {
                    using (SqlCommand cmd = new SqlCommand(
                        "LOAD_XuatHCTN", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@matruc", "");
                        await conn.OpenAsync();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {

                                list.Add(new LOADXUATHCTNDto
                                {
                                    Matruc = reader["Matruc"]?.ToString(),
                                    Matbi = reader["Matbi"]?.ToString(),
                                    Macd = reader["Macd"]?.ToString(),
                                    LoaiSX = reader["LoaiSX"]?.ToString(),
                                    Masoqt = reader["Masoqt"]?.ToString(),

                                    Sometmoc = reader["Sometmoc"] == DBNull.Value
                                    ? (decimal?)null
                                    : Convert.ToDecimal(reader["Sometmoc"]),

                                    ChieudaiQT = reader["ChieudaiQT"] == DBNull.Value
                                    ? (decimal?)null
                                    : Convert.ToDecimal(reader["ChieudaiQT"]),

                                    MetTP_tinhtoan = reader["MetTP_tinhtoan"] == DBNull.Value
                                    ? (decimal?)null
                                    : Convert.ToDecimal(reader["MetTP_tinhtoan"]),

                                    Doco_lythuyet = reader["Doco_lythuyet"] == DBNull.Value
                                    ? (decimal?)null
                                    : Convert.ToDecimal(reader["Doco_lythuyet"]),

                                    MaDH = reader["MaDH"]?.ToString(),
                                    MaMH = reader["MaMH"]?.ToString(),
                                    MaHC = reader["MaHC"]?.ToString(),
                                    TenHC = reader["TenHC"]?.ToString(),

                                    Nongdo = reader["Nongdo"] == DBNull.Value
                                    ? (decimal?)null
                                    : Convert.ToDecimal(reader["Nongdo"]),

                                    DVT = reader["DVT"]?.ToString(),

                                    KL_1TrucMoc = reader["KL_1TrucMoc"] == DBNull.Value
                                    ? (decimal?)null
                                    : Convert.ToDecimal(reader["KL_1TrucMoc"]),

                                    Thoigian_batdau = reader["Thoigian_batdau"] == DBNull.Value
                                    ? (DateTime?)null
                                    : Convert.ToDateTime(reader["Thoigian_batdau"]),

                                    KhoaID = reader["KhoaID"]?.ToString(),
                                    Nguoichinhsua = reader["Nguoichinhsua"]?.ToString(),

                                    Ngaychinhsua = reader["Ngaychinhsua"] == DBNull.Value
                                    ? (DateTime?)null
                                    : Convert.ToDateTime(reader["Ngaychinhsua"]),

                                    Noidungsua = reader["Noidungsua"]?.ToString(),


                                });
                            }
                        }
                    }
                }
                return list;
            }
            catch (Exception ex) { 
                throw new Exception();
            }
            
        }
    }
}