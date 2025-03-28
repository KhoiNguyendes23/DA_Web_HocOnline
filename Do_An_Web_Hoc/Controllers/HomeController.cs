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

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

}
