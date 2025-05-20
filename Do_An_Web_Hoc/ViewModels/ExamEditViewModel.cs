using System.ComponentModel.DataAnnotations;

namespace Do_An_Web_Hoc.ViewModels
{
    public class ExamEditViewModel
    {
        public int ExamID { get; set; }

        [Required(ErrorMessage = "Tiêu đề không được bỏ trống")]
        public string ExamName { get; set; }

        public string? Description { get; set; }

        [Range(1, 1000, ErrorMessage = "Tổng điểm phải lớn hơn 0")]
        public int TotalMarks { get; set; }

        public int CourseID { get; set; }

        public int? Duration { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public List<QuizViewModel> Quizzes { get; set; } = new();
        public int? LectureID { get; set; }
    }
}
