using Do_An_Web_Hoc.Models;

namespace Do_An_Web_Hoc.Repositories.Interfaces
{
    public interface IEnrollmentsRepository
    {
        Task AddEnrollmentAsync(Enrollments enrollment);

        Task<bool> IsUserEnrolledAsync(int userId, int courseId);

        // 📌 Lấy danh sách khóa học người dùng đã đăng ký
        Task<IEnumerable<Enrollments>> GetEnrollmentsByUserAsync(int userId);

        // 📌 Hủy đăng ký (nếu cho phép)
        Task RemoveEnrollmentAsync(int userId, int courseId);

        // 📌 Cập nhật trạng thái hoàn thành
        Task UpdateCompletionStatusAsync(int enrollmentId, bool isCompleted);

        // 📌 Thống kê tổng số người đăng ký một khóa học
        Task<int> CountEnrollmentsForCourseAsync(int courseId);

    }
}
