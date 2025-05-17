using System.Text;
using System.Text.Json;
using Do_An_Web_Hoc.Services.Interfaces;

namespace Do_An_Web_Hoc.Services.Odoo
{
    public class OdooRoleService : IOdooRoleService
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;
        private readonly string _url;
        private readonly string _db;
        private readonly string _username;
        private readonly string _password;

        public OdooRoleService(IConfiguration config)
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

            var response = await _httpClient.PostAsync(
                $"{_url}/web/session/authenticate",
                new StringContent(JsonSerializer.Serialize(loginPayload), Encoding.UTF8, "application/json"));

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("result", out var resultElement) &&
                resultElement.TryGetProperty("uid", out var uidElement))
            {
                return uidElement.GetInt32();
            }

            Console.WriteLine("❌ Login to Odoo thất bại.");
            return null;
        }

        public async Task<int?> GetOrCreateRoleAsync(string roleName, int expectedId)
        {
            int? uid = await LoginAsync();
            if (uid == null || uid == 0) return null;

            // 1. Search role by name
            var searchPayload = new
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
                        "res.partner.role", "search_read",
                        new object[]
                        {
                            new object[] { new object[] { "name", "=", roleName } }
                        },
                        new { fields = new[] { "id", "name" }, limit = 1 }
                    }
                }
            };

            var searchResponse = await _httpClient.PostAsync($"{_url}/jsonrpc",
                new StringContent(JsonSerializer.Serialize(searchPayload), Encoding.UTF8, "application/json"));

            var searchJson = await searchResponse.Content.ReadAsStringAsync();
            var searchDoc = JsonDocument.Parse(searchJson);

            if (searchDoc.RootElement.TryGetProperty("result", out var resultArray) &&
                resultArray.ValueKind == JsonValueKind.Array &&
                resultArray.GetArrayLength() > 0)
            {
                return resultArray[0].GetProperty("id").GetInt32();
            }

            // 2. Nếu không có → tạo mới
            var createPayload = new
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
                        "res.partner.role", "create",
                        new object[]
                        {
                            new Dictionary<string, object>
                            {
                                { "name", roleName }
                            }
                        }
                    }
                }
            };

            var createResponse = await _httpClient.PostAsync($"{_url}/jsonrpc",
                new StringContent(JsonSerializer.Serialize(createPayload), Encoding.UTF8, "application/json"));

            var createJson = await createResponse.Content.ReadAsStringAsync();
            var createDoc = JsonDocument.Parse(createJson);

            if (createDoc.RootElement.TryGetProperty("result", out var createdId))
            {
                return createdId.GetInt32();
            }

            // Nếu có lỗi
            if (createDoc.RootElement.TryGetProperty("error", out var error))
            {
                Console.WriteLine($"❌ Lỗi khi tạo vai trò: {error}");
            }

            return null;
        }
    }
}
