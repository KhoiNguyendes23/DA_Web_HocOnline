namespace Do_An_Web_Hoc.Models.ViewModels
{
    public class CompletedQuizViewModel
    {
        public int QuizID { get; set; }
        public string QuizName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? TotalMarks { get; set; }
    }

}
