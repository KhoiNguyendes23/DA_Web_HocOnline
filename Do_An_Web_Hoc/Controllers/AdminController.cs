using Do_An_Web_Hoc.Models;
using Do_An_Web_Hoc.Repositories;
using Do_An_Web_Hoc.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.WebSockets;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;
using ClosedXML.Excel;
using System.IO;

namespace Do_An_Web_Hoc.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IUserAccountRepository _userRepo;
        private readonly ICoursesRepository _coursesRepo;
        private readonly IExamsRepository _examsRepo;
        private readonly ILogger<AdminController> _logger;
        private readonly ICatogoriesRepository _catogoriesRepository;
        private readonly IEnrollmentsRepository _enrollmentRepo;
        public AdminController(IUserAccountRepository userRepo, ICoursesRepository coursesRepo, IExamsRepository examsRepo, ILogger<AdminController> logger, ICatogoriesRepository catogoriesRepository, IEnrollmentsRepository enrollmentRepo)
        {
            _userRepo = userRepo;
            _coursesRepo = coursesRepo;
            _examsRepo = examsRepo;
            _logger = logger;
            _catogoriesRepository = catogoriesRepository;
            _enrollmentRepo = enrollmentRepo;
        }
        public IActionResult Dashboard()
        {
            var fullName = User.FindFirstValue(ClaimTypes.Name);
            var roleName = User.FindFirstValue(ClaimTypes.Role);
            var email = User.FindFirstValue(ClaimTypes.Email);
            ViewData["FullName"] = fullName;
            ViewData["RoleName"] = roleName;
            ViewData["Email"] = email;
            return View();
        }
        public async Task<IActionResult> ListCourse()
        {
            var courses = await _coursesRepo.GetAllCoursesAsync();
            return View(courses);
        }
        [HttpGet]
        public async Task<IActionResult> AddCourse()
        {
            var categories = await _catogoriesRepository.GetAllCategoriesAsync();

            ViewBag.Categories = categories.Select(c => new SelectListItem
            {
                Value = c.CategoryId.ToString(),
                Text = c.CategoryName
            }).ToList();

            ViewBag.StatusOptions = new List<SelectListItem>
         {
            new SelectListItem { Value = "1", Text = "Hoạt động" },
            new SelectListItem { Value = "0", Text = "Không hoạt động" }
         };

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCourse(Courses course, IFormFile image)
        {
            if (!ModelState.IsValid)
            {
                var categories = await _catogoriesRepository.GetAllCategoriesAsync();
                ViewBag.Categories = categories.Select(c => new SelectListItem
                {
                    Value = c.CategoryId.ToString(),
                    Text = c.CategoryName
                }).ToList();

                ViewBag.StatusOptions = new List<SelectListItem>
        {
            new SelectListItem { Value = "1", Text = "Hoạt động" },
            new SelectListItem { Value = "0", Text = "Không hoạt động" }
        };

                return View(course);
            }

            if (image != null && image.Length > 0)
            {
                var ext = Path.GetExtension(image.FileName).ToLower();
                var allowedExts = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                if (allowedExts.Contains(ext))
                {
                    var fileName = Guid.NewGuid().ToString() + ext;
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "Course_images", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }

                    course.ImageUrl = "/images/Course_images/" + fileName;
                }
            }

            await _coursesRepo.AddCourseAsync(course);
            return RedirectToAction("ListCourse");
        }

        // thêm danh mục
        [HttpPost]
        public async Task<IActionResult> AddCategory(string CategoryName, int Status)
        {
            if (string.IsNullOrWhiteSpace(CategoryName)) return RedirectToAction("AddCourse");

            var newCategory = new Categories
            {
                CategoryName = CategoryName.Trim(),
                Status = Status
            };

            await _catogoriesRepository.AddCategoryAsync(newCategory);

            // Chuyển về lại trang thêm khóa học
            return RedirectToAction("AddCourse");
        }


        public async Task<IActionResult> ListStudent()
        {
            var students = await _userRepo.GetUsersByRoleAsync(3);
            return View(students);
        }
        public async Task<IActionResult> ListTeacher()
        {
            var lecturers = await _userRepo.GetUsersByRoleAsync(2);
            return View(lecturers);
        }

        //public IActionResult ListExam()
        //{
        //    return View();
        //}
        //public IActionResult AddExam()
        //{
        //    return View();
        //}
        //public IActionResult ViewExam()
        //{
        //    return View();
        //}
        //public IActionResult UpdateExam()
        //{
        //    return View();
        //}
        //public IActionResult DeleteExam()
        //{
        //    return View();
        //}
        //public IActionResult TestResult()
        //{
        //    return View();
        //}
        public async Task<IActionResult> StatisticalCourse()
        {
            var allCourses = await _coursesRepo.GetAllCoursesAsync();

            var result = allCourses
                .Select(course => new StatisticalCourseViewModel
                {
                    CourseName = course.CourseName,
                    EnrollmentCount = course.Enrollments?.Count(e => e.IsPaid) ?? 0
                })
                .ToList();

            return View(result);
        }
        // Xuất Excel
        public async Task<IActionResult> ExportRevenueToExcel()
        {
            var data = await _enrollmentRepo.GetMonthlyRevenueStatisticsAsync();


            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Thống kê doanh thu");

                // Header
                worksheet.Cell(1, 1).Value = "Năm";
                worksheet.Cell(1, 2).Value = "Tháng";
                worksheet.Cell(1, 3).Value = "Lượt đăng ký";
                worksheet.Cell(1, 4).Value = "Tổng doanh thu (VNĐ)";
                worksheet.Range("A1:D1").Style.Font.Bold = true;

                int row = 2;
                foreach (var item in data)
                {
                    worksheet.Cell(row, 1).Value = item.Year;
                    worksheet.Cell(row, 2).Value = item.Month;
                    worksheet.Cell(row, 3).Value = item.TotalEnrollments;
                    worksheet.Cell(row, 4).Value = item.TotalRevenue;
                    row++;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    return File(stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"ThongKeDoanhThu_{DateTime.Now:yyyyMMdd}.xlsx");
                }
            }
        }

        public async Task<IActionResult> StatisticalRevenue()
        {
            var revenueData = await _enrollmentRepo.GetMonthlyRevenueStatisticsAsync();
            return View(revenueData);
        }
        public async Task<IActionResult> PersonalPage()
        {
            // Lấy email từ Claims
            var currentUserEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(currentUserEmail))
            {
                return RedirectToAction("Login", "Account");
            }
            var userAccount = await _userRepo.GetByEmailAsync(currentUserEmail);

            if (userAccount == null)
            {
                return View("Error");
            }

            return View(userAccount);
        }
        public IActionResult Decentralization()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> UpdatePersonalPage()
        {
            // Lấy email từ Claims
            
            var currentUserEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(currentUserEmail))
            {
                _logger.LogWarning("User is not logged in.");
                return RedirectToAction("Login", "Account");
            }

            var userAccount = await _userRepo.GetByEmailAsync(currentUserEmail);
            if (userAccount == null)
            {
                _logger.LogWarning($"User with email {currentUserEmail} not found.");
                return View("Error");
            }

            return View(userAccount);
        }


        [HttpPost]
        public async Task<IActionResult> UpdatePersonalPage(UserAccount updatedUser, IFormFile image)
        {
            //if (!ModelState.IsValid)
            //{
            //    _logger.LogWarning("Model state is invalid.");
            //    return View(updatedUser); // Return the same view with validation errors
            //}

            var currentUserEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(currentUserEmail))
            {
                _logger.LogWarning("User is not logged in.");
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var userAccount = await _userRepo.GetByEmailAsync(currentUserEmail);
                if (userAccount == null)
                {
                    _logger.LogWarning($"User with email {currentUserEmail} not found.");
                    return View("Error");
                }

                // Không cho phép cập nhật email
                // userAccount.Email = updatedUser.Email;  // => Bỏ dòng này

                // Cập nhật thông tin người dùng
                userAccount.FullName = updatedUser.FullName;
                userAccount.PhoneNumber = updatedUser.PhoneNumber;
                userAccount.Birthday = updatedUser.Birthday;
                userAccount.Address = updatedUser.Address;

                // Kiểm tra và xử lý upload ảnh
                if (image != null && image.Length > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var fileExtension = Path.GetExtension(image.FileName).ToLower();

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("", "Only image files (.jpg, .jpeg, .png, .gif) are allowed.");
                        return View(updatedUser);
                    }

                    // Xóa ảnh cũ nếu không phải ảnh mặc định
                    if (!string.IsNullOrEmpty(userAccount.Image) && userAccount.Image != "/images/default-avatar.png")
                    {
                        var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", userAccount.Image.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    // Lưu ảnh mới
                    var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "Avatar_images", uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }

                    userAccount.Image = "/images/Avatar_images/" + uniqueFileName;
                    _logger.LogInformation("Updated user image.");
                }

                // Nếu không có ảnh thì giữ nguyên ảnh cũ hoặc đặt ảnh mặc định
                if (string.IsNullOrEmpty(userAccount.Image))
                {
                    userAccount.Image = "/images/default-avatar.png";
                }

                // Lưu vào database
                await _userRepo.UpdateAsync(userAccount);
                _logger.LogInformation($"User {currentUserEmail} updated successfully.");

                return RedirectToAction("PersonalPage");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating user {currentUserEmail}: {ex.Message}");
                return StatusCode(500, "An error occurred while updating the user");
            }
        }

        public async Task<IActionResult> ViewCourse(int id)
        {
            var course = await _coursesRepo.GetCourseByIdAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            // Lấy tên danh mục
            var category = await _catogoriesRepository.GetCategoryByIdAsync(course.CategoryID ?? 0);

            ViewData["CategoryName"] = category?.CategoryName ?? "Không xác định";
            ViewData["Status"] = course.Status == 1 ? "Hoạt động" : "Ngừng hoạt động";
            ViewData["Price"] = string.Format("{0:N0} VNĐ", course.Price);

            return View(course);
        }



        [HttpGet]
        public async Task<IActionResult> UpdateCourse(int id)
        {
            var course = await _coursesRepo.GetCourseByIdAsync(id);
            if (course == null) return NotFound();

            // Load danh mục
            var categories = await _catogoriesRepository.GetAllCategoriesAsync();
            ViewBag.Categories = categories.Select(c => new SelectListItem
            {
                Value = c.CategoryId.ToString(),
                Text = c.CategoryName
            }).ToList();

            // Load trạng thái
            ViewBag.StatusOptions = new List<SelectListItem>
    {
        new SelectListItem { Value = "1", Text = "Hoạt động" },
        new SelectListItem { Value = "2", Text = "Ngừng hoạt động" }
    };

            return View(course);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCourse(Courses updatedCourse, IFormFile image)
        {
            // Kiểm tra ModelState trước
            if (!ModelState.IsValid)
            {
                // Load lại các danh mục và trạng thái nếu có lỗi validation
                await LoadCategoriesAndStatusAsync();
                return View(updatedCourse);
            }

            // Lấy thông tin khóa học cần cập nhật
            var course = await _coursesRepo.GetCourseByIdAsync(updatedCourse.CourseID);
            if (course == null) return NotFound();

            // Cập nhật các trường dữ liệu của khóa học ngay cả khi không có ảnh mới
            course.CourseName = updatedCourse.CourseName;
            course.Description = updatedCourse.Description;
            course.Price = updatedCourse.Price;
            course.CategoryID = updatedCourse.CategoryID;
            course.Status = updatedCourse.Status;

            // Xử lý hình ảnh mới nếu có
            if (image != null && image.Length > 0)
            {
                var updateResult = await UpdateCourseImageAsync(course, image);
                if (!updateResult)
                {
                    ModelState.AddModelError("Image", "Lỗi khi tải lên hình ảnh.");
                    await LoadCategoriesAndStatusAsync();
                    return View(updatedCourse);
                }
            }
            else
            {
                // Nếu không có ảnh mới, giữ lại ảnh cũ
                // Đảm bảo rằng giá trị ImageUrl không bị null
                if (string.IsNullOrEmpty(course.ImageUrl))
                {
                    course.ImageUrl = "/images/default-course.png"; // Hoặc giữ giá trị mặc định nếu cần
                }
            }

            // Cập nhật khóa học vào cơ sở dữ liệu
            try
            {
                var updateSuccessful = await _coursesRepo.UpdateCourseAsync(course);
                if (!updateSuccessful)
                {
                    ModelState.AddModelError("", "Cập nhật khóa học không thành công.");
                    await LoadCategoriesAndStatusAsync();
                    return View(updatedCourse);
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi khi cập nhật khóa học
                Console.WriteLine($"Error updating course: {ex.Message}");
                ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật khóa học.");
                await LoadCategoriesAndStatusAsync();
                return View(updatedCourse);
            }

            // Chuyển hướng tới danh sách khóa học
            return RedirectToAction("ListCourse");
        }

        private async Task LoadCategoriesAndStatusAsync()
        {
            // Load danh mục
            var categories = await _catogoriesRepository.GetAllCategoriesAsync();
            ViewBag.Categories = categories.Select(c => new SelectListItem
            {
                Value = c.CategoryId.ToString(),
                Text = c.CategoryName
            }).ToList();

            // Load trạng thái
            ViewBag.StatusOptions = new List<SelectListItem>
    {
        new SelectListItem { Value = "1", Text = "Hoạt động" },
        new SelectListItem { Value = "2", Text = "Ngừng hoạt động" }
    };
        }

        private async Task<bool> UpdateCourseImageAsync(Courses course, IFormFile image)
        {
            try
            {
                var ext = Path.GetExtension(image.FileName).ToLower();
                var allowedExts = new[] { ".jpg", ".jpeg", ".png", ".gif" };

                // Kiểm tra xem file có hợp lệ không
                if (!allowedExts.Contains(ext))
                {
                    return false; // File không hợp lệ
                }

                // Xóa ảnh cũ nếu có và không phải ảnh mặc định
                if (!string.IsNullOrEmpty(course.ImageUrl) && course.ImageUrl != "/images/default-course.png")
                {
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", course.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath))
                    {
                        try
                        {
                            System.IO.File.Delete(oldPath); // Xóa ảnh cũ
                        }
                        catch (Exception ex)
                        {
                            // Log lỗi nếu có
                            Console.WriteLine($"Error deleting old image: {ex.Message}");
                            return false; // Nếu có lỗi xóa ảnh cũ, trả về false
                        }
                    }
                }

                // Tạo tên mới cho tệp hình ảnh
                var fileName = Guid.NewGuid().ToString() + ext;
                var newPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "Course_images", fileName);

                // Lưu ảnh mới
                using (var stream = new FileStream(newPath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                // Cập nhật ImageUrl trong khóa học
                course.ImageUrl = "/images/Course_images/" + fileName;
                return true;
            }
            catch (Exception ex)
            {
                // Log lỗi nếu có
                Console.WriteLine($"Error saving new image: {ex.Message}");
                return false; // Trả về false nếu có lỗi
            }
        }




        [HttpGet]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var course = await _coursesRepo.GetCourseByIdAsync(id);
            if (course == null)
            {
                return NotFound();
            }
            return View(course);
        }
        [HttpPost, ActionName("DeleteCourse")]
        public async Task<IActionResult> DeleteCourseConfirmed(int id)
        {
            var course = await _coursesRepo.GetCourseByIdAsync(id);
            if (course == null) return NotFound();

            course.Status = 2; // Ngừng hoạt động
            await _coursesRepo.UpdateCourseAsync(course);
            return RedirectToAction("ListCourse");
        }


        [HttpGet]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            var students = await _userRepo.GetUsersByRoleAsync(3); // role = 3 = học viên
            var student = students.FirstOrDefault(s => s.UserID == id);

            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteStudent(int id, int statusId)
        {
            if (statusId != 2 && statusId != 3)
            {
                return BadRequest("Trạng thái không hợp lệ.");
            }

            await _userRepo.UpdateUserStatusAsync(id, statusId);
            return RedirectToAction("ListStudent");
        }


        [HttpPost]
        public async Task<IActionResult> UpdateStudent(int id, UserAccount updatedStudent)
        {
            var students = await _userRepo.GetUsersByRoleAsync(3);
            var student = students.FirstOrDefault(s => s.UserID == id);
            if (student == null)
            {
                return NotFound();
            }

            student.FullName = updatedStudent.FullName;
            student.Email = updatedStudent.Email;
            student.PhoneNumber = updatedStudent.PhoneNumber;
            student.Birthday = updatedStudent.Birthday;
            student.Status = updatedStudent.Status;

            await _userRepo.UpdateUserAsync(student);
            return RedirectToAction("ListStudent");
        }
        [HttpGet]
        public async Task<IActionResult> UpdateStudent(int id)
        {
            var students = await _userRepo.GetUsersByRoleAsync(3);
            var student = students.FirstOrDefault(s => s.UserID == id);
            if (student == null)
            {
                return NotFound();
            }
            return View(student);
        }
        public async Task<IActionResult> ViewStudent(int id)
        {
            var students = await _userRepo.GetUsersByRoleAsync(3);
            var student = students.FirstOrDefault(s => s.UserID == id);

            if (student == null)
            {
                return NotFound();
            }

            ViewData["FullName"] = student.FullName;
            ViewData["Email"] = student.Email;
            ViewData["PhoneNumber"] = student.PhoneNumber;
            ViewData["Birthday"] = student.Birthday?.ToString("dd/MM/yyyy");
            ViewData["Status"] = student.Status == 1 ? "Active" : "Ngừng Học";
            
            return View(student);
        }
        public async Task<IActionResult> ViewTeacher(int id)
        {   var lecturers = await _userRepo.GetUsersByRoleAsync(2);
            var teacher = lecturers.FirstOrDefault(l => l.UserID == id);
            if (teacher == null) {
                return NotFound();
            }

            ViewData["FullName"] = teacher.FullName;
            ViewData["Email"] = teacher.Email;
            ViewData["PhoneNumber"] = teacher.PhoneNumber;
            ViewData["Birthday"] = teacher.Birthday?.ToString("dd/MM/yyyy");
            ViewData["Status"] = teacher.Status == 1 ? "Hoạt Động" : "Ngừng Dạy";


            return View(teacher);
        }
        public IActionResult AddTeacher()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateTeacher(int id, UserAccount updatedTeacher)
        {
            // Lấy danh sách các giáo viên (role = 2)
            var teachers = await _userRepo.GetUsersByRoleAsync(2);
            // Lọc giáo viên theo ID
            var teacher = teachers.FirstOrDefault(t => t.UserID == id);
            // Kiểm tra xem giáo viên có tồn tại không
            if (teacher == null)
            {
                return NotFound();
            }
            // Cập nhật thông tin của giáo viên
            teacher.FullName = updatedTeacher.FullName;
            teacher.Email = updatedTeacher.Email;
            teacher.PhoneNumber = updatedTeacher.PhoneNumber;
            teacher.Birthday = updatedTeacher.Birthday;
            teacher.Status = updatedTeacher.Status;
            // Lưu lại thông tin đã cập nhật
            await _userRepo.UpdateUserAsync(teacher);
            // Chuyển hướng về danh sách giáo viên
            return RedirectToAction("ListTeacher");
        }

        [HttpGet]
        public async Task<IActionResult> UpdateTeacher(int id)
        {
            var teachers = await _userRepo.GetUsersByRoleAsync(2);
            var teacher = teachers.FirstOrDefault(t => t.UserID == id);

            if (teacher == null)
            {
                return NotFound();
            }

            return View(teacher);
        }
        [HttpGet]
        public async Task<IActionResult> DeleteTeacher(int id)
        {
            var teachers = await _userRepo.GetUsersByRoleAsync(2); // 2 = giáo viên
            var teacher = teachers.FirstOrDefault(t => t.UserID == id);

            if (teacher == null)
            {
                return NotFound();
            }

            return View(teacher);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteTeacher(int id, int statusId)
        {
            if (statusId != 2 && statusId != 3)
            {
                return BadRequest("Trạng thái không hợp lệ.");
            }

            await _userRepo.UpdateUserStatusAsync(id, statusId);
            return RedirectToAction("ListTeacher");
        }
        //[HttpPost]
        //public async Task<IActionResult> EncryptOldPasswords()
        //{
        //    var users = await _userRepo.GetAllUsersAsync();
        //    var hasher = new PasswordHasher<UserAccount>();

        //    foreach (var user in users)
        //    {
        //        // Nếu mật khẩu chưa được mã hóa (ví dụ: độ dài nhỏ hơn 30)
        //        if (!string.IsNullOrEmpty(user.Password) && user.Password.Length < 30)
        //        {
        //            user.Password = hasher.HashPassword(user, user.Password);
        //        }
        //    }

        //    await _userRepo.SaveAllUsersAsync(users);
        //    return Content("Đã mã hóa mật khẩu thành công!");
        //}

    }
}
