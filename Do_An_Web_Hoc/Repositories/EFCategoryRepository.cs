using Do_An_Web_Hoc.Models;
using Do_An_Web_Hoc.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

public class EFCategoryRepository : ICategoryRepository
{
    private readonly ApplicationDbContext _context;

    public EFCategoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // Lấy tất cả danh mục dựa trên trạng thái "Hoạt động" hoặc "Ngừng hoạt động"
    public async Task<IEnumerable<Categories>> GetAllAsync(bool showAll)
    {
        if (showAll)
        {
            // Trả về tất cả các danh mục (bao gồm cả ngừng hoạt động)
            return await _context.Categories.ToListAsync();
        }
        else
        {
            // Chỉ trả về danh mục "Hoạt động"
            return await _context.Categories.Where(c => c.Status == 1).ToListAsync();
        }
    }

    // Lấy danh mục theo ID
    public async Task<Categories> GetByIdAsync(int id)
    {
        return await _context.Categories.FindAsync(id);
    }

    // Thêm danh mục mới
    public async Task AddAsync(Categories category)
    {
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();
    }

    // Cập nhật danh mục
    public async Task UpdateAsync(Categories category)
    {
        _context.Categories.Update(category);
        await _context.SaveChangesAsync();
    }

    // Xóa danh mục (Chuyển trạng thái thành "Ngừng hoạt động")
    public async Task DeleteAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category != null)
        {
            category.Status = 2;  // Đánh dấu trạng thái là "Ngừng hoạt động"
            await _context.SaveChangesAsync();
        }
    }

    // Khôi phục danh mục từ "Ngừng hoạt động" về "Hoạt động"
    public async Task RestoreAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category != null && category.Status == 2)
        {
            category.Status = 1;  // Khôi phục trạng thái thành "Hoạt động"
            await _context.SaveChangesAsync();
        }
    }
}
