using System.ComponentModel.DataAnnotations;

namespace Do_An_Web_Hoc.Models
{
    public class Reviews
    {
        [Key]
        public int ReviewID { get; set; }
        public int? UserID { get; set; }
        public int? CourseID { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
