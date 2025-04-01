using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        public bool IsPaid { get; set; } // đã thanh toán hay chưa
        public string? PaymentMethod { get; set; } // Ví dụ: VNPAY, MOMO
        public DateTime? PaymentDate { get; set; } // Ngày thanh toán

        // Thêm navigation property + [ForeignKey]
        //[ForeignKey("CourseID")]
        public Courses Course { get; set; }

        //[ForeignKey("UserID")]
        public UserAccount User { get; set; }

    }
}
