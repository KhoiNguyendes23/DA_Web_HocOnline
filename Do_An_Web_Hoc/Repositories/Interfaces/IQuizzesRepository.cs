using System.Collections.Generic;
using System.Threading.Tasks;
using Do_An_Web_Hoc.Models;

namespace Do_An_Web_Hoc.Repositories.Interfaces
{
    public interface IQuizzesRepository
    {
        // Lấy tất cả các bài quiz
        Task<IEnumerable<Quizzes>> GetAllQuizzesAsync();

        // Lấy danh sách bài quiz theo ExamID
        Task<IEnumerable<Quizzes>> GetQuizzesByExamIdAsync(int examId);

        // Lấy thông tin chi tiết của một bài quiz dựa trên ID
        Task<Quizzes> GetQuizByIdAsync(int quizId);

        // Thêm một bài quiz mới vào cơ sở dữ liệu
        Task AddQuizAsync(Quizzes quiz);

        // Cập nhật thông tin của một bài quiz
        Task UpdateQuizAsync(Quizzes quiz);

        // Xóa một bài quiz dựa trên ID
        Task DeleteQuizAsync(int quizId);

        // Tìm kiếm bài quiz theo tên (QuizName)
        Task<IEnumerable<Quizzes>> SearchQuizzesByNameAsync(string quizName);

        Task<Quizzes> GetQuizByExamIdAsync(int examId);

    }
}
