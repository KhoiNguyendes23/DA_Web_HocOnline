using System.ComponentModel.DataAnnotations;

namespace Do_An_Web_Hoc.Models
{
    public class Payments
    {
        [Key]

        public int PaymentID { get; set; }
        public int? UserID { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }
        public string TransactionID { get; set; }
    }
}
