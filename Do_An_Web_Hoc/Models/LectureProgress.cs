using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Do_An_Web_Hoc.Models
{
    public class LectureProgress
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int UserId { get; set; }         // Học viên
        public int LectureID { get; set; }      // Bài giảng

        public double Score { get; set; }       // Điểm quiz
        public bool IsPassed { get; set; }      // Qua >=50%
        public DateTime LastAttempt { get; set; }
    }
}
