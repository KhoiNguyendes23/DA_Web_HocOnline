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
            return await _context.Courses.ToListAsync();
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
            _context.Courses.Update(course);
            return await _context.SaveChangesAsync() > 0;
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
    }
}

