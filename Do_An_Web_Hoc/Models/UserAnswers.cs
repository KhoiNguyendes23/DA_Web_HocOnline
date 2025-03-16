using System.ComponentModel.DataAnnotations;

namespace Do_An_Web_Hoc.Models
{
    public class UserAnswers
    {
        [Key]
        public int UserAnswerID { get; set; }
        public int? UserID { get; set; }
        public int? QuestionID { get; set; }
        public int AnswerID { get; set; }
    }
}
