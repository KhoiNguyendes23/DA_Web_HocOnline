using System.ComponentModel.DataAnnotations;

namespace Do_An_Web_Hoc.Models
{
    public enum CourseType
    {
        Video = 1,
        Live = 2
    }

    public class Courses
    {
        [Key]
        public int CourseID { get; set; }
        public string CourseName { get; set; }
        public string Description { get; set; }
        public decimal? Price { get; set; }
        public int? CategoryID { get; set; }
        public int? Status { get; set; }
        public string? ImageUrl { get; set; }

        // Nullable để có thể chưa chọn loại khóa học
        public CourseType? Type { get; set; }

        public virtual ICollection<Enrollments> Enrollments { get; set; } = new List<Enrollments>();

        public virtual ICollection<LiveMeeting> LiveMeetings { get; set; } = new List<LiveMeeting>();
    }

}
