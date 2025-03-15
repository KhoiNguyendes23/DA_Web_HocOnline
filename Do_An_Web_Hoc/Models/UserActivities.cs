using System.ComponentModel.DataAnnotations;

namespace Do_An_Web_Hoc.Models
{
    public class UserActivities
    {
        [Key]
        public int ActivityId { get; set; }
        public int UserId { get; set; }

        public string ActivityType { get; set; }

        public DateTime ActivityDate { get; set; }
    }
}
