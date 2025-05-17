using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.Configuration;

namespace Do_An_Web_Hoc.Services
{
    public class BigBlueButtonService : IBigBlueButtonService
    {
        private readonly string _baseUrl;
        private readonly string _secret;
        private readonly HttpClient _httpClient;

        public BigBlueButtonService(IConfiguration config)
        {
            _baseUrl = config["BigBlueButton:BaseUrl"];
            _secret = config["BigBlueButton:Secret"];
            _httpClient = new HttpClient();
        }

        // Tạo cuộc họp mới trên BBB
        public async Task<bool> CreateMeetingAsync(string meetingID, string meetingName, string moderatorPassword, string attendeePassword)
        {
            var query = $"name={HttpUtility.UrlEncode(meetingName)}" +
                        $"&meetingID={HttpUtility.UrlEncode(meetingID)}" +
                        $"&attendeePW={attendeePassword}" +
                        $"&moderatorPW={moderatorPassword}";

            var checksum = GenerateChecksum("create", query);
            var url = $"{_baseUrl}/create?{query}&checksum={checksum}";

            var response = await _httpClient.GetAsync(url);
            return response.IsSuccessStatusCode;
        }

        // Tạo URL để người học hoặc giảng viên tham gia cuộc họp
        public string GetJoinMeetingUrl(string meetingID, string fullName, string password)
        {
            var query = $"fullName={HttpUtility.UrlEncode(fullName)}" +
                        $"&meetingID={HttpUtility.UrlEncode(meetingID)}" +
                        $"&password={HttpUtility.UrlEncode(password)}";

            var checksum = GenerateChecksum("join", query);
            return $"{_baseUrl}/join?{query}&checksum={checksum}";
        }

        // Tính checksum bảo mật theo yêu cầu của BBB
        private string GenerateChecksum(string action, string query)
        {
            var data = action + query + _secret;
            using var sha1 = SHA1.Create();
            var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(data));
            var sb = new StringBuilder();
            foreach (var b in hash) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }
}
