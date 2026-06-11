namespace Library.DTOs
{
    public class AuthorDTO
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Surname { get; set; }
        public required string Country { get; set; }
        public List<string>? Books { get; set; }
    }
}
