using System.ComponentModel.DataAnnotations;

namespace Do_An_Web_Hoc.Models
{
    public class Documents
    {
        [Key]
        public int DocumentID { get; set; }
        public string Title { get; set; }
        public string FilePath { get; set; }
        public int? UploadedBy { get; set; }
        public DateTime? UploadDate { get; set; }
        public int? CourseID { get; set; }
        public int? LectureID { get; set; }
        public int Status { get; set; }
    }
}
