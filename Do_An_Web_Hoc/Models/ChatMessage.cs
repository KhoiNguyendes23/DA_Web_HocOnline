using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Do_An_Web_Hoc.Models
{
    public class ChatMessage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int SenderId { get; set; }

        [Required]
        public int ReceiverId { get; set; }

        public string? Message { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string? ImageUrl { get; set; }
        public bool IsRead { get; set; } = false;

        // Đổi sang nullable để không gây lỗi nếu không gán khi insert
        [ForeignKey("SenderId")]
        [InverseProperty("SentMessages")]
        public UserAccount? Sender { get; set; }

        [ForeignKey("ReceiverId")]
        [InverseProperty("ReceivedMessages")]
        public UserAccount? Receiver { get; set; }
    }
}
