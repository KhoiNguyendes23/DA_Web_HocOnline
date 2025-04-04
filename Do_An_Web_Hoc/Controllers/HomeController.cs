using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Do_An_Web_Hoc.Models;
using Do_An_Web_Hoc.Repositories.Interfaces;

namespace Do_An_Web_Hoc.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ICoursesRepository _coursesRepo;
    private readonly ICatogoriesRepository _catogoriesRepository;

    public HomeController(ILogger<HomeController> logger, ICoursesRepository coursesRepo, ICatogoriesRepository catogoriesRepository)
    {
        _coursesRepo = coursesRepo;
        _logger = logger;
        _catogoriesRepository = catogoriesRepository;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Reviews()
    {
        return View();
    }

    // ✅ Xem danh sách khóa học theo danh mục
    public async Task<IActionResult> CoursesByCategory(int id)
    {
        var category = await _catogoriesRepository.GetCategoryByIdAsync(id);
        if (category == null || category.Status != 1)
        {
            return NotFound();
        }

        var courses = await _coursesRepo.GetCoursesByCategoryAsync(id);
        courses = courses.Where(c => c.Status == 1).ToList(); // chỉ lấy các khóa học đang hoạt động

        ViewBag.CategoryName = category.CategoryName;
        return View(courses);
    }

    // ✅ Xem chi tiết khóa học
    public async Task<IActionResult> CourseDetails(int id)
    {
        var course = await _coursesRepo.GetCourseByIdAsync(id);
        if (course == null || course.Status != 1)
        {
            return NotFound();
        }
        return View(course);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
