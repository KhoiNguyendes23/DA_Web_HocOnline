using Do_An_Web_Hoc.Models;
using Do_An_Web_Hoc.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Do_An_Web_Hoc.Controllers
{
    [Authorize(Roles = "User")]
    public class UserController : Controller
    {
        private readonly ICoursesRepository _coursesRepo;
        private readonly IEnrollmentsRepository _enrollmentsRepo;
        private readonly ILecturesRepository _lecturesRepo;

        public UserController(ICoursesRepository coursesRepo, IEnrollmentsRepository enrollmentsRepo, ILecturesRepository lecturesRepo)
        {
            _coursesRepo = coursesRepo;
            _enrollmentsRepo = enrollmentsRepo;
            _lecturesRepo = lecturesRepo;
        }
        public async Task<IActionResult> Dashboard(string keyword)
        {
            var fullName = User.FindFirstValue(ClaimTypes.Name);
            var email = User.FindFirstValue(ClaimTypes.Email);
            ViewData["FullName"] = fullName;
            ViewData["Email"] = email;

            var courses = await _coursesRepo.GetAllCoursesAsync();

            if (!string.IsNullOrEmpty(keyword))
            {
                keyword = keyword.ToLower();
                courses = courses.Where(c =>
                    (c.CourseName != null && c.CourseName.ToLower().Contains(keyword)) ||
                    (c.Description != null && c.Description.ToLower().Contains(keyword))
                ).ToList();
            }

            ViewBag.Keyword = keyword; 
            return View(courses);
        }


        public IActionResult Courses()
        {
            return View();
        }

        public async Task<IActionResult> NotEnrolled()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var enrollments = await _enrollmentsRepo.GetEnrollmentsByUserAsync(userId);
            var courseIds = enrollments.Select(e => e.CourseID).ToList();

            var allCourses = await _coursesRepo.GetAllCoursesAsync();
            var notEnrolledCourses = allCourses.Where(c => !courseIds.Contains(c.CourseID)).ToList();

            return View(notEnrolledCourses);
        }

        public async Task<IActionResult> Enrolled()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var enrollments = await _enrollmentsRepo.GetEnrollmentsByUserAsync(userId);
            var courseIds = enrollments.Select(e => e.CourseID).ToList();

            var allCourses = await _coursesRepo.GetAllCoursesAsync();
            var enrolledCourses = allCourses.Where(c => courseIds.Contains(c.CourseID)).ToList();

            return View(enrolledCourses);
        }

        public IActionResult InProgress()
        {
            return View();
        }

        public async Task<IActionResult> FreeCourses()
        {
            var freeCourses = (await _coursesRepo.GetAllCoursesAsync())
                              .Where(c => c.Price == 0 || c.Price == null).ToList();
            return View(freeCourses);
        }

        public async Task<IActionResult> PaidCourses()
        {
            var paidCourses = (await _coursesRepo.GetAllCoursesAsync())
                              .Where(c => c.Price > 0).ToList();
            return View(paidCourses);
        }

        public IActionResult Quiz()
        {
            return View();
        }

        public IActionResult Practice()
        {
            return View();
        }

        public IActionResult Ranking()
        {
            return View();
        }

        public IActionResult Support()
        {
            return View();
        }

        public IActionResult ContactLecturer()
        {
            return View();
        }

        public IActionResult Community()
        {
            return View();
        }

        public IActionResult Profile()
        {
            return View();
        }

        public IActionResult PaymentHistory()
        {
            return View();
        }
        public async Task<IActionResult> Payment(int courseId)
        {
            var course = (await _coursesRepo.GetAllCoursesAsync()).FirstOrDefault(c => c.CourseID == courseId);
            if (course == null)
            {
                TempData["Message"] = "Khóa học không tồn tại.";
                return RedirectToAction("Dashboard");
            }

            ViewBag.Course = course;
            return View();
        }

        public async Task<IActionResult> LearnCourse(int id)
        {
            var course = await _coursesRepo.GetCourseByIdAsync(id);
            if (course == null) return NotFound();

            var lectures = await _lecturesRepo.GetLecturesByCourseIdAsync(id);
            ViewBag.CourseName = course.CourseName;
            ViewBag.CourseId = course.CourseID;

            return View(lectures);
        }





        public IActionResult SecuritySettings()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> RegisterCourse(int courseId)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                return Unauthorized();
            }

            // Kiểm tra đã đăng ký chưa
            if (await _enrollmentsRepo.IsUserEnrolledAsync(userId, courseId))
            {
                TempData["Message"] = "Bạn đã đăng ký khóa học này.";
                return RedirectToAction("Enrolled");
            }

            // Lấy thông tin khóa học
            var course = (await _coursesRepo.GetAllCoursesAsync()).FirstOrDefault(c => c.CourseID == courseId);
            if (course == null)
            {
                TempData["Message"] = "Không tìm thấy khóa học.";
                return RedirectToAction("Dashboard");
            }

            // Nếu khóa miễn phí => cho đăng ký ngay
            if (course.Price == 0 || course.Price == null)
            {
                var enrollment = new Enrollments
                {
                    UserID = userId,
                    CourseID = courseId,
                    EnrollmentDate = DateTime.Now,
                    CompletionStatus = false
                };
                await _enrollmentsRepo.AddEnrollmentAsync(enrollment);

                TempData["Message"] = "Bạn đã đăng ký khóa học thành công!";
                return RedirectToAction("Enrolled");
            }
            else
            {
                // Nếu là khóa trả phí => chuyển hướng đến trang thanh toán
                return RedirectToAction("Payment", new { courseId = courseId });
            }
        }
    }
}
