using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Do_An_Web_Hoc.Models;
using Do_An_Web_Hoc.Repositories.Interfaces;

namespace Do_An_Web_Hoc.Repositories
{
    public class EFQuestionsRepository : IQuestionsRepository
    {
        private readonly ApplicationDbContext _context;

        public EFQuestionsRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Lấy tất cả câu hỏi
        public async Task<IEnumerable<Questions>> GetAllQuestionsAsync()
        {
            return await _context.Questions.ToListAsync();
        }

        // Lấy danh sách câu hỏi theo QuizID
        public async Task<IEnumerable<Questions>> GetQuestionsByQuizIdAsync(int quizId)
        {
            return await _context.Questions
                .Where(q => q.QuizID == quizId)
                .ToListAsync();
        }

        // Lấy thông tin chi tiết của một câu hỏi theo ID
        public async Task<Questions> GetQuestionByIdAsync(int questionId)
        {
            return await _context.Questions.FindAsync(questionId);
        }

        // Thêm một câu hỏi mới
        public async Task AddQuestionAsync(Questions question)
        {
            await _context.Questions.AddAsync(question);
            await _context.SaveChangesAsync();
        }

        // Cập nhật thông tin câu hỏi
        public async Task UpdateQuestionAsync(Questions question)
        {
            _context.Questions.Update(question);
            await _context.SaveChangesAsync();
        }

        // Xóa một câu hỏi theo ID
        public async Task DeleteQuestionAsync(int questionId)
        {
            var question = await _context.Questions.FindAsync(questionId);
            if (question != null)
            {
                _context.Questions.Remove(question);
                await _context.SaveChangesAsync();
            }
        }

        // Tìm kiếm câu hỏi theo nội dung (QuestionText)
        public async Task<IEnumerable<Questions>> SearchQuestionsByTextAsync(string questionText)
        {
            return await _context.Questions
                .Where(q => q.QuestionText.Contains(questionText))
                .ToListAsync();
        }
    }
}
