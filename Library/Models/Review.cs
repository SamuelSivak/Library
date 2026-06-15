    namespace Library.Models
    {
        public class Review
        {
            public int Id { get; set; }
            public string? Text { get; set; }   
            public int BookId { get; set; }
            public int ReviewerId { get; set; }
            public int Rating { get; set; }
            public DateTime CreatedAt { get; set; }

            public Book? Book { get; set; }
            public Reviewer? Reviewer { get; set; }
            
            public string? UserId { get; set; }
            public ApplicationUser? User { get; set; }
        }
    }
