using Library.DTOs;

namespace Library.Repositories
{
    public interface IReviewerRepository
    {
        Task<IEnumerable<ReviewerDTO>> GetAllAsync();
        Task<ReviewerDTO?> GetByIdAsync(int id);
        Task<ReviewerDTO> CreateAsync(CreateReviewerDTO reviewer);
        Task<bool> ExistsAsync(string name);
    }
}
