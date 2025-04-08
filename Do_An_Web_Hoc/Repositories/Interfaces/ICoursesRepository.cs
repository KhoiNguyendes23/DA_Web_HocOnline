using Do_An_Web_Hoc.Models;

namespace Do_An_Web_Hoc.Repositories.Interfaces
{
    public interface ICoursesRepository
    {
        // Lấy tất cả khóa học
        Task<IEnumerable<Courses>> GetAllCoursesAsync();
        // Lấy khóa học theo ID
        Task<Courses> GetCourseByIdAsync(int courseId);
        // Thêm khóa học mới
        Task AddCourseAsync(Courses course);

        // Cập nhật thông tin khóa học
        Task<bool> UpdateCourseAsync(Courses course);

        // Xóa khóa học
        Task DeleteCourseAsync(int courseId);

        // Lấy danh sách khóa học theo danh mục
        Task<IEnumerable<Courses>> GetCoursesByCategoryAsync(int categoryId);

        // Lấy danh sách khóa học theo trạng thái
        Task<IEnumerable<Courses>> GetCoursesByStatusAsync(int statusId);
        // Tìm Kiếm Theo Tên Khóa Học
        Task<IEnumerable<Courses>> SearchCoursesByNameAsync(string keyword);
        // Xóa Mềm khóa học
        Task<bool> SoftDeleteCourseAsync(int courseId);
        // Kiểm tra học viên đã đăng ký khóa học chứa bài kiểm tra chưa
        Task<bool> IsUserEnrolledInExamCourseAsync(int userId, int examId);

    }
}

