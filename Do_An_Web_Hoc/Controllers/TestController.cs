using Do_An_Web_Hoc.Repositories.Interfaces;
using Do_An_Web_Hoc.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Do_An_Web_Hoc.Controllers
{
    public class TestController : Controller
    {
        private readonly IUserAccountRepository _userRepo;
        private readonly ICoursesRepository _courseRepo;

        public TestController(IUserAccountRepository userRepo, ICoursesRepository courseRepo)
        {
            _userRepo = userRepo;
            _courseRepo = courseRepo;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = new List<UserAccount>();  // Khởi tạo danh sách rỗng để tránh lỗi Null
            var courses = await _courseRepo.GetAllCoursesAsync();  // Lấy danh sách khóa học

            return View(Tuple.Create(users.AsEnumerable(), courses)); // Đảm bảo đúng kiểu dữ liệu
        }


        [HttpPost]
        public async Task<IActionResult> Index(int? roleId, string courseKeyword)
        {
            var users = roleId.HasValue
                ? await _userRepo.GetUsersByRoleAsync(roleId.Value)
                : new List<UserAccount>(); // Tránh lỗi null

            var courses = !string.IsNullOrEmpty(courseKeyword)
                ? await _courseRepo.SearchCoursesByNameAsync(courseKeyword)
                : await _courseRepo.GetAllCoursesAsync();

            return View(Tuple.Create(users.AsEnumerable(), courses));
        }

        [HttpPost]
        public async Task<IActionResult> SearchCourses(string keyword)
        {
            var users = await _userRepo.GetUsersByRoleAsync(1); // Lấy danh sách người dùng (nếu cần)
            var courses = await _courseRepo.SearchCoursesByNameAsync(keyword);

            // Nếu không tìm thấy khóa học nào, lấy toàn bộ danh sách
            if (!courses.Any())
            {
                courses = await _courseRepo.GetAllCoursesAsync();
            }

            return View("Index", Tuple.Create(users, courses)); // ✅ Trả về đúng Tuple
        }


        [HttpPost]
        public async Task<IActionResult> AddCourse(Courses course)
        {
            if (ModelState.IsValid)
            {
                await _courseRepo.AddCourseAsync(course);
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> EditCourse(int id)
        {
            var course = await _courseRepo.GetCourseByIdAsync(id);
            if (course == null)
            {
                return NotFound(); // Nếu không tìm thấy khóa học, trả về lỗi 404
            }

            return View(course); // Trả về view EditCourse với thông tin khóa học
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCourse(Courses course)
        {
            if (!ModelState.IsValid)
            {
                return View("EditCourse", course); // Nếu dữ liệu không hợp lệ, trả lại view với thông tin khóa học
            }

            // Lấy thông tin khóa học từ cơ sở dữ liệu dựa trên CourseID
            var existingCourse = await _courseRepo.GetCourseByIdAsync(course.CourseID);
            if (existingCourse == null)
            {
                return NotFound(); // Nếu không tìm thấy khóa học, trả về lỗi 404
            }

            // Cập nhật thông tin khóa học
            existingCourse.CourseName = course.CourseName;
            existingCourse.Description = course.Description;
            existingCourse.Price = course.Price;
            existingCourse.Status = course.Status; // Cập nhật trạng thái (Hoạt động hoặc Ngừng hoạt động)

            // Cập nhật khóa học trong cơ sở dữ liệu
            var updateSuccess = await _courseRepo.UpdateCourseAsync(existingCourse);
            if (!updateSuccess)
            {
                // Nếu không thành công, hiển thị thông báo lỗi
                TempData["ErrorMessage"] = "Cập nhật khóa học không thành công!";
                return View("EditCourse", existingCourse); // Trả lại view với thông tin đã cập nhật
            }

            // Thông báo thành công và chuyển hướng về trang danh sách khóa học
            TempData["SuccessMessage"] = "Cập nhật khóa học thành công!";
            return RedirectToAction("Index"); // Chuyển hướng về trang danh sách khóa học
        }




        [HttpPost]
        public async Task<IActionResult> DeleteCourse(int courseId)
        {
            var result = await _courseRepo.SoftDeleteCourseAsync(courseId);
            if (!result)
            {
                TempData["Error"] = "Không tìm thấy khóa học hoặc xóa thất bại!";
            }
            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> SoftDeleteCourse(int courseId)
        {
            var result = await _courseRepo.SoftDeleteCourseAsync(courseId);
            if (!result)
            {
                ViewBag.Error = "Không tìm thấy khóa học!";
            }

            var users = await _userRepo.GetUsersByRoleAsync(1);
            var courses = await _courseRepo.GetAllCoursesAsync();

            return View("Index", Tuple.Create(users, courses));
        }
    }
}
