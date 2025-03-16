using Do_An_Web_Hoc.Models;

namespace Do_An_Web_Hoc.Repositories.Interfaces
{
    public interface ICatogoriesRepository
    {
        // lấy tất cả danh mục khóa học
        Task<IEnumerable<Categories>> GetAllCategoriesAsync();
        // lấy danh mục theo ID
        Task<Categories> GetCategoryByIdAsync(int categoryId);
        // thêm danh mục mới
        Task AddCategoryAsync(Categories category);
        // cập nhật thông tin danh mục
        Task<bool> UpdateCategoryAsync(Categories category);
        // xóa danh mục
        Task DeleteCategoryAsync(int categoryId);
        // Tìm kiếm danh mục theo tên
        Task<IEnumerable<Categories>> SearchCategoriesByNameAsync(string keyword);
        // xóa mềm danh mục
        Task<bool> SoftDeleteCategoryAsync(int categoryId);


    }
}
