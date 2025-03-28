using Do_An_Web_Hoc.Models;
using Do_An_Web_Hoc.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

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
            await _context.Answers.AddAsync(answer);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Answers>> GetAnswersByQuestionIdAsync(int questionId)
        {
            return await _context.Answers
                .Where(a => a.QuestionID == questionId)
                .ToListAsync();
        }

        public async Task DeleteAnswersByQuestionIdAsync(int questionId)
        {
            var answers = await _context.Answers
                .Where(a => a.QuestionID == questionId)
                .ToListAsync();

            _context.Answers.RemoveRange(answers);
            await _context.SaveChangesAsync();
        }
    }
}
