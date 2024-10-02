using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Drawing;
using System.Drawing.Printing;

namespace DATN.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrintBillController : ControllerBase
    {
        [HttpPost("PrintReceipt")]
        public IActionResult PrintReceipt()
        {
            try
            {
                PrintDocument printDoc = new PrintDocument();
                printDoc.PrintPage += new PrintPageEventHandler(PrintPage);

                // Có thể chọn máy in cụ thể (tùy chọn)
                // printDoc.PrinterSettings.PrinterName = "Tên máy in";

                printDoc.Print();
                return Ok("In thành công.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi khi in: " + ex.Message);
            }
        }

        private void PrintPage(object sender, PrintPageEventArgs e)
        {
            string receiptText = "Hóa đơn\n\n";
            receiptText += "Sản phẩm: ABC\n";
            receiptText += "Số lượng: 2\n";
            receiptText += "Giá: 30,000 VND\n";
            receiptText += "\nTổng cộng: 60,000 VND\n";

            Font font = new Font("Arial", 12);
            float x = 10;
            float y = 20;

            e.Graphics.DrawString(receiptText, font, Brushes.Black, x, y);
        }
    }
}
