using System.Text.Json;
using System.Text;
using Do_An_Web_Hoc.Services.Interfaces;

namespace Do_An_Web_Hoc.Services.Odoo
{
    public class OdooCourseService:IOdooCourseService
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;
        private readonly string _url;
        private readonly string _db;
        private readonly string _username;
        private readonly string _password;

        public OdooCourseService(IConfiguration config)
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

        public async Task<int?> CreateCourseAsync(string courseName, string description)
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
                        "elearning.course", "create",
                        new object[]
                        {
                            new Dictionary<string, object>
                            {
                                { "name", courseName },
                                { "description", description }
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

        public async Task<int?> SearchCourseByNameAsync(string courseName)
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
                        "elearning.course", "search",
                        new object[]
                        {
                            new object[]
                            {
                                new object[] { "name", "=", courseName }
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

        public async Task<bool> UpdateCourseAsync(int courseId, Dictionary<string, object> fieldsToUpdate)
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
                        "elearning.course", "write",
                        new object[]
                        {
                            new int[] { courseId },
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
