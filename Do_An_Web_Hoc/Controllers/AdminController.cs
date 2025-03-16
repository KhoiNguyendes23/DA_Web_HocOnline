using Do_An_Web_Hoc.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Do_An_Web_Hoc.Controllers
{
    public class AdminController : Controller
    {
        private readonly IUserAccountRepository _userRepo;
        public AdminController(IUserAccountRepository userRepo)
        {
            _userRepo = userRepo;
        }
        public IActionResult Dashboard()
        {
            return View();
        }
        public IActionResult ListCourse()
        {
            return View();
        }
        public IActionResult AddCourse()
        {
            return View();
        }
        public async Task<IActionResult> ListStudent()
        {
            // lấy danh sách các user có roleID = 3 (sinh viên)
            var students = await _userRepo.GetUsersByRoleAsync(3);
            return View(students);
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


    }
}
