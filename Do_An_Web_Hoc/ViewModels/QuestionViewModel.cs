using System.ComponentModel.DataAnnotations;

namespace Do_An_Web_Hoc.ViewModels
{
    public class QuestionViewModel
    {
        [Required(ErrorMessage = "Câu hỏi không được để trống")]
        public string QuestionText { get; set; }

        [Required] public string OptionA { get; set; }
        [Required] public string OptionB { get; set; }
        [Required] public string OptionC { get; set; }
        [Required] public string OptionD { get; set; }

        [Required(ErrorMessage = "Phải chọn đáp án đúng")]
        public string CorrectAnswer { get; set; } // A, B, C, D
    }
}
