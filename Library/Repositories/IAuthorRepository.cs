using Library.DTOs;

namespace Library.Repositories
{
    public interface IAuthorRepository
    {
        Task<IEnumerable<AuthorDTO>> GetAllAsync();
        Task<AuthorDTO?> GetByIdAsync(int id);
        Task<AuthorDTO> CreateAsync(CreateAuthorDTO author);
        Task<bool> ExistsAsync(string name, string surname);
    }
}
