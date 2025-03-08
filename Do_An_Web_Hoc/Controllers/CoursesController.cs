using Microsoft.AspNetCore.Mvc;

namespace Do_An_Web_Hoc.Controllers
{
    public class CoursesController : Controller
    {
        public IActionResult WebDevelopment()
        {
            return View();
        }

        public IActionResult GameDevelopment()
        {
            return View();
        }

        public IActionResult DataScience()
        {
            return View();
        }

        public IActionResult AI_MachineLearning()
        {
            return View();
        }

        public IActionResult AlgorithmsDataStructures()
        {
            return View();
        }
    }
}
