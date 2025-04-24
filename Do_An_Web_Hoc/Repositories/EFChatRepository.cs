using Do_An_Web_Hoc.Models;
using Do_An_Web_Hoc.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Do_An_Web_Hoc.Repositories
{
    public class EFChatRepository : IChatRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EFChatRepository> _logger;

        public EFChatRepository(ApplicationDbContext context, ILogger<EFChatRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task SaveMessageAsync(int senderId, int receiverId, string? message, string? imageUrl)
        {
            if (string.IsNullOrWhiteSpace(message) && string.IsNullOrWhiteSpace(imageUrl))
            {
                _logger.LogWarning("❗ Tin nhắn rỗng không được lưu");
                return;
            }

            var chat = new ChatMessage
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Message = string.IsNullOrWhiteSpace(message) ? null : message,
                ImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl,
                Timestamp = DateTime.Now,
                Sender = null,
                Receiver = null
            };

            _context.ChatMessages.Add(chat);
            await _context.SaveChangesAsync();
        }

        public async Task MarkMessagesAsReadAsync(int senderId, int receiverId)
        {
            var unreadMessages = await _context.ChatMessages
                .Where(m => m.SenderId == senderId && m.ReceiverId == receiverId && !m.IsRead)
                .ToListAsync();

            foreach (var message in unreadMessages)
            {
                message.IsRead = true;
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation($"✅ Đã đánh dấu {unreadMessages.Count} tin nhắn là đã đọc từ {senderId} đến {receiverId}");
        }
    }
}
