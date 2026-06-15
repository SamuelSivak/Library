using Microsoft.EntityFrameworkCore;
using Library.DataContext;
using Library.Models;
using Library.DTOs;
using Library.Services;
using Hangfire;

namespace Library.Repositories
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly LibraryContext _context;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly ICacheService _cache;

        public ReviewRepository(LibraryContext context, IBackgroundJobClient backgroundJobClient, ICacheService cache)
        {
            _context = context;
            _backgroundJobClient = backgroundJobClient;
            _cache = cache;
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

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName!.ToLower() == username.ToLower());

            var newReview = new Review
            {
                Text = review.Text,
                Rating = review.Rating,
                CreatedAt = DateTime.UtcNow,
                BookId = review.BookId,
                ReviewerId = reviewer.Id,
                UserId = user?.Id
            };
            _context.Reviews.Add(newReview);
            await _context.SaveChangesAsync();
            _backgroundJobClient.Enqueue<IBookAnalyticsService>(x => x.UpdateAnalyticsForBookAsync(newReview.BookId));

            await _cache.RemoveAsync("reviewers_all");
            foreach (var lang in new[] { "SK", "EN", "GR" })
            {
                await _cache.RemoveAsync($"book_detail_{newReview.BookId}_{lang}");
            }

            return await GetByIdAsync(newReview.Id)
                ?? throw new InvalidOperationException($"Review with id {newReview.Id} not found after creation.");
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null) return false;

            var bookId = review.BookId;
            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
            _backgroundJobClient.Enqueue<IBookAnalyticsService>(x => x.UpdateAnalyticsForBookAsync(bookId));

            await _cache.RemoveAsync("reviewers_all");
            foreach (var lang in new[] { "SK", "EN", "GR" })
            {
                await _cache.RemoveAsync($"book_detail_{bookId}_{lang}");
            }

            return true;
        }
        public async Task<bool> ExistsAsync(string text)
        {
            return await _context.Reviews
                .AnyAsync(r => r.Text!.ToLower() == text.ToLower());
        }
    }
}
