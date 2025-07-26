using Do_An_Web_Hoc.Repositories.Interfaces;

namespace Do_An_Web_Hoc.Repositories
{
    public class BBBService : IBBBService
    {
        public Task<string> GenerateJoinUrlAsync(int courseId, string fullName)
        {
            // Nhúng link session trực tiếp (chỉ dùng test)
            string directUrl = "https://demo6.bigbluebutton.org/html5client/?sessionToken=sii3g3ynoac3tenq";
            return Task.FromResult(directUrl);
        }
    }
}
