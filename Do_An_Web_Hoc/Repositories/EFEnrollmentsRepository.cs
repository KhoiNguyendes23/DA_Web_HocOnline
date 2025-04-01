using Do_An_Web_Hoc.Models;
using Do_An_Web_Hoc.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Do_An_Web_Hoc.Repositories
{
    public class EFEnrollmentsRepository : IEnrollmentsRepository
    {
        private readonly ApplicationDbContext _context;

        public EFEnrollmentsRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddEnrollmentAsync(Enrollments enrollment)
        {
            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsUserEnrolledAsync(int userId, int courseId)
        {
            return await _context.Enrollments
                .AnyAsync(e => e.UserID == userId && e.CourseID == courseId);
        }

        // ✅ Lấy danh sách khóa học người dùng đã đăng ký
        public async Task<IEnumerable<Enrollments>> GetEnrollmentsByUserAsync(int userId)
        {
            return await _context.Enrollments
                .Where(e => e.UserID == userId)
                .ToListAsync();
        }

        // ✅ Hủy đăng ký
        public async Task RemoveEnrollmentAsync(int userId, int courseId)
        {
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.UserID == userId && e.CourseID == courseId);

            if (enrollment != null)
            {
                _context.Enrollments.Remove(enrollment);
                await _context.SaveChangesAsync();
            }
        }

        // ✅ Cập nhật trạng thái hoàn thành
        public async Task UpdateCompletionStatusAsync(int enrollmentId, bool isCompleted)
        {
            var enrollment = await _context.Enrollments.FindAsync(enrollmentId);
            if (enrollment != null)
            {
                enrollment.CompletionStatus = isCompleted;
                await _context.SaveChangesAsync();
            }
        }

        // ✅ Thống kê số lượng học viên đã đăng ký một khóa học
        public async Task<int> CountEnrollmentsForCourseAsync(int courseId)
        {
            return await _context.Enrollments
                .CountAsync(e => e.CourseID == courseId);
        }
        // Thống Kê Doanh Thu

        public async Task<IEnumerable<RevenueStatisticViewModel>> GetMonthlyRevenueStatisticsAsync()
        {
            var data = await _context.Enrollments
                .Where(e => e.IsPaid && e.PaymentDate.HasValue)
                .Include(e => e.Course) // <- dùng Include để lấy luôn khóa học
                .GroupBy(e => new
                {
                    Year = e.PaymentDate.Value.Year,
                    Month = e.PaymentDate.Value.Month
                })
                .Select(g => new RevenueStatisticViewModel
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalEnrollments = g.Count(),
                    TotalRevenue = g.Sum(e => e.Course.Price ?? 0) // lấy giá từ navigation property
                })
                .OrderByDescending(x => x.Year)
                .ThenByDescending(x => x.Month)
                .ToListAsync();

            return data;
        }


    }
}
