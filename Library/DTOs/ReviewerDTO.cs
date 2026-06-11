namespace Library.DTOs
{
    public class ReviewerDTO
    {
        public int Id { get; set; }
        public required string Name { get; set; }    
        public required List<string> Reviews { get; set; }
    }
}
