
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using ToolsApp.Authentication;
using ToolsApp.EntityFramework.KhoSoi;
using ToolsApp.EntityFramework.VatTu;
using System.Net;
using NSClient;
using ToolsApp.com.netsuite.webservices;

using ToolsApp.EntityFramework.NhuomAH;
using ToolsApp.Utilities;
using System.Data.Entity;
using ToolsApp.Helper;

namespace ToolsApp.Controllers
{
    [Authorize]
    public class YeuCauHoaChatCongDoanController : BaseController
    {
        private wqlkhosoiEntities db_ = new wqlkhosoiEntities();
        private wqlvattuEntities dbvt = new wqlvattuEntities();
        private QLSX_Nhuom_AnhHongEntities nhuomah = new QLSX_Nhuom_AnhHongEntities();
        // GET: XemXet_BM01
        public ActionResult Index()
        {
            #region công đoạn
            ViewBag.congdoan = nhuomah.Spload_tblCongDoan().ToList();
            ViewBag.noilayhc = nhuomah.NOILAYHOACHATs.ToList();
            ViewBag.mahang = nhuomah.XuatHCTN_theotruc
                           .Where(p => (p.QCxacnhan ?? false) == false)
                           .GroupBy(p => p.MaMH)
                           .Select(g => g.Key)
                           .ToList();
            #endregion


            return View();
        }
        public ActionResult _GetList(string congdoan, string mahang)
        {
           
            var list = nhuomah.SPLOAD_XUATHCTN_THEOTRUC(congdoan, mahang, "NO1").ToList();
            ViewBag.List = list;
            return PartialView();
        }

        public ActionResult _GetList2(string congdoan, string mahang)
        {

            var list2 = nhuomah.SPLOAD_XUATHCTN_THEOTRUC(congdoan, mahang, "NO2").ToList();
            ViewBag.List2 = list2;
            return PartialView();
        }


        [HttpPost]
        public JsonResult _ChuyenXN(List<string> ids, string LayHCTU = "") // Đổi thành List<string> nếu KhoaID là chuỗi kiệu chữ
        {
            try
            {
                if (ids == null || ids.Count == 0)
                {
     
                    return Json(new { status = -1, title = "", text = "Không có dữ liệu nào được chọn. " , obj = "" }, JsonRequestBehavior.AllowGet);
                }

                using (var db = new QLSX_Nhuom_AnhHongEntities()) // Thay bằng DbContext thực tế của bạn
                {
                    // Cách 1: Cập nhật trạng thái hàng loạt
                    // Ví dụ: Tìm các bản ghi có KhoaID nằm trong danh sách được chọn và đổi trạng thái
                    var records = db.XuatHCTN_theotruc.Where(x => ids.Contains(x.KhoaID)).ToList();
                    foreach (var item in records)
                    {
                        // Thực hiện thay đổi dữ liệu của bạn ở đây
                         item.Xacnhan_NMN = true;
                         item.Ngayxacnhan = DateTime.Now;
                         item.Manvxacnhan =  User.UserName;
                         item.LayHCtu = LayHCTU;
                    }

                    // Cách 2: Hoặc nếu bạn muốn insert các dòng này sang một bảng Xuất Hóa Chất khác:
                    // foreach (var id in ids) { ... tạo thực thể mới và db.Table.Add(...) ... }

                    // Lưu thay đổi vào Database
                    db.SaveChanges();
                }

                return Json(new { status = 1, title = "", text = "Đã lưu thành công " + ids.Count + " dòng!", obj = "" }, JsonRequestBehavior.AllowGet);
            
            }
            catch (Exception ex)
            {
                // Ghi log lỗi nếu cần thiết
                return Json(new { status = 1, title = "", text = "Lỗi hệ thống: " + ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
          
            }
        }



    }
}