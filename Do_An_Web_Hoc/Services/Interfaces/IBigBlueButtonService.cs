namespace Do_An_Web_Hoc.Services
{
    public interface IBigBlueButtonService
    {
        Task<bool> CreateMeetingAsync(string meetingID, string meetingName, string moderatorPassword, string attendeePassword);
        string GetJoinMeetingUrl(string meetingID, string fullName, string password);
    }
}
