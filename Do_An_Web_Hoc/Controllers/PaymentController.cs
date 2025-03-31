using Do_An_Web_Hoc.Models;
using Do_An_Web_Hoc.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
namespace Do_An_Web_Hoc.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IMomoService _momoService;

        public PaymentController(IMomoService momoService)
        {
            _momoService = momoService;
        }

        //[HttpPost]
        //public async Task<IActionResult> CreatePaymentUrl(OrderInfoModel model)
        //{
        //    if (!ModelState.IsValid || model.Amount <= 0)
        //    {
        //        TempData["Message"] = "Dữ liệu thanh toán không hợp lệ!";
        //        return RedirectToAction("Dashboard", "User");
        //    }

        //    var response = await _momoService.CreatePaymentAsync(model);

        //    if (response == null || string.IsNullOrEmpty(response.PayUrl))
        //    {
        //        TempData["Message"] = "Tạo thanh toán thất bại!";
        //        return RedirectToAction("Dashboard", "User");
        //    }

        //    return Redirect(response.PayUrl);
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePaymentUrl(OrderInfoModel model)
        {
            Console.WriteLine(">>> [DEBUG] Đã gọi CreatePaymentUrl");

            if (!ModelState.IsValid || model.Amount <= 0)
            {
                // In lỗi chi tiết từng field
                foreach (var entry in ModelState)
                {
                    var field = entry.Key;
                    var errors = entry.Value.Errors;
                    foreach (var error in errors)
                    {
                        Console.WriteLine($"[DEBUG] Field: {field}, Error: {error.ErrorMessage}");
                    }
                }

                TempData["Message"] = "Dữ liệu thanh toán không hợp lệ!";
                return RedirectToAction("Dashboard", "User");
            }

            var response = await _momoService.CreatePaymentAsync(model);

            if (response == null || string.IsNullOrEmpty(response.PayUrl))
            {
                TempData["Message"] = "Tạo thanh toán thất bại!";
                return RedirectToAction("Dashboard", "User");
            }

            return Redirect(response.PayUrl);
        }



        [HttpGet]
        public IActionResult Test()
        {
            return Content("✔ PaymentController hoạt động!");
        }



        public IActionResult PaymentCallBack()
        {
            return View(); // Xác nhận đơn hàng ở đây
        }

        public IActionResult MomoNotify()
        {
            return Ok(); // Xử lý thông báo server (nếu cần)
        }
    }
}
