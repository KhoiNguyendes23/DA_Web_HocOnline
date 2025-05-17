using Microsoft.AspNetCore.Mvc;
using Do_An_Web_Hoc.Repositories.Interfaces;
using Do_An_Web_Hoc.Services.Interfaces;
using Do_An_Web_Hoc.Models.Odoo;

namespace Do_An_Web_Hoc.Controllers
{
    public class OdooPartnerController : Controller
    {
        private readonly IOdooPartnerService _odooService;
        private readonly IUserAccountRepository _userRepo;

        public OdooPartnerController(
            IOdooPartnerService odooService,
            IUserAccountRepository userRepo)
        {
            _odooService = odooService;
            _userRepo = userRepo;
        }

        [HttpGet]
        public IActionResult CreateStudent() => View();

        [HttpPost]
        public async Task<IActionResult> CreateStudent(string fullName, string email)
        {
            var roleId = await _odooService.SearchRoleIdByNameAsync("Học viên");
            if (roleId == null)
            {
                ViewBag.Result = "❌ Không tìm thấy vai trò Học viên trong Odoo.";
                return View("Result");
            }

            var dto = new OdooPartnerDto
            {
                FullName = fullName,
                Email = email,
                RoleId = roleId,
                Status = 1,
                IsStudent = true,
                IsLecturer = false
            };

            return await CreateOrUpdatePartner(dto, "học viên");
        }

        [HttpPost]
        public async Task<IActionResult> CreateTeacher(string fullName, string email)
        {
            var roleId = await _odooService.SearchRoleIdByNameAsync("Giảng viên");
            if (roleId == null)
            {
                ViewBag.Result = "❌ Không tìm thấy vai trò Giảng viên trong Odoo.";
                return View("Result");
            }

            var dto = new OdooPartnerDto
            {
                FullName = fullName,
                Email = email,
                RoleId = roleId,
                Status = 1,
                IsStudent = false,
                IsLecturer = true
            };

            return await CreateOrUpdatePartner(dto, "giảng viên");
        }


        [HttpGet]
        public IActionResult SyncAll() => View();

        [HttpPost]
        public async Task<IActionResult> SyncAllStudents()
        {
            ViewBag.Result = await SyncByRole(3, "Học viên");
            return View("Result");
        }

        [HttpPost]
        public async Task<IActionResult> SyncAllTeachers()
        {
            ViewBag.Result = await SyncByRole(2, "Giảng viên");
            return View("Result");
        }

        private async Task<string> SyncByRole(int localRoleId, string roleName)
        {
            var users = (await _userRepo.GetAllUsersAsync())
                .Where(u => u.RoleID == localRoleId && !string.IsNullOrWhiteSpace(u.Email)).ToList();

            var results = new List<string>();
            var odooRoleId = await _odooService.SearchRoleIdByNameAsync(roleName);

            if (odooRoleId == null)
                return $"❌ Không tìm thấy vai trò {roleName} trong Odoo.";

            foreach (var user in users)
            {
                var dto = new OdooPartnerDto
                {
                    FullName = user.FullName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Address = user.Address,
                    Username = user.UserName,
                    Birthday = user.Birthday,
                    RoleId = odooRoleId,
                    Status = user.Status,
                    ExternalUserId = user.UserID,
                    ImageUrl = user.Image,
                    IsStudent = localRoleId == 3,
                    IsLecturer = localRoleId == 2
                };


                var existingId = await _odooService.SearchPartnerByEmailAsync(user.Email);
                int? result;

                if (existingId == null)
                {
                    result = await _odooService.CreatePartnerAsync(dto);
                    results.Add(result != null
                        ? $"✅ Tạo {roleName.ToLower()} <b>{user.FullName}</b> thành công (ID: {result})"
                        : $"❌ Tạo {roleName.ToLower()} <b>{user.FullName}</b> thất bại.");
                }
                else
                {
                    var updated = await _odooService.UpdatePartnerAsync(existingId.Value, dto);
                    result = updated ? existingId : null;
                    results.Add(result != null
                        ? $"✅ Cập nhật {roleName.ToLower()} <b>{user.FullName}</b> thành công (ID: {result})"
                        : $"❌ Cập nhật {roleName.ToLower()} <b>{user.FullName}</b> thất bại.");
                }
            }

            return string.Join("<br/>", results);
        }

        private async Task<IActionResult> CreateOrUpdatePartner(OdooPartnerDto dto, string roleName)
        {
            var existingId = await _odooService.SearchPartnerByEmailAsync(dto.Email);
            int? result;

            if (existingId == null)
            {
                result = await _odooService.CreatePartnerAsync(dto);
            }
            else
            {
                var updated = await _odooService.UpdatePartnerAsync(existingId.Value, dto);
                result = updated ? existingId : null;
            }

            ViewBag.Result = result != null
                ? $"✅ {(existingId == null ? "Tạo" : "Cập nhật")} {roleName} thành công (Odoo ID: {result})"
                : $"❌ Thao tác với {roleName} thất bại.";

            return View("Result");
        }

        [HttpGet]
        public IActionResult Result()
        {
            ViewBag.Result = "🧪 Đây là trang kết quả test";
            return View();
        }
    }
}
