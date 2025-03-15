using Do_An_Web_Hoc.Models;
using Do_An_Web_Hoc.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
namespace Do_An_Web_Hoc { 
public class TestController : Controller
{
    private readonly ICategoryRepository _categoryRepository;

    public TestController(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

        // Hiển thị danh sách danh mục, người dùng có thể lọc xem các danh mục hoạt động hay ngừng hoạt động
        public async Task<IActionResult> Index(bool showAll = false)
        {
            // Gán giá trị cho ViewData để xác định tab nào đang được hiển thị
            ViewData["ShowAll"] = showAll;

            // Lấy danh sách danh mục từ Repository
            var categories = await _categoryRepository.GetAllAsync(showAll);
            return View(categories);
        }



        // Hiển thị form thêm danh mục
        public IActionResult Create()
    {
        return View();
    }

    // Xử lý thêm danh mục mới
    [HttpPost]
    public async Task<IActionResult> Create(Categories category)
    {
        if (ModelState.IsValid)
        {
            await _categoryRepository.AddAsync(category);
            return RedirectToAction("Index");
        }
        return View(category);
    }

    // Hiển thị form chỉnh sửa danh mục
    public async Task<IActionResult> Edit(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
        {
            return NotFound();
        }
        return View(category);
    }

    // Xử lý cập nhật danh mục
    [HttpPost]
    public async Task<IActionResult> Edit(int id, Categories category)
    {
        if (id != category.CategoryId)
        {
            return BadRequest();
        }

        if (ModelState.IsValid)
        {
            await _categoryRepository.UpdateAsync(category);
            return RedirectToAction("Index");
        }
        return View(category);
    }

    // Xử lý xóa danh mục (chuyển trạng thái thành "Ngừng hoạt động")
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
        {
            return NotFound();
        }
        await _categoryRepository.DeleteAsync(id);
        return RedirectToAction("Index");
    }

    // Khôi phục danh mục từ "Ngừng hoạt động" về "Hoạt động"
    public async Task<IActionResult> Restore(int id)
    {
        await _categoryRepository.RestoreAsync(id);
        return RedirectToAction("Index");
    }
}
}
