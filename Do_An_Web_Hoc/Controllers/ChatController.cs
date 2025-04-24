using Do_An_Web_Hoc.Models;
using Do_An_Web_Hoc.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Do_An_Web_Hoc.Controllers
{
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IChatRepository _chatRepository;
        public ChatController(ApplicationDbContext context, IChatRepository chatRepository)
        {
            _context = context;
            _chatRepository = chatRepository;
        }

        // Giao diện Chat chính
        public async Task<IActionResult> Index()
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
                return RedirectToAction("Login", "Account");

            var currentUser = await _context.UserAccounts.FindAsync(currentUserId.Value);
            if (currentUser == null)
                return RedirectToAction("Login", "Account");

            // ✅ Gán thông tin người dùng cho layout
            ViewData["FullName"] = currentUser.FullName;
            ViewData["RoleName"] = GetRoleName(currentUser.RoleID ?? 0);
            ViewData["ImagePath"] = currentUser.Image;
            ViewData["UserID"] = currentUser.UserID;

            // ✅ Gán layout theo role
            ViewData["Layout"] = (currentUser.RoleID ?? 0) switch
            {
                1 => "_LayoutAdmin",
                2 => "_LayoutLecturer",
                3 => "_LayoutUsers",     // học viên dùng layout này
                _ => "_Layout"           // fallback mặc định
            };

            // Danh sách người để chat
            var users = await _context.UserAccounts
                .Where(u => u.UserID != currentUser.UserID)
                .ToListAsync();

            ViewBag.CurrentUserId = currentUser.UserID;
            ViewBag.Users = users;

            return View();
        }


        // API: Lấy lịch sử tin nhắn giữa 2 người
        [HttpGet("api/chat/messages")]
        public async Task<IActionResult> GetMessages(int senderId, int receiverId)
        {
            var messages = await _context.ChatMessages
                .Where(m =>
                    (m.SenderId == senderId && m.ReceiverId == receiverId) ||
                    (m.SenderId == receiverId && m.ReceiverId == senderId))
                .OrderBy(m => m.Timestamp)
                .ToListAsync();

            return Json(messages);
        }

        // API: Trả danh sách người nhận (có thể tùy theo vai trò)
        [HttpGet("api/chat/receivers")]
        public async Task<IActionResult> GetReceivers()
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
                return Unauthorized();

            var currentUser = await _context.UserAccounts.FindAsync(currentUserId.Value);
            if (currentUser == null)
                return Unauthorized();

            IQueryable<UserAccount> query = _context.UserAccounts.Where(u => u.UserID != currentUser.UserID);

            // Học viên chỉ được chat với giảng viên
            if (currentUser.RoleID == 3)
            {
                query = query.Where(u => u.RoleID == 2);
            }

            var users = await query.Select(u => new
            {
                userID = u.UserID,
                fullName = u.FullName,
                roleID = u.RoleID
            }).ToListAsync();

            return Json(users);
        }

        // Helper: Lấy ID người dùng từ Claims
        private int? GetCurrentUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userIdStr, out var userId) ? userId : null;
        }

        // Helper: Đổi RoleID thành tên
        private string GetRoleName(int roleId)
        {
            return roleId switch
            {
                1 => "Admin",
                2 => "Giảng viên",
                3 => "Học viên",
                _ => "Không xác định"
            };
        }


        //[HttpGet]
        //public async Task<IActionResult> TestInsertMessage()
        //{
        //    var chat = new ChatMessage
        //    {
        //        SenderId = 1, // ID người gửi có tồn tại
        //        ReceiverId = 2, // ID người nhận có tồn tại
        //        Message = "Tin nhắn test thủ công",
        //        Timestamp = DateTime.Now,
        //        IsRead = false
        //    };

        //    _context.ChatMessages.Add(chat);
        //    await _context.SaveChangesAsync();

        //    return Content("✅ Lưu thành công!");
        //}


        [HttpPost("api/chat/upload")]
        public async Task<IActionResult> UploadImage(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                return BadRequest("Không có ảnh được gửi");

            try
            {
                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(imageFile.FileName)}";
                var imagePath = Path.Combine("wwwroot/images/Chat_images", fileName);

                // Lưu file vào thư mục
                using (var stream = new FileStream(imagePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                // Đường dẫn public để client truy cập ảnh
                var imageUrl = $"/images/Chat_images/{fileName}";
                return Ok(new { imageUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi khi upload ảnh: " + ex.Message);
            }
        }
        [HttpPost("api/chat/mark-as-read")]
        public async Task<IActionResult> MarkAsRead(int senderId, int receiverId)
        {
            await _chatRepository.MarkMessagesAsReadAsync(senderId, receiverId);
            return Ok("✅ Đã đánh dấu đã đọc");
        }
    }
}
