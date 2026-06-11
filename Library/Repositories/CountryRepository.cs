using Microsoft.EntityFrameworkCore;
using Library.DataContext;
using Library.DTOs;
using Library.Models;

namespace Library.Repositories
{
    public class CountryRepository : ICountryRepository
    {
        private readonly LibraryContext _context;

        public CountryRepository(LibraryContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CountryDTO>> GetAllAsync()
        {
            return await _context.Country
                .Select(c => new CountryDTO
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .ToListAsync();
        }

        public async Task<CountryDTO?> GetByIdAsync(int id)
        {
            return await _context.Country
                .Where(c => c.Id == id)
                .Select(c => new CountryDTO
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .FirstOrDefaultAsync();
        }

        public async Task<CountryDTO> CreateAsync(CreateCountryDTO country)
        {
            var newCountry = new Country
            {
                Name = country.Name
            };

            _context.Country.Add(newCountry);
            await _context.SaveChangesAsync();

            return new CountryDTO
            {
                Id = newCountry.Id,
                Name = newCountry.Name
            };
        }
        public async Task<bool> ExistsAsync(string name)
        {
            return await _context.Country
                .AnyAsync(c => c.Name.ToLower() == name.ToLower());
        }
    }
}