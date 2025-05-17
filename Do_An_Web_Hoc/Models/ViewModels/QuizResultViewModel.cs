namespace Do_An_Web_Hoc.Models.ViewModels
{
    public class QuizResultViewModel
    {
        public string QuizName { get; set; }
        public int Score { get; set; }
        public int TotalScore { get; set; }
        public List<QuestionResult> Questions { get; set; }
    }
}
