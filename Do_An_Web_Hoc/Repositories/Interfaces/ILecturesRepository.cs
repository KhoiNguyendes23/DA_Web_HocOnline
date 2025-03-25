using Do_An_Web_Hoc.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Do_An_Web_Hoc.Repositories.Interfaces
{
    public interface ILecturesRepository
    {
       

        Task<IEnumerable<Lectures>> GetAllLecturesAsync();
        Task<Lectures> GetLectureByIdAsync(int lectureId);
        Task AddLectureAsync(Lectures lecture);
        Task UpdateLectureAsync(Lectures lecture);
        Task DeleteLectureAsync(int lectureId);
        Task<IEnumerable<Lectures>> SearchLecturesByTitleAsync(string title);
        Task<IEnumerable<Lectures>> GetLecturesByCourseIdAsync(int courseId);
        Task<IEnumerable<Lectures>> GetRecentLecturesAsync(int count);
        Task<bool> LectureExistsAsync(int lectureId);
        Task<int> CountLecturesAsync();
        Task<IEnumerable<Lectures>> GetLecturesPagedAsync(int pageIndex, int pageSize);
        // Thêm vào cuối interface
        
        Task UpdateLecturerProfileAsync(Lectures lecturer);

    }
}
