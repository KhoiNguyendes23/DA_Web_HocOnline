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

        [Required]
        public string Message { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.Now;

        public bool IsRead { get; set; } = false;

        [ForeignKey("SenderId")]
        [InverseProperty("SentMessages")]
        public UserAccount Sender { get; set; }

        [ForeignKey("ReceiverId")]
        [InverseProperty("ReceivedMessages")]
        public UserAccount Receiver { get; set; }
    }
}
