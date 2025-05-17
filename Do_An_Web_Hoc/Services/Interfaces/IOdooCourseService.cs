using System.Collections.Generic;
using System.Threading.Tasks;

namespace Do_An_Web_Hoc.Services.Interfaces
{
    public interface IOdooCourseService
    {
        Task<int?> CreateCourseAsync(string courseName, string description);
        Task<int?> SearchCourseByNameAsync(string courseName);
        Task<bool> UpdateCourseAsync(int courseId, Dictionary<string, object> fieldsToUpdate);
    }
}
