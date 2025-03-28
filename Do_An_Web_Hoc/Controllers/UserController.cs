using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Do_An_Web_Hoc.Controllers
{
    [Authorize(Roles = "User")]
    public class UserController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }

        public IActionResult Courses()
        {
            return View();
        }
        
        public IActionResult Completed()
        {
            return View();
        }

        public IActionResult RegisteredCourses()
        {
            return View();
        }

        public IActionResult InProgress()
        {
            return View();
        }

        public IActionResult FreeCourses()
        {
            return View();
        }

        public IActionResult PaidCourses()
        {
            return View();
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

        public IActionResult SecuritySettings()
        {
            return View();
        }
    }
}
