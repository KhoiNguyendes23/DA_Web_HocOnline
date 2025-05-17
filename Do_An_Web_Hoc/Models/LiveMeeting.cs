using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Do_An_Web_Hoc.Models
{
    /// <summary>
    /// Đại diện cho một buổi học trực tuyến (Meeting) sử dụng BigBlueButton hoặc nền tảng tương tự.
    /// </summary>
    public class LiveMeeting
    {
        [Key]
        public int MeetingId { get; set; }

        /// <summary>
        /// Tiêu đề của buổi học (ví dụ: "Buổi học tuần 1", "Ôn tập giữa kỳ")
        /// </summary>
        [Required]
        public string Title { get; set; }

        /// <summary>
        /// Mô tả chi tiết nội dung của buổi học
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Mã nhận diện duy nhất của buổi học dùng cho hệ thống BBB (meetingID BBB)
        /// </summary>
        public string MeetingCode { get; set; }

        /// <summary>
        /// Mật khẩu cho người điều phối (giảng viên) để quản lý buổi học trên BBB
        /// </summary>
        public string ModeratorPassword { get; set; }

        /// <summary>
        /// Mật khẩu cho người tham dự (học viên)
        /// </summary>
        public string AttendeePassword { get; set; }

        /// <summary>
        /// Thời điểm bắt đầu của buổi học
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Thời điểm kết thúc (dự kiến hoặc thực tế) của buổi học
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// ID của khóa học mà buổi học này thuộc về
        /// </summary>
        public int CourseId { get; set; }

        /// <summary>
        /// ID của giảng viên tổ chức buổi học
        /// </summary>
        public int CreatedBy { get; set; }

        /// <summary>
        /// Trạng thái của buổi học (sắp diễn ra, đang diễn ra, đã kết thúc)
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// URL để học viên tham gia buổi học (được tạo dựa trên BBB API)
        /// </summary>
        public string JoinUrl { get; set; }

        /// <summary>
        /// URL để giảng viên (moderator) tham gia buổi học
        /// </summary>
        public string ModeratorJoinUrl { get; set; }
        /// <summary>
        /// Thời điểm tạo buổi học
        /// </summary>
        public DateTime CreateAt { get; set; }

        // ===== Navigation Properties =====

        [ForeignKey("CourseId")]
        public Courses Course { get; set; }

        [ForeignKey("CreatedBy")]
        public UserAccount Lecturer { get; set; }
    }
}
