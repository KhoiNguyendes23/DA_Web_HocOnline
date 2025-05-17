using Microsoft.AspNetCore.Mvc;
using Do_An_Web_Hoc.Services.Interfaces;

namespace Do_An_Web_Hoc.Controllers
{
    public class OdooLectureController : Controller
    {
        private readonly IOdooLectureService _odooLectureService;

        public OdooLectureController(IOdooLectureService odooLectureService)
        {
            _odooLectureService = odooLectureService;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(string title, string content, string videoUrl, int courseId)
        {
            var lectureId = await _odooLectureService.CreateLectureAsync(title, content, videoUrl, courseId);

            ViewBag.Result = lectureId != null
                ? $"✅ Tạo bài giảng thành công (ID: {lectureId})"
                : "❌ Tạo bài giảng thất bại";

            return View("Result");
        }

        [HttpGet]
        public IActionResult Search()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Search(string title)
        {
            var lectureId = await _odooLectureService.SearchLectureByTitleAsync(title);

            ViewBag.Result = lectureId != null
                ? $"🔍 Bài giảng tìm thấy (ID: {lectureId})"
                : "⚠️ Không tìm thấy bài giảng nào.";

            return View("Result");
        }
    }
}
