using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ToolsApp.App_Start;
using ToolsApp.Authentication;
using ToolsApp.EntityFramework.VatTu;
using ToolsApp.Helper;
using ToolsApp.Models;
using ToolsApp.Utilities;

namespace ToolsApp.Controllers.TacNghiepHCTN
{
    public class DieuChinhHoaChatController : BaseController
    {
        // GET: DieuChinhHoaChat
        public async Task<ActionResult> Index()
        {
            var list = await GetXuatHCTNTheoTrucAsync();
            var listMaHC = await GetDanhMucHoaChat();


            return View();
        }

        #region Load View
        public async Task<ActionResult> _Getlist()
        {

                var List = await GetLoadXuatHCTNAsync();
                ViewBag.List = List;


                return PartialView();
        }
        #endregion

        #region Insert

        [ValidateInput(false)]
        [HttpPost]
        public async Task<JsonResult> _InsertFun(LOADXUATHCTNModel model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.Matruc))
                {
                    return Json(new
                    {
                        status = -1,
                        title = "Thông báo",
                        text = "Mã trục không được rỗng.",
                        obj = ""
                    }, JsonRequestBehavior.AllowGet);
                }

                if (string.IsNullOrWhiteSpace(model.MAHCNEW))
                {
                    return Json(new
                    {
                        status = -1,
                        title = "Thông báo",
                        text = "Mã hóa chất thay thế không được rỗng.",
                        obj = ""
                    }, JsonRequestBehavior.AllowGet);
                }

                if (string.IsNullOrWhiteSpace(model.Lydosua))
                {
                    return Json(new
                    {
                        status = -1,
                        title = "Thông báo",
                        text = "Nội dung sửa không được rỗng.",
                        obj = ""
                    }, JsonRequestBehavior.AllowGet);
                }

                if (model.KLHC <= 0)
                {
                    return Json(new
                    {
                        status = -1,
                        title = "Thông báo",
                        text = "Khối lượng phải lớn hơn 0.",
                        obj = ""
                    }, JsonRequestBehavior.AllowGet);
                }

                // Lấy ConnectionString từ Web.config
                string connectionString =
                    ConfigurationManager
                        .ConnectionStrings["QLSX_Nhuom_AnhHongConnectionString"].ToString();

                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand("spUpdate_XuatHCTN", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 120;

                    cmd.Parameters.Add("@matruc", SqlDbType.VarChar, 50)
                        .Value = model.Matruc;

                    cmd.Parameters.Add("@KhoaID", SqlDbType.VarChar, 50)
                        .Value = model.KhoaID;

                    cmd.Parameters.Add("@mahcNew", SqlDbType.VarChar, 50)
                        .Value = model.MAHCNEW;

                    cmd.Parameters.Add("@kl", SqlDbType.Decimal)
                        .Value = model.KLHC;

                    cmd.Parameters.Add("@noidungsua", SqlDbType.NVarChar, 250)
                        .Value = model.Lydosua;

                    cmd.Parameters.Add("@manv", SqlDbType.VarChar, 50)
                        .Value = User.UserName;

                    await conn.OpenAsync();

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            int success = Convert.ToInt32(reader["Success"]);
                            string message = reader["Message"] == DBNull.Value
                                ? ""
                                : reader["Message"].ToString();

                            if (success == 1)
                            {
                                return Json(new
                                {
                                    status = 1,
                                    title = "Thành công",
                                    text = message,
                                    obj = ""
                                }, JsonRequestBehavior.AllowGet);
                            }

                            return Json(new
                            {
                                status = -1,
                                title = "Không thể cập nhật",
                                text = message,
                                obj = ""
                            }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }

                return Json(new
                {
                    status = -1,
                    title = "Lỗi",
                    text = "Không trả về kết quả.",
                    obj = ""
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    status = -1,
                    title = "Lỗi",
                    text = ex.Message,
                    obj = ""
                }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Load View Update
        public async Task<ActionResult> _Update(string ID)
        {

                 var model = await GetXuatHCTNTheoTrucByIdKhoaAsync(ID);
                ViewBag.item = model;

                    var listMaHC = await GetDanhMucHoaChat();

                    ViewBag.MaHCs = listMaHC
                        .Select(p => new VatTuDto
                        {
                            MAVT = p.MAVT,
                            TENVT = p.TenVT
                        })

                        .ToList();

            return PartialView("Edits", Mapper.MapFrom(model));


        }
        #endregion

        #region change mã hóa chất cũ
        [ValidateInput(false)]
        [HttpPost]
        public async Task<ActionResult> _LoadMahcOld(string maTruc)
        {
            try
            {
                var list = await GetXuatHCTNTheoTrucByMaTrucAsync(maTruc);

                return Json(new
                {
                    status = 1,
                    title = "",
                    text = "",
                    obj = list
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new
                {
                    status = -1,
                    title = "",
                    text = e.Message,
                    obj = ""
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [ValidateInput(false)]
        [HttpPost]
        public async Task<ActionResult> _LoadkhoiLuongOld(string maTruc, string mahcold)
        {
            try
            {
                decimal? kl = 0;
                var list = await GetXuatHCTNTheoTrucByMaTrucAsync(maTruc);

                var hc = list.Where(p => p.MaHC == mahcold).ToList();
                if(hc.Any())
                {
                    kl = hc.FirstOrDefault().KL_1TrucMoc;
                }    

                return Json(new
                {
                    status = 1,
                    title = "",
                    text = "",
                    obj = hc
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new
                {
                    status = -1,
                    title = "",
                    text = e.Message,
                    obj = ""
                }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region private
        private async Task<List<DANHMUCVATTU>> GetDanhMucHoaChat()
        {
            try
            {
                using (var db_ = new wqlvattuEntities())
                {
                    var list = await db_.DANHMUCVATTUs.Where(p => p.LoaiVT == "HOACHAT" &&(p.MAVT.StartsWith("B")
             || p.MAVT.StartsWith("C"))).ToListAsync();

                    return list;
                }
            }
            catch (Exception e)
            {
                throw;
            }

        }

        private async Task<List<XuatHCTNTheotrucDto>> GetXuatHCTNTheoTrucAsync()
        {
            StoreDatabaseHelper helper = new StoreDatabaseHelper();
           return await helper.GetXuatHCTNTheoTrucAsync();

        }

        private async Task<List<XuatHCTNTheotrucDto>> GetXuatHCTNTheoTrucByMaTrucAsync(string maTruc)
        {
            StoreDatabaseHelper helper = new StoreDatabaseHelper();
            return await helper.GetXuatHCTNTheoTrucByMaTrucAsync(maTruc);

        }

        private async Task<XuatHCTNTheotrucDto> GetXuatHCTNTheoTrucByIdKhoaAsync(string id)
        {
            StoreDatabaseHelper helper = new StoreDatabaseHelper();
            return await helper.GetXuatHCTNTheoTrucByIDKhoaAsync(id);

        }

        private async Task<List<LOADXUATHCTNDto>> GetLoadXuatHCTNAsync()
        {
            StoreDatabaseHelper helper = new StoreDatabaseHelper();
            return await helper.GetLoadXuatHCTNAsync();

        }
        #endregion

    }
}