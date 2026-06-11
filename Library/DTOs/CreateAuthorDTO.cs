using System.ComponentModel.DataAnnotations;

namespace Library.DTOs
{
    public class CreateAuthorDTO
    {
        [Required(ErrorMessage = "Author name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Author surname is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Surname must be between 2 and 100 characters")]
        public required string Surname { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "CountryId must be a valid ID")]
        public required int CountryId { get; set; }
    }
}

