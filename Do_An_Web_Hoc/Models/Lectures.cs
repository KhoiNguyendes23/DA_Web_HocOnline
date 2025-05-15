using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Do_An_Web_Hoc.Models
{
    public class Lectures
    {
        [Key]
        public int LectureID { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string VideoUrl { get; set; }
        public string DocumentURL { get; set; }
        public DateTime? CreateAt { get; set; }
        public int CourseID { get; set; }
        public int Order { get; set; }
        public Quizzes? Quiz { get; set; }
        public int Status { get; set; } = 1; // 1: Hoạt động, 2: Ngưng hoạt động

        [NotMapped]
        public bool IsLocked { get; set; }
    }
}
