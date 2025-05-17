using Microsoft.AspNetCore.Mvc;
using Do_An_Web_Hoc.Services.Odoo;
using Do_An_Web_Hoc.Repositories.Interfaces;
using Do_An_Web_Hoc.Models;
using Do_An_Web_Hoc.Services.Interfaces;

namespace Do_An_Web_Hoc.Controllers
{
    public class OdooEnrollmentController : Controller
    {
        private readonly IOdooEnrollmentService _odooEnrollmentService;
        private readonly IOdooPartnerService _odooPartnerService;
        private readonly IOdooCourseService _odooCourseService;
        private readonly IEnrollmentsRepository _enrollmentsRepository;
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly ICoursesRepository _coursesRepository;

        public OdooEnrollmentController(
            IOdooEnrollmentService odooEnrollmentService,
            IOdooPartnerService odooPartnerService,
            IOdooCourseService odooCourseService,
            IEnrollmentsRepository enrollmentsRepository,
            IUserAccountRepository userAccountRepository,
            ICoursesRepository coursesRepository)
        {
            _odooEnrollmentService = odooEnrollmentService;
            _odooPartnerService = odooPartnerService;
            _odooCourseService = odooCourseService;
            _enrollmentsRepository = enrollmentsRepository;
            _userAccountRepository = userAccountRepository;
            _coursesRepository = coursesRepository;
        }

        // GET: Hiển thị form ghi danh thủ công
        [HttpGet]
        public IActionResult CreateEnrollment()
        {
            return View();
        }

        // POST: Thực hiện ghi danh thủ công
        [HttpPost]
        public async Task<IActionResult> CreateEnrollment(int studentOdooId, int courseOdooId)
        {
            var existingEnrollmentId = await _odooEnrollmentService.SearchEnrollmentAsync(studentOdooId, courseOdooId);

            if (existingEnrollmentId != null)
            {
                ViewBag.Result = $"⚠️ Học viên đã ghi danh vào khóa học (Enrollment ID: {existingEnrollmentId}).";
            }
            else
            {
                var newEnrollmentId = await _odooEnrollmentService.CreateEnrollmentAsync(studentOdooId, courseOdooId);

                ViewBag.Result = newEnrollmentId != null
                    ? $"✅ Ghi danh thành công (Enrollment ID: {newEnrollmentId})."
                    : "❌ Ghi danh thất bại.";
            }

            return View("Result");
        }

        // GET: Hiển thị form đồng bộ tất cả ghi danh
        [HttpGet]
        public IActionResult SyncAll()
        {
            return View();
        }

        // POST: Thực hiện đồng bộ tất cả ghi danh
        [HttpPost]
        public async Task<IActionResult> SyncAllEnrollments()
        {
            var enrollments = await _enrollmentsRepository.GetAllEnrollmentsAsync();

            int successCount = 0;

            foreach (var enrollment in enrollments)
            {
                int? studentOdooId = await GetOdooPartnerId(enrollment.UserID);
                int? courseOdooId = await GetOdooCourseId(enrollment.CourseID);

                if (studentOdooId == null || courseOdooId == null)
                    continue;

                var existing = await _odooEnrollmentService.SearchEnrollmentAsync(studentOdooId.Value, courseOdooId.Value);

                if (existing == null)
                {
                    var created = await _odooEnrollmentService.CreateEnrollmentAsync(studentOdooId.Value, courseOdooId.Value);
                    if (created != null)
                        successCount++;
                }
            }

            ViewBag.Result = $"✅ Đồng bộ thành công {successCount}/{enrollments.Count()} lượt ghi danh.";
            return View("Result");
        }

        // Hỗ trợ: Lấy Odoo ID học viên từ UserID SQL
        private async Task<int?> GetOdooPartnerId(int? userId)
        {
            if (userId == null) return null;
            var user = await _userAccountRepository.GetByIdAsync(userId.Value);
            if (user == null || string.IsNullOrEmpty(user.Email)) return null;

            return await _odooPartnerService.SearchPartnerByEmailAsync(user.Email);
        }

        // Hỗ trợ: Lấy Odoo ID khóa học từ CourseID SQL
        private async Task<int?> GetOdooCourseId(int? courseId)
        {
            if (courseId == null) return null;
            var course = await _coursesRepository.GetCourseByIdAsync(courseId.Value);
            if (course == null) return null;

            return await _odooCourseService.SearchCourseByNameAsync(course.CourseName);
        }
    }
}
