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
using Do_An_Web_Hoc.ViewModels;

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


        /// <summary>
        /// Lấy thông tin giảng viên từ session/email và truyền vào ViewData để dùng trong layout
        /// </summary>
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

                    // Xử lý ảnh: nếu null → dùng ảnh mặc định
                    if (string.IsNullOrEmpty(user.Image))
                    {
                        ViewData["ImagePath"] = "/images/Avatar_images/default-avatar.png";
                    }
                    else
                    {
                        // Nếu Image đã có sẵn dấu "/" ở đầu, không cần thêm nữa
                        ViewData["ImagePath"] = user.Image.StartsWith("/")
                            ? user.Image
                            : "/images/Avatar_images/" + user.Image;
                    }

                    HttpContext.Session.SetInt32("LecturerID", user.UserID);
                }
            }
            else
            {
                ViewData["FullName"] = "Giảng viên";
                ViewData["RoleName"] = "Giảng viên";
                ViewData["ImagePath"] = "/images/Avatar_images/default-avatar.png";
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

            // Gọi hàm để gán ViewData cho layout
            SetLecturerViewData();

            return View(userAccount);
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var currentUserEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(currentUserEmail))
            {
                _logger.LogWarning("Giảng viên chưa đăng nhập.");
                return RedirectToAction("Login", "Account");
            }

            var userAccount = await _userAccountRepository.GetByEmailAsync(currentUserEmail);
            if (userAccount == null)
            {
                _logger.LogWarning($"Không tìm thấy người dùng với email: {currentUserEmail}");
                return View("Error");
            }
            // Gọi hàm để gán ViewData cho layout
            SetLecturerViewData();

            return View(userAccount);
        }


        [HttpPost]
        public async Task<IActionResult> EditProfile(UserAccount updatedUser, IFormFile image)
        {
            var currentUserEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(currentUserEmail))
            {
                _logger.LogWarning("Giảng viên chưa đăng nhập.");
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var userAccount = await _userAccountRepository.GetByEmailAsync(currentUserEmail);
                if (userAccount == null)
                {
                    _logger.LogWarning($"Không tìm thấy người dùng với email: {currentUserEmail}");
                    return View("Error");
                }

                // Cập nhật thông tin cơ bản
                userAccount.FullName = updatedUser.FullName;
                userAccount.PhoneNumber = updatedUser.PhoneNumber;
                userAccount.Birthday = updatedUser.Birthday;
                userAccount.Address = updatedUser.Address;

                // Xử lý ảnh đại diện
                if (image != null && image.Length > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var ext = Path.GetExtension(image.FileName).ToLower();

                    if (!allowedExtensions.Contains(ext))
                    {
                        ModelState.AddModelError("", "Chỉ cho phép ảnh có định dạng .jpg, .jpeg, .png, .gif.");
                        return View(updatedUser);
                    }

                    // Xóa ảnh cũ nếu không phải mặc định
                    if (!string.IsNullOrEmpty(userAccount.Image) && userAccount.Image != "/images/default-avatar.png")
                    {
                        var oldPath = Path.Combine("wwwroot", userAccount.Image.TrimStart('/'));
                        if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                    }

                    // Lưu ảnh mới
                    var fileName = Guid.NewGuid().ToString() + ext;
                    var savePath = Path.Combine("wwwroot/images/Avatar_images", fileName);
                    using (var stream = new FileStream(savePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }

                    userAccount.Image = "/images/Avatar_images/" + fileName;
                }

                // Nếu chưa có ảnh thì đặt mặc định
                if (string.IsNullOrEmpty(userAccount.Image))
                {
                    userAccount.Image = "/images/default-avatar.png";
                }

                // Lưu
                await _userAccountRepository.UpdateAsync(userAccount);
                _logger.LogInformation($"Giảng viên {currentUserEmail} đã cập nhật hồ sơ thành công.");

                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi khi cập nhật hồ sơ giảng viên {currentUserEmail}: {ex.Message}");
                return StatusCode(500, "Có lỗi xảy ra khi cập nhật hồ sơ.");
            }
        }


        public async Task<IActionResult> ListExam(string searchTerm, string statusFilter)
        {
            SetLecturerViewData();
            var exams = await _examsRepository.GetAllExamsAsync();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                exams = exams.Where(e => e.ExamName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
                ViewBag.SearchTerm = searchTerm;
            }

            if (!string.IsNullOrEmpty(statusFilter) && int.TryParse(statusFilter, out int status))
            {
                exams = exams.Where(e => e.Status == status);
                ViewBag.StatusFilter = statusFilter;
            }

            return View(exams);
        }

        public async Task<IActionResult> RestoreExam(int id)
        {
            await _examsRepository.RestoreExamAsync(id);
            return RedirectToAction("ListExam");
        }


        [HttpGet]
        public IActionResult AddExam()
        {
            SetLecturerViewData();

            ViewBag.CourseList = _context.Courses
                .Select(c => new SelectListItem
                {
                    Value = c.CourseID.ToString(),
                    Text = c.CourseName
                }).ToList();

            return View(new ExamEditViewModel());
        }


        [HttpPost]
        public async Task<IActionResult> AddExam(ExamEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                SetLecturerViewData();
                ViewBag.CourseList = _context.Courses.Select(c => new SelectListItem
                {
                    Value = c.CourseID.ToString(),
                    Text = c.CourseName
                }).ToList();
                return View(model);
            }

            var exam = new Exams
            {
                ExamName = model.ExamName,
                Description = model.Description,
                TotalMarks = model.TotalMarks,
                CourseID = model.CourseID,
                Duration = model.Duration,
                StartTime = model.StartTime,
                EndTime = model.EndTime,
                CreatedAt = DateTime.Now,
                Status = 1
            };
            _context.Exams.Add(exam);
            await _context.SaveChangesAsync();

            foreach (var quizVM in model.Quizzes)
            {
                var quiz = new Quizzes
                {
                    QuizName = quizVM.QuizName,
                    Description = quizVM.Description,
                    ExamID = exam.ExamID,
                    TotalMarks = quizVM.TotalMarks
                };
                _context.Quizzes.Add(quiz);
                await _context.SaveChangesAsync();

                foreach (var q in quizVM.Questions)
                {
                    var question = new Questions
                    {
                        QuizID = quiz.QuizID,
                        QuestionText = q.QuestionText,
                        QuestionType = q.QuestionType
                    };

                    if (q.QuestionImage != null && q.QuestionImage.Length > 0)
                    {
                        string ext = Path.GetExtension(q.QuestionImage.FileName).ToLower();
                        string[] allowed = { ".jpg", ".png", ".jpeg", ".gif" };
                        if (allowed.Contains(ext))
                        {
                            var fileName = Guid.NewGuid() + ext;
                            var path = Path.Combine("wwwroot/images/questions", fileName);

                            using (var stream = new FileStream(path, FileMode.Create))
                            {
                                await q.QuestionImage.CopyToAsync(stream);
                            }
                            question.ImagePath = "/images/questions/" + fileName;
                        }
                    }

                    _context.Questions.Add(question);
                    await _context.SaveChangesAsync();

                    if (q.QuestionType == "MCQ")
                    {
                        string[] options = { q.OptionA, q.OptionB, q.OptionC, q.OptionD };
                        string[] labels = { "A", "B", "C", "D" };

                        for (int i = 0; i < 4; i++)
                        {
                            _context.Answers.Add(new Answers
                            {
                                QuestionID = question.QuestionID,
                                AnswerText = options[i],
                                IsCorrect = (q.CorrectAnswer == labels[i])
                            });
                        }
                        await _context.SaveChangesAsync();
                    }
                }
            }

            TempData["SuccessMessage"] = "Thêm bài kiểm tra thành công!";
            return RedirectToAction("ListExam");
        }




        public async Task<IActionResult> EditExam(int id)
        {
            SetLecturerViewData();

            ViewBag.CourseList = _context.Courses.Select(c => new SelectListItem
            {
                Value = c.CourseID.ToString(),
                Text = c.CourseName
            }).ToList();

            var exam = await _context.Exams.FindAsync(id);
            if (exam == null) return NotFound();

            var quizzes = await _context.Quizzes.Where(q => q.ExamID == id).ToListAsync();

            var model = new ExamEditViewModel
            {
                ExamID = exam.ExamID,
                ExamName = exam.ExamName,
                Description = exam.Description,
                TotalMarks = exam.TotalMarks,
                CourseID = exam.CourseID,
                Duration = exam.Duration,
                StartTime = exam.StartTime,
                EndTime = exam.EndTime,
                Quizzes = new List<QuizViewModel>()
            };

            foreach (var quiz in quizzes)
            {
                var questions = await _context.Questions
                    .Where(q => q.QuizID == quiz.QuizID)
                    .ToListAsync();

                var quizVM = new QuizViewModel
                {
                    QuizName = quiz.QuizName,
                    Description = quiz.Description,
                    TotalMarks = quiz.TotalMarks,
                    Questions = new List<QuestionViewModel>()
                };

                foreach (var question in questions)
                {
                    var answers = await _context.Answers
                        .Where(a => a.QuestionID == question.QuestionID)
                        .ToListAsync();

                    string correctOption = "";
                    string? optionA = "", optionB = "", optionC = "", optionD = "";
                    string? correctAnswer = "";

                    if (question.QuestionType == "MCQ" && answers.Count >= 4)
                    {
                        optionA = answers.ElementAtOrDefault(0)?.AnswerText ?? "";
                        optionB = answers.ElementAtOrDefault(1)?.AnswerText ?? "";
                        optionC = answers.ElementAtOrDefault(2)?.AnswerText ?? "";
                        optionD = answers.ElementAtOrDefault(3)?.AnswerText ?? "";

                        var correct = answers.FirstOrDefault(a => a.IsCorrect == true);
                        if (correct != null)
                        {
                            if (correct.AnswerText == optionA) correctOption = "A";
                            else if (correct.AnswerText == optionB) correctOption = "B";
                            else if (correct.AnswerText == optionC) correctOption = "C";
                            else if (correct.AnswerText == optionD) correctOption = "D";
                            correctAnswer = correctOption;
                        }
                    }

                    quizVM.Questions.Add(new QuestionViewModel
                    {
                        QuestionText = question.QuestionText,
                        OptionA = optionA,
                        OptionB = optionB,
                        OptionC = optionC,
                        OptionD = optionD,
                        CorrectAnswer = correctAnswer,
                        QuestionType = question.QuestionType
                    });
                }

                model.Quizzes.Add(quizVM);
            }

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> EditExam(ExamEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                SetLecturerViewData();
                ViewBag.CourseList = _context.Courses.Select(c => new SelectListItem
                {
                    Value = c.CourseID.ToString(),
                    Text = c.CourseName
                }).ToList();
                return View(model);
            }

            var exam = await _context.Exams.FindAsync(model.ExamID);
            if (exam == null) return NotFound();

            exam.ExamName = model.ExamName;
            exam.Description = model.Description;
            exam.TotalMarks = model.TotalMarks;
            exam.CourseID = model.CourseID;
            exam.Duration = model.Duration;
            exam.StartTime = model.StartTime;
            exam.EndTime = model.EndTime;
            exam.Status = 1;

            await _context.SaveChangesAsync();

            var oldQuizzes = _context.Quizzes.Where(q => q.ExamID == exam.ExamID).ToList();
            foreach (var quiz in oldQuizzes)
            {
                var questions = _context.Questions.Where(q => q.QuizID == quiz.QuizID).ToList();
                foreach (var question in questions)
                {
                    var answers = _context.Answers.Where(a => a.QuestionID == question.QuestionID);
                    _context.Answers.RemoveRange(answers);
                }
                _context.Questions.RemoveRange(questions);
            }
            _context.Quizzes.RemoveRange(oldQuizzes);
            await _context.SaveChangesAsync();

            foreach (var quizVM in model.Quizzes)
            {
                var quiz = new Quizzes
                {
                    ExamID = exam.ExamID,
                    QuizName = quizVM.QuizName,
                    Description = quizVM.Description,
                    TotalMarks = quizVM.TotalMarks
                };

                _context.Quizzes.Add(quiz);
                await _context.SaveChangesAsync();

                foreach (var q in quizVM.Questions)
                {
                    var question = new Questions
                    {
                        QuizID = quiz.QuizID,
                        QuestionText = q.QuestionText,
                        QuestionType = q.QuestionType
                    };

                    if (q.QuestionImage != null && q.QuestionImage.Length > 0)
                    {
                        string ext = Path.GetExtension(q.QuestionImage.FileName).ToLower();
                        string[] allowed = { ".jpg", ".png", ".jpeg", ".gif" };
                        if (allowed.Contains(ext))
                        {
                            var fileName = Guid.NewGuid() + ext;
                            var path = Path.Combine("wwwroot/images/questions", fileName);

                            using (var stream = new FileStream(path, FileMode.Create))
                            {
                                await q.QuestionImage.CopyToAsync(stream);
                            }
                            question.ImagePath = "/images/questions/" + fileName;
                        }
                    }

                    _context.Questions.Add(question);
                    await _context.SaveChangesAsync();

                    if (q.QuestionType == "MCQ")
                    {
                        string[] options = { q.OptionA, q.OptionB, q.OptionC, q.OptionD };
                        string[] labels = { "A", "B", "C", "D" };

                        for (int i = 0; i < 4; i++)
                        {
                            _context.Answers.Add(new Answers
                            {
                                QuestionID = question.QuestionID,
                                AnswerText = options[i],
                                IsCorrect = q.CorrectAnswer == labels[i]
                            });
                        }
                        await _context.SaveChangesAsync();
                    }
                }
            }

            TempData["SuccessMessage"] = "Đã cập nhật bài kiểm tra thành công!";
            return RedirectToAction("ListExam");
        }



        public async Task<IActionResult> DeleteExam(int id)
        {
            SetLecturerViewData();

            var exam = await _examsRepository.GetExamByIdAsync(id);

            if (exam == null)
            {
                return NotFound();
            }

            // Lấy thêm danh sách Quiz và Question để hiển thị thông tin chi tiết nếu cần
            var quizzes = await _context.Quizzes.Where(q => q.ExamID == id).ToListAsync();
            var quizIds = quizzes.Select(q => q.QuizID).ToList();
            var questions = await _context.Questions.Where(q => quizIds.Contains(q.QuizID ?? 0)).ToListAsync();

            ViewBag.Quizzes = quizzes;
            ViewBag.Questions = questions;

            return View(exam);
        }
        [HttpPost, ActionName("DeleteExam")]
        public async Task<IActionResult> ConfirmDeleteExam(int id)
        {
            var exam = await _examsRepository.GetExamByIdAsync(id);
            if (exam == null)
            {
                return NotFound();
            }

            await _examsRepository.SoftDeleteExamAsync(id);

            TempData["SuccessMessage"] = "Đã xóa bài kiểm tra thành công!";
            return RedirectToAction("ListExam", "Lecturer");
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

            // Đổ dropdown danh sách khóa học
            ViewBag.CourseList = _context.Courses
                .Select(c => new SelectListItem
                {
                    Value = c.CourseID.ToString(),
                    Text = c.CourseName
                }).ToList();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddLecture(Lectures lecture)
        {
            if (ModelState.IsValid)
            {
                lecture.CreateAt = DateTime.Now;

                // Nếu có xử lý link Google Drive thì thêm đoạn này
                if (!string.IsNullOrEmpty(lecture.VideoUrl))
                {
                    if (lecture.VideoUrl.Contains("drive.google.com") && lecture.VideoUrl.Contains("/view"))
                    {
                        lecture.VideoUrl = lecture.VideoUrl.Replace("/view", "/preview");
                    }
                    else if (lecture.VideoUrl.Contains("open?id="))
                    {
                        var fileId = lecture.VideoUrl.Split("open?id=").Last();
                        lecture.VideoUrl = $"https://drive.google.com/file/d/{fileId}/preview";
                    }
                }

                await _lecturesRepository.AddLectureAsync(lecture);
                TempData["SuccessMessage"] = "Đã thêm bài giảng thành công!";
                return RedirectToAction("ListLecture");
            }

            // Nếu không hợp lệ → load lại danh sách Course để hiển thị
            ViewBag.CourseList = _context.Courses
                .Select(c => new SelectListItem
                {
                    Value = c.CourseID.ToString(),
                    Text = c.CourseName
                }).ToList();

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
                //  Kiểm tra và chuyển đổi VideoUrl nếu là link Google Drive
                if (!string.IsNullOrEmpty(lecture.VideoUrl))
                {
                    // Nếu là link Google Drive file và có "view?usp=sharing" thì đổi thành preview
                    if (lecture.VideoUrl.Contains("drive.google.com") && lecture.VideoUrl.Contains("/view"))
                    {
                        lecture.VideoUrl = lecture.VideoUrl.Replace("/view", "/preview");
                    }

                    // Nếu link dạng "open?id=..." → chuyển thành dạng file/d/ID/preview
                    if (lecture.VideoUrl.Contains("open?id="))
                    {
                        var fileId = lecture.VideoUrl.Split("open?id=").Last();
                        lecture.VideoUrl = $"https://drive.google.com/file/d/{fileId}/preview";
                    }
                }

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
            TempData["SuccessMessage"] = "Đã xóa bài kiểm tra (tạm thời).";
            return RedirectToAction("ListLecture");
        }
        public async Task<IActionResult> ListCourse()
        {
            SetLecturerViewData();
            var courses = await _coursesRepository.GetAllCoursesAsync();
            return View(courses);
        }
        public async Task<IActionResult> AddCourse()
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
        public async Task<IActionResult> ListQuiz()
        {
            SetLecturerViewData();
            var quizzes = await _quizzesRepository.GetAllQuizzesAsync();
            return View(quizzes);
        }
        public async Task<IActionResult> AddQuiz()
        {
            SetLecturerViewData();
            return View();
        }
        public async Task<IActionResult> EditQuiz(int id)
        {
            SetLecturerViewData();
            var quiz = await _quizzesRepository.GetQuizByIdAsync(id);
            return View(quiz);
        }
        public async Task<IActionResult> DeleteQuiz(int id)
        {
            SetLecturerViewData();
            var quiz = await _quizzesRepository.GetQuizByIdAsync(id);
            return View(quiz);
        }
        public async Task<IActionResult> ListQuestion()
        {
            SetLecturerViewData();
            var questions = await _questionsRepository.GetAllQuestionsAsync();
            return View(questions);
        }
        public async Task<IActionResult> AddQuestion()
        {
            SetLecturerViewData();
            return View();
        }
        public async Task<IActionResult> EditQuestion(int id)
        {
            SetLecturerViewData();
            var question = await _questionsRepository.GetQuestionByIdAsync(id);
            return View(question);
        }
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            SetLecturerViewData();
            var question = await _questionsRepository.GetQuestionByIdAsync(id);
            return View(question);
        }
        public async Task<IActionResult> ListResult()
        {
            SetLecturerViewData();

            // Lấy ID của giảng viên từ session
            var userId = HttpContext.Session.GetInt32("LecturerID") ?? 0;

            // RoleID của giảng viên
            var roleId = 2;

            // Lấy kết quả theo giảng viên hiện tại
            var results = await _resultsRepository.GetResultsByUserIdAsync(userId, roleId);

            return View(results);
        }


        public async Task<IActionResult> AddResult()
        {
            SetLecturerViewData();
            return View();
        }
        public async Task<IActionResult> EditResult(int id)
        {
            SetLecturerViewData();
            var result = await _resultsRepository.GetResultByIdAsync(id);
            return View(result);
        }
        public async Task<IActionResult> DeleteResult(int id)
        {
            SetLecturerViewData();
            var result = await _resultsRepository.GetResultByIdAsync(id);
            return View(result);
        }
        // Hiển thị thông tin chi tiết học viên theo ID
        public async Task<IActionResult> ViewStudent(int id)
        {
            SetLecturerViewData();

            // Gọi repository để lấy học viên theo ID
            var student = await _userAccountRepository.GetByIdAsync(id);

            // Nếu không tìm thấy → trả về 404
            if (student == null)
            {
                return NotFound(); // hoặc return View("Error");
            }

            // Trả về view với model là đối tượng học viên
            return View(student); // Views/Lecturer/ViewStudent.cshtml
        }


    }
}
