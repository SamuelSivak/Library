using System.ComponentModel.DataAnnotations;

namespace Library.DTOs
{
    public class CreateGenreDTO
    {
        [Required(ErrorMessage = "Genre name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
        public required string Name { get; set; }
    }
}
