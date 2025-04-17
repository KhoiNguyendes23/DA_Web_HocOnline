using Do_An_Web_Hoc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class ChatController : Controller
{
    private readonly ApplicationDbContext _context;

    public ChatController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var currentUserId = HttpContext.Session.GetInt32("UserID"); // Hoặc lấy từ Claims

        // Lấy tất cả người dùng trừ chính mình
        var users = await _context.UserAccounts
            .Where(u => u.UserID != currentUserId)
            .ToListAsync();

        ViewBag.CurrentUserId = currentUserId;
        ViewBag.Users = users;

        return View();
    }
}
