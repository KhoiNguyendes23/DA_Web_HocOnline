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
    private readonly ILecturesRepository _lecturesRepo;

    public HomeController(ILogger<HomeController> logger, ICoursesRepository coursesRepo,
                          ICatogoriesRepository catogoriesRepository, ILecturesRepository lecturesRepo)
    {
        _logger = logger;
        _coursesRepo = coursesRepo;
        _catogoriesRepository = catogoriesRepository;
        _lecturesRepo = lecturesRepo;
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

    // ✅ Xem tất cả các khóa học (dành cho nút "Xem thêm các khóa học")
    public async Task<IActionResult> AllCourses(string searchString)
    {
        var courses = await _coursesRepo.GetAllCoursesAsync();
        courses = courses.Where(c => c.Status == 1).ToList(); // chỉ lấy khóa học đang hoạt động

        if (!string.IsNullOrEmpty(searchString))
        {
            // Chuyển về chữ thường để tìm không phân biệt hoa thường
            string loweredSearch = searchString.ToLower();
            var filteredCourses = courses
                .Where(c => c.CourseName != null && c.CourseName.ToLower().Contains(loweredSearch))
                .ToList();

            if (!filteredCourses.Any())
            {
                ViewBag.SearchMessage = $"Không tìm thấy khóa học nào với từ khóa \"{searchString}\".";
            }

            return View(filteredCourses);
        }

        return View(courses);
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

        var lectures = await _lecturesRepo.GetLecturesByCourseIdAsync(id);

        var viewModel = new CourseDetailViewModel
        {
            Course = course,
            Lectures = lectures.ToList()
        };

        return View(viewModel);
    }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
