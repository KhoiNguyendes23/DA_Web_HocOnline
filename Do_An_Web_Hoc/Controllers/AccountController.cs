using Do_An_Web_Hoc.Models;
using Do_An_Web_Hoc.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

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
                return View("Index");
            }
            int roleId = user.RoleID ?? 0;
            string roleName = roleId switch
            {
                1 => "Admin",
                2 => "Lecturer",
                _ => "User" // Nếu không có RoleID, mặc định là "User"
            };
            // Gán Role vào Claims để `[Authorize(Roles="User")]` nhận diện được
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.UserName),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Role, roleName) //Đây là Role mà `[Authorize]` sử dụng
    };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties { IsPersistent = true };
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity), authProperties);
            Console.WriteLine($"[DEBUG] Đăng nhập thành công: {user.UserName} - Role: {roleName}");
            // Chuyển hướng theo Role
            return roleId switch
            {
                1 => RedirectToAction("Dashboard", "Admin"),
                2 => RedirectToAction("Dashboard", "Lecturer"),
                _ => RedirectToAction("Dashboard", "User")
            };
        }


        // Xử lý đăng ký
        [HttpPost]
        public async Task<IActionResult> Register(UserAccount model)
        {
            if (await _userAccountRepository.CheckUserExistsAsync(model.Email))
            {
                ViewBag.Error = "Email đã tồn tại!";
                return View("Index", model);
            }

            var newUser = await _userAccountRepository.RegisterAsync(model, model.Password); //Thêm mật khẩu

            if (newUser == null)
            {
                ViewBag.Error = "Đăng ký thất bại!";
                return View(model);
            }

            return RedirectToAction("Index");
        }

        // Đăng xuất
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
        // Trang yêu cầu gửi OTP
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email) || !email.Contains("@"))
                return Json(new { success = false, message = "Email không hợp lệ!" });

            bool result = await _userAccountRepository.SendOTPAsync(email);
            if (!result)
                return Json(new { success = false, message = "Email không tồn tại!" });

            TempData["EmailOTP"] = email;
            return Json(new { success = true });
        }

        // nhập OTP
        public IActionResult VerifyOTP()
        {
            if (TempData["EmailOTP"] == null)
                return RedirectToAction("ForgotPassword");

            return View("ForgotPassword"); // Chỉ định rõ view
        }

        [HttpPost]
        public async Task<IActionResult> VerifyOTP([FromForm] string otp)
        {
            Console.WriteLine($"[DEBUG] OTP nhận được từ request: '{otp}'");

            if (string.IsNullOrEmpty(otp))
            {
                return Json(new { success = false, message = "OTP không được để trống!" });
            }

            otp = otp.Trim();

            var email = TempData["EmailOTP"] as string;
            if (email == null)
            {
                return Json(new { success = false, message = "Phiên OTP đã hết hạn!" });
            }

            // Kiểm tra OTP trong database
            bool isOtpValid = await _userAccountRepository.VerifyOTPAsync(email, otp);

            if (!isOtpValid)
            {
                TempData["EmailOTP"] = email;
                return Json(new { success = false, message = "Mã OTP không hợp lệ hoặc hết hạn!" });
            }

            //TempData["EmailVerified"] = email;
            HttpContext.Session.SetString("EmailVerified", email);
            return Json(new { success = true });
        }

        [HttpGet]
        public IActionResult Login()
        {
            return RedirectToAction("Index"); // Chuyển hướng đến trang Index (chứa cả đăng nhập & đăng ký)
        }

        // Reset mật khẩu
        [HttpPost]
        public async Task<IActionResult> ResetPassword(string newPassword, string confirmPassword)
        {
            try
            {
                if (newPassword != confirmPassword)
                {
                    return Json(new { success = false, message = "Mật khẩu xác nhận không khớp!" });
                }
                var email = HttpContext.Session.GetString("EmailVerified");
                if (string.IsNullOrEmpty(email))
                {
                    return Json(new { success = false, message = "Phiên đặt lại mật khẩu đã hết hạn! Vui lòng thực hiện lại từ đầu." });
                }
                // Kiểm tra độ mạnh mật khẩu
                if (newPassword.Length < 8 || !newPassword.Any(char.IsUpper) ||
                    !newPassword.Any(char.IsDigit) || !newPassword.Any(ch => !char.IsLetterOrDigit(ch)))
                {
                    return Json(new { success = false, message = "Mật khẩu không đủ mạnh! Yêu cầu: ít nhất 8 ký tự, 1 chữ hoa, 1 số, 1 ký tự đặc biệt." });
                }
                // Thực hiện đặt lại mật khẩu
                bool resetSuccess = await _userAccountRepository.ResetPasswordByOTPAsync(email, newPassword);
                if (!resetSuccess)
                {
                    return Json(new { success = false, message = "Đặt lại mật khẩu thất bại! Vui lòng thử lại." });
                }
                // Xóa session sau khi đặt lại mật khẩu thành công
                HttpContext.Session.Remove("EmailVerified");
                return Json(new { success = true, redirectUrl = Url.Action("Index", "Account") });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Lỗi khi đặt lại mật khẩu: {ex.Message}");
                return Json(new { success = false, message = "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau." });
            }
        }
    }
}

