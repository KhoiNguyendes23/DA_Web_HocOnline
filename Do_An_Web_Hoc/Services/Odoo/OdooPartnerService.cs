using Do_An_Web_Hoc.Models.Odoo;
using Do_An_Web_Hoc.Services.Interfaces;
using System.Text;
using System.Text.Json;

namespace Do_An_Web_Hoc.Services.Odoo
{
    public class OdooPartnerService : IOdooPartnerService
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;
        private readonly string _url;
        private readonly string _db;
        private readonly string _username;
        private readonly string _password;

        public OdooPartnerService(IConfiguration config)
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

            return doc.RootElement.GetProperty("result").GetProperty("uid").GetInt32();
        }

        public async Task<int?> SearchPartnerByEmailAsync(string email)
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
                        "res.partner", "search",
                        new object[]
                        {
                            new object[] { new object[] { "email", "=", email } }
                        }
                    }
                }
            };

            var response = await _httpClient.PostAsync($"{_url}/jsonrpc",
                new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"));

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("result", out var result) &&
                result.ValueKind == JsonValueKind.Array &&
                result.GetArrayLength() > 0)
            {
                return result[0].GetInt32();
            }

            return null;
        }

        private async Task AddImageIfExistsAsync(OdooPartnerDto dto, Dictionary<string, object> data)
        {
            if (!string.IsNullOrWhiteSpace(dto.ImageUrl))
            {
                try
                {
                    var rootPath = Directory.GetCurrentDirectory();
                    var fileName = Path.GetFileName(dto.ImageUrl);
                    var imagePath = Path.Combine(rootPath, "wwwroot", "images", "Avatar_images", fileName);

                    if (File.Exists(imagePath))
                    {
                        var imageBytes = await File.ReadAllBytesAsync(imagePath);
                        data["image_1920"] = Convert.ToBase64String(imageBytes);
                        Console.WriteLine($"✅ Đã đọc ảnh: {imagePath}");
                    }
                    else
                    {
                        Console.WriteLine($"❌ Không tìm thấy ảnh: {imagePath}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Lỗi đọc ảnh: {ex.Message}");
                }
            }
        }

        public async Task<int?> CreatePartnerAsync(OdooPartnerDto dto)
        {
            int? uid = await LoginAsync();
            if (uid == null || uid == 0) return null;

            var data = new Dictionary<string, object>
            {
                { "name", dto.FullName },
                { "email", dto.Email },
                { "phone", dto.PhoneNumber ?? "" },
                { "street", dto.Address ?? "" },
                { "username", dto.Username ?? "" },
                { "external_user_id", dto.ExternalUserId },
                { "role_id", dto.RoleId ?? 3 },
                { "status_code", dto.Status ?? 1 },
                { "is_student", dto.IsStudent },
                { "is_lecturer", dto.IsLecturer }
            };

            if (dto.Birthday.HasValue)
            {
                data["birthday"] = dto.Birthday.Value.ToString("yyyy-MM-dd");
            }

            await AddImageIfExistsAsync(dto, data);

            var payload = new
            {
                jsonrpc = "2.0",
                method = "call",
                @params = new
                {
                    service = "object",
                    method = "execute_kw",
                    args = new object[] { _db, uid, _password, "res.partner", "create", new object[] { data } }
                }
            };

            var response = await _httpClient.PostAsync($"{_url}/jsonrpc",
                new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"));

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("result", out var result))
                return result.GetInt32();

            if (doc.RootElement.TryGetProperty("error", out var error))
                Console.WriteLine($"❌ CreatePartnerAsync error: {error}");

            return null;
        }

        public async Task<bool> UpdatePartnerAsync(int partnerId, OdooPartnerDto dto)
        {
            int? uid = await LoginAsync();
            if (uid == null || uid == 0) return false;

            var data = new Dictionary<string, object>
            {
                { "name", dto.FullName },
                { "email", dto.Email },
                { "phone", dto.PhoneNumber ?? "" },
                { "street", dto.Address ?? "" },
                { "username", dto.Username ?? "" },
                { "external_user_id", dto.ExternalUserId },
                { "role_id", dto.RoleId ?? 3 },
                { "status_code", dto.Status ?? 1 },
                { "is_student", dto.IsStudent },
                { "is_lecturer", dto.IsLecturer }
            };

            if (dto.Birthday.HasValue)
            {
                data["birthday"] = dto.Birthday.Value.ToString("yyyy-MM-dd");
            }

            await AddImageIfExistsAsync(dto, data);

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
                        _db, uid, _password, "res.partner", "write",
                        new object[] { new int[] { partnerId }, data }
                    }
                }
            };

            var response = await _httpClient.PostAsync($"{_url}/jsonrpc",
                new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"));

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("result", out var result))
                return result.GetBoolean();

            if (doc.RootElement.TryGetProperty("error", out var error))
                Console.WriteLine($"❌ UpdatePartnerAsync error: {error}");

            return false;
        }

        public async Task<int?> SearchRoleIdByNameAsync(string roleName)
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
                        "res.partner.role", "search_read",
                        new object[] { new object[] { new object[] { "name", "=", roleName } } },
                        new { fields = new[] { "id" }, limit = 1 }
                    }
                }
            };

            var response = await _httpClient.PostAsync($"{_url}/jsonrpc",
                new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"));

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("result", out var result) &&
                result.ValueKind == JsonValueKind.Array &&
                result.GetArrayLength() > 0)
            {
                return result[0].GetProperty("id").GetInt32();
            }

            return null;
        }
    }
}
