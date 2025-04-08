using Do_An_Web_Hoc.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Do_An_Web_Hoc.Repositories.Interfaces
{
    public interface IUserAnswersRepository
    {
        Task SaveUserAnswerAsync(UserAnswers userAnswer);
        Task<IEnumerable<UserAnswers>> GetUserAnswersByQuizAsync(int userId, int quizId);
    }
}