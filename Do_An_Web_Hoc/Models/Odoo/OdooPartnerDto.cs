namespace Do_An_Web_Hoc.Models.Odoo
{
    public class OdooPartnerDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Username { get; set; }
        public int ExternalUserId { get; set; } // Dùng UserID
        public DateTime? Birthday { get; set; }
        public int? RoleId { get; set; }
        public int? Status { get; set; }
        public string? Address { get; set; }
        public string? ImageUrl { get; set; } // URL ảnh
        public bool IsStudent { get; set; } = false;
        public bool IsLecturer { get; set; } = false;

    }
}