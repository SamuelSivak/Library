using Library.DTOs;

namespace Library.Repositories
{
    public interface ICountryRepository
    {
        Task<IEnumerable<CountryDTO>> GetAllAsync();
        Task<CountryDTO?> GetByIdAsync(int id);
        Task<CountryDTO> CreateAsync(CreateCountryDTO country);
        Task<bool> ExistsAsync(string name);    
    }

}
