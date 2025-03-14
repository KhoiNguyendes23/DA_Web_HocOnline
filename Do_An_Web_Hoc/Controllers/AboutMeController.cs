using Microsoft.AspNetCore.Mvc;

namespace Do_An_Web_Hoc.Controllers
{
    public class AboutMeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
