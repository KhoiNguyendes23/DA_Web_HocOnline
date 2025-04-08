using Do_An_Web_Hoc.Models;
using Do_An_Web_Hoc.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Do_An_Web_Hoc.Repositories
{
    public class EFCourseRepository : ICoursesRepository
    {
        private readonly ApplicationDbContext _context;

        public EFCourseRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Lấy tất cả khóa học
        public async Task<IEnumerable<Courses>> GetAllCoursesAsync()
        {
            //return await _context.Courses.ToListAsync()
            return await _context.Courses
                .Include(c => c.Enrollments) // Load luôn danh sách đăng ký
                .ToListAsync();
        }

        // Lấy khóa học theo ID
        public async Task<Courses> GetCourseByIdAsync(int courseId)
        {
            return await _context.Courses.FindAsync(courseId);
        }

        // Thêm khóa học mới
        public async Task AddCourseAsync(Courses course)
        {
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();
        }

        // Cập nhật thông tin khóa học
        public async Task<bool> UpdateCourseAsync(Courses course)
        {
            var existingCourse = await _context.Courses.FindAsync(course.CourseID);
            if (existingCourse == null)
            {
                return false; // Không tìm thấy khóa học để cập nhật
            }

            // Cập nhật thông tin khóa học từ đối tượng `course` mới
            existingCourse.CourseName = course.CourseName;
            existingCourse.Description = course.Description;
            existingCourse.Price = course.Price;
            existingCourse.CategoryID = course.CategoryID;
            existingCourse.Status = course.Status;
            existingCourse.ImageUrl = course.ImageUrl; // Nếu có thay đổi hình ảnh

            // Nếu có thay đổi, thực hiện lưu lại
            await _context.SaveChangesAsync();
            return true;
        }


        // Xóa khóa học theo ID
        public async Task DeleteCourseAsync(int courseId)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course != null)
            {
                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();
            }
        }

        // Lấy danh sách khóa học theo danh mục
        public async Task<IEnumerable<Courses>> GetCoursesByCategoryAsync(int categoryId)
        {
            return await _context.Courses
                .Where(c => c.CategoryID == categoryId)
                .ToListAsync();
        }

        // Lấy danh sách khóa học theo trạng thái
        public async Task<IEnumerable<Courses>> GetCoursesByStatusAsync(int statusId)
        {
            return await _context.Courses
                .Where(c => c.Status == statusId)
                .ToListAsync();
        }
        public async Task<IEnumerable<Courses>> SearchCoursesByNameAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return await GetAllCoursesAsync(); // Nếu từ khóa rỗng, trả về tất cả
            }

            return await _context.Courses
                .Where(c => c.CourseName.Contains(keyword)) // Tìm kiếm theo tên
                .ToListAsync();
        }
        public async Task<bool> SoftDeleteCourseAsync(int courseId)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null)
            {
                return false; // Khóa học không tồn tại
            }

            // Cập nhật trạng thái thành "Ngừng hoạt động" (giả sử 0 là ngừng hoạt động, 1 là hoạt động)
            course.Status = 2;
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> IsUserEnrolledInExamCourseAsync(int userId, int examId)
        {
            var exam = await _context.Exams.FindAsync(examId);
            if (exam == null) return false;

            return await _context.Enrollments
                .AnyAsync(e => e.UserID == userId && e.CourseID == exam.CourseID);
        }

    }
}

