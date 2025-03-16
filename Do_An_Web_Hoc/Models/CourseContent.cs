using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace Do_An_Web_Hoc.Models
{
    public class CourseContent
    {
        [Key]
        public int ContentId { get; set; }

        public int CourseId { get; set; }

        public string ContentType { get; set; }

    }
}
