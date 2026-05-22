using DocumentFormat.OpenXml.Spreadsheet;
using SpreadsheetLight;
using System;
using System.Collections.Generic;
using System.IO;
using ToolsApp.EntityFramework.VatTu;

public static class ExcelTienDoMuaHangHelper
{
    public static byte[] Export(List<VATTU2025_SP_THEODOIPHIEUPD_CUGC_Result> list)
    {
        using (var sl = new SLDocument())
        {
            // ================= STYLE HEADER =================
            SLStyle headerStyle = sl.CreateStyle();
            headerStyle.Font.Bold = true;
            headerStyle.Alignment.Horizontal = HorizontalAlignmentValues.Center;
            headerStyle.Fill.SetPattern(
                PatternValues.Solid,
                System.Drawing.Color.LightSteelBlue,
                System.Drawing.Color.LightSteelBlue
            );

            // ================= MAP CỘT =================
            var columns = new (string Header, Func<VATTU2025_SP_THEODOIPHIEUPD_CUGC_Result, object> Value)[]
            {
                ("Mã phiếu", x => x.MAPHIEU),
                ("Mã phiếu PD", x => x.MAPHIEUPD),
                ("Mã vật tư", x => x.MAVT),
                ("Tên vật tư", x => x.TENVT),
                ("ĐVT", x => x.DVT),
                ("SL PD", x => x.SLPD),
                ("Đã nhập", x => x.NHAP),
                ("Đã xuất", x => x.XUAT),
                ("Chưa mua", x => x.CHUAMUA),
                ("Ngày PD", x => x.NGAYPD),
                ("Ngày CUGC", x => x.NGAYCUGC),
                ("PH kỹ thuật", x => x.PHTinhTrangKyThuatCU),
                ("Nhà máy PH", x => x.TgNhaMayPHCU),
                ("Tình trạng NCC", x => x.PHTinhTrangNCC),
                ("Thanh toán", x => x.PHTinhTrangThanhtoan),
                ("PH đáp ứng", x => x.PHDapUngCU),
                ("Hạn thanh toán", x => x.PHHanThanhToan),
                ("Tiến độ thực tế", x => x.PHTienDoDapUngTTCU),
                ("Tình trạng đáp ứng", x => x.TinhTrangDapUngCU),
                ("Lý do trễ", x => x.LyDoTreHanCU),
                ("NV mua hàng", x => x.MANVMuaHang),
                ("Ghi chú", x => x.GhiChuCUng)
            };

            // ================= HEADER =================
            for (int c = 0; c < columns.Length; c++)
            {
                sl.SetCellValue(1, c + 1, columns[c].Header);
                sl.SetCellStyle(1, c + 1, headerStyle);
                sl.SetColumnWidth(c + 1, 20);
            }

            // ================= DATA =================
            int row = 2;
            foreach (var item in list)
            {
                for (int col = 0; col < columns.Length; col++)
                {
                    var value = columns[col].Value(item);

                    if (value is DateTime dt)
                    {
                        sl.SetCellValue(row, col + 1, dt);
                        sl.SetCellStyle(row, col + 1, GetDateStyle(sl));
                    }
                    else
                    {
                        sl.SetCellValue(row, col + 1, value?.ToString());
                    }
                }

                // Tô màu trễ hạn
                if (item.TinhTrangDapUngCU == "Trễ hạn")
                {
                    SLStyle lateStyle = sl.CreateStyle();
                    lateStyle.Fill.SetPattern(
                        PatternValues.Solid,
                        System.Drawing.Color.MistyRose,
                        System.Drawing.Color.MistyRose
                    );

                    sl.SetRowStyle(row, lateStyle);
                }

                row++;
            }

            // Freeze header
            sl.FreezePanes(1, 0);

            using (var ms = new MemoryStream())
            {
                sl.SaveAs(ms);
                return ms.ToArray();
            }
        }
    }

    private static SLStyle GetDateStyle(SLDocument sl)
    {
        SLStyle style = sl.CreateStyle();
        style.FormatCode = "dd/MM/yyyy";
        return style;
    }
}
