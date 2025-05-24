using Do_An_Web_Hoc.Repositories.Interfaces;

namespace Do_An_Web_Hoc.Repositories
{
    public class BBBService : IBBBService
    {
        public Task<string> GenerateJoinUrlAsync(int courseId, string fullName)
        {
            // Tạo URL giả định (không có API, chỉ để test)
            string meetingID = $"course-{courseId}";
            string displayName = Uri.EscapeDataString(fullName);

            // Giả sử đây là trang BBB demo hoặc placeholder
            string fakeUrl = $"https://demo.bigbluebutton.org/gl?room={meetingID}&name={displayName}";

            return Task.FromResult(fakeUrl);
        }
    }
}
