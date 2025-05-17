using Microsoft.AspNetCore.Mvc;
using Do_An_Web_Hoc.Models;
using Microsoft.EntityFrameworkCore;

namespace Do_An_Web_Hoc.Controllers
{
    public class LiveMeetingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LiveMeetingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Xem danh sách các buổi học trực tuyến
        public async Task<IActionResult> Index()
        {
            var meetings = await _context.LiveMeetings
                .Include(m => m.Course)
                .Include(m => m.Lecturer)
                .ToListAsync();

            return View(meetings);
        }

        // Hiển thị form tạo buổi học mới
        public IActionResult Create()
        {
            ViewBag.Courses = _context.Courses.ToList();
            ViewBag.Lecturers = _context.UserAccounts.Where(u => u.RoleID == 2).ToList(); // Giả sử RoleID = 2 là giảng viên
            return View();
        }

        // Xử lý POST tạo buổi học mới
        [HttpPost]
        public async Task<IActionResult> Create(LiveMeeting meeting)
        {
            if (ModelState.IsValid)
            {
                meeting.Status = "Sắp diễn ra";
                _context.LiveMeetings.Add(meeting);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.Courses = _context.Courses.ToList();
            ViewBag.Lecturers = _context.UserAccounts.Where(u => u.RoleID == 2).ToList();
            return View(meeting);
        }
    }
}
