using System.ComponentModel.DataAnnotations;

namespace Do_An_Web_Hoc.Models
{
    public class Results
    {
        [Key]
        public int ResultID { get; set; }

        public float? Score { get; set; }

        public int? UserID { get; set; } 

        public int? QuizID { get; set; }

        public DateTime? SubmissionTime { get; set; }

    }
}
