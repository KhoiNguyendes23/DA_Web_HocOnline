using Do_An_Web_Hoc.Models;
using System.Text;
using System.Text.Json;

using Do_An_Web_Hoc.Services.Interfaces;

namespace Do_An_Web_Hoc.Services.Odoo
{
    public class OdooEnrollmentService : IOdooEnrollmentService

    {
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;
        private readonly string _url;
        private readonly string _db;
        private readonly string _username;
        private readonly string _password;

        public OdooEnrollmentService(IConfiguration config)
        {
            _config = config;
            _httpClient = new HttpClient();
            _url = _config["Odoo:Url"]!;
            _db = _config["Odoo:Db"]!;
            _username = _config["Odoo:Username"]!;
            _password = _config["Odoo:Password"]!;
        }

        private async Task<int?> LoginAsync()
        {
            var loginPayload = new
            {
                jsonrpc = "2.0",
                method = "call",
                @params = new
                {
                    db = _db,
                    login = _username,
                    password = _password
                }
            };

            var response = await _httpClient.PostAsync($"{_url}/web/session/authenticate",
                new StringContent(JsonSerializer.Serialize(loginPayload), Encoding.UTF8, "application/json"));
            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);

            return doc.RootElement.GetProperty("result").GetProperty("uid").GetInt32();
        }

        public async Task<int?> CreateEnrollmentAsync(int studentOdooId, int courseOdooId)
        {
            int? uid = await LoginAsync();
            if (uid == null || uid == 0) return null;

            var payload = new
            {
                jsonrpc = "2.0",
                method = "call",
                @params = new
                {
                    service = "object",
                    method = "execute_kw",
                    args = new object[]
                    {
                        _db, uid, _password,
                        "elearning.enrollment", "create",
                        new object[]
                        {
                            new Dictionary<string, object>
                            {
                                { "student_id", studentOdooId },
                                { "course_id", courseOdooId },
                                { "enrollment_date", DateTime.UtcNow.ToString("yyyy-MM-dd") },
                                { "is_paid", true },
                                { "status", "enrolled" }
                            }
                        }
                    }
                }
            };

            var response = await _httpClient.PostAsync($"{_url}/jsonrpc",
                new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"));
            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);

            return doc.RootElement.TryGetProperty("result", out var result) ? result.GetInt32() : null;
        }

        public async Task<int?> SearchEnrollmentAsync(int studentOdooId, int courseOdooId)
        {
            int? uid = await LoginAsync();
            if (uid == null || uid == 0) return null;

            var payload = new
            {
                jsonrpc = "2.0",
                method = "call",
                @params = new
                {
                    service = "object",
                    method = "execute_kw",
                    args = new object[]
                    {
                        _db, uid, _password,
                        "elearning.enrollment", "search",
                        new object[]
                        {
                            new object[]
                            {
                                new object[] { "student_id", "=", studentOdooId },
                                new object[] { "course_id", "=", courseOdooId }
                            }
                        }
                    }
                }
            };

            var response = await _httpClient.PostAsync($"{_url}/jsonrpc",
                new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"));
            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("result", out var result)
                && result.ValueKind == JsonValueKind.Array
                && result.GetArrayLength() > 0)
            {
                return result[0].GetInt32();
            }

            return null;
        }

        public async Task<int> SyncAllEnrollmentsToOdooAsync(IEnumerable<Enrollments> enrollments)
        {
            int successCount = 0;
            int? uid = await LoginAsync();
            if (uid == null || uid == 0) return 0;

            foreach (var enrollment in enrollments)
            {
                if (enrollment.UserID == null || enrollment.CourseID == null) continue;

                // Lấy Odoo ID thực từ các service phụ
                var partnerService = new OdooPartnerService(_config);
                var courseService = new OdooCourseService(_config);

                var studentOdooId = await partnerService.SearchPartnerByEmailAsync(enrollment.User?.Email ?? "");
                var courseOdooId = await courseService.SearchCourseByNameAsync(enrollment.Course?.CourseName ?? "");

                if (studentOdooId == null || courseOdooId == null) continue;

                var exists = await SearchEnrollmentAsync(studentOdooId.Value, courseOdooId.Value);
                if (exists == null)
                {
                    var result = await CreateEnrollmentAsync(studentOdooId.Value, courseOdooId.Value);
                    if (result != null) successCount++;
                }
            }

            return successCount;
        }

        public async Task<bool> UpdateEnrollmentAsync(int enrollmentId, Dictionary<string, object> fieldsToUpdate)
        {
            int? uid = await LoginAsync();
            if (uid == null || uid == 0) return false;

            var payload = new
            {
                jsonrpc = "2.0",
                method = "call",
                @params = new
                {
                    service = "object",
                    method = "execute_kw",
                    args = new object[]
                    {
                        _db, uid, _password,
                        "elearning.enrollment", "write",
                        new object[] { new int[] { enrollmentId }, fieldsToUpdate }
                    }
                }
            };

            var response = await _httpClient.PostAsync($"{_url}/jsonrpc",
                new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"));
            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);

            return doc.RootElement.TryGetProperty("result", out var result) && result.GetBoolean();
        }
    }
}
