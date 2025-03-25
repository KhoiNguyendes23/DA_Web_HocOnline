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
        public AdminController(IUserAccountRepository userRepo, ICoursesRepository coursesRepo, IExamsRepository examsRepo, ILogger<AdminController> logger, ICatogoriesRepository catogoriesRepository)
        {
            _userRepo = userRepo;
            _coursesRepo = coursesRepo;
            _examsRepo = examsRepo;
            _logger = logger;
            _catogoriesRepository = catogoriesRepository;
        }
        public IActionResult Dashboard()
        {
            var fullName = User.FindFirstValue(ClaimTypes.Name);
            var roleName = User.FindFirstValue(ClaimTypes.Role);
            ViewData["FullName"] = fullName;
            ViewData["RoleName"] = roleName;
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
        public async Task<IActionResult> AddCourse(Courses course)
        {
            if (!ModelState.IsValid)
            {
                // Load lại dropdown nếu có lỗi
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
        public IActionResult ListExam()
        {
            return View();
        }
        public IActionResult AddExam()
        {
            return View();
        }
        public IActionResult ViewExam()
        {
            return View();
        }
        public IActionResult UpdateExam()
        {
            return View();
        }
        public IActionResult DeleteExam()
        {
            return View();
        }
        public IActionResult TestResult()
        {
            return View();
        }
        public IActionResult StatisticalCourse()
        {
            return View();
        }
        public IActionResult StatisticalRevenue()
        {
            return View();
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
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }

                    userAccount.Image = "/images/" + uniqueFileName;
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


        public IActionResult ViewCourse()
        {
            return View();
        }
        public IActionResult UpdateCourse()
        {
            return View();
        }
        public IActionResult DeleteCourse()
        {
            return View();
        }
        public IActionResult AddStudent()
        {
            return View();
        }
        public IActionResult DeleteStudent()
        {
            return View();
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


        public IActionResult DeleteTeacher()
        {
            return View();
        }
    }
}
