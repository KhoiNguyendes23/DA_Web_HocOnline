using System.Collections.Generic;
using System.Threading.Tasks;
using ModelsResults = Do_An_Web_Hoc.Models.Results;
using Do_An_Web_Hoc.Models.ViewModels;



namespace Do_An_Web_Hoc.Repositories.Interfaces
{
    public interface IResultsRepository
    {
        // Lấy tất cả kết quả (Chỉ Admin mới có quyền)
        Task<IEnumerable<Do_An_Web_Hoc.Models.Results>> GetAllResultsAsync(int userRoleId);

        // Lấy danh sách kết quả theo UserID (Chỉ xem kết quả của chính mình trừ Admin)
        Task<IEnumerable<Do_An_Web_Hoc.Models.Results>> GetResultsByUserIdAsync(int userId, int userRoleId);

        // Lấy danh sách kết quả theo QuizID
        Task<IEnumerable<Do_An_Web_Hoc.Models.Results>> GetResultsByQuizIdAsync(int quizId);

        // Lấy thông tin chi tiết của một kết quả dựa trên ID
        Task<Do_An_Web_Hoc.Models.Results> GetResultByIdAsync(int resultId);

        // Thêm kết quả mới (Chỉ Admin hoặc Giáo viên mới có thể thêm)
        Task AddResultAsync(Do_An_Web_Hoc.Models.Results result, int userRoleId);

        // Cập nhật điểm số (Chỉ Admin hoặc Giáo viên mới có thể chỉnh sửa)
        Task UpdateResultAsync(Do_An_Web_Hoc.Models.Results result, int userRoleId);

        // Xóa kết quả (Chỉ Admin mới có thể xóa)
        Task DeleteResultAsync(int resultId, int userRoleId);
        Task SaveResultFromUserAsync(ModelsResults result);
        Task<IEnumerable<ResultExamViewModel>> GetExamResultsForLecturerAsync();

        Task<IEnumerable<RankingViewModel>> GetUserRankingAsync();
    }
}
