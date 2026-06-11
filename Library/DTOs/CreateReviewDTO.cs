using System.ComponentModel.DataAnnotations;

namespace Library.DTOs
{
    public class CreateReviewDTO
    {
        [Required(ErrorMessage = "Review text is required")]
        [StringLength(256, MinimumLength = 1, ErrorMessage = "Text must be between 1 and 256 characters")]
        public required string Text { get; set; }

        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "BookId must be a valid ID")]
        public int BookId { get; set; }

        public int? ReviewerId { get; set; }
    }
}
