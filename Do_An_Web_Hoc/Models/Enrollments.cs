using System.ComponentModel.DataAnnotations;

namespace Do_An_Web_Hoc.Models
{
    public class Enrollments
    {
        [Key]
        public int EnrollmentID { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public bool CompletionStatus { get; set; }
        public int UserID { get; set; }
        public int CourseID { get; set; }
    }
}
