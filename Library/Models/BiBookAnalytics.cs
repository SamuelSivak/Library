using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Library.Models
{
    public class BiBookAnalytics
    {
        [Key]
        [ForeignKey("Book")]
        public int BookId { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int Clicks { get; set; }
        public int PositiveReviews { get; set; }
        public int NegativeReviews { get; set; }
        public DateTime LastUpdated { get; set; }

        public Book? Book { get; set; }
    }
}
