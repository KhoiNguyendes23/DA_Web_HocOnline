using Microsoft.AspNetCore.Mvc;
using Do_An_Web_Hoc.Models;
using Do_An_Web_Hoc.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
namespace Do_An_Web_Hoc.Controllers
{
    [Authorize(Roles = "Lecturer")]

    public class LecturerController : Controller
    {
        private readonly ILecturesRepository _lecturesRepository;
        private readonly ICoursesRepository _coursesRepository;
        private readonly IExamsRepository _examsRepository;
        private readonly IResultsRepository _resultsRepository;
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly ILogger<LecturerController> _logger;
        public LecturerController(
            ILecturesRepository lecturesRepository,
            ICoursesRepository coursesRepository,
            IExamsRepository examsRepository,
            IResultsRepository resultsRepository,
            IUserAccountRepository userAccountRepository,
             ILogger<LecturerController> logger)
        {
            _lecturesRepository = lecturesRepository;
            _coursesRepository = coursesRepository;
            _examsRepository = examsRepository;
            _resultsRepository = resultsRepository;
            _userAccountRepository = userAccountRepository;
            _logger = logger;
        }

        private void SetLecturerViewData()
        {
            var email = HttpContext.Session.GetString("UserEmail");

            if (!string.IsNullOrEmpty(email))
            {
                var user = _userAccountRepository.GetByEmailAsync(email).Result;
                if (user != null)
                {
                    ViewData["FullName"] = user.FullName;
                    ViewData["RoleName"] = "Giảng viên";
                    ViewData["ImagePath"] = string.IsNullOrEmpty(user.Image)
                        ? "~/images/default-avatar.png"
                        : "~/images/" + user.Image;
                    HttpContext.Session.SetInt32("LecturerID", user.UserID);
                }
            }
            else
            {
                ViewData["FullName"] = "Giảng viên";
                ViewData["RoleName"] = "Giảng viên";
                ViewData["ImagePath"] = "~/images/default-avatar.png";
            }
        }

        public IActionResult Dashboard()
        {
            SetLecturerViewData();
            ViewData["Title"] = "Giảng viên";
            return View();
        }

        public async Task<IActionResult> Profile ()
        {
            // Lấy email từ Claims
            var currentUserEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(currentUserEmail))
            {
                return RedirectToAction("Login", "Account");
            }
            var userAccount = await _userAccountRepository.GetByEmailAsync(currentUserEmail);

            if (userAccount == null)
            {
                return View("Error");
            }

            return View(userAccount);
        }



        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var currentUserEmail = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(currentUserEmail)) return RedirectToAction("Login", "Account");

            var userAccount = await _userAccountRepository.GetByEmailAsync(currentUserEmail);
            if (userAccount == null) return View("Error");

            return View(userAccount);
        }

        [HttpPost]
        public async Task<IActionResult> EditProfile(UserAccount updatedUser, IFormFile image)
        {
            var currentUserEmail = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(currentUserEmail)) return RedirectToAction("Login", "Account");

            var userAccount = await _userAccountRepository.GetByEmailAsync(currentUserEmail);
            if (userAccount == null) return View("Error");

            userAccount.FullName = updatedUser.FullName;
            userAccount.PhoneNumber = updatedUser.PhoneNumber;
            userAccount.Birthday = updatedUser.Birthday;
            userAccount.Address = updatedUser.Address;

            if (image != null && image.Length > 0)
            {
                var extension = Path.GetExtension(image.FileName).ToLower();
                var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                if (!allowed.Contains(extension))
                {
                    ModelState.AddModelError("", "Chỉ chấp nhận ảnh .jpg, .jpeg, .png, .gif");
                    return View(userAccount);
                }

                var oldPath = Path.Combine("wwwroot", userAccount.Image?.TrimStart('/') ?? "");
                if (!string.IsNullOrEmpty(userAccount.Image) && System.IO.File.Exists(oldPath))
                {
                    System.IO.File.Delete(oldPath);
                }

                var fileName = Guid.NewGuid().ToString() + extension;
                var path = Path.Combine("wwwroot/images", fileName);
                using var stream = new FileStream(path, FileMode.Create);
                await image.CopyToAsync(stream);

                userAccount.Image = "/images/" + fileName;
            }

            if (string.IsNullOrEmpty(userAccount.Image))
                userAccount.Image = "/images/default-avatar.png";

            await _userAccountRepository.UpdateAsync(userAccount);
            return RedirectToAction("Profile");
        }

        public async Task<IActionResult> ListCourse()
        {
            SetLecturerViewData();
            var courses = await _coursesRepository.GetAllCoursesAsync();
            return View(courses);
        }

        public IActionResult AddCourse()
        {
            SetLecturerViewData();
            return View();
        }

        public async Task<IActionResult> EditCourse(int id)
        {
            SetLecturerViewData();
            var course = await _coursesRepository.GetCourseByIdAsync(id);
            return View(course);
        }

        public async Task<IActionResult> DeleteCourse(int id)
        {
            SetLecturerViewData();
            var course = await _coursesRepository.GetCourseByIdAsync(id);
            return View(course);
        }

        public async Task<IActionResult> ListExam()
        {
            SetLecturerViewData();
            var exams = await _examsRepository.GetAllExamsAsync();
            return View(exams);
        }

        public IActionResult AddExam()
        {
            SetLecturerViewData();
            return View();
        }

        public async Task<IActionResult> EditExam(int id)
        {
            SetLecturerViewData();
            var exam = await _examsRepository.GetExamByIdAsync(id);
            return View(exam);
        }

        public async Task<IActionResult> DeleteExam(int id)
        {
            SetLecturerViewData();
            var exam = await _examsRepository.GetExamByIdAsync(id);
            return View(exam);
        }

        public async Task<IActionResult> ViewExam(int id)
        {
            SetLecturerViewData();
            var exam = await _examsRepository.GetExamByIdAsync(id);
            return View(exam);
        }

        public async Task<IActionResult> ResultExam()
        {
            SetLecturerViewData();
            var results = await _examsRepository.GetAllExamsAsync();
            return View(results);
        }

        public async Task<IActionResult> ListStudent()
        {
            SetLecturerViewData();
            var students = await _userAccountRepository.GetUsersByRoleAsync(3);
            return View(students);
        }

        public async Task<IActionResult> ListLecture()
        {
            SetLecturerViewData();
            var lectures = await _lecturesRepository.GetAllLecturesAsync();
            return View(lectures);
        }

        public IActionResult AddLecture()
        {
            SetLecturerViewData();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddLecture(Lectures lecture)
        {
            if (ModelState.IsValid)
            {
                await _lecturesRepository.AddLectureAsync(lecture);
                return RedirectToAction("ListLecture");
            }
            return View(lecture);
        }

        public async Task<IActionResult> EditLecture(int id)
        {
            SetLecturerViewData();
            var lecture = await _lecturesRepository.GetLectureByIdAsync(id);
            return View(lecture);
        }

        [HttpPost]
        public async Task<IActionResult> EditLecture(Lectures lecture)
        {
            if (ModelState.IsValid)
            {
                await _lecturesRepository.UpdateLectureAsync(lecture);
                return RedirectToAction("ListLecture");
            }
            return View(lecture);
        }

        public async Task<IActionResult> DeleteLecture(int id)
        {
            SetLecturerViewData();
            var lecture = await _lecturesRepository.GetLectureByIdAsync(id);
            return View(lecture);
        }

        [HttpPost, ActionName("DeleteLecture")]
        public async Task<IActionResult> ConfirmDeleteLecture(int id)
        {
            await _lecturesRepository.DeleteLectureAsync(id);
            return RedirectToAction("ListLecture");
        }
    }
}
