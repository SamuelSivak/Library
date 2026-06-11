namespace Library.DTOs
{
    public class BookDTO
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? ISBN { get; set; }
        public int PageCount { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime Published { get; set; }
        public string? AuthorName { get; set; }
        public List<string>? Genres { get; set; }
        public List<ReviewDTO>? Reviews { get; set; }
        public double AverageRating { get; set; }
    }
}
