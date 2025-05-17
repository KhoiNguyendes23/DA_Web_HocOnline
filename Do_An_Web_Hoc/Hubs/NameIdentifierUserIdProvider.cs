using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Do_An_Web_Hoc.Hubs
{
    public class NameIdentifierUserIdProvider : IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection)
        {
            // Lấy userId từ Claims (chính là User.Identity.Name hoặc NameIdentifier)
            return connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
