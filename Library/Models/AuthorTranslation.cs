using System.ComponentModel.DataAnnotations;

namespace Library.Models
{
    public class AuthorTranslation
    {
        public int Id { get; set; }
        public int AuthorId { get; set; }
        
        [Required]
        [MaxLength(10)]
        public required string LanguageCode { get; set; } 
        
        [Required]
        [MaxLength(100)]
        public required string Name { get; set; }
        
        [Required]
        [MaxLength(100)]
        public required string Surname { get; set; }

        public Author? Author { get; set; }
    }
}
