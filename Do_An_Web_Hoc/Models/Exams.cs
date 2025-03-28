using System.ComponentModel.DataAnnotations;

namespace Do_An_Web_Hoc.Models
{
    public class Exams
    {
        [Key]
        public int ExamID { get; set; }
        public string ExamName { get; set; }
        public string Description { get; set; }
        public int TotalMarks { get; set; }
        public int CourseID { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? Status { get; set; }
        // Thêm thời gian bắt đầu và kết thúc
        public DateTime? StartTime { get; set; }  // Nullable DateTime
        public DateTime? EndTime { get; set; }
        public int? Duration { get; set; }
    }
}
