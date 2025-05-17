using Microsoft.AspNetCore.SignalR;
using Do_An_Web_Hoc.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace Do_An_Web_Hoc.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IChatRepository _chatRepository;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(IChatRepository chatRepository, ILogger<ChatHub> logger)
        {
            _chatRepository = chatRepository;
            _logger = logger;
        }

        // Gửi tin nhắn và/hoặc ảnh
        public async Task SendMessageFull(int senderId, int receiverId, string message, string imageUrl)
        {
            try
            {
                // Nếu cả message và image đều trống → không gửi
                if (string.IsNullOrWhiteSpace(message) && string.IsNullOrWhiteSpace(imageUrl))
                {
                    _logger.LogWarning($"⚠️ Tin nhắn rỗng không được gửi từ {senderId} đến {receiverId}");
                    return;
                }

                // Gọi repository để lưu
                await _chatRepository.SaveMessageAsync(senderId, receiverId, message, imageUrl);

                // Gửi realtime
                await Clients.User(receiverId.ToString())
                    .SendAsync("ReceiveMessageFull", senderId, message, imageUrl);

                _logger.LogInformation($"✅ Tin nhắn gửi thành công từ {senderId} → {receiverId}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Lỗi tại SendMessageFull: {ex.Message}");
            }
        }
    }
}
