 using System.ComponentModel.DataAnnotations;

namespace Do_An_Web_Hoc.Models
{
    public class UserStatus
    {
        [Key]
        public int StatusId { get; set; }

        public string StatusName { get; set; }
    }
}
