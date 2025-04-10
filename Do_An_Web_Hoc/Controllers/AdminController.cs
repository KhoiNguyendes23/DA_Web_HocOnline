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
using Microsoft.AspNetCore.Mvc.Filters;
using Do_An_Web_Hoc.Models.ViewModels;
using DocumentFormat.OpenXml.InkML;

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
        private readonly IRolesRepository _roleRepo;
        private readonly ILecturesRepository _lecturesRepository;

        private readonly ApplicationDbContext _context;
        public AdminController(IUserAccountRepository userRepo,
                               ICoursesRepository coursesRepo, 
                               IExamsRepository examsRepo, 
                               ILogger<AdminController> logger,
                               ICatogoriesRepository catogoriesRepository,
                               IEnrollmentsRepository enrollmentRepo, 
                               IRolesRepository rolesRepository,
                               ILecturesRepository lecturesRepository,
                               ApplicationDbContext context)
        {
            _userRepo = userRepo;
            _coursesRepo = coursesRepo;
            _examsRepo = examsRepo;
            _logger = logger;
            _catogoriesRepository = catogoriesRepository;
            _enrollmentRepo = enrollmentRepo;
            _roleRepo = rolesRepository;
            _lecturesRepository = lecturesRepository;
            _context = context;
        }
        private async Task SetAdminViewData()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (!string.IsNullOrEmpty(email))
            {
                var user = await _userRepo.GetByEmailAsync(email);
                ViewData["FullName"] = user?.FullName ?? "Admin";
                ViewData["RoleName"] = "Admin";
                ViewData["ImagePath"] = user?.Image;
            }
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            await SetAdminViewData(); // Tự động gán dữ liệu cho ViewData ở mọi action
            await base.OnActionExecutionAsync(context, next);
        }

        public async Task<IActionResult> Dashboard()
        {
            await SetAdminViewData(); // Đảm bảo ảnh, tên, role hiện đúng ở layout

            // Tổng số lượng
            ViewBag.TotalStudents = (await _userRepo.GetUsersByRoleAsync(3)).Count();
            ViewBag.TotalLecturers = (await _userRepo.GetUsersByRoleAsync(2)).Count();
            ViewBag.TotalCourses = (await _coursesRepo.GetAllCoursesAsync()).Count();

            // Doanh thu tháng hiện tại
            ViewBag.MonthlyRevenue = await _enrollmentRepo.GetRevenueByMonthAsync(DateTime.Now.Month, DateTime.Now.Year);

            // Dữ liệu biểu đồ: doanh thu các tháng
            var revenueStatistics = await _enrollmentRepo.GetMonthlyRevenueStatisticsAsync();

            // Top 5 khóa học được đăng ký nhiều nhất
            var topCourses = await _enrollmentRepo.GetTopCoursesAsync(5);

            // 5 người dùng (giảng viên, học viên) mới nhất
            var recentUsers = await _enrollmentRepo.GetRecentUsersAsync(5);

            // Truyền vào ViewModel hoặc ViewBag
            ViewBag.RevenueStatistics = revenueStatistics;
            ViewBag.TopCourses = topCourses;
            ViewBag.RecentUsers = recentUsers;

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
                var ws = workbook.Worksheets.Add("Thống kê doanh thu");

                // Tiêu đề lớn (merge A1-D1)
                ws.Range("A1:D1").Merge().Value = "BÁO CÁO DOANH THU THEO THÁNG";
                var titleCell = ws.Cell("A1");
                titleCell.Style.Font.SetBold().Font.FontSize = 18;
                titleCell.Style.Font.FontColor = XLColor.White;
                titleCell.Style.Fill.BackgroundColor = XLColor.DarkBlue;
                titleCell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                titleCell.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Row(1).Height = 35;

                // Header (A2:D2)
                ws.Cell(2, 1).Value = "Năm";
                ws.Cell(2, 2).Value = "Tháng";
                ws.Cell(2, 3).Value = "Lượt đăng ký";
                ws.Cell(2, 4).Value = "Tổng doanh thu (VNĐ)";
                var headerRange = ws.Range("A2:D2");
                headerRange.Style.Font.SetBold();
                headerRange.Style.Fill.BackgroundColor = XLColor.SkyBlue;
                headerRange.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                headerRange.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thick;
                headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws.Row(2).Height = 25;

                // Dữ liệu
                int row = 3;
                foreach (var item in data)
                {
                    ws.Cell(row, 1).Value = item.Year;
                    ws.Cell(row, 2).Value = item.Month;
                    ws.Cell(row, 3).Value = item.TotalEnrollments;
                    ws.Cell(row, 4).Value = item.TotalRevenue;
                    ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0";

                    ws.Range(row, 1, row, 4).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                    ws.Range(row, 1, row, 4).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                    ws.Range(row, 1, row, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    ws.Range(row, 1, row, 4).Style.Border.InsideBorder = XLBorderStyleValues.Dotted;

                    row++;
                }

                // Căn độ rộng và +2 để dễ nhìn
                ws.Columns().AdjustToContents();
                for (int col = 1; col <= 4; col++)
                {
                    ws.Column(col).Width += 2;
                }

                // Xuất file
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    return File(stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"BaoCaoDoanhThu_{DateTime.Now:yyyyMMdd}.xlsx");
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
        public async Task<IActionResult> Decentralization(string keyword)
        {
            await SetAdminViewData();
            var users = await _userRepo.GetAllUsersAsync();

            if (!string.IsNullOrEmpty(keyword))
            {
                keyword = keyword.ToLower();
                users = users.Where(u =>
                    (!string.IsNullOrEmpty(u.FullName) && u.FullName.ToLower().Contains(keyword)) ||
                    (!string.IsNullOrEmpty(u.Email) && u.Email.ToLower().Contains(keyword)) ||
                    (u.RoleID == 1 && "admin".Contains(keyword)) ||
                    (u.RoleID == 2 && "giảng viên".Contains(keyword)) ||
                    (u.RoleID == 3 && "học viên".Contains(keyword))
                );
            }

            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUserRole([FromBody] RoleUpdateRequest request)
        {
            var currentUserEmail = User.FindFirstValue(ClaimTypes.Email);
            var currentUser = await _userRepo.GetByEmailAsync(currentUserEmail);
            var targetUser = await _userRepo.GetByIdAsync(request.UserId);

            // Không được tự đổi mình
            if (currentUser?.UserID == request.UserId)
            {
                return Json(new { success = false, message = "Bạn không thể thay đổi vai trò của chính mình!" });
            }

            // Không được đổi Admin khác
            if (targetUser?.RoleID == 1)
            {
                return Json(new { success = false, message = "Không thể thay đổi vai trò của người dùng Admin!" });
            }

            await _userRepo.UpdateUserRoleAsync(request.UserId, request.NewRoleId);
            return Json(new { success = true });
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
            if (!ModelState.IsValid)
            {
                await LoadCategoriesAndStatusAsync();
                return View(updatedCourse);
            }

            var course = await _coursesRepo.GetCourseByIdAsync(updatedCourse.CourseID);
            if (course == null) return NotFound();

            if (updatedCourse.CategoryID == null)
            {
                ModelState.AddModelError("", "Danh mục không hợp lệ.");
                await LoadCategoriesAndStatusAsync();
                return View(updatedCourse);
            }

            var category = await _catogoriesRepository.GetCategoryByIdAsync(updatedCourse.CategoryID.Value);
            if (category == null || category.Status == 2)
            {
                ModelState.AddModelError("", "Không thể cập nhật vì danh mục đã ngưng hoạt động.");
                await LoadCategoriesAndStatusAsync();
                return View(updatedCourse);
            }
            course.CourseName = updatedCourse.CourseName;
            course.Description = updatedCourse.Description;
            course.Price = updatedCourse.Price;
            course.CategoryID = updatedCourse.CategoryID;
            if (updatedCourse.Status == 2)
            {
                course.Status = 2;
                var lectures = await _lecturesRepository.GetLecturesByCourseIdAsync(course.CourseID);
                foreach (var lecture in lectures) lecture.Status = 2;
                await _lecturesRepository.SaveChangesAsync();
            }
            else if (updatedCourse.Status == 1)
            {
                course.Status = 1;
                var lectures = await _lecturesRepository.GetLecturesByCourseIdAsync(course.CourseID);
                foreach (var lecture in lectures) lecture.Status = 1;
                await _lecturesRepository.SaveChangesAsync();
            }
            if (image != null && image.Length > 0)
            {
                var result = await UpdateCourseImageAsync(course, image);
                if (!result)
                {
                    ModelState.AddModelError("Image", "Lỗi khi tải ảnh.");
                    await LoadCategoriesAndStatusAsync();
                    return View(updatedCourse);
                }
            }
            if (string.IsNullOrEmpty(course.ImageUrl))
            {
                course.ImageUrl = "/images/default-course.png";
            }

            // ✅ Lưu cập nhật
            var success = await _coursesRepo.UpdateCourseAsync(course);
            if (!success)
            {
                ModelState.AddModelError("", "Không thể cập nhật khóa học.");
                await LoadCategoriesAndStatusAsync();
                return View(updatedCourse);
            }

            TempData["SuccessMessage"] = "Cập nhật khóa học thành công.";
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
                if (!allowedExts.Contains(ext)) return false;

                if (!string.IsNullOrEmpty(course.ImageUrl) && course.ImageUrl != "/images/default-course.png")
                {
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", course.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }

                var fileName = Guid.NewGuid() + ext;
                var newPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "Course_images", fileName);

                using (var stream = new FileStream(newPath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                course.ImageUrl = "/images/Course_images/" + fileName;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving image: {ex.Message}");
                return false;
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
            await SetAdminViewData(); // Dùng cho layout

            var students = await _userRepo.GetUsersByRoleAsync(3);
            var student = students.FirstOrDefault(s => s.UserID == id);

            if (student == null)
            {
                return NotFound();
            }

            return View(student); // Chỉ truyền model, không set ViewData
        }

        public async Task<IActionResult> ViewTeacher(int id)
        {
            await SetAdminViewData(); 

            var lecturers = await _userRepo.GetUsersByRoleAsync(2);
            var teacher = lecturers.FirstOrDefault(l => l.UserID == id);
            if (teacher == null)
            {
                return NotFound();
            }
            return View(teacher); // truyền bằng Model là đủ
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
        [HttpGet]
        public async Task<IActionResult> EditCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        [HttpPost]
        public async Task<IActionResult> EditCategory(Categories category)
        {
            if (!ModelState.IsValid)
            {
                return View(category);
            }

            var existingCategory = await _context.Categories.FindAsync(category.CategoryId);
            if (existingCategory == null)
            {
                return NotFound();
            }

            // Cập nhật thông tin danh mục
            existingCategory.CategoryName = category.CategoryName;
            existingCategory.Status = category.Status;

            await _context.SaveChangesAsync();

            // Tìm tất cả khóa học thuộc danh mục
            var relatedCourses = _context.Courses
                .Where(c => c.CategoryID == category.CategoryId)
                .ToList();

            if (category.Status == 2) // Ngưng danh mục → ngưng khóa học và bài giảng
            {
                foreach (var course in relatedCourses)
                {
                    course.Status = 2;

                    // Ngưng các bài giảng thuộc khóa học
                    var lectures = _context.Lectures.Where(l => l.CourseID == course.CourseID).ToList();
                    foreach (var lecture in lectures)
                    {
                        lecture.Status = 2;
                    }
                }

                await _context.SaveChangesAsync();
            }
            else if (category.Status == 1) // Kích hoạt danh mục → kích hoạt lại khóa học & bài giảng
            {
                foreach (var course in relatedCourses)
                {
                    course.Status = 1;

                    var lectures = _context.Lectures.Where(l => l.CourseID == course.CourseID).ToList();
                    foreach (var lecture in lectures)
                    {
                        lecture.Status = 1;
                    }
                }

                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "Đã cập nhật danh mục thành công.";
            return RedirectToAction("ListCategory");
        }
        public async Task<IActionResult> ListCategory()
        {
            var categories = await _context.Categories.ToListAsync();
            return View(categories);
        }
    }
}
