using Microsoft.EntityFrameworkCore;
using Library.DataContext;
using Library.Models;
using Library.DTOs;

namespace Library.Repositories
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly LibraryContext _context;
        public ReviewRepository(LibraryContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ReviewDTO>> GetAllAsync()
        {
            return await _context.Reviews
                .Include(r => r.Reviewer)
                .Select(r => new ReviewDTO
                {
                    Id = r.Id,
                    Text = r.Text!,
                    Rating = r.Rating,
                    CreatedAt = r.CreatedAt,
                    BookId = r.BookId,
                    ReviewerId = r.ReviewerId,
                    ReviewerName = r.Reviewer != null ? r.Reviewer.Name : "Anonymous"
                })
                .ToListAsync();
        }
        public async Task<ReviewDTO?> GetByIdAsync(int id)
        {
            return await _context.Reviews
                .Include(r => r.Reviewer)
                .Where(r => r.Id == id)
                .Select(r => new ReviewDTO
                {
                    Id = r.Id,
                    Text = r.Text!,
                    Rating = r.Rating,
                    CreatedAt = r.CreatedAt,
                    BookId = r.BookId,
                    ReviewerId = r.ReviewerId,
                    ReviewerName = r.Reviewer != null ? r.Reviewer.Name : "Anonymous"
                })
                .FirstOrDefaultAsync();
        }
        public async Task<ReviewDTO> CreateAsync(CreateReviewDTO review, string username)
        {
            var reviewer = await _context.Reviewers.FirstOrDefaultAsync(r => r.Name.ToLower() == username.ToLower());
            if (reviewer == null)
            {
                reviewer = new Reviewer { Name = username };
                _context.Reviewers.Add(reviewer);
                await _context.SaveChangesAsync();
            }

            var newReview = new Review
            {
                Text = review.Text,
                Rating = review.Rating,
                CreatedAt = DateTime.UtcNow,
                BookId = review.BookId,
                ReviewerId = reviewer.Id
            };
            _context.Reviews.Add(newReview);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(newReview.Id)
                ?? throw new InvalidOperationException($"Review with id {newReview.Id} not found after creation.");
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null) return false;

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> ExistsAsync(string text)
        {
            return await _context.Reviews
                .AnyAsync(r => r.Text!.ToLower() == text.ToLower());
        }
    }
}
