using Microsoft.AspNetCore.SignalR;
using Do_An_Web_Hoc.Models;

namespace Do_An_Web_Hoc.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;

        public ChatHub(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SendMessage(int senderId, int receiverId, string message)
        {
            var chat = new ChatMessage
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Message = message
            };

            _context.ChatMessages.Add(chat);
            await _context.SaveChangesAsync();

            await Clients.User(receiverId.ToString()).SendAsync("ReceiveMessage", senderId, message);
        }
    }
}
