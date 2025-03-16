using System.ComponentModel.DataAnnotations;

namespace Do_An_Web_Hoc.Models
{
    public class CourseStatus
    {
        [Key]
        public int StatusID { get; set; }

        public string StatusName { get; set; }
    }
}
