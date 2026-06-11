using System.ComponentModel.DataAnnotations;

namespace Library.DTOs
{
    public class UpdateBookDTO
    {
        [Required(ErrorMessage = "Book title is required")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 200 characters")]
        public required string Title { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }

        [RegularExpression(@"^\d{13}$", ErrorMessage = "ISBN must be a 13-digit numeric code")]
        public string? ISBN { get; set; }

        [Range(1, 10000, ErrorMessage = "Page count must be between 1 and 10000")]
        public int PageCount { get; set; }

        [Url(ErrorMessage = "ImageUrl must be a valid URL")]
        public string? ImageUrl { get; set; }

        [Required(ErrorMessage = "Publication date is required")]
        public DateTime Published { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "AuthorId must be a valid ID")]
        public int AuthorId { get; set; }

        public List<int>? GenreIds { get; set; }
    }
}
