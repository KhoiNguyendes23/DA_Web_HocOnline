namespace Do_An_Web_Hoc.Repositories.Interfaces
{
    public interface IBBBService
    {
        Task<string> GenerateJoinUrlAsync(int courseId, string fullName);
    }
}
