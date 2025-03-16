using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Do_An_Web_Hoc.Models;
using Do_An_Web_Hoc.Repositories.Interfaces;

namespace Do_An_Web_Hoc.Repositories
{
    public class EFDocumentsRepository : IDocumentsRepository
    {
        private readonly ApplicationDbContext _context;

        public EFDocumentsRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Lấy tất cả tài liệu (cả hoạt động và ngừng hoạt động)
        public async Task<IEnumerable<Documents>> GetAllDocumentsAsync()
        {
            return await _context.Documents.ToListAsync();
        }

        // Lấy danh sách tài liệu chỉ bao gồm trạng thái hoạt động (Status = 1)
        public async Task<IEnumerable<Documents>> GetActiveDocumentsAsync()
        {
            return await _context.Documents
                .Where(d => d.Status == 1)
                .ToListAsync();
        }

        // Lấy tài liệu theo ID
        public async Task<Documents> GetDocumentByIdAsync(int documentId)
        {
            return await _context.Documents.FindAsync(documentId);
        }

        // Thêm tài liệu mới
        public async Task AddDocumentAsync(Documents document)
        {
            document.Status = 1; // Mặc định tài liệu được thêm sẽ ở trạng thái hoạt động
            await _context.Documents.AddAsync(document);
            await _context.SaveChangesAsync();
        }

        // Cập nhật thông tin tài liệu
        public async Task UpdateDocumentAsync(Documents document)
        {
            _context.Documents.Update(document);
            await _context.SaveChangesAsync();
        }

        // Xóa mềm tài liệu (chuyển trạng thái sang ngừng hoạt động)
        public async Task SoftDeleteDocumentAsync(int documentId)
        {
            var document = await _context.Documents.FindAsync(documentId);
            if (document != null)
            {
                document.Status = 2; // Đặt trạng thái thành ngừng hoạt động
                await _context.SaveChangesAsync();
            }
        }

        // Khôi phục tài liệu đã xóa mềm
        public async Task RestoreDocumentAsync(int documentId)
        {
            var document = await _context.Documents.FindAsync(documentId);
            if (document != null && document.Status == 2)
            {
                document.Status = 1; // Đặt trạng thái về hoạt động
                await _context.SaveChangesAsync();
            }
        }

        // Tìm kiếm tài liệu theo tiêu đề (chỉ tìm trong các tài liệu đang hoạt động)
        public async Task<IEnumerable<Documents>> SearchDocumentsByTitleAsync(string title)
        {
            return await _context.Documents
                .Where(d => d.Status == 1 && d.Title.Contains(title))
                .ToListAsync();
        }
    }
}
