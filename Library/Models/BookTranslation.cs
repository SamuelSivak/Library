using System.ComponentModel.DataAnnotations;

namespace Library.Models
{
    public class BookTranslation
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        
        [Required]
        [MaxLength(10)]
        public required string LanguageCode { get; set; } 
        
        [Required]
        [MaxLength(255)]
        public required string Title { get; set; }
        
        public string? Description { get; set; }

        public Book? Book { get; set; }
    }
}
