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

        //public async Task<IActionResult> ViewExam(int id)
        //{
        //    SetLecturerViewData();

        //    // Lấy bài kiểm tra
        //    var exam = await _examsRepository.GetExamByIdAsync(id);
        //    if (exam == null) return NotFound();

        //    // Lấy quiz của bài kiểm tra
        //    var quiz = await _quizzesRepository.GetQuizByExamIdAsync(exam.ExamID);
        //    if (quiz == null) return View(exam); // Nếu chưa có quiz thì chỉ hiển thị thông tin exam

        //    // Lấy danh sách câu hỏi
        //    var questions = await _questionsRepository.GetQuestionsByQuizIdAsync(quiz.QuizID);

        //    // Gắn vào model
        //    var questionList = new List<QuestionTempModel>();
        //    foreach (var q in questions)
        //    {
        //        var questionModel = new QuestionTempModel
        //        {
        //            QuestionText = q.QuestionText,
        //            QuestionType = q.QuestionType
        //        };

        //        if (q.QuestionType == "Trắc nghiệm")
        //        {
        //            var answers = await _answersRepository.GetAnswersByQuestionIdAsync(q.QuestionID);

        //            questionModel.OptionA = answers.FirstOrDefault(a => a.AnswerText != null && a.AnswerText == a.AnswerText && a.IsCorrect == (a.AnswerText == "A"))?.AnswerText;
        //            questionModel.OptionB = answers.FirstOrDefault(a => a.AnswerText != null && a.AnswerText == a.AnswerText && a.IsCorrect == (a.AnswerText == "B"))?.AnswerText;
        //            questionModel.OptionC = answers.FirstOrDefault(a => a.AnswerText != null && a.AnswerText == a.AnswerText && a.IsCorrect == (a.AnswerText == "C"))?.AnswerText;
        //            questionModel.OptionD = answers.FirstOrDefault(a => a.AnswerText != null && a.AnswerText == a.AnswerText && a.IsCorrect == (a.AnswerText == "D"))?.AnswerText;
        //            if (answers.Count() >= 4)
        //            {
        //                var list = answers.ToList();
        //                questionModel.OptionA = list[0].AnswerText;
        //                questionModel.OptionB = list[1].AnswerText;
        //                questionModel.OptionC = list[2].AnswerText;
        //                questionModel.OptionD = list[3].AnswerText;

        //                questionModel.CorrectAnswer = list.FirstOrDefault(a => a.IsCorrect == true)?.AnswerText;
        //            }


        //        }

        //        questionList.Add(questionModel);
        //    }

        //    exam.Questions = questionList;
        //    exam.Description = quiz.Description;
        //    exam.TotalMarks = quiz.TotalMarks ?? 0;

        //    return View(exam);
        //}

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


        // Hiển thị form thêm bài kiểm tra
        [HttpGet]
        public async Task<IActionResult> AddExam()
        {
            SetLecturerViewData();

            var courses = await _coursesRepository.GetAllCoursesAsync();
            ViewBag.Courses = new SelectList(courses, "CourseID", "CourseName");

            return View();
        }


        // Xử lý khi submit form
//      [HttpPost]
//[ValidateAntiForgeryToken]
//public async Task<IActionResult> AddExam(Exams exam)
//{
//    if (ModelState.IsValid)
//    {
//        exam.CreatedAt = DateTime.Now;
//        exam.Status = 1;

//        await _examsRepository.AddExamAsync(exam); // ✅ Lưu bài kiểm tra chính

//        // ✅ Bước 1: Tạo quiz cho exam
//        var quiz = new Quizzes
//        {
//            QuizName = exam.QuizName ?? "Quiz tự động",
//            Description = exam.Description,
//            TotalMarks = exam.TotalMarks,
//            ExamID = exam.ExamID
//        };
//        await _quizzesRepository.AddQuizAsync(quiz);

//        // ✅ Bước 2: Lưu câu hỏi và đáp án
//        if (exam.Questions != null)
//        {
//            foreach (var q in exam.Questions)
//            {
//                var question = new Questions
//                {
//                    QuizID = quiz.QuizID,
//                    QuestionText = q.QuestionText,
//                    QuestionType = q.QuestionType
//                };

//                int questionId = await _questionsRepository.AddQuestionAsync(question);

//                // Nếu là trắc nghiệm thì lưu thêm đáp án
//                if (q.QuestionType == "Trắc nghiệm")
//                {
//                    var answers = new List<Answers>
//                    {
//                        new() { QuestionID = questionId, AnswerText = q.OptionA, IsCorrect = q.CorrectAnswer == "A" },
//                        new() { QuestionID = questionId, AnswerText = q.OptionB, IsCorrect = q.CorrectAnswer == "B" },
//                        new() { QuestionID = questionId, AnswerText = q.OptionC, IsCorrect = q.CorrectAnswer == "C" },
//                        new() { QuestionID = questionId, AnswerText = q.OptionD, IsCorrect = q.CorrectAnswer == "D" },
//                    };

//                    foreach (var a in answers)
//                        await _answersRepository.AddAnswerAsync(a);
//                }
//            }
//        }

//        return RedirectToAction("ListExam");
//    }

    // Nếu lỗi thì load lại view
//    SetLecturerViewData();
//    var courses = await _coursesRepository.GetAllCoursesAsync();
//    ViewBag.Courses = new SelectList(courses, "CourseID", "CourseName");

//    return View(exam);
//}







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
