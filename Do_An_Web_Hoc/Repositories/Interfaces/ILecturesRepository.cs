using Do_An_Web_Hoc.Models;

namespace Do_An_Web_Hoc.Repositories.Interfaces
{
    public interface ILecturesRepository
    {
        // Lấy danh sách tất cả các bài giảng
        Task<IEnumerable<Lectures>> GetAllLecturesAsync();

        // Lấy thông tin chi tiết của một bài giảng dựa trên ID
        Task<Lectures> GetLectureByIdAsync(int lectureId);

        // Thêm một bài giảng mới vào cơ sở dữ liệu
        Task AddLectureAsync(Lectures lecture);

        // Cập nhật thông tin của một bài giảng
        Task UpdateLectureAsync(Lectures lecture);

        // Xóa một bài giảng dựa trên ID
        Task DeleteLectureAsync(int lectureId);

        // Tìm kiếm bài giảng theo tiêu đề (tên)
        Task<IEnumerable<Lectures>> SearchLecturesByTitleAsync(string title);
    }
}
