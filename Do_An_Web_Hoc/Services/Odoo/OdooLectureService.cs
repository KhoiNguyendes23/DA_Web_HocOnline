using System.Text.Json;
using System.Text;
using Do_An_Web_Hoc.Services.Interfaces;

namespace Do_An_Web_Hoc.Services.Odoo
{
    public class OdooLectureService:IOdooLectureService
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;
        private readonly string _url;
        private readonly string _db;
        private readonly string _username;
        private readonly string _password;

        public OdooLectureService(IConfiguration config)
        {
            _config = config;
            _httpClient = new HttpClient();
            _url = _config["Odoo:Url"];
            _db = _config["Odoo:Db"];
            _username = _config["Odoo:Username"];
            _password = _config["Odoo:Password"];
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

            var loginResponse = await _httpClient.PostAsync($"{_url}/web/session/authenticate",
                new StringContent(JsonSerializer.Serialize(loginPayload), Encoding.UTF8, "application/json"));

            var loginJson = await loginResponse.Content.ReadAsStringAsync();
            var loginDoc = JsonDocument.Parse(loginJson);

            return loginDoc.RootElement.GetProperty("result").GetProperty("uid").GetInt32();
        }

        public async Task<int?> CreateLectureAsync(string title, string content, string videoUrl, int courseId)
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
                        "elearning.lecture", "create",
                        new object[]
                        {
                            new Dictionary<string, object>
                            {
                                { "title", title },
                                { "content", content },
                                { "video_url", videoUrl },
                                { "course_id", courseId }
                            }
                        }
                    }
                }
            };

            var response = await _httpClient.PostAsync($"{_url}/jsonrpc",
                new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"));
            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("result", out var result))
            {
                return result.GetInt32();
            }

            return null;
        }

        public async Task<int?> SearchLectureByTitleAsync(string title)
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
                        "elearning.lecture", "search",
                        new object[]
                        {
                            new object[]
                            {
                                new object[] { "title", "=", title }
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

        public async Task<bool> UpdateLectureAsync(int lectureId, Dictionary<string, object> fieldsToUpdate)
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
                        "elearning.lecture", "write",
                        new object[]
                        {
                            new int[] { lectureId },
                            fieldsToUpdate
                        }
                    }
                }
            };

            var response = await _httpClient.PostAsync($"{_url}/jsonrpc",
                new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"));
            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("result", out var result))
            {
                return result.GetBoolean();
            }

            return false;
        }
    }
}
