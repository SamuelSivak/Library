using Library.DTOs;

namespace Library.Repositories
{
    public interface IBookRepository
    {
        Task<IEnumerable<BookDTO>> GetAllAsync(string? search = null, string? genre = null, int page = 1, int pageSize = 20, string? sortBy = null);
        Task<int> GetTotalCountAsync(string? search = null, string? genre = null);
        Task<BookDTO?> GetByIdAsync(int id);
        Task<BookDTO> CreateAsync(CreateBookDTO book);
        Task<BookDTO?> UpdateAsync(int id, UpdateBookDTO book);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(string title, int authorId);
        Task<IEnumerable<BookDTO>> GetTopRatedAsync(int limit);
    }
}
