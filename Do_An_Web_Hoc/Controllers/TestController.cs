using Do_An_Web_Hoc.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Do_An_Web_Hoc.Controllers
{
    public class TestController : Controller
    {
        private readonly IUserAccountRepository _userRepo;

        public TestController(IUserAccountRepository userRepo)
        {
            _userRepo = userRepo;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(); // View hiển thị form tìm kiếm ban đầu
        }

        [HttpPost]
        public async Task<IActionResult> Index(int roleId)
        {
            var users = await _userRepo.GetUsersByRoleAsync(roleId);
            return View(users);
        }
    }
}