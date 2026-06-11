using System.ComponentModel.DataAnnotations;

namespace Library.Models
{
    public class GenreTranslation
    {
        public int Id { get; set; }
        public int GenreId { get; set; }
        
        [Required]
        [MaxLength(10)]
        public required string LanguageCode { get; set; } 
        
        [Required]
        [MaxLength(100)]
        public required string Name { get; set; }

        public Genre? Genre { get; set; }
    }
}
