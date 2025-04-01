using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Do_An_Web_Hoc.ViewModels
{
    public class QuestionViewModel
    {
        [Required(ErrorMessage = "Câu hỏi không được để trống")]
        public string QuestionText { get; set; }

        public string? OptionA { get; set; }
        public string? OptionB { get; set; }
        public string? OptionC { get; set; }
        public string? OptionD { get; set; }

        public string? CorrectAnswer { get; set; }
        public string? QuestionType { get; set; }

        // ✅ Thêm mới:
        public IFormFile? QuestionImage { get; set; }

        // ✅ Tuỳ chọn: để hiển thị lại khi Edit
        public string? ImagePath { get; set; }
    }
}
