namespace Do_An_Web_Hoc.Models.ViewModels
{
    public class CompletedQuizViewModel
    {
        public int QuizID { get; set; }
        public string QuizName { get; set; }
        public DateTime? SubmissionTime { get; set; } // Thời gian làm bài
        public int? TotalMarks { get; set; }
        public float? Score { get; set; } // Điểm đạt được
    }

}
