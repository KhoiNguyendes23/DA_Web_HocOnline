using System.ComponentModel.DataAnnotations;

namespace Do_An_Web_Hoc.Models
{
    public class Courses
    {
        [Key]
        public int CourseID { get; set; }
        public string CourseName { get; set; }
        public string Description { get; set; }
        public decimal? Price { get; set; }
        public int? CategoryID { get; set; }
        public int? Status { get; set; }
    }
}
