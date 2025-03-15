using Do_An_Web_Hoc.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Do_An_Web_Hoc.Controllers
{
    public class AdminController : Controller
    {
        private readonly IUserAccountRepository _userRepo;
        private readonly ICoursesRepository _courseRepo;

        public AdminController(IUserAccountRepository userRepo, ICoursesRepository courseRepo)
        {
            _userRepo = userRepo;
            _courseRepo = courseRepo;
        }
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
