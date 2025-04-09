using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Do_An_Web_Hoc.Models;
using Do_An_Web_Hoc.Repositories.Interfaces;
using ModelsResults = Do_An_Web_Hoc.Models.Results;
using Do_An_Web_Hoc.Models.ViewModels; // Khắc phục lỗi xung đột namespace

namespace Do_An_Web_Hoc.Repositories
{
    public class EFResultsRepository : IResultsRepository
    {
        private readonly ApplicationDbContext _context;

        public EFResultsRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Lấy tất cả kết quả (Chỉ Admin mới có quyền)
        public async Task<IEnumerable<ModelsResults>> GetAllResultsAsync(int userRoleId)
        {
            if (userRoleId == 1) // Chỉ Admin có quyền xem tất cả kết quả
            {
                return await _context.Results.ToListAsync();
            }
            return new List<ModelsResults>(); // Trả về danh sách rỗng nếu không phải Admin
        }

        // Lấy danh sách kết quả theo UserID
        public async Task<IEnumerable<ModelsResults>> GetResultsByUserIdAsync(int userId, int userRoleId)
        {
            return await _context.Results
                .Where(r => userRoleId == 1 || r.UserID == userId) // Admin xem tất cả, học viên chỉ xem của mình
                .ToListAsync();
        }

        // Lấy danh sách kết quả theo QuizID
        public async Task<IEnumerable<ModelsResults>> GetResultsByQuizIdAsync(int quizId)
        {
            return await _context.Results
                .Where(r => r.QuizID == quizId)
                .ToListAsync();
        }

        // Lấy thông tin chi tiết của một kết quả dựa trên ID
        public async Task<ModelsResults> GetResultByIdAsync(int resultId)
        {
            return await _context.Results.FindAsync(resultId);
        }

        // Thêm kết quả mới (Chỉ Admin hoặc Giáo viên mới có thể thêm)
        public async Task AddResultAsync(ModelsResults result, int userRoleId)
        {
            if (userRoleId == 1 || userRoleId == 2) // Chỉ Admin (1) hoặc Giáo viên (2) mới có thể thêm kết quả
            {
                await _context.Results.AddAsync(result);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new UnauthorizedAccessException("Bạn không có quyền thêm kết quả.");
            }
        }

        // Cập nhật điểm số (Chỉ Admin hoặc Giáo viên mới có thể chỉnh sửa)
        public async Task UpdateResultAsync(ModelsResults result, int userRoleId)
        {
            if (userRoleId == 1 || userRoleId == 2) // Chỉ Admin hoặc Giáo viên có quyền chỉnh sửa
            {
                _context.Results.Update(result);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new UnauthorizedAccessException("Bạn không có quyền chỉnh sửa kết quả.");
            }
        }

        // Xóa kết quả (Chỉ Admin mới có thể xóa)
        public async Task DeleteResultAsync(int resultId, int userRoleId)
        {
            if (userRoleId == 1) // Chỉ Admin mới có thể xóa kết quả
            {
                var result = await _context.Results.FindAsync(resultId);
                if (result != null)
                {
                    _context.Results.Remove(result);
                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                throw new UnauthorizedAccessException("Bạn không có quyền xóa kết quả.");
            }
        }
        public async Task SaveResultFromUserAsync(ModelsResults result)
        {
            await _context.Results.AddAsync(result);
            await _context.SaveChangesAsync();
        }
        public async Task<IEnumerable<RankingViewModel>> GetUserRankingAsync()
        {
            var rankings = await (from r in _context.Results
                                  join u in _context.UserAccounts on r.UserID equals u.UserID
                                  group r by new { r.UserID, u.FullName } into g
                                  select new RankingViewModel
                                  {
                                      UserID = g.Key.UserID ?? 0, // ép kiểu int? → int
                                      FullName = g.Key.FullName,
                                      TotalScore = (int)(g.Sum(x => x.Score ?? 0)), // xử lý null + ép float? → int
                                      QuizCount = g.Count()
                                  })
                                 .OrderByDescending(r => r.TotalScore)
                                 .ThenBy(r => r.FullName)
                                 .ToListAsync();

            return rankings;
        }
        public async Task<IEnumerable<ResultExamViewModel>> GetExamResultsForLecturerAsync()
        {
            var results = await (from r in _context.Results
                                 join u in _context.UserAccounts on r.UserID equals u.UserID
                                 join q in _context.Quizzes on r.QuizID equals q.QuizID
                                 join e in _context.Exams on q.ExamID equals e.ExamID
                                 select new ResultExamViewModel
                                 {
                                     StudentName = u.FullName,
                                     ExamName = e.ExamName,
                                     QuizName = q.QuizName,
                                     Score = r.Score,
                                     SubmissionTime = r.SubmissionTime
                                 })
                                 .OrderByDescending(r => r.SubmissionTime)
                                 .ToListAsync();

            return results;
        }
    }
}
