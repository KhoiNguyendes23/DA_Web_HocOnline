using System.ComponentModel.DataAnnotations;

namespace Do_An_Web_Hoc.Models
{
    public class Answers
    {
        [Key]
        public int AnswerID { get; set; }
        public int? QuestionID { get; set; }
        public string AnswerText { get; set; }
        public bool? IsCorrect { get; set; }
    }
}
