using System.ComponentModel.DataAnnotations;

namespace Do_An_Web_Hoc.Models
{
    public class Questions
    {
        [Key]
        public int QuestionID { get; set; }
        public int? QuizID { get; set; }
        public string QuestionText { get; set; }
        public string QuestionType { get; set; }
        public string? ImagePath { get; set; }

    }
}
 