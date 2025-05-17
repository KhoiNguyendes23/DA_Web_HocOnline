using Microsoft.AspNetCore.Mvc;
using Do_An_Web_Hoc.Services.Interfaces;
using Do_An_Web_Hoc.Models;

namespace Do_An_Web_Hoc.Controllers
{
    public class OdooCourseController : Controller
    {
        private readonly IOdooCourseService _odooCourseService;

        public OdooCourseController(IOdooCourseService odooCourseService)
        {
            _odooCourseService = odooCourseService;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(string courseName, string description)
        {
            var courseId = await _odooCourseService.CreateCourseAsync(courseName, description);
            ViewBag.Result = courseId != null
                ? $"✅ Tạo khóa học thành công (ID: {courseId})"
                : "❌ Tạo khóa học thất bại.";
            return View("Result");
        }

        [HttpGet]
        public IActionResult Search()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Search(string courseName)
        {
            var courseId = await _odooCourseService.SearchCourseByNameAsync(courseName);
            ViewBag.Result = courseId != null
                ? $"🔍 Tìm thấy khóa học (ID: {courseId})"
                : "⚠️ Không tìm thấy khóa học.";
            return View("Result");
        }
    }
}
