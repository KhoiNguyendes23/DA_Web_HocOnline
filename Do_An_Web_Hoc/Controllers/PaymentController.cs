using Do_An_Web_Hoc.Models;
using Do_An_Web_Hoc.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace Do_An_Web_Hoc.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IMomoService _momoService;
        private readonly IEnrollmentsRepository _enrollmentsRepo;
        private readonly ICoursesRepository _coursesRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PaymentController(
            IMomoService momoService,
            IEnrollmentsRepository enrollmentsRepo,
            ICoursesRepository coursesRepo,
            IHttpContextAccessor httpContextAccessor)
        {
            _momoService = momoService;
            _enrollmentsRepo = enrollmentsRepo;
            _coursesRepo = coursesRepo;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePaymentUrl(OrderInfoModel model)
        {
            Console.WriteLine(">>> [DEBUG] Đã gọi CreatePaymentUrl");

            if (!ModelState.IsValid || model.Amount <= 0)
            {
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
        public async Task<IActionResult> PaymentCallBack()
        {
            var query = Request.Query;
            var momoResult = _momoService.PaymentExecuteAsync(query);

            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                TempData["Message"] = "Không xác định được người dùng!";
                return RedirectToAction("Dashboard", "User");
            }
            int userId = int.Parse(userIdClaim.Value);

            var courseId = ExtractCourseIdFromOrderInfo(momoResult.OrderInfo);
            if (courseId == null)
            {
                TempData["Message"] = "Không thể xác định khóa học!";
                return RedirectToAction("Dashboard", "User");
            }

            var enrollment = new Enrollments
            {
                CourseID = courseId.Value,
                UserID = userId,
                EnrollmentDate = DateTime.Now,
                CompletionStatus = false,
                IsPaid = true,
                PaymentMethod = "MOMO",
                PaymentDate = DateTime.Now
            };

            await _enrollmentsRepo.AddEnrollmentAsync(enrollment);

            TempData["Message"] = "Thanh toán thành công và bạn đã được đăng ký vào khóa học!";
            return RedirectToAction("Enrolled", "User");
        }

        private int? ExtractCourseIdFromOrderInfo(string orderInfo)
        {
            try
            {
                var parts = orderInfo.Split("khóa học:");
                if (parts.Length < 2) return null;
                var courseName = parts[1].Trim();

                var course = _coursesRepo.GetAllCoursesAsync().Result.FirstOrDefault(c => c.CourseName == courseName);
                return course?.CourseID;
            }
            catch
            {
                return null;
            }
        }

        public IActionResult Test()
        {
            return Content("✔ PaymentController hoạt động!");
        }

        public IActionResult MomoNotify()
        {
            return Ok();
        }
    }
}