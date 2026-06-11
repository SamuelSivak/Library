using Library.DTOs;

namespace Library.Repositories
{
    public interface IGenreRepository
    {
        Task<IEnumerable<GenreDTO>> GetAllAsync(bool includeEmpty = false);
        Task<GenreDTO?> GetByIdAsync(int id);
        Task<GenreDTO> CreateAsync(CreateGenreDTO genre);
        Task<bool> ExistsAsync(string name);
    }
}
