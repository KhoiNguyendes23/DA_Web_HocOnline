// EFAnswersRepository.cs
using Do_An_Web_Hoc.Models;
using Do_An_Web_Hoc.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Do_An_Web_Hoc.Repositories
{
    public class EFAnswersRepository : IAnswersRepository
    {
        private readonly ApplicationDbContext _context;

        public EFAnswersRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAnswerAsync(Answers answer)
        {
            _context.Answers.Add(answer);
            await _context.SaveChangesAsync();
        }

        public async Task AddMultipleAnswersAsync(IEnumerable<Answers> answers)
        {
            await _context.Answers.AddRangeAsync(answers);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAnswerAsync(Answers answer)
        {
            _context.Answers.Update(answer);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAnswerAsync(int answerId)
        {
            var answer = await _context.Answers.FindAsync(answerId);
            if (answer != null)
            {
                _context.Answers.Remove(answer);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Answers>> GetAllAnswersAsync()
        {
            return await _context.Answers.ToListAsync();
        }

        public async Task<Answers> GetAnswerByIdAsync(int answerId)
        {
            return await _context.Answers.FindAsync(answerId);
        }

        public async Task<IEnumerable<Answers>> GetAnswersByQuestionIdAsync(int questionId)
        {
            return await _context.Answers
                .Where(a => a.QuestionID == questionId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Answers>> GetAnswersByQuestionIdsAsync(IEnumerable<int> questionIds)
        {
            return await _context.Answers
                .Where(a => questionIds.Contains(a.QuestionID ?? 0))
                .ToListAsync();
        }

        public async Task<Answers?> GetCorrectAnswerAsync(int questionId)
        {
            return await _context.Answers
                .FirstOrDefaultAsync(a => a.QuestionID == questionId && a.IsCorrect == true);
        }
    }
}
