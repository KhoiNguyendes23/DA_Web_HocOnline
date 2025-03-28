using Do_An_Web_Hoc.Models;

namespace Do_An_Web_Hoc.Repositories.Interfaces
{
    public interface IAnswersRepository
    {
        Task AddAnswerAsync(Answers answer);
        Task<IEnumerable<Answers>> GetAnswersByQuestionIdAsync(int questionId);
        Task DeleteAnswersByQuestionIdAsync(int questionId);
    }
}
