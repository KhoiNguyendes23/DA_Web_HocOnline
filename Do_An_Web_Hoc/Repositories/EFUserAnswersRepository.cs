using Do_An_Web_Hoc.Models;
using Do_An_Web_Hoc.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Do_An_Web_Hoc.Repositories
{
    public class EFUserAnswersRepository : IUserAnswersRepository
    {
        private readonly ApplicationDbContext _context;

        public EFUserAnswersRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SaveUserAnswerAsync(UserAnswers userAnswer)
        {
            _context.UserAnswers.Add(userAnswer);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<UserAnswers>> GetUserAnswersByQuizAsync(int userId, int quizId)
        {
            var questionIds = await _context.Questions
                .Where(q => q.QuizID == quizId)
                .Select(q => q.QuestionID)
                .ToListAsync();

            return await _context.UserAnswers
                .Where(ua => ua.UserID == userId && questionIds.Contains(ua.QuestionID ?? 0))
                .ToListAsync();
        }
    }
}