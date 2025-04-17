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

        public async Task SendMessage(int senderId, int receiverId, string message)
        {
            try
            {
                var chat = new ChatMessage
                {
                    SenderId = senderId,
                    ReceiverId = receiverId,
                    Message = message,
                    Timestamp = DateTime.Now,

                    //  QUAN TRỌNG: Nếu có Navigation Property thì phải gán null nếu không dùng
                    Sender = null,
                    Receiver = null
                };

                _context.ChatMessages.Add(chat);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Tin nhắn từ {senderId} đến {receiverId} đã lưu DB và gửi realtime");

                // Gửi realtime đến người nhận (nếu họ đang online)
                await Clients.User(receiverId.ToString()).SendAsync("ReceiveMessage", senderId, message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Lỗi khi gửi tin nhắn: {ex.Message}");
            }
        }
    }
}
