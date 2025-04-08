using Do_An_Web_Hoc.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Do_An_Web_Hoc.Repositories.Interfaces
{
    public interface IAnswersRepository
    {
        // Lấy tất cả các đáp án
        Task<IEnumerable<Answers>> GetAllAnswersAsync();

        // Lấy danh sách đáp án theo QuestionID
        Task<IEnumerable<Answers>> GetAnswersByQuestionIdAsync(int questionId);

        // Lấy danh sách đáp án theo danh sách câu hỏi
        Task<IEnumerable<Answers>> GetAnswersByQuestionIdsAsync(IEnumerable<int> questionIds);

        // Thêm đáp án mới
        Task AddAnswerAsync(Answers answer);

        // Thêm nhiều đáp án
        Task AddMultipleAnswersAsync(IEnumerable<Answers> answers);

        // Xóa đáp án theo ID
        Task DeleteAnswerAsync(int answerId);

        // Cập nhật đáp án
        Task UpdateAnswerAsync(Answers answer);

        // Tìm đáp án đúng theo QuestionID
        Task<Answers?> GetCorrectAnswerAsync(int questionId);
    }
}
