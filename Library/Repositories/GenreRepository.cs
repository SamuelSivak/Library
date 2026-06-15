using Library.DataContext;
using Library.DTOs;
using Library.Models;
using Microsoft.EntityFrameworkCore;
using Library.Services;

namespace Library.Repositories
{
    public class GenreRepository : IGenreRepository
    {
        private readonly LibraryContext _context;
        private readonly ILanguageService _languageService;
        private readonly ICacheService _cache;

        public GenreRepository(LibraryContext context, ILanguageService languageService, ICacheService cache)
        {
            _context = context;
            _languageService = languageService;
            _cache = cache;
        }

        public async Task<IEnumerable<GenreDTO>> GetAllAsync(bool includeEmpty = false)
        {   
            var currentLang = _languageService.GetCurrentLanguage();
            var cacheKey = $"genres_all_{currentLang}_{includeEmpty.ToString().ToLower()}";

            var cachedGenres = await _cache.GetAsync<IEnumerable<GenreDTO>>(cacheKey);
            if (cachedGenres != null)
            {
                return cachedGenres;
            }

            IQueryable<Genre> query = _context.Genres;
            if (!includeEmpty)
            {
                query = query.Where(g => g.BookGenres != null && g.BookGenres.Any());
            }
            
            var genres = await query
                .Select(c => new GenreDTO
                {
                    Id = c.Id,
                    Name = c.Translations!.FirstOrDefault(t => t.LanguageCode == currentLang) != null
                        ? c.Translations!.FirstOrDefault(t => t.LanguageCode == currentLang)!.Name
                        : c.Name
                }).ToListAsync();

            await _cache.SetAsync(cacheKey, genres, TimeSpan.FromMinutes(30));
            return genres;
        }

        public async Task<GenreDTO?> GetByIdAsync(int id)
        {
            var currentLang = _languageService.GetCurrentLanguage();
            var cacheKey = $"genre_{id}_{currentLang}";

            var cachedGenre = await _cache.GetAsync<GenreDTO>(cacheKey);
            if (cachedGenre != null)
            {
                return cachedGenre;
            }

            var genre = await _context.Genres
                .Where(c => c.Id == id)
                .Select(c => new GenreDTO
                {
                    Id = c.Id,
                    Name = c.Translations!.FirstOrDefault(t => t.LanguageCode == currentLang) != null
                        ? c.Translations!.FirstOrDefault(t => t.LanguageCode == currentLang)!.Name
                        : c.Name
                }).FirstOrDefaultAsync();

            if (genre != null)
            {
                await _cache.SetAsync(cacheKey, genre, TimeSpan.FromMinutes(30));
            }
            return genre;
        }

        public async Task<GenreDTO> CreateAsync(CreateGenreDTO genre)
        {
            var newGenre = new Genre
            {
                Name = genre.Name
            };
            _context.Genres.Add(newGenre);
            await _context.SaveChangesAsync();

            // Invalidate caches
            var languages = new[] { "SK", "GR", "EL", "EN" };
            foreach (var lang in languages)
            {
                await _cache.RemoveAsync($"genres_all_{lang}_true");
                await _cache.RemoveAsync($"genres_all_{lang}_false");
            }

            return new GenreDTO
            {
                Id = newGenre.Id,
                Name = newGenre.Name
            };
        }

        public async Task<bool> ExistsAsync(string name)
        {
            return await _context.Genres
                .AnyAsync(g => g.Name.ToLower() == name.ToLower());
        }
    }
}
