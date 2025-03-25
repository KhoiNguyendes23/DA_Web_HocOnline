using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Do_An_Web_Hoc.Repositories.Interfaces;

namespace Do_An_Web_Hoc.Controllers
{
    [Authorize(Roles = "Lecturer")]
    public class LecturerController : Controller
    {
        // Commented out repositories to only test view rendering
        // private readonly ILecturesRepository _lecturesRepository;
        // private readonly ICoursesRepository _coursesRepository;
        // private readonly IExamsRepository _examsRepository;
        // private readonly IResultsRepository _resultsRepository;
        // private readonly IUserAccountRepository _userAccountRepository;

        public LecturerController()
        {
        }

        public IActionResult Dashboard()
        {
            ViewData["Title"] = "Giảng viên";
            return View();
        }

        public IActionResult ListExam()
        {
            // Test view rendering only
            return View();
        }

        public IActionResult AddExam()
        {
            return View();
        }

        public IActionResult ListCourse()
        {
            return View();
        }
        public IActionResult ViewExam()
        {
            return View();
        }
        public IActionResult EditExam()
        {
            return View();
        }
        public IActionResult DeleteExam()
        {
            return View();
        }
        public IActionResult AddCourse()
        {
            return View();
        }

        public IActionResult ListStudent()
        {
            return View();
        }

        public IActionResult ResultExam()
        {
            return View();
        }

        public IActionResult Profile()
        {
            return View();
        }

        public IActionResult Forum()
        {
            return View();
        }
    }
}
