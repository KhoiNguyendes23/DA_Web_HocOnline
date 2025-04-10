using Do_An_Web_Hoc.Models;
using Do_An_Web_Hoc.Models.ViewModels;

namespace Do_An_Web_Hoc.Repositories.Interfaces
{
    public interface IExamsRepository
    {
        // Lấy tất cả các bài thi (cả hoạt động và ngừng hoạt động)
        Task<IEnumerable<Exams>> GetAllExamsAsync();

        // Lấy danh sách bài thi chỉ bao gồm trạng thái hoạt động (Status = 1)
        Task<IEnumerable<Exams>> GetActiveExamsAsync();

        // Lấy danh sách bài thi thuộc một khóa học cụ thể
        Task<IEnumerable<Exams>> GetExamsByCourseIdAsync(int courseId);

        // Lấy thông tin chi tiết của một bài thi dựa trên ID
        Task<Exams> GetExamByIdAsync(int examId);

        // Thêm một bài thi mới vào cơ sở dữ liệu
        Task AddExamAsync(Exams exam);

        // Cập nhật thông tin của một bài thi
        Task UpdateExamAsync(Exams exam);

        // Xóa mềm một bài thi (chuyển trạng thái thành ngừng hoạt động thay vì xóa khỏi DB)
        Task SoftDeleteExamAsync(int examId);

        // Khôi phục bài thi đã bị xóa mềm
        Task RestoreExamAsync(int examId);

        // Tìm kiếm bài thi theo tên (ExamName) trong danh sách bài thi đang hoạt động
        Task<IEnumerable<Exams>> SearchExamsByNameAsync(string examName);

        Task<IEnumerable<Exams>> GetExamsByEnrolledUserAsync(int userId);

        Task<QuizResultViewModel> GetQuizReviewResultAsync(int quizId, int userId);

        Task<IEnumerable<CompletedQuizViewModel>> GetCompletedQuizzesByUserAsync(int userId);


    }
}
