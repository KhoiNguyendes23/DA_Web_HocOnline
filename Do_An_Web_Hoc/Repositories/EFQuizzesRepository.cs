using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Do_An_Web_Hoc.Models;
using Do_An_Web_Hoc.Repositories.Interfaces;

namespace Do_An_Web_Hoc.Repositories
{
    public class EFQuizzesRepository : IQuizzesRepository
    {
        private readonly ApplicationDbContext _context;

        public EFQuizzesRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<Quizzes> GetQuizByExamIdAsync(int examId)
        {
            return await _context.Quizzes.FirstOrDefaultAsync(q => q.ExamID == examId);
        }

        // Lấy tất cả các bài quiz
        public async Task<IEnumerable<Quizzes>> GetAllQuizzesAsync()
        {
            return await _context.Quizzes.ToListAsync();
        }

        // Lấy danh sách bài quiz theo ExamID
        public async Task<IEnumerable<Quizzes>> GetQuizzesByExamIdAsync(int examId)
        {
            return await _context.Quizzes
                .Where(q => q.ExamID == examId)
                .ToListAsync();
        }

        // Lấy thông tin chi tiết của một bài quiz theo ID
        public async Task<Quizzes> GetQuizByIdAsync(int quizId)
        {
            return await _context.Quizzes.FindAsync(quizId);
        }

        // Thêm một bài quiz mới
        public async Task AddQuizAsync(Quizzes quiz)
        {
            await _context.Quizzes.AddAsync(quiz);
            await _context.SaveChangesAsync();
        }

        // Cập nhật thông tin bài quiz
        public async Task UpdateQuizAsync(Quizzes quiz)
        {
            _context.Quizzes.Update(quiz);
            await _context.SaveChangesAsync();
        }

        // Xóa một bài quiz theo ID
        public async Task DeleteQuizAsync(int quizId)
        {
            var quiz = await _context.Quizzes.FindAsync(quizId);
            if (quiz != null)
            {
                _context.Quizzes.Remove(quiz);
                await _context.SaveChangesAsync();
            }
        }

        // Tìm kiếm bài quiz theo tên (QuizName)
        public async Task<IEnumerable<Quizzes>> SearchQuizzesByNameAsync(string quizName)
        {
            return await _context.Quizzes
                .Where(q => q.QuizName.Contains(quizName))
                .ToListAsync();
        }
    }
}
