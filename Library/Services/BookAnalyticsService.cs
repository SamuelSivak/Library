using Library.DataContext;
using Library.Models;
using Microsoft.EntityFrameworkCore;

namespace Library.Services
{
    public class BookAnalyticsService : IBookAnalyticsService
    {
        private readonly LibraryContext _context;

        public BookAnalyticsService(LibraryContext context)
        {
            _context = context;
        }

        public async Task UpdateAnalyticsForBookAsync(int bookId)
        {
            var stats = await _context.Reviews
                .Where(r => r.BookId == bookId)
                .GroupBy(r => r.BookId)
                .Select(g => new
                {
                    Average = g.Average(r => r.Rating),
                    Count = g.Count(),
                    Positive = g.Count(r => r.Rating >= 4),
                    Negative = g.Count(r => r.Rating <= 2)
                })
                .FirstOrDefaultAsync();

            var avg = stats != null ? Math.Round(stats.Average, 1) : 0.0;
            var count = stats != null ? stats.Count : 0;
            var positive = stats != null ? stats.Positive : 0;
            var negative = stats != null ? stats.Negative : 0;

            var analytics = await _context.BiBookAnalytics
                .FirstOrDefaultAsync(ba => ba.BookId == bookId);

            if (analytics == null)
            {
                analytics = new BiBookAnalytics
                {
                    BookId = bookId,
                    AverageRating = avg,
                    TotalReviews = count,
                    Clicks = 0,
                    PositiveReviews = positive,
                    NegativeReviews = negative,
                    LastUpdated = DateTime.UtcNow
                };
                _context.BiBookAnalytics.Add(analytics);
            }
            else
            {
                analytics.AverageRating = avg;
                analytics.TotalReviews = count;
                analytics.PositiveReviews = positive;
                analytics.NegativeReviews = negative;
                analytics.LastUpdated = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        public async Task IncrementViewsAsync(int bookId)
        {
            var analytics = await _context.BiBookAnalytics
                .FirstOrDefaultAsync(ba => ba.BookId == bookId);

            if (analytics == null)
            {
                analytics = new BiBookAnalytics
                {
                    BookId = bookId,
                    AverageRating = 0,
                    TotalReviews = 0,
                    Clicks = 1,
                    PositiveReviews = 0,
                    NegativeReviews = 0,
                    LastUpdated = DateTime.UtcNow
                };
                _context.BiBookAnalytics.Add(analytics);
            }
            else
            {
                analytics.Clicks += 1;
                analytics.LastUpdated = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }
    }
}