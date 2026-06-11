using Library.DTOs;

namespace Library.Repositories
{
    public interface IReviewRepository
    {
        Task<IEnumerable<ReviewDTO>> GetAllAsync();
        Task<ReviewDTO?> GetByIdAsync(int id);
        Task<ReviewDTO> CreateAsync(CreateReviewDTO review, string username);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(string text);
    }
}
