using Do_An_Web_Hoc.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Do_An_Web_Hoc.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IUserAccountRepository _userRepo;
        private readonly ICoursesRepository _coursesRepo;
        public AdminController(IUserAccountRepository userRepo, ICoursesRepository coursesRepo)
        {
            _userRepo = userRepo;
            _coursesRepo = coursesRepo;
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
        public IActionResult UpdateStudent()
        {
            return View();
        }
        public IActionResult ViewStudent()
        {
            return View();
        }
        public IActionResult ViewTeacher()
        {
            return View();
        }
        public IActionResult AddTeacher()
        {
            return View();
        }
        public IActionResult UpdateTeacher()
        {
            return View();
        }
        public IActionResult DeleteTeacher()
        {
            return View();
        }
    }
}
