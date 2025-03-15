using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace Do_An_Web_Hoc.Models
{
    public class Categories
    {
        [Key]
        public int CategoryId { get; set; }

        public string CategoryName { get; set; }

        public int? Status { get; set; }
    }
}
