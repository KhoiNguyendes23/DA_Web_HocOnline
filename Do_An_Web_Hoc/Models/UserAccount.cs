using System.ComponentModel.DataAnnotations;

namespace Do_An_Web_Hoc.Models
{
    public class UserAccount
    {
        [Key]
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }
        [Required]
        public DateTime CreateAt { get; set; }
        public int? RoleID { get; set; }
        public string FullName { get; set; }
        public DateTime? Birthday { get; set; }
        public int? Status { get; set; }
    }
}
