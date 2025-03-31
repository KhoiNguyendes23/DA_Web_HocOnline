using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Do_An_Web_Hoc.Models;
using Do_An_Web_Hoc.Repositories.Interfaces;

namespace Do_An_Web_Hoc.Repositories
{
    public class EFExamsRepository : IExamsRepository
    {
        private readonly ApplicationDbContext _context;

        public EFExamsRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        //public void AddExam(Exams exam)
        //{
        //    _context.Exams.Add(exam);
        //    _context.SaveChanges();
        //}

        // Lấy tất cả bài thi (cả hoạt động và ngừng hoạt động)
        public async Task<IEnumerable<Exams>> GetAllExamsAsync()
        {
            return await _context.Exams.ToListAsync();
        }

        // Lấy danh sách bài thi chỉ bao gồm trạng thái hoạt động (Status = 1)
        public async Task<IEnumerable<Exams>> GetActiveExamsAsync()
        {
            return await _context.Exams
                .Where(e => e.Status == 1)
                .ToListAsync();
        }

        // Lấy danh sách bài thi theo khóa học
        public async Task<IEnumerable<Exams>> GetExamsByCourseIdAsync(int courseId)
        {
            return await _context.Exams
                .Where(e => e.CourseID == courseId && e.Status == 1)
                .ToListAsync();
        }

        // Lấy thông tin chi tiết của một bài thi theo ID
        public async Task<Exams> GetExamByIdAsync(int examId)
        {
            return await _context.Exams.FindAsync(examId);
        }

        // Thêm một bài thi mới
        public async Task AddExamAsync(Exams exam)
        {
            exam.Status = 1; // Mặc định bài thi khi thêm sẽ ở trạng thái hoạt động
            await _context.Exams.AddAsync(exam);
            await _context.SaveChangesAsync();
        }

        // Cập nhật thông tin bài thi
        public async Task UpdateExamAsync(Exams exam)
        {
            _context.Exams.Update(exam);
            await _context.SaveChangesAsync();
        }

        // Xóa mềm bài thi (chuyển trạng thái sang ngừng hoạt động)
        public async Task SoftDeleteExamAsync(int examId)
        {
            var exam = await _context.Exams.FindAsync(examId);
            if (exam != null && exam.Status == 1)
            {
                exam.Status = 2;
                await _context.SaveChangesAsync();
            }

        }

        // Khôi phục bài thi đã xóa mềm
        public async Task RestoreExamAsync(int examId)
        {
            var exam = await _context.Exams.FindAsync(examId);
            if (exam != null && exam.Status == 2)
            {
                exam.Status = 1; // Đặt trạng thái về hoạt động
                await _context.SaveChangesAsync();
            }
        }

        // Tìm kiếm bài thi theo tên (chỉ tìm trong các bài thi đang hoạt động)
        public async Task<IEnumerable<Exams>> SearchExamsByNameAsync(string examName)
        {
            return await _context.Exams
                .Where(e => e.Status == 1 && e.ExamName.Contains(examName))
                .ToListAsync();
        }
    }
}
