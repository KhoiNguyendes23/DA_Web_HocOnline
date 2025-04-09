using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Do_An_Web_Hoc.Models;
using Do_An_Web_Hoc.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Do_An_Web_Hoc.Repositories
{
    public class EFLecturesRepository : ILecturesRepository
    {
        private readonly ApplicationDbContext _context;

        public EFLecturesRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Lectures>> GetAllLecturesAsync()
        {
            var query = from l in _context.Lectures
                        join c in _context.Courses on l.CourseID equals c.CourseID
                        join cat in _context.Categories on c.CategoryID equals cat.CategoryId
                        where l.Status == 1 && c.Status == 1 && cat.Status == 1
                        select l;

            return await query.ToListAsync();
        }




        public async Task<Lectures> GetLectureByIdAsync(int lectureId)
        {
            return await _context.Lectures.FindAsync(lectureId);
        }

        public async Task AddLectureAsync(Lectures lecture)
        {
            lecture.CreateAt = DateTime.Now;
            _context.Lectures.Add(lecture);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateLectureAsync(Lectures lecture)
        {
            _context.Lectures.Update(lecture);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteLectureAsync(int lectureId)
        {
            var lecture = await _context.Lectures.FindAsync(lectureId);
            if (lecture != null)
            {
                _context.Lectures.Remove(lecture);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Lectures>> SearchLecturesByTitleAsync(string title)
        {
            return await _context.Lectures
                .Where(l => l.Title.Contains(title))
                .ToListAsync();
        }

        public async Task<IEnumerable<Lectures>> GetLecturesByCourseIdAsync(int courseId)
        {
            return await _context.Lectures
                .Where(l => l.CourseID == courseId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Lectures>> GetRecentLecturesAsync(int count)
        {
            return await _context.Lectures
                .OrderByDescending(l => l.CreateAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<bool> LectureExistsAsync(int lectureId)
        {
            return await _context.Lectures.AnyAsync(l => l.LectureID == lectureId);
        }

        public async Task<int> CountLecturesAsync()
        {
            return await _context.Lectures.CountAsync();
        }

        public async Task<IEnumerable<Lectures>> GetLecturesPagedAsync(int pageIndex, int pageSize)
        {
            return await _context.Lectures
                .OrderByDescending(l => l.CreateAt)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task UpdateLecturerProfileAsync(Lectures lecturer)
        {
            _context.Lectures.Update(lecturer);
            await _context.SaveChangesAsync();
        }
        public async Task<IEnumerable<Lectures>> GetLecturesWithActiveCoursesAsync()
        {
            return await (from l in _context.Lectures
                          join c in _context.Courses on l.CourseID equals c.CourseID
                          join cat in _context.Categories on c.CategoryID equals cat.CategoryId
                          where c.Status == 1 && cat.Status == 1
                          select l).ToListAsync();
        }
    }
}
