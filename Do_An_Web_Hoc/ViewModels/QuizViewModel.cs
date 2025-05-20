using System.ComponentModel.DataAnnotations;

namespace Do_An_Web_Hoc.ViewModels
{
    public class QuizViewModel
    {
        [Required(ErrorMessage = "Tên Quiz không được để trống")]
        public string QuizName { get; set; }
        public string? Description { get; set; }
        public int? TotalMarks { get; set; }
        public int? LectureID { get; set; }
        public List<QuestionViewModel> Questions { get; set; } = new();
    }
}
