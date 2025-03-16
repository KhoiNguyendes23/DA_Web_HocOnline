using Do_An_Web_Hoc.Models;
using Do_An_Web_Hoc.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Do_An_Web_Hoc.Repositories
{
    public class EFCatogoriesRepository : ICatogoriesRepository
    {
        private readonly ApplicationDbContext _context;


        public EFCatogoriesRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        // lấy tất cả danh mục khóa học
        public async Task<IEnumerable<Categories>> GetAllCategoriesAsync()
        {
            return await _context.Categories
                .Where(c => c.Status == 1) // 1: Hoạt động
                .ToListAsync();
        }
        // Lấy danh mục theo ID
        public async Task<Categories> GetCategoryByIdAsync(int categoryId)
        {
            return await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == categoryId);
        }
        // Thêm danh mục mới
        public async Task AddCategoryAsync(Categories category)
        {
            category.Status = 1; // Khi thêm mới, trạng thái mặc định là hoạt động
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
        }
        // Cập nhật thông tin danh mục
        public async Task<bool> UpdateCategoryAsync(Categories category)
        {
            var existingCategory = await _context.Categories.FindAsync(category.CategoryId);
            if (existingCategory == null)
            {
                return false;
            }

            existingCategory.CategoryName = category.CategoryName;
            existingCategory.Status = category.Status;
            existingCategory.Status = category.Status;
            await _context.SaveChangesAsync();
            return true;
        }
        // Xóa danh mục (xóa vĩnh viễn)
        public async Task DeleteCategoryAsync(int categoryId)
        {
            var category = await _context.Categories.FindAsync(categoryId);
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }
        }
        // Tìm kiếm danh mục theo tên
        public async Task<IEnumerable<Categories>> SearchCategoriesByNameAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return await GetAllCategoriesAsync();
            }

            return await _context.Categories
                .Where(c => c.CategoryName.Contains(keyword) && c.Status == 1)
                .ToListAsync();
        }
        // Xóa mềm danh mục (đặt trạng thái là ngừng hoạt động)
        public async Task<bool> SoftDeleteCategoryAsync(int categoryId)
        {
            var category = await _context.Categories.FindAsync(categoryId);
            if (category == null)
            {
                return false;
            }

            category.Status = 2; // 2: Ngừng hoạt động
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
