using Do_An_Web_Hoc.Models;
using Do_An_Web_Hoc.Models.ViewModels;

namespace Do_An_Web_Hoc.Repositories.Interfaces
{
    public interface IEnrollmentsRepository
    {
        Task AddEnrollmentAsync(Enrollments enrollment);

        Task<bool> IsUserEnrolledAsync(int userId, int courseId);

        // Lấy danh sách khóa học người dùng đã đăng ký
        Task<IEnumerable<Enrollments>> GetEnrollmentsByUserAsync(int userId);

        // Hủy đăng ký (nếu cho phép)
        Task RemoveEnrollmentAsync(int userId, int courseId);

        // Cập nhật trạng thái hoàn thành
        Task UpdateCompletionStatusAsync(int enrollmentId, bool isCompleted);

        //  Thống kê tổng số người đăng ký một khóa học
        Task<int> CountEnrollmentsForCourseAsync(int courseId);
        // Doanh Thu
        Task<IEnumerable<RevenueStatisticViewModel>> GetMonthlyRevenueStatisticsAsync();
        //  Lấy danh sách các khóa học đã thanh toán của người dùng
        Task<IEnumerable<Enrollments>> GetPaidEnrollmentsByUserAsync(int userId);

        Task<decimal> GetRevenueByMonthAsync(int month, int year);
        Task<List<TopCourseViewModel>> GetTopCoursesAsync(int topN);
        Task<List<RecentUserViewModel>> GetRecentUsersAsync(int topN);

    }
}
