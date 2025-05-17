namespace Do_An_Web_Hoc.Models.ViewModels
{
    public class ResultExamViewModel
    {
        public string StudentName { get; set; }
        public string ExamName { get; set; }
        public string QuizName { get; set; }
        public float? Score { get; set; }             // Điểm đạt
        public int? TotalMarks { get; set; }          // Tổng điểm
        public DateTime? SubmissionTime { get; set; }
    }

}
