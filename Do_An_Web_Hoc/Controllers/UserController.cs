using Do_An_Web_Hoc.Models;
using Do_An_Web_Hoc.Models.ViewModels;
using Do_An_Web_Hoc.Repositories;
using Do_An_Web_Hoc.Repositories.Interfaces;
using Do_An_Web_Hoc.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using ResultsModel = Do_An_Web_Hoc.Models.Results;

namespace Do_An_Web_Hoc.Controllers
{
    [Authorize(Roles = "User")]
    public class UserController : Controller
    {
        private readonly ICoursesRepository _coursesRepo;
        private readonly IEnrollmentsRepository _enrollmentsRepo;
        private readonly ILecturesRepository _lecturesRepo;
        private readonly IQuizzesRepository _quizzesRepository;
        private readonly IQuestionsRepository _questionsRepository;
        private readonly IAnswersRepository _answersRepository;
        private readonly IExamsRepository _examsRepository;
        private readonly IUserAnswersRepository _userAnswersRepository;
        private readonly IResultsRepository _resultsRepository;
        private readonly IUserAccountRepository _userAccountRepository;
        public UserController(ICoursesRepository coursesRepo, 
            IEnrollmentsRepository enrollmentsRepo, 
            ILecturesRepository lecturesRepo,
            IQuizzesRepository quizzesRepository, 
            IQuestionsRepository questionsRepository,
            IAnswersRepository answersRepository,
            IExamsRepository examsRepository,
            IUserAnswersRepository userAnswersRepository, 
            IResultsRepository resultsRepository, 
            IUserAccountRepository userAccountRepository)
        {
            _coursesRepo = coursesRepo;
            _enrollmentsRepo = enrollmentsRepo;
            _lecturesRepo = lecturesRepo;
            _quizzesRepository = quizzesRepository;
            _questionsRepository = questionsRepository;
            _answersRepository = answersRepository;
            _examsRepository = examsRepository;
            _userAnswersRepository = userAnswersRepository;
            _resultsRepository = resultsRepository;
            _userAccountRepository = userAccountRepository;
        }
        private void SetUserViewData()
        {
            var email = User.FindFirstValue(ClaimTypes.Email); // Lấy email từ Claim

            if (!string.IsNullOrEmpty(email))
            {
                var user = _userAccountRepository.GetByEmailAsync(email).Result;
                if (user != null)
                {
                    ViewData["FullName"] = user.FullName;
                    ViewData["RoleName"] = "Học viên";

                    if (string.IsNullOrEmpty(user.Image))
                    {
                        ViewData["ImagePath"] = "/images/Avatar_images/default-avatar.png";
                    }
                    else
                    {
                        ViewData["ImagePath"] = user.Image.StartsWith("/")
                            ? user.Image
                            : "/images/Avatar_images/" + user.Image;
                    }

                    HttpContext.Session.SetInt32("UserID", user.UserID);
                }
            }
            else
            {
                ViewData["FullName"] = "Học viên";
                ViewData["RoleName"] = "Học viên";
                ViewData["ImagePath"] = "/images/Avatar_images/default-avatar.png";
            }
        }



        public async Task<IActionResult> Dashboard(string keyword, string priceType, decimal? maxPrice)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Danh sách khóa học đã đăng ký
            var enrolledCourseIds = (await _enrollmentsRepo.GetEnrollmentsByUserAsync(userId))
                                    .Select(e => e.CourseID)
                                    .ToList();

            // Toàn bộ khóa học
            var courses = await _coursesRepo.GetAvailableCoursesForUserAsync(userId);

            // Tìm kiếm theo keyword
            if (!string.IsNullOrEmpty(keyword))
            {
                keyword = keyword.ToLower();
                courses = courses.Where(c =>
                    (!string.IsNullOrEmpty(c.CourseName) && c.CourseName.ToLower().Contains(keyword)) ||
                    (!string.IsNullOrEmpty(c.Description) && c.Description.ToLower().Contains(keyword))
                ).ToList();
            }

            // Lọc theo loại giá
            if (!string.IsNullOrEmpty(priceType))
            {
                if (priceType == "free")
                    courses = courses.Where(c => c.Price == 0).ToList();
                else if (priceType == "paid")
                    courses = courses.Where(c => c.Price > 0).ToList();
            }

            // Lọc theo giá tối đa
            if (maxPrice.HasValue)
            {
                courses = courses.Where(c => c.Price <= maxPrice.Value).ToList();
            }

            // Truyền dữ liệu ra View
            ViewBag.EnrolledCourseIds = enrolledCourseIds;
            ViewBag.Keyword = keyword;
            ViewBag.PriceType = priceType;
            ViewBag.MaxPrice = maxPrice;

            SetUserViewData();
            return View(courses);
        }

        public IActionResult Courses()
        {
            SetUserViewData();
            return View();
        }

        public async Task<IActionResult> NotEnrolled()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var notEnrolledCourses = await _coursesRepo.GetNotEnrolledCoursesAsync(userId);

            SetUserViewData();
            return View(notEnrolledCourses);
        }

        public async Task<IActionResult> Enrolled()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var enrollments = await _enrollmentsRepo.GetEnrollmentsByUserAsync(userId);
            var courseIds = enrollments.Select(e => e.CourseID).ToList();

            var allCourses = await _coursesRepo.GetAllCoursesAsync();
            var enrolledCourses = allCourses.Where(c => courseIds.Contains(c.CourseID)).ToList();

            SetUserViewData();
            return View(enrolledCourses);
        }

        public IActionResult InProgress()
        {
            SetUserViewData();
            return View();
        }

        public async Task<IActionResult> FreeCourses()
        {
            var freeCourses = (await _coursesRepo.GetAllCoursesAsync())
                              .Where(c => c.Price == 0 || c.Price == null).ToList();
            return View(freeCourses);
        }

        public async Task<IActionResult> PaidCourses()
        {
            var paidCourses = (await _coursesRepo.GetAllCoursesAsync())
                              .Where(c => c.Price > 0).ToList();
            return View(paidCourses);
        }




        // Controller tách bốc rõ ràng quy trình: 1. Chọn Exam -> 2. Chọn Quiz -> 3. Làm bài

        // Trang 1: Chọn Exam
        public async Task<IActionResult> QuizExamSelection()
        {
            SetUserViewData();
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var exams = await _examsRepository.GetExamsByEnrolledUserAsync(userId);
            ViewBag.Exams = exams;
            return View("QuizExamSelection");
        }

        // Trang 2: Chọn Quiz theo Exam
        public async Task<IActionResult> QuizSelection(int examId)
        {
            SetUserViewData();

            var exam = await _examsRepository.GetExamByIdAsync(examId);
            if (exam == null)
            {
                TempData["Message"] = "Bài kiểm tra không hợp lệ.";
                return RedirectToAction("QuizExamSelection");
            }

            var quizzes = await _quizzesRepository.GetQuizzesByExamIdAsync(examId);
            if (quizzes == null || !quizzes.Any())
            {
                TempData["Message"] = "Bài kiểm tra này không có quiz.";
                return RedirectToAction("QuizExamSelection");
            }

            ViewBag.Quizzes = quizzes;
            ViewBag.ExamName = exam.ExamName;
            return View("QuizSelection");
        }

        // Trang 3: Làm bài theo quizId
        public async Task<IActionResult> TakeQuiz(int quizId)
        {
            SetUserViewData();
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var quiz = await _quizzesRepository.GetQuizByIdAsync(quizId);
            if (quiz == null)
            {
                TempData["Message"] = "Quiz không tồn tại.";
                return RedirectToAction("QuizExamSelection");
            }

            var exam = await _examsRepository.GetExamByIdAsync(quiz.ExamID.Value);
            var enrolledExams = await _examsRepository.GetExamsByEnrolledUserAsync(userId);

            if (!enrolledExams.Any(e => e.ExamID == quiz.ExamID))
            {
                TempData["Message"] = "Bạn không được làm quiz này.";
                return RedirectToAction("QuizExamSelection");
            }

            var questions = await _questionsRepository.GetQuestionsByQuizIdAsync(quiz.QuizID);
            var questionIds = questions.Select(q => q.QuestionID).ToList();
            var answers = await _answersRepository.GetAnswersByQuestionIdsAsync(questionIds);

            ViewBag.Quiz = quiz;
            ViewBag.Questions = questions;
            ViewBag.Answers = answers;
            ViewBag.ExamName = exam?.ExamName;
            ViewBag.Duration = exam?.Duration ?? 60;

            return View("TakeQuiz");
        }







        public IActionResult Practice()
        {
            SetUserViewData();
            return View();
        }

        public async Task<IActionResult> Ranking()
        {
            var rankings = await _resultsRepository.GetUserRankingAsync();
            SetUserViewData();
            return View(rankings);
        }


        public IActionResult Support()
        {
            SetUserViewData();
            return View();
        }

        public IActionResult ContactLecturer()
        {
            SetUserViewData();
            return View();
        }

        public IActionResult Community()
        {
            SetUserViewData();
            return View();
        }

        public async Task<IActionResult> Profile()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email)) return RedirectToAction("Login", "Account");

            var user = await _userAccountRepository.GetByEmailAsync(email);
            if (user == null) return View("Error");

            ViewData["FullName"] = user.FullName;
            ViewData["Email"] = user.Email;
            ViewData["PhoneNumber"] = user.PhoneNumber ?? "";
            ViewData["Birthday"] = user.Birthday?.ToString("dd/MM/yyyy") ?? "";
            ViewData["Address"] = user.Address ?? "";

            // Xử lý ảnh đại diện
            ViewData["ImagePath"] = string.IsNullOrEmpty(user.Image)
                ? "/images/Avatar_images/default-avatar.png"
                : (user.Image.StartsWith("/") ? user.Image : "/images/Avatar_images/" + user.Image);

            return View();
        }


        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email)) return RedirectToAction("Login", "Account");

            var user = await _userAccountRepository.GetByEmailAsync(email);
            if (user == null) return View("Error");

            SetUserViewData(); // Gán thông tin cho layout

            return View(user);
        }
        [HttpPost]
        public async Task<IActionResult> EditProfile(UserAccount model, IFormFile image)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email)) return RedirectToAction("Login", "Account");

            var user = await _userAccountRepository.GetByEmailAsync(email);
            if (user == null) return View("Error");

            // Cập nhật thông tin
            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;
            user.Birthday = model.Birthday;
            user.Address = model.Address;

            // Nếu có upload ảnh mới
            if (image != null && image.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/Avatar_images", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                user.Image = "/images/Avatar_images/" + fileName;
            }

            await _userAccountRepository.UpdateAsync(user);

            // Cập nhật lại layout
            SetUserViewData();

            TempData["Message"] = "Cập nhật hồ sơ thành công!";
            return RedirectToAction("Profile");
        }


        public async Task<IActionResult> PaymentHistory()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return Unauthorized();
            }

            var paidEnrollments = await _enrollmentsRepo.GetPaidEnrollmentsByUserAsync(userId);

            var model = paidEnrollments.Select(e => new
            {
                e.Course.CourseName,
                e.Course.ImageUrl,
                e.Course.Price,
                e.PaymentMethod,
                e.PaymentDate
            }).ToList();

            ViewBag.PaidCourses = model;
            SetUserViewData();
            return View();
        }

        public async Task<IActionResult> Payment(int courseId)
        {
            var course = (await _coursesRepo.GetAllCoursesAsync()).FirstOrDefault(c => c.CourseID == courseId);
            if (course == null)
            {
                TempData["Message"] = "Khóa học không tồn tại.";
                return RedirectToAction("Dashboard");
            }

            ViewBag.Course = course;
            SetUserViewData();
            return View();
        }

        public async Task<IActionResult> LearnCourse(int id)
        {
            var course = await _coursesRepo.GetCourseByIdAsync(id);
            if (course == null) return NotFound();

            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Nếu là khóa học trả phí thì bắt buộc phải đăng ký & đã thanh toán
            if (course.Price > 0)
            {
                bool isEnrolled = await _enrollmentsRepo.IsUserEnrolledAsync(userId, course.CourseID);
                if (!isEnrolled)
                {
                    TempData["Message"] = "Bạn cần đăng ký khóa học này trước khi học.";
                    return RedirectToAction("Dashboard");
                }
            }

            var lectures = await _lecturesRepo.GetLecturesByCourseIdAsync(id);
            ViewBag.CourseName = course.CourseName;
            ViewBag.CourseId = course.CourseID;
            SetUserViewData();
            return View(lectures);
        }




        [HttpPost]
        public async Task<IActionResult> SubmitQuiz(IFormCollection form)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            int quizId = int.Parse(form["QuizID"]);

            // Lấy danh sách câu hỏi trong quiz
            var questions = await _questionsRepository.GetQuestionsByQuizIdAsync(quizId);
            var questionIds = questions.Select(q => q.QuestionID).ToList();

            // Lấy đáp án đúng
            var correctAnswers = await _answersRepository.GetAnswersByQuestionIdsAsync(questionIds);

            int correctCount = 0;

            foreach (var question in questions)
            {
                if (question.QuestionType != "MCQ") continue; // Bỏ qua câu Essay

                int questionId = question.QuestionID;
                string key = $"Question_{questionId}";

                if (form.ContainsKey(key))
                {
                    string answerValue = form[key];

                    if (int.TryParse(answerValue, out int selectedAnswerId))
                    {
                        var correctAnswer = correctAnswers
                            .FirstOrDefault(a => a.QuestionID == questionId && a.IsCorrect == true);

                        if (correctAnswer != null && correctAnswer.AnswerID == selectedAnswerId)
                        {
                            correctCount++;
                        }

                        var userAnswer = new UserAnswers
                        {
                            UserID = userId,
                            QuestionID = questionId,
                            AnswerID = selectedAnswerId
                        };

                        await _userAnswersRepository.SaveUserAnswerAsync(userAnswer);
                    }
                }
            }

            // Lấy tổng điểm từ Quiz
            var quiz = await _quizzesRepository.GetQuizByIdAsync(quizId);
            int totalQuestions = questions.Count();
            int totalMarks = quiz?.TotalMarks ?? totalQuestions; // fallback nếu null

            // Tính điểm: số câu đúng / tổng câu * totalMarks
            int score = 0;
            if (totalQuestions > 0)
            {
                score = (int)Math.Round((double)correctCount * totalMarks / totalQuestions);
            }

            // Lưu kết quả bài làm
            var result = new ResultsModel
            {
                UserID = userId,
                QuizID = quizId,
                Score = score,
                SubmissionTime = DateTime.Now
            };

            await _resultsRepository.SaveResultFromUserAsync(result);

            ViewBag.Score = score;
            ViewBag.Total = totalMarks;
            ViewBag.CorrectAnswers = correctCount;
            ViewBag.TotalQuestions = totalQuestions;
            SetUserViewData();
            return View("QuizResult");
        }




        [HttpGet]
        public IActionResult SecuritySettings()
        {
            SetUserViewData();
            return View();
        }



        [HttpPost]
        public async Task<IActionResult> SecuritySettings(ChangePasswordViewModel model)
        {
            SetUserViewData(); // Gán lại thông tin người dùng

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Vui lòng nhập đầy đủ thông tin.";
                return View(model);
            }

            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userAccountRepository.GetByEmailAsync(email);
            if (user == null)
            {
                TempData["Error"] = "Người dùng không tồn tại.";
                return RedirectToAction("Login", "Account");
            }

            // Dùng PasswordHasher để so sánh mật khẩu cũ
            var hasher = new PasswordHasher<UserAccount>();
            var result = hasher.VerifyHashedPassword(user, user.Password, model.CurrentPassword);
            if (result != PasswordVerificationResult.Success)
            {
                TempData["Error"] = "Mật khẩu hiện tại không chính xác.";
                return View(model);
            }

            if (model.NewPassword != model.ConfirmPassword)
            {
                TempData["Error"] = "Mật khẩu mới không khớp.";
                return View(model);
            }

            // Mã hóa và lưu lại mật khẩu mới
            user.Password = hasher.HashPassword(user, model.NewPassword);
            await _userAccountRepository.UpdateUserAsync(user);

            TempData["Success"] = "Cập nhật mật khẩu thành công!";
            return RedirectToAction("SecuritySettings");
        }





        [HttpPost]
        public async Task<IActionResult> RegisterCourse(int courseId)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                return Unauthorized();
            }

            // Kiểm tra đã đăng ký chưa
            if (await _enrollmentsRepo.IsUserEnrolledAsync(userId, courseId))
            {
                TempData["Message"] = "Bạn đã đăng ký khóa học này.";
                return RedirectToAction("Enrolled");
            }

            // Lấy thông tin khóa học
            var course = (await _coursesRepo.GetAllCoursesAsync()).FirstOrDefault(c => c.CourseID == courseId);
            if (course == null)
            {
                TempData["Message"] = "Không tìm thấy khóa học.";
                return RedirectToAction("Dashboard");
            }

            // Nếu khóa miễn phí => cho đăng ký ngay
            if (course.Price == 0 || course.Price == null)
            {
                var enrollment = new Enrollments
                {
                    UserID = userId,
                    CourseID = courseId,
                    EnrollmentDate = DateTime.Now,
                    CompletionStatus = false
                };
                await _enrollmentsRepo.AddEnrollmentAsync(enrollment);

                TempData["Message"] = "Bạn đã đăng ký khóa học thành công!";
                return RedirectToAction("Enrolled");
            }
            else
            {
                // Nếu là khóa trả phí => chuyển hướng đến trang thanh toán
                return RedirectToAction("Payment", new { courseId = courseId });
            }
        }
    }
}
