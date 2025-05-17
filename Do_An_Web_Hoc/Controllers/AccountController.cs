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
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Facebook;
using Do_An_Web_Hoc.Models.Odoo;
using Do_An_Web_Hoc.Services.Interfaces;

namespace Do_An_Web_Hoc.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserAccountRepository _userAccountRepository; // Repository truy cập dữ liệu người dùng
        private readonly IConfiguration _configuration; // Cấu hình appsettings
        private readonly IOdooPartnerService _odooPartnerService; // Dịch vụ đồng bộ Odoo mới (Odoo 17)

        public AccountController(
            IUserAccountRepository userAccountRepository,
            IConfiguration configuration,
            IOdooPartnerService odooPartnerService)
        {
            _userAccountRepository = userAccountRepository;
            _configuration = configuration;
            _odooPartnerService = odooPartnerService;
        }

        // Hiển thị trang đăng nhập / đăng ký
        public IActionResult Index() => View();

        // Xử lý đăng nhập
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _userAccountRepository.LoginAsync(email, password);

            // Kiểm tra tài khoản hợp lệ và không bị cấm
            if (user == null || user.Status == 3)
            {
                ViewBag.Error = user == null ? "Sai email hoặc mật khẩu!" : "Tài khoản của bạn đã bị cấm.";
                return View("Index");
            }

            // Gán vai trò dựa trên RoleID
            int roleId = user.RoleID ?? 0;
            string roleName = roleId switch
            {
                1 => "Admin",
                2 => "Lecturer",
                _ => "User"
            };

            // Tạo danh sách claim để xác thực Cookie
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, roleName)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties { IsPersistent = true };

            // Lưu session và thực hiện đăng nhập
            HttpContext.Session.SetString("UserEmail", user.Email);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity), authProperties);

            // Điều hướng tới trang Dashboard tương ứng với role
            return roleId switch
            {
                1 => RedirectToAction("Dashboard", "Admin"),
                2 => RedirectToAction("Dashboard", "Lecturer"),
                _ => RedirectToAction("Dashboard", "User")
            };
        }

        // Xử lý đăng ký người dùng mới
        [HttpPost]
        public async Task<IActionResult> Register(UserAccount model)
        {
            // Kiểm tra email đã tồn tại chưa
            if (await _userAccountRepository.CheckUserExistsAsync(model.Email))
            {
                ViewBag.Error = "Email đã tồn tại!";
                return View("Index", model);
            }

            // Gán role mặc định là học viên nếu chưa có
            model.RoleID ??= 3;
            model.CreateAt = model.CreateAt == default ? DateTime.Now : model.CreateAt;

            // Đăng ký người dùng trong SQL Server
            var newUser = await _userAccountRepository.RegisterAsync(model, model.Password);
            if (newUser == null)
            {
                ViewBag.Error = "Đăng ký thất bại!";
                return View(model);
            }

            // ✅ Sau khi đăng ký thành công → gửi dữ liệu sang Odoo để tạo res.partner
            try
            {
                var dto = new OdooPartnerDto
                {
                    FullName = newUser.FullName,
                    Email = newUser.Email,
                    PhoneNumber = newUser.PhoneNumber,
                    Address = newUser.Address,
                   
                    Username = newUser.UserName,
                    ExternalUserId = newUser.UserID,
                    Birthday = newUser.Birthday,
                    RoleId = newUser.RoleID,
                    Status = newUser.Status,
                    ImageUrl = string.IsNullOrWhiteSpace(newUser.Image)
               ? null
               : $"{Request.Scheme}://{Request.Host}/images/{newUser.Image}"
                };

                var odooId = await _odooPartnerService.CreatePartnerAsync(dto); // sử dụng Odoo 17 API mới
                Console.WriteLine($"✅ Đồng bộ Odoo thành công. ID: {odooId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Lỗi đồng bộ Odoo: " + ex.Message);
            }

            return RedirectToAction("Index");
        }

        // Đăng xuất
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();              // Hủy xác thực cookie
            HttpContext.Session.Clear();                   // Xóa session
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

            HttpContext.Session.SetString("EmailOTP", email); // Dùng Session
            return Json(new { success = true });
        }

        // nhập OTP
        public IActionResult VerifyOTP()
        {
            var email = HttpContext.Session.GetString("EmailOTP");
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("ForgotPassword");
            }

            return View("ForgotPassword"); // Trả về đúng giao diện nhập OTP
        }

        [HttpPost]
        public async Task<IActionResult> VerifyOTP([FromForm] string otp)
        {
            Console.WriteLine($"[DEBUG] OTP nhận được từ request: '{otp}'");

            if (string.IsNullOrWhiteSpace(otp))
            {
                return Json(new { success = false, message = "OTP không được để trống!" });
            }

            otp = otp.Trim();

            // ✅ Lấy email từ Session thay vì TempData
            var email = HttpContext.Session.GetString("EmailOTP");
            if (string.IsNullOrEmpty(email))
            {
                return Json(new { success = false, message = "Phiên OTP đã hết hạn!" });
            }

            // ✅ Kiểm tra OTP trong DB
            bool isOtpValid = await _userAccountRepository.VerifyOTPAsync(email, otp);
            if (!isOtpValid)
            {
                return Json(new { success = false, message = "Mã OTP không hợp lệ hoặc hết hạn!" });
            }

            // ✅ Nếu thành công, chuyển sang bước 3: lưu lại email đã xác thực
            HttpContext.Session.SetString("EmailVerified", email);

            return Json(new { success = true });
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
                    return Json(new { success = false, message = "Phiên đặt lại mật khẩu đã hết hạn!" });
                }

                // Kiểm tra độ mạnh mật khẩu
                if (newPassword.Length < 8 || !newPassword.Any(char.IsUpper) ||
                    !newPassword.Any(char.IsDigit) || !newPassword.Any(ch => !char.IsLetterOrDigit(ch)))
                {
                    return Json(new { success = false, message = "Mật khẩu không đủ mạnh!" });
                }

                bool resetSuccess = await _userAccountRepository.ResetPasswordByOTPAsync(email, newPassword);
                if (!resetSuccess)
                {
                    return Json(new { success = false, message = "Đặt lại mật khẩu thất bại!" });
                }

                HttpContext.Session.Remove("EmailVerified");
                return Json(new { success = true, redirectUrl = Url.Action("Index", "Account") });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Lỗi khi đặt lại mật khẩu: {ex.Message}");
                return Json(new { success = false, message = "Đã xảy ra lỗi hệ thống." });
            }
        }
        //Để mã hóa mật khẩu cũ
        //[HttpGet]
        //[AllowAnonymous] // hoặc [Authorize(Roles = "Admin")] nếu muốn giới hạn
        //public async Task<IActionResult> EncryptOldPasswords()
        //{
        //    var users = await _userAccountRepository.GetAllUsersAsync();
        //    var hasher = new PasswordHasher<UserAccount>();

        //    foreach (var user in users)
        //    {
        //        // Nếu độ dài password nhỏ hơn 30 ký tự => có thể là chưa mã hóa
        //        if (!string.IsNullOrEmpty(user.Password) && user.Password.Length < 30)
        //        {
        //            user.Password = hasher.HashPassword(user, user.Password);
        //        }
        //    }

        //    await _userAccountRepository.SaveAllUsersAsync(users);
        //    return Content("✅ Mã hóa mật khẩu cũ thành công!");
        //}

        // Đăng nhập bằng Facebook
        public IActionResult LoginFacebook()
        {
            var redirectUrl = Url.Action("FacebookResponse", "Account");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, FacebookDefaults.AuthenticationScheme);
        }

        // Xử lý phản hồi từ Facebook
        public async Task<IActionResult> FacebookResponse()
        {
            var result = await HttpContext.AuthenticateAsync(FacebookDefaults.AuthenticationScheme);

            if (!result.Succeeded)
            {
                return BadRequest();
            }

            var externalInfo = result.Principal;
            var name = externalInfo.FindFirstValue(ClaimTypes.Name);
            var email = externalInfo.FindFirstValue(ClaimTypes.Email);

            // Kiểm tra nếu email đã tồn tại trong hệ thống
            var user = await _userAccountRepository.GetByEmailAsync(email);
            if (user == null)
            {
                var newUser = new UserAccount
                {
                    UserName = name,
                    Email = email,
                    Password = "",
                    Status = 1,
                    RoleID = 3
                };

                await _userAccountRepository.RegisterAsync(newUser, newUser.Password);
                user = newUser;
            }

            var roleName = "User";

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, roleName)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties { IsPersistent = true };

            HttpContext.Session.SetString("UserEmail", user.Email);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity), authProperties);

            Console.WriteLine($"[DEBUG] Đăng nhập qua Facebook thành công: {user.UserName} - Role: {roleName}");

            return roleName switch
            {
                "Admin" => RedirectToAction("Dashboard", "Admin"),
                "Lecturer" => RedirectToAction("Dashboard", "Lecturer"),
                _ => RedirectToAction("Dashboard", "User")
            };
        }
    }
}
