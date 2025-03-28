namespace Do_An_Web_Hoc.Models
{
    public class QuestionTempModel
    {
        public string QuestionText { get; set; }
        public string QuestionType { get; set; } // Ví dụ: "Tự luận", "Trắc nghiệm"

        public string OptionA { get; set; }
        public string OptionB { get; set; }
        public string OptionC { get; set; }
        public string OptionD { get; set; }
        public string CorrectAnswer { get; set; }
    }
}
