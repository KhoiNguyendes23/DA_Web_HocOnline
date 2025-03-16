using System.ComponentModel.DataAnnotations;

namespace Do_An_Web_Hoc.Models
{
    public class Ratings
    {
        [Key]
        public int RatingID { get; set; }
        public int? UserID { get; set; }
        public int? CourseID { get; set; }
        public int RatingValue { get; set; }
    }
}
