namespace Library.Models
{
    public class Reviewer
    {
        public int Id { get; set; }
        public required string Name { get; set; }

        public ICollection<Review>? Reviews { get; set; }
    }
}
