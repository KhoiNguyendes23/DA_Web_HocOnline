using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Do_An_Web_Hoc.Models;
using Do_An_Web_Hoc.Repositories.Interfaces;
using System;

namespace Do_An_Web_Hoc.Repositories
{
    public class EFLecturesRepository : ILecturesRepository
    {
        private readonly ApplicationDbContext _context;

        public EFLecturesRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Lấy danh sách tất cả các bài giảng
        public async Task<IEnumerable<Lectures>> GetAllLecturesAsync()
        {
            return await _context.Lectures.ToListAsync();
        }

        // Lấy thông tin chi tiết của một bài giảng theo ID
        public async Task<Lectures> GetLectureByIdAsync(int lectureId)
        {
            return await _context.Lectures.FindAsync(lectureId);
        }

        // Thêm một bài giảng mới
        public async Task AddLectureAsync(Lectures lecture)
        {
            await _context.Lectures.AddAsync(lecture);
            await _context.SaveChangesAsync();
        }

        // Cập nhật thông tin bài giảng
        public async Task UpdateLectureAsync(Lectures lecture)
        {
            _context.Lectures.Update(lecture);
            await _context.SaveChangesAsync();
        }

        // Xóa một bài giảng theo ID
        public async Task DeleteLectureAsync(int lectureId)
        {
            var lecture = await _context.Lectures.FindAsync(lectureId);
            if (lecture != null)
            {
                _context.Lectures.Remove(lecture);
                await _context.SaveChangesAsync();
            }
        }

        // Tìm kiếm bài giảng theo tiêu đề (tên)
        public async Task<IEnumerable<Lectures>> SearchLecturesByTitleAsync(string title)
        {
            return await _context.Lectures
                .Where(l => l.Title.Contains(title))
                .ToListAsync();
        }
    }
}
