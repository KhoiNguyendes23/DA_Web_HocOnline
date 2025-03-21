using Do_An_Web_Hoc.Models;
using Do_An_Web_Hoc.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.WebSockets;
using System.Security.Claims;

namespace Do_An_Web_Hoc.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IUserAccountRepository _userRepo;
        private readonly ICoursesRepository _coursesRepo;
        private readonly IExamsRepository _examsRepo;
        public AdminController(IUserAccountRepository userRepo, ICoursesRepository coursesRepo, IExamsRepository examsRepo)
        {
            _userRepo = userRepo;
            _coursesRepo = coursesRepo;
            _examsRepo = examsRepo;
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
        public IActionResult AddCourse()
        {
            return View();
        }
        public async Task<IActionResult> ListStudent()
        {
            var students = await _userRepo.GetUsersByRoleAsync(3);
            return View(students);
        }
        public async  Task<IActionResult> ListTeacher()
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
        public IActionResult PersonalPage()
        {
            return View();
        }
        public IActionResult Decentralization()
        {
            return View();
        }
        public IActionResult UpdatePersonalPage()
        {
            return View();
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
