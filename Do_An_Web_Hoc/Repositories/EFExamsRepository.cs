using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Do_An_Web_Hoc.Models;
using Do_An_Web_Hoc.Repositories.Interfaces;
using Do_An_Web_Hoc.Models.ViewModels;

namespace Do_An_Web_Hoc.Repositories
{
    public class EFExamsRepository : IExamsRepository
    {
        private readonly ApplicationDbContext _context;

        public EFExamsRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        //public void AddExam(Exams exam)
        //{
        //    _context.Exams.Add(exam);
        //    _context.SaveChanges();
        //}

        // Lấy tất cả bài thi (cả hoạt động và ngừng hoạt động)
        public async Task<IEnumerable<Exams>> GetAllExamsAsync()
        {
            return await _context.Exams.ToListAsync();
        }

        // Lấy danh sách bài thi chỉ bao gồm trạng thái hoạt động (Status = 1)
        public async Task<IEnumerable<Exams>> GetActiveExamsAsync()
        {
            return await _context.Exams
                .Where(e => e.Status == 1)
                .ToListAsync();
        }

        // Lấy danh sách bài thi theo khóa học
        public async Task<IEnumerable<Exams>> GetExamsByCourseIdAsync(int courseId)
        {
            return await _context.Exams
                .Where(e => e.CourseID == courseId && e.Status == 1)
                .ToListAsync();
        }

        // Lấy thông tin chi tiết của một bài thi theo ID
        public async Task<Exams> GetExamByIdAsync(int examId)
        {
            return await _context.Exams.FindAsync(examId);
        }

        // Thêm một bài thi mới
        public async Task AddExamAsync(Exams exam)
        {
            exam.Status = 1; // Mặc định bài thi khi thêm sẽ ở trạng thái hoạt động
            await _context.Exams.AddAsync(exam);
            await _context.SaveChangesAsync();
        }

        // Cập nhật thông tin bài thi
        public async Task UpdateExamAsync(Exams exam)
        {
            _context.Exams.Update(exam);
            await _context.SaveChangesAsync();
        }

        // Xóa mềm bài thi (chuyển trạng thái sang ngừng hoạt động)
        public async Task SoftDeleteExamAsync(int examId)
        {
            var exam = await _context.Exams.FindAsync(examId);
            if (exam != null && exam.Status == 1)
            {
                exam.Status = 2;
                await _context.SaveChangesAsync();
            }

        }

        // Khôi phục bài thi đã xóa mềm
        public async Task RestoreExamAsync(int examId)
        {
            var exam = await _context.Exams.FindAsync(examId);
            if (exam != null && exam.Status == 2)
            {
                exam.Status = 1; // Đặt trạng thái về hoạt động
                await _context.SaveChangesAsync();
            }
        }

        // Tìm kiếm bài thi theo tên (chỉ tìm trong các bài thi đang hoạt động)
        public async Task<IEnumerable<Exams>> SearchExamsByNameAsync(string examName)
        {
            return await _context.Exams
                .Where(e => e.Status == 1 && e.ExamName.Contains(examName))
                .ToListAsync();
        }

        public async Task<IEnumerable<Exams>> GetExamsByEnrolledUserAsync(int userId)
        {
            // Lấy danh sách CourseID mà user đã đăng ký
            var enrolledCourseIds = await _context.Enrollments
                .Where(e => e.UserID == userId)
                .Select(e => e.CourseID)
                .ToListAsync();

            // Lấy các bài thi thuộc các CourseID đó, chỉ lấy bài thi đang hoạt động
            return await _context.Exams
                .Where(exam => enrolledCourseIds.Contains(exam.CourseID) && exam.Status == 1)
                .ToListAsync();
        }
        public async Task<QuizResultViewModel> GetQuizReviewResultAsync(int quizId, int userId)
        {
            var quiz = await _context.Quizzes.FindAsync(quizId);
            var questions = await _context.Questions
                .Where(q => q.QuizID == quizId)
                .ToListAsync();

            var questionIds = questions.Select(q => q.QuestionID).ToList();

            // Lấy AttemptId mới nhất
            var latestAttemptId = await _context.UserAnswers
                .Where(ua => ua.UserID == userId && questionIds.Contains(ua.QuestionID ?? 0))
                .OrderByDescending(ua => ua.CreatedAt)
                .Select(ua => ua.AttemptId)
                .FirstOrDefaultAsync();

            // Lấy toàn bộ câu trả lời thuộc lần làm gần nhất
            var userAnswers = await _context.UserAnswers
                .Where(ua => ua.UserID == userId
                          && questionIds.Contains(ua.QuestionID ?? 0)
                          && ua.AttemptId == latestAttemptId)
                .ToListAsync();

            var allAnswers = await _context.Answers
                .Where(a => questionIds.Contains(a.QuestionID ?? 0))
                .ToListAsync();

            var result = new QuizResultViewModel
            {
                QuizName = quiz.QuizName,
                Questions = questions.Select(q => new QuestionResult
                {
                    QuestionText = q.QuestionText,
                    Answers = allAnswers
                        .Where(a => a.QuestionID == q.QuestionID)
                        .Select(a => new AnswerResult
                        {
                            AnswerText = a.AnswerText,
                            IsCorrect = a.IsCorrect ?? false,
                            IsSelected = userAnswers.Any(ua => ua.QuestionID == q.QuestionID && ua.AnswerID == a.AnswerID)
                        }).ToList()
                }).ToList()
            };

            result.TotalScore = result.Questions.Count;

            // Tính đúng nếu: chọn đúng tất cả đáp án, không chọn sai
            result.Score = result.Questions.Count(q =>
                q.Answers.All(a => a.IsCorrect == a.IsSelected));

            return result;
        }





        public async Task<IEnumerable<CompletedQuizViewModel>> GetCompletedQuizzesByUserAsync(int userId)
        {
            var completedQuizIds = await (from ua in _context.UserAnswers
                                          join q in _context.Questions on ua.QuestionID equals q.QuestionID
                                          where ua.UserID == userId
                                          select q.QuizID)
                                           .Distinct()
                                           .ToListAsync();

            return await (from quiz in _context.Quizzes
                          join exam in _context.Exams on quiz.ExamID equals exam.ExamID
                          where completedQuizIds.Contains(quiz.QuizID)
                          select new CompletedQuizViewModel
                          {
                              QuizID = quiz.QuizID,
                              QuizName = quiz.QuizName,
                              CreatedAt = exam.CreatedAt, //Lấy từ Exam
                              TotalMarks = quiz.TotalMarks
                          }).ToListAsync();
        }
    }
}
