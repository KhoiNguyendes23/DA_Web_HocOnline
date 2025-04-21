using Microsoft.AspNetCore.SignalR;
using Do_An_Web_Hoc.Models;
using Microsoft.Extensions.Logging;

namespace Do_An_Web_Hoc.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(ApplicationDbContext context, ILogger<ChatHub> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ✅ Gửi tin nhắn và/hoặc ảnh
        public async Task SendMessageFull(int senderId, int receiverId, string message, string imageUrl)
        {
            try
            {
                // Nếu cả tin nhắn và ảnh đều trống thì không xử lý
                if (string.IsNullOrWhiteSpace(message) && string.IsNullOrWhiteSpace(imageUrl))
                {
                    _logger.LogWarning("⚠️ Bỏ qua tin nhắn trống từ " + senderId);
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

                _logger.LogInformation($"✅ [SendMessageFull] {senderId} → {receiverId} đã lưu DB");

                await Clients.User(receiverId.ToString())
                    .SendAsync("ReceiveMessageFull", senderId, chat.Message, chat.ImageUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Lỗi tại SendMessageFull: {ex.Message}");
            }
        }
    }
}
