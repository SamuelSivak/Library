namespace Library.Models
{
    public class Book
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public string? ISBN { get; set; }
        public int PageCount { get; set; }
        public string? ImageUrl { get; set; }
        public required DateTime Published { get; set; }
        public required int AuthorId { get; set; }

        public Author? Author { get; set; }

        public ICollection<BookGenre>? BookGenres { get; set; }
        public ICollection<Review>? Reviews { get; set; }
        public ICollection<BookTranslation>? Translations { get; set; }
    }
}
