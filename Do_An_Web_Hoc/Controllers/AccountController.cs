using Do_An_Web_Hoc.Models;
using Do_An_Web_Hoc.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Do_An_Web_Hoc.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserAccountRepository _userAccountRepository;

        public AccountController(IUserAccountRepository userAccountRepository)
        {
            _userAccountRepository = userAccountRepository;
        }

        // Trang đăng nhập / đăng ký
        public IActionResult Index()
        {
            return View();
        }

        // Xử lý đăng nhập
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _userAccountRepository.LoginAsync(email, password);
            if (user == null)
            {
                ViewBag.Error = "Sai email hoặc mật khẩu!";
                return Index();
            }

            // Lấy RoleID an toàn (nếu null thì gán = 0)
            int roleId = user.RoleID ?? 0;

            // Lưu thông tin đăng nhập vào Session
            HttpContext.Session.SetString("UserID", user.UserID.ToString());
            HttpContext.Session.SetString("UserName", user.UserName);
            HttpContext.Session.SetInt32("RoleID", roleId);

            // Điều hướng theo RoleID
            return roleId switch
            {
                1 => RedirectToAction("Dashboard", "Admin"),    // Admin
                2 => RedirectToAction("Dashboard", "Lecturer"), // Giảng viên
                _ => RedirectToAction("Dashboard", "User")      // Người dùng thông thường
            };
        }

        // Trang đăng ký
        public IActionResult Register()
        {
            return View();
        }
        // Xử lý đăng ký
        [HttpPost]
        public async Task<IActionResult> Register(UserAccount model)
        {
            if (await _userAccountRepository.CheckUserExistsAsync(model.Email))
            {
                ViewBag.Error = "Email đã tồn tại!";
                return View(model);
            }

            var newUser = await _userAccountRepository.RegisterAsync(model, model.Password); //Thêm mật khẩu

            if (newUser == null)
            {
                ViewBag.Error = "Đăng ký thất bại!";
                return View(model);
            }

            return RedirectToAction("Login");
        }

        // Đăng xuất
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
