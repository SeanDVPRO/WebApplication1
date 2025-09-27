using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Book
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Author { get; set; } = string.Empty;

        public string? Genre { get; set; }

        [Display(Name = "Year Published")]
        [DataType(DataType.Date)]
        public DateTime? YearPublished { get; set; }
    }
}
