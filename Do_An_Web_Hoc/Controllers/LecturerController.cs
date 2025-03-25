using Microsoft.AspNetCore.Mvc;
using Do_An_Web_Hoc.Models;
using Do_An_Web_Hoc.Repositories;
using Microsoft.AspNetCore.Http;
using Do_An_Web_Hoc.Repositories.Interfaces;
using System.Threading.Tasks;

namespace Do_An_Web_Hoc.Controllers
{
    public class LecturerController : Controller
    {
        private readonly ILecturesRepository _lecturesRepository;
        private readonly ICoursesRepository _coursesRepository;
        private readonly IExamsRepository _examsRepository;
        private readonly IResultsRepository _resultsRepository;
        private readonly IUserAccountRepository _userAccountRepository;

        public LecturerController(
            ILecturesRepository lecturesRepository,
            ICoursesRepository coursesRepository,
            IExamsRepository examsRepository,
            IResultsRepository resultsRepository,
            IUserAccountRepository userAccountRepository)
        {
            _lecturesRepository = lecturesRepository;
            _coursesRepository = coursesRepository;
            _examsRepository = examsRepository;
            _resultsRepository = resultsRepository;
            _userAccountRepository = userAccountRepository;
        }

        public IActionResult Dashboard()
        {
            ViewData["Title"] = "Giảng viên";
            ViewData["FullName"] = HttpContext.Session.GetString("FullName") ?? "Giảng viên";
            ViewData["RoleName"] = "Giảng viên";
            return View();
        }

        public async Task<IActionResult> ListCourse()
        {
            var courses = await _coursesRepository.GetAllCoursesAsync();
            return View(courses);
        }

        public IActionResult AddCourse() => View();

        public async Task<IActionResult> EditCourse(int id)
        {
            var course = await _coursesRepository.GetCourseByIdAsync(id);
            return View(course);
        }

        public async Task<IActionResult> DeleteCourse(int id)
        {
            var course = await _coursesRepository.GetCourseByIdAsync(id);
            return View(course);
        }

        public async Task<IActionResult> ListExam()
        {
            var exams = await _examsRepository.GetAllExamsAsync();
            return View(exams);
        }

        public IActionResult AddExam() => View();

        public async Task<IActionResult> EditExam(int id)
        {
            var exam = await _examsRepository.GetExamByIdAsync(id);
            return View(exam);
        }

        public async Task<IActionResult> DeleteExam(int id)
        {
            var exam = await _examsRepository.GetExamByIdAsync(id);
            return View(exam);
        }

        public async Task<IActionResult> ViewExam(int id)
        {
            var exam = await _examsRepository.GetExamByIdAsync(id);
            return View(exam);
        }

        public async Task<IActionResult> ListStudent()
        {
            var students = await _userAccountRepository.GetUsersByRoleAsync(3);
            return View(students);
        }

        public async Task<IActionResult> ResultExam()
        {
            var results = await _examsRepository.GetAllExamsAsync();
            return View(results);
        }

        public async Task<IActionResult> ListLecture()
        {
            var lectures = await _lecturesRepository.GetAllLecturesAsync();
            return View(lectures);
        }

        public IActionResult AddLecture()
        {
            return View();
        }

        public IActionResult Profile()
        {
            var lecturerId = HttpContext.Session.GetInt32("LecturerID");

            if (lecturerId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var lecturer = _lecturesRepository.GetLectureByIdAsync((int)lecturerId);

            if (lecturer == null)
            {
                return NotFound();
            }

            return View(lecturer);
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var lecturerId = HttpContext.Session.GetInt32("LecturerID");
            if (lecturerId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var lecturer = await _lecturesRepository.GetLectureByIdAsync((int)lecturerId);
            return View(lecturer);
        }

        [HttpPost]
        public async Task<IActionResult> EditProfile(Lectures lecturer)
        {
            if (ModelState.IsValid)
            {
                await _lecturesRepository.UpdateLecturerProfileAsync(lecturer);
                return RedirectToAction("Profile");
            }

            return View(lecturer);
        }
    }
}
