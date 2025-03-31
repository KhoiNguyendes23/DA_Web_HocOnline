using Microsoft.AspNetCore.Mvc;
using Do_An_Web_Hoc.Models;
using Do_An_Web_Hoc.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

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
        private readonly IQuestionsRepository _questionsRepository;
        private readonly IQuizzesRepository _quizzesRepository;
        private readonly ApplicationDbContext _context;
       

        public LecturerController(
            ILecturesRepository lecturesRepository,
            ICoursesRepository coursesRepository,
            IExamsRepository examsRepository,
            IResultsRepository resultsRepository,
            IUserAccountRepository userAccountRepository,
            ILogger<LecturerController> logger,
            IQuestionsRepository questionsRepository,
            IQuizzesRepository quizzesRepository,
          
            ApplicationDbContext context)

        {
            _lecturesRepository = lecturesRepository;
            _coursesRepository = coursesRepository;
            _examsRepository = examsRepository;
            _resultsRepository = resultsRepository;
            _userAccountRepository = userAccountRepository;
            _logger = logger;
            _questionsRepository = questionsRepository;
            _quizzesRepository = quizzesRepository;
           
            _context = context;
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

        public async Task<IActionResult> Profile()
        {
            var currentUserEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(currentUserEmail)) return RedirectToAction("Login", "Account");

            var userAccount = await _userAccountRepository.GetByEmailAsync(currentUserEmail);
            if (userAccount == null) return View("Error");

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

        public async Task<IActionResult> ListExam()
        {
            SetLecturerViewData();
            var exams = await _examsRepository.GetAllExamsAsync();
            return View(exams);
        }

        //public IActionResult AddExam()
        //{
        //    SetLecturerViewData();
        //    return View();
        //}




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

        public IActionResult ViewExam(int id)
        {
            var exam = _context.Exams.FirstOrDefault(e => e.ExamID == id);
            if (exam == null) return NotFound();

            var quizzes = _context.Quizzes.Where(q => q.ExamID == id).ToList();
            var quizIds = quizzes.Select(q => q.QuizID).ToList();
            var questions = _context.Questions.Where(q => quizIds.Contains(q.QuizID ?? 0)).ToList();
            var questionIds = questions.Select(q => q.QuestionID).ToList();
            var answers = _context.Answers.Where(a => questionIds.Contains(a.QuestionID ?? 0)).ToList();

            ViewBag.Quizzes = quizzes;
            ViewBag.Questions = questions;
            ViewBag.Answers = answers;

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

        [HttpGet]
        public IActionResult AddExam()
        {
            SetLecturerViewData();

            var courses = _context.Courses
                .Select(c => new SelectListItem
                {
                    Value = c.CourseID.ToString(),
                    Text = c.CourseName
                }).ToList();

            ViewBag.CourseList = courses;

            return View();
        }


        // Hiển thị form thêm bài kiểm tra
        [HttpPost]
        public IActionResult AddExam(Exams exam)
        {
            if (!ModelState.IsValid)
                return View(exam);

            // 1. Thêm bài kiểm tra
            exam.CreatedAt = DateTime.Now;
            exam.Status = 1;
            _context.Exams.Add(exam);
            _context.SaveChanges();

            // 2. Lặp qua các quiz được gửi lên từ form
            int quizIndex = 0;
            while (true)
            {
                var quizName = Request.Form[$"Quizzes[{quizIndex}].QuizName"];
                if (string.IsNullOrWhiteSpace(quizName)) break;

                var quiz = new Quizzes
                {
                    QuizName = quizName,
                    Description = Request.Form[$"Quizzes[{quizIndex}].Description"],
                    ExamID = exam.ExamID,
                    TotalMarks = 50
                };
                _context.Quizzes.Add(quiz);
                _context.SaveChanges();

                // 3. Lặp các câu hỏi trong mỗi quiz
                int questionIndex = 0;
                while (true)
                {
                    var questionText = Request.Form[$"Quizzes[{quizIndex}].Questions[{questionIndex}].QuestionText"];
                    if (string.IsNullOrWhiteSpace(questionText)) break;

                    var questionType = Request.Form[$"Quizzes[{quizIndex}].Questions[{questionIndex}].QuestionType"];
                    var question = new Questions
                    {
                        QuizID = quiz.QuizID,
                        QuestionText = questionText,
                        QuestionType = questionType == "Trắc nghiệm" ? "MCQ" : "Essay"
                    };
                    _context.Questions.Add(question);
                    _context.SaveChanges();

                    // Nếu là trắc nghiệm → thêm 4 đáp án
                    if (questionType == "Trắc nghiệm")
                    {
                        string[] options = { "A", "B", "C", "D" };
                        var correctAnswer = Request.Form[$"Quizzes[{quizIndex}].Questions[{questionIndex}].CorrectAnswer"];

                        foreach (var opt in options)
                        {
                            var text = Request.Form[$"Quizzes[{quizIndex}].Questions[{questionIndex}].Option{opt}"];
                            var answer = new Answers
                            {
                                QuestionID = question.QuestionID,
                                AnswerText = text,
                                IsCorrect = (opt == correctAnswer)
                            };
                            _context.Answers.Add(answer);
                        }
                        _context.SaveChanges();
                    }

                    questionIndex++;
                }

                quizIndex++;
            }

            return RedirectToAction("ListExam");
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
