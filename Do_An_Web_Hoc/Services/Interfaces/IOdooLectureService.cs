using System.Collections.Generic;
using System.Threading.Tasks;

namespace Do_An_Web_Hoc.Services.Interfaces
{
    public interface IOdooLectureService
    {
        Task<int?> CreateLectureAsync(string title, string content, string videoUrl, int courseId);
        Task<int?> SearchLectureByTitleAsync(string title);
        Task<bool> UpdateLectureAsync(int lectureId, Dictionary<string, object> fieldsToUpdate);
    }
}
