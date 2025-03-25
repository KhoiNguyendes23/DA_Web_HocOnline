using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Do_An_Web_Hoc.Models;
using Do_An_Web_Hoc.Repositories.Interfaces;

namespace Do_An_Web_Hoc.Repositories
{
    public class EFLecturesRepository : ILecturesRepository
    {
        // ✅ Constructor không cần context khi test view
        public EFLecturesRepository()
        {
        }

        public Task<IEnumerable<Lectures>> GetAllLecturesAsync()
        {
            // ✅ Mock danh sách bài giảng
            var lectures = new List<Lectures>
            {
                new Lectures { LectureID = 1, Title = "Bài giảng C#", Content = "Nội dung chi tiết", CourseID = 1, CreateAt = DateTime.Now },
                new Lectures { LectureID = 2, Title = "Bài giảng ASP.NET", Content = "Nội dung web", CourseID = 2, CreateAt = DateTime.Now }
            };

            return Task.FromResult(lectures.AsEnumerable());
        }

        public Task<Lectures> GetLectureByIdAsync(int lectureId)
        {
            var lecture = new Lectures
            {
                LectureID = lectureId,
                Title = "Bài giảng mẫu",
                Content = "Demo Content",
                CourseID = 1,
                CreateAt = DateTime.Now
            };

            return Task.FromResult(lecture);
        }

        public Task AddLectureAsync(Lectures lecture)
        {
            // ❌ Không làm gì khi test view
            return Task.CompletedTask;
        }

        public Task UpdateLectureAsync(Lectures lecture)
        {
            // ❌ Không làm gì khi test view
            return Task.CompletedTask;
        }

        public Task DeleteLectureAsync(int lectureId)
        {
            // ❌ Không làm gì khi test view
            return Task.CompletedTask;
        }

        public Task<IEnumerable<Lectures>> SearchLecturesByTitleAsync(string title)
        {
            var filtered = new List<Lectures>
            {
                new Lectures { LectureID = 3, Title = $"Tìm kiếm: {title}", Content = "Kết quả demo", CourseID = 1 }
            };

            return Task.FromResult(filtered.AsEnumerable());
        }

        public Task<IEnumerable<Lectures>> GetLecturesByCourseIdAsync(int courseId)
        {
            var byCourse = new List<Lectures>
            {
                new Lectures { LectureID = 4, Title = "Course Specific Lecture", CourseID = courseId }
            };

            return Task.FromResult(byCourse.AsEnumerable());
        }

        public Task<IEnumerable<Lectures>> GetRecentLecturesAsync(int count)
        {
            var recent = Enumerable.Range(1, count).Select(i => new Lectures
            {
                LectureID = i,
                Title = $"Bài giảng mới {i}",
                CreateAt = DateTime.Now.AddDays(-i)
            });

            return Task.FromResult(recent);
        }

        public Task<bool> LectureExistsAsync(int lectureId)
        {
            return Task.FromResult(true); // ✅ Luôn "tồn tại"
        }

        public Task<int> CountLecturesAsync()
        {
            return Task.FromResult(5); // ✅ Mặc định 5 bài giảng
        }

        public Task<IEnumerable<Lectures>> GetLecturesPagedAsync(int pageIndex, int pageSize)
        {
            var paged = Enumerable.Range(1, pageSize).Select(i => new Lectures
            {
                LectureID = (pageIndex - 1) * pageSize + i,
                Title = $"Bài giảng phân trang {i}"
            });

            return Task.FromResult(paged);
        }

      
        public Task UpdateLecturerProfileAsync(Lectures lecturer)
        {
            throw new NotImplementedException();
        }
    }
}
