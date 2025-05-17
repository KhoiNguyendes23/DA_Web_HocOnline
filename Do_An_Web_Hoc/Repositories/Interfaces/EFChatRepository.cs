namespace Do_An_Web_Hoc.Repositories.Interfaces
{
    public interface IChatRepository
    {
        Task SaveMessageAsync(int senderId, int receiverId, string? message, string? imageUrl);
        Task MarkMessagesAsReadAsync(int senderId, int receiverId);
    }
}
