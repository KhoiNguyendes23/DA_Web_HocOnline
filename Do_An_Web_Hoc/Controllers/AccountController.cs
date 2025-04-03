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
            // Nếu người dùng bị cấm (Status == 3), không cho đăng nhập
            if (user.Status == 3)
            {
                ViewBag.Error = "Tài khoản của bạn đã bị cấm.";
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
        new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
        new Claim(ClaimTypes.Name, user.UserName),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Role, roleName) //Đây là Role mà `[Authorize]` sử dụng
        //new Claim("FullName", user.FullName)
    };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties { IsPersistent = true };

            // Lưu email vào Session sau khi đăng nhập thành công
            HttpContext.Session.SetString("UserEmail", user.Email);  // Lưu email vào session
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

            // Mã hóa mật khẩu trước khi lưu
            //var hasher = new PasswordHasher<UserAccount>();
            //model.Password = hasher.HashPassword(model, model.Password);

            var newUser = await _userAccountRepository.RegisterAsync(model, model.Password);

            if (newUser == null)
            {
                ViewBag.Error = "Đăng ký thất bại!";
                return View(model);
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
                // Mã hóa mật khẩu trước khi lưu
                var hasher = new PasswordHasher<UserAccount>();
                var hashedPassword = hasher.HashPassword(new UserAccount(), newPassword);

                bool resetSuccess = await _userAccountRepository.ResetPasswordByOTPAsync(email, hashedPassword);
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


        // đăng nhập bằng facebook
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

            // Lấy thông tin người dùng từ Facebook
            var name = externalInfo.FindFirstValue(ClaimTypes.Name);
            var email = externalInfo.FindFirstValue(ClaimTypes.Email);

            // Kiểm tra nếu email đã tồn tại trong hệ thống
            var user = await _userAccountRepository.GetByEmailAsync(email);
            if (user == null)
            {
                // Nếu không có user, có thể tạo mới tài khoản từ thông tin Facebook
                var newUser = new UserAccount
                {
                    UserName = name,
                    Email = email,
                    Password = "", // Có thể để trống vì không cần mật khẩu khi đăng nhập qua Facebook
                    Status = 1, // Trạng thái hoạt động, có thể thay đổi tùy theo yêu cầu
                    RoleID = 3  // Đặt role tùy theo yêu cầu
                };

                await _userAccountRepository.RegisterAsync(newUser, newUser.Password);
                user = newUser;
            }

            // Gán Role vào Claims để `[Authorize(Roles="User")]` nhận diện được
            var roleName = "User"; // Mặc định là User

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.UserName),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Role, roleName)
    };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties { IsPersistent = true };

            // Lưu email vào Session sau khi đăng nhập thành công
            HttpContext.Session.SetString("UserEmail", user.Email);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity), authProperties);

            Console.WriteLine($"[DEBUG] Đăng nhập qua Facebook thành công: {user.UserName} - Role: {roleName}");

            // Chuyển hướng theo Role
            return roleName switch
            {
                "Admin" => RedirectToAction("Dashboard", "Admin"),
                "Lecturer" => RedirectToAction("Dashboard", "Lecturer"),
                _ => RedirectToAction("Dashboard", "User")
            };
        }
    }
}

