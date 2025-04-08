using Do_An_Web_Hoc.Models;
using Do_An_Web_Hoc.Repositories;
using Do_An_Web_Hoc.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        public UserController(ICoursesRepository coursesRepo, IEnrollmentsRepository enrollmentsRepo, ILecturesRepository lecturesRepo,
            IQuizzesRepository quizzesRepository, IQuestionsRepository questionsRepository, IAnswersRepository answersRepository, IExamsRepository examsRepository,
            IUserAnswersRepository userAnswersRepository, IResultsRepository resultsRepository)
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
        }
        public async Task<IActionResult> Dashboard(string keyword, string priceType, decimal? maxPrice)
        {
            var fullName = User.FindFirstValue(ClaimTypes.Name);
            var email = User.FindFirstValue(ClaimTypes.Email);
            ViewData["FullName"] = fullName;
            ViewData["Email"] = email;

            var courses = await _coursesRepo.GetAllCoursesAsync();

            if (!string.IsNullOrEmpty(keyword))
            {
                keyword = keyword.ToLower();
                courses = courses.Where(c =>
                    (!string.IsNullOrEmpty(c.CourseName) && c.CourseName.ToLower().Contains(keyword)) ||
                    (!string.IsNullOrEmpty(c.Description) && c.Description.ToLower().Contains(keyword))
                ).ToList();
            }

            if (!string.IsNullOrEmpty(priceType))
            {
                if (priceType == "free")
                {
                    courses = courses.Where(c => c.Price == 0).ToList();
                }
                else if (priceType == "paid")
                {
                    courses = courses.Where(c => c.Price > 0).ToList();
                }
            }

            if (maxPrice.HasValue)
            {
                courses = courses.Where(c => c.Price <= maxPrice.Value).ToList();
            }

            ViewBag.Keyword = keyword;
            ViewBag.PriceType = priceType;
            ViewBag.MaxPrice = maxPrice;

            return View(courses);
        }



        public IActionResult Courses()
        {
            return View();
        }

        public async Task<IActionResult> NotEnrolled()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var enrollments = await _enrollmentsRepo.GetEnrollmentsByUserAsync(userId);
            var courseIds = enrollments.Select(e => e.CourseID).ToList();

            var allCourses = await _coursesRepo.GetAllCoursesAsync();
            var notEnrolledCourses = allCourses.Where(c => !courseIds.Contains(c.CourseID)).ToList();

            return View(notEnrolledCourses);
        }

        public async Task<IActionResult> Enrolled()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var enrollments = await _enrollmentsRepo.GetEnrollmentsByUserAsync(userId);
            var courseIds = enrollments.Select(e => e.CourseID).ToList();

            var allCourses = await _coursesRepo.GetAllCoursesAsync();
            var enrolledCourses = allCourses.Where(c => courseIds.Contains(c.CourseID)).ToList();

            return View(enrolledCourses);
        }

        public IActionResult InProgress()
        {
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

        public async Task<IActionResult> Quiz(int examId)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Chỉ lấy các bài thi thuộc các khóa học mà user đã đăng ký
            var exams = await _examsRepository.GetExamsByEnrolledUserAsync(userId);
            ViewBag.Exams = exams;

            // Nếu chưa chọn bài thi
            if (examId == 0)
            {
                return View();
            }

            // Kiểm tra xem bài thi được chọn có thuộc danh sách được phép không
            if (!exams.Any(e => e.ExamID == examId))
            {
                TempData["Message"] = "Bạn không được phép làm bài kiểm tra này.";
                return RedirectToAction("Quiz");
            }

            // Lấy Quiz, câu hỏi, đáp án
            var quizzes = await _quizzesRepository.GetQuizzesByExamIdAsync(examId);
            if (quizzes == null || !quizzes.Any())
            {
                return View(); // Không có quiz
            }

            var quiz = quizzes.First();
            var questions = await _questionsRepository.GetQuestionsByQuizIdAsync(quiz.QuizID);
            var questionIds = questions.Select(q => q.QuestionID).ToList();
            var answers = await _answersRepository.GetAnswersByQuestionIdsAsync(questionIds);

            ViewBag.Quiz = quiz;
            ViewBag.Questions = questions;
            ViewBag.Answers = answers;

            return View();
        }





        public IActionResult Practice()
        {
            return View();
        }

        public async Task<IActionResult> Ranking()
        {
            var rankings = await _resultsRepository.GetUserRankingAsync();
            return View(rankings);
        }


        public IActionResult Support()
        {
            return View();
        }

        public IActionResult ContactLecturer()
        {
            return View();
        }

        public IActionResult Community()
        {
            return View();
        }

        public IActionResult Profile()
        {
            return View();
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
            return View();
        }

        public async Task<IActionResult> LearnCourse(int id)
        {
            var course = await _coursesRepo.GetCourseByIdAsync(id);
            if (course == null) return NotFound();

            var lectures = await _lecturesRepo.GetLecturesByCourseIdAsync(id);
            ViewBag.CourseName = course.CourseName;
            ViewBag.CourseId = course.CourseID;

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

            int score = 0;

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
                            score++;
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


            // Lưu kết quả bài làm vào bảng Results
            var result = new ResultsModel
            {
                UserID = userId,
                QuizID = quizId,
                Score = score,
                SubmissionTime = DateTime.Now
            };


            await _resultsRepository.SaveResultFromUserAsync(result); // Không kiểm tra quyền ở đây vì là học viên

            ViewBag.Score = score;
            ViewBag.Total = questionIds.Count;

            return View("QuizResult");
        }



        public IActionResult SecuritySettings()
        {
            return View();
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
