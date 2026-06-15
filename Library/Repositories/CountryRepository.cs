using Microsoft.EntityFrameworkCore;
using Library.DataContext;
using Library.DTOs;
using Library.Models;
using Library.Services;
using System;

namespace Library.Repositories
{
    public class CountryRepository : ICountryRepository
    {
        private readonly LibraryContext _context;
        private readonly ICacheService _cache;

        public CountryRepository(LibraryContext context, ICacheService cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<IEnumerable<CountryDTO>> GetAllAsync()
        {
            var cacheKey = "countries_all";

            var cachedCountries = await _cache.GetAsync<IEnumerable<CountryDTO>>(cacheKey);
            if (cachedCountries != null)
            {
                return cachedCountries;
            }

            var countries = await _context.Country
                .Select(c => new CountryDTO
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .ToListAsync();

            await _cache.SetAsync(cacheKey, countries, TimeSpan.FromMinutes(30));
            return countries;
        }

        public async Task<CountryDTO?> GetByIdAsync(int id)
        {
            var cacheKey = $"country_{id}";

            var cachedCountry = await _cache.GetAsync<CountryDTO>(cacheKey);
            if (cachedCountry != null)
            {
                return cachedCountry;
            }

            var country = await _context.Country
                .Where(c => c.Id == id)
                .Select(c => new CountryDTO
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .FirstOrDefaultAsync();

            if (country != null)
            {
                await _cache.SetAsync(cacheKey, country, TimeSpan.FromMinutes(30));
            }
            return country;
        }

        public async Task<CountryDTO> CreateAsync(CreateCountryDTO country)
        {
            var newCountry = new Country
            {
                Name = country.Name
            };

            _context.Country.Add(newCountry);
            await _context.SaveChangesAsync();

            // Invalidate cache
            await _cache.RemoveAsync("countries_all");

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