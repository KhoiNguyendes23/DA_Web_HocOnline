using Do_An_Web_Hoc.Models;
using Do_An_Web_Hoc.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;

namespace Do_An_Web_Hoc.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly IConfiguration _configuration;
        public AccountController(IUserAccountRepository userAccountRepository, IConfiguration configuration)
        {
            _userAccountRepository = userAccountRepository;
            _configuration = configuration;
        }

        // Trang đăng nhập
        public IActionResult Login()
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
                return View();
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
        // Trang yêu cầu gửi OTP
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            bool result = await _userAccountRepository.SendOTPAsync(email);
            if (!result)
            {
                ViewBag.Error = "Email không tồn tại!";
                return View();
            }

            TempData["EmailOTP"] = email;
            return RedirectToAction("VerifyOTP");
        }
        // Trang nhập OTP
        public IActionResult VerifyOTP() => View();

        [HttpPost]
        public async Task<IActionResult> VerifyOTP(string otp)
        {
            var email = TempData["EmailOTP"] as string;
            if (email == null)
                return RedirectToAction("ForgotPassword");

            bool isOtpValid = await _userAccountRepository.VerifyOTPAsync(email, otp);
            if (!isOtpValid)
            {
                ViewBag.Error = "Mã OTP không hợp lệ hoặc hết hạn!";
                TempData["EmailOTP"] = email; // Giữ lại email để thử lại
                return View();
            }

            TempData["EmailVerified"] = email;
            return RedirectToAction("ResetPassword");
        }
        // Trang đặt mật khẩu mới
        public IActionResult ResetPassword() => View();

        [HttpPost]
        public async Task<IActionResult> ResetPassword(string newPassword)
        {
            var email = TempData["EmailVerified"] as string;
            if (email == null)
                return RedirectToAction("ForgotPassword");

            bool resetSuccess = await _userAccountRepository.ResetPasswordByOTPAsync(email, newPassword);

            if (!resetSuccess)
            {
                ViewBag.Error = "Đặt lại mật khẩu thất bại!";
                return View();
            }

            return RedirectToAction("Login");
        }
    }
}

