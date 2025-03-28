using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Do_An_Web_Hoc.Models
{
    public class Exams
    {
        [Key]
        public int ExamID { get; set; }

        [Required(ErrorMessage = "Tên bài kiểm tra không được để trống")]
        public string ExamName { get; set; }

        public string Description { get; set; }

        [Required(ErrorMessage = "Tổng điểm là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "Tổng điểm phải lớn hơn 0")]
        public int TotalMarks { get; set; }

        [Required(ErrorMessage = "Mã khóa học là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "Mã khóa học phải hợp lệ")]
        public int CourseID { get; set; }

        public DateTime? CreatedAt { get; set; }

        public int? Status { get; set; }

        [NotMapped]
        public string QuizName { get; set; }

        [NotMapped]
        public List<QuestionTempModel> Questions { get; set; } = new List<QuestionTempModel>();


    }

}
