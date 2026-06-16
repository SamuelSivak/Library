using Microsoft.EntityFrameworkCore;
using Library.DataContext;
using Library.DTOs;
using Library.Models;
using Library.Services;

namespace Library.Repositories
{
    public class ReviewerRepository : IReviewerRepository
    {
        private readonly LibraryContext _context;
        private readonly ICacheService _cache;

        public ReviewerRepository(LibraryContext context, ICacheService cache)
        {
            _context = context;
            _cache = cache;
        }
        public async Task<IEnumerable<ReviewerDTO>> GetAllAsync()
        {
            var cacheKey = "reviewers_all";
            var cached = await _cache.GetAsync<IEnumerable<ReviewerDTO>>(cacheKey);
            if (cached != null)
            {
                return cached;
            }

            var result = await _context.Reviewers
                .AsNoTracking()
                .Select(r => new ReviewerDTO
                {
                    Id = r.Id,
                    Name = r.Name,
                    Reviews = r.Reviews!.Select(review => review.Text!).ToList()
                })
                .ToListAsync();

            await _cache.SetAsync(cacheKey, (IEnumerable<ReviewerDTO>)result, TimeSpan.FromMinutes(1));
            return result;
        }
        public async Task<ReviewerDTO?> GetByIdAsync(int id)
        {
            var cacheKey = $"reviewer_{id}";
            var cached = await _cache.GetAsync<ReviewerDTO>(cacheKey);
            if (cached != null)
            {
                return cached;
            }

            var reviewer = await _context.Reviewers
                .AsNoTracking()
                .Where(r => r.Id == id)
                .Select(r => new ReviewerDTO
                {
                    Id = r.Id,
                    Name = r.Name,
                    Reviews = r.Reviews!.Select(review => review.Text!).ToList()
                })
                .FirstOrDefaultAsync();

            if (reviewer != null)
            {
                await _cache.SetAsync(cacheKey, reviewer, TimeSpan.FromMinutes(30));
            }
            return reviewer;
        }
        public async Task<ReviewerDTO> CreateAsync(CreateReviewerDTO reviewer)
        {
            var newReviewer = new Reviewer
            {
                Name = reviewer.Name
            };
            _context.Reviewers.Add(newReviewer);
            await _context.SaveChangesAsync();

            await _cache.RemoveAsync("reviewers_all");

            return new ReviewerDTO
            {
                Id = newReviewer.Id,
                Name = newReviewer.Name,
                Reviews = new List<string>()
            };
        }
        public async Task<bool> ExistsAsync(string name)
        {
            return await _context.Reviewers
                .AnyAsync(r => r.Name.ToLower() == name.ToLower());
        }
    }
}
