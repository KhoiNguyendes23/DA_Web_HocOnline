using Microsoft.AspNetCore.Mvc;

namespace Do_An_Web_Hoc.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
