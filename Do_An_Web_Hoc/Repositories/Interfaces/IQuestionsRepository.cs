using System.Collections.Generic;
using System.Threading.Tasks;
using Do_An_Web_Hoc.Models;

namespace Do_An_Web_Hoc.Repositories.Interfaces
{
    public interface IQuestionsRepository
    {
        // Lấy tất cả câu hỏi
        Task<IEnumerable<Questions>> GetAllQuestionsAsync();

        // Lấy danh sách câu hỏi theo QuizID
        Task<IEnumerable<Questions>> GetQuestionsByQuizIdAsync(int quizId);

        // Lấy thông tin chi tiết của một câu hỏi dựa trên ID
        Task<Questions> GetQuestionByIdAsync(int questionId);

        // Thêm một câu hỏi mới vào cơ sở dữ liệu
        Task<int> AddQuestionAsync(Questions question);


        // Cập nhật thông tin của một câu hỏi
        Task UpdateQuestionAsync(Questions question);

        // Xóa một câu hỏi dựa trên ID
        Task DeleteQuestionAsync(int questionId);

        // Tìm kiếm câu hỏi theo nội dung (QuestionText)
        Task<IEnumerable<Questions>> SearchQuestionsByTextAsync(string questionText);
    }
}
