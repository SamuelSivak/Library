namespace Library.DTOs
{
    public class ReviewDTO
    {
        public int Id { get; set; }
        public required string Text { get; set; }
        public int Rating { get; set; }
        public DateTime CreatedAt { get; set; }
        public int BookId{ get; set; }
        public int ReviewerId { get; set; }
        public string? ReviewerName { get; set; }

    }
}
