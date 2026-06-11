using Microsoft.EntityFrameworkCore;
using Library.DataContext;
using Library.DTOs;
using Library.Models;

namespace Library.Repositories
{
    public class ReviewerRepository : IReviewerRepository
    {
        private readonly LibraryContext _context;

        public ReviewerRepository(LibraryContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<ReviewerDTO>> GetAllAsync()
        {
            return await _context.Reviewers
                .Include(r => r.Reviews)
                .Select(r => new ReviewerDTO
                {
                    Id = r.Id,
                    Name = r.Name,
                    Reviews = r.Reviews!.Select(review => review.Text!).ToList()
                })
                .ToListAsync();
        }
        public async Task<ReviewerDTO?> GetByIdAsync(int id)
        {
            return await _context.Reviewers
                .Include(r => r.Reviews)
                .Where(r => r.Id == id)
                .Select(r => new ReviewerDTO
                {
                    Id = r.Id,
                    Name = r.Name,
                    Reviews = r.Reviews!.Select(review => review.Text!).ToList()
                })
                .FirstOrDefaultAsync();
        }
        public async Task<ReviewerDTO> CreateAsync(CreateReviewerDTO reviewer)
        {
            var newReviewer = new Reviewer
            {
                Name = reviewer.Name
            };
            _context.Reviewers.Add(newReviewer);
            await _context.SaveChangesAsync();
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
