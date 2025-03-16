using System.Collections.Generic;
using System.Threading.Tasks;
using Do_An_Web_Hoc.Models;

namespace Do_An_Web_Hoc.Repositories.Interfaces
{
    public interface IDocumentsRepository
    {
        // Lấy danh sách tất cả các tài liệu (bao gồm cả hoạt động và ngừng hoạt động)
        Task<IEnumerable<Documents>> GetAllDocumentsAsync();

        // Lấy danh sách tài liệu chỉ bao gồm trạng thái hoạt động (Status = 1)
        Task<IEnumerable<Documents>> GetActiveDocumentsAsync();

        // Lấy thông tin chi tiết của một tài liệu dựa trên ID
        Task<Documents> GetDocumentByIdAsync(int documentId);

        // Thêm một tài liệu mới vào cơ sở dữ liệu
        Task AddDocumentAsync(Documents document);

        // Cập nhật thông tin của một tài liệu
        Task UpdateDocumentAsync(Documents document);

        // Xóa mềm một tài liệu (chuyển trạng thái thành ngừng hoạt động thay vì xóa khỏi DB)
        Task SoftDeleteDocumentAsync(int documentId);

        // Khôi phục tài liệu đã xóa mềm (đặt lại trạng thái thành hoạt động)
        Task RestoreDocumentAsync(int documentId);

        // Tìm kiếm tài liệu theo tiêu đề (chỉ tìm trong các tài liệu đang hoạt động)
        Task<IEnumerable<Documents>> SearchDocumentsByTitleAsync(string title);
    }
}
