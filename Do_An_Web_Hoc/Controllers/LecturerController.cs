using Microsoft.AspNetCore.Mvc;
using Do_An_Web_Hoc.Models;
using Do_An_Web_Hoc.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
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

        private void SetLecturerViewData()
        {
            ViewData["FullName"] = HttpContext.Session.GetString("FullName") ?? "Giảng viên";
            ViewData["RoleName"] = "Giảng viên";
        }

        public IActionResult Dashboard()
        {
            SetLecturerViewData();
            ViewData["Title"] = "Giảng viên";
            return View();
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

        public async Task<IActionResult> ListStudent()
        {
            SetLecturerViewData();
            var students = await _userAccountRepository.GetUsersByRoleAsync(3); // 3: Học viên
            return View(students);
        }

        public async Task<IActionResult> ResultExam()
        {
            SetLecturerViewData();
            var results = await _examsRepository.GetAllExamsAsync(); // Hoặc gọi từ ResultRepo nếu cần
            return View(results);
        }

        public async Task<IActionResult> Profile()
        {
            SetLecturerViewData();
            var lecturerId = HttpContext.Session.GetInt32("LecturerID");

            if (lecturerId == null)
                return RedirectToAction("Login", "Account");

            var lecturer = await _lecturesRepository.GetLectureByIdAsync(lecturerId.Value);

            if (lecturer == null)
                return NotFound();

            return View(lecturer);
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            SetLecturerViewData();
            var lecturerId = HttpContext.Session.GetInt32("LecturerID");
            if (lecturerId == null)
                return RedirectToAction("Login", "Account");

            var lecturer = await _lecturesRepository.GetLectureByIdAsync(lecturerId.Value);
            return View(lecturer);
        }


        [HttpPost]
        public async Task<IActionResult> EditProfile(Lectures lecturer)
        {
            SetLecturerViewData();
            if (ModelState.IsValid)
            {
                await _lecturesRepository.UpdateLecturerProfileAsync(lecturer);
                TempData["Success"] = "Cập nhật thành công!";
                return RedirectToAction("Profile");
            }

            return View(lecturer);
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
