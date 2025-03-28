using System.ComponentModel.DataAnnotations;

namespace Do_An_Web_Hoc.Models
{
    public class Quizzes
    {
        [Key]
        public int QuizID { get; set; }
        public string QuizName { get; set; }
        public string Description { get; set; }
        public int? TotalMarks { get; set; }
        public int? ExamID { get; set; }
        public int? Duration { get; set; }
    }
}
