namespace Library.Models
{
    public class Genre
    {
        public int Id { get; set; }
        public required string Name { get; set; }

        public ICollection<BookGenre>? BookGenres { get; set; }
        public ICollection<GenreTranslation>? Translations { get; set; }
    }
}
    