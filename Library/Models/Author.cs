namespace Library.Models
{
    public class Author
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Surname { get; set; }
        public int CountryId { get; set; }  


        public Country? Country { get; set; }
        public ICollection<Book>? Books { get; set; }
        public ICollection<AuthorTranslation>? Translations { get; set; }
    }
}
