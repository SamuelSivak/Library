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

        public GenreRepository(LibraryContext context, ILanguageService languageService)
        {
            _context = context;
            _languageService = languageService;
        }
        public async Task<IEnumerable<GenreDTO>> GetAllAsync(bool includeEmpty = false)
        {   
            var currentLang = _languageService.GetCurrentLanguage();
            IQueryable<Genre> query = _context.Genres;
            if (!includeEmpty)
            {
                query = query.Where(g => g.BookGenres != null && g.BookGenres.Any());
            }
            return await query
                .Select(c => new GenreDTO
                {
                    Id = c.Id,
                    Name = c.Translations!.FirstOrDefault(t => t.LanguageCode == currentLang) != null
                        ? c.Translations!.FirstOrDefault(t => t.LanguageCode == currentLang)!.Name
                        : c.Name
                }).ToListAsync();
        }
        public async Task<GenreDTO?> GetByIdAsync(int id)
        {
            var currentLang = _languageService.GetCurrentLanguage();
            return await _context.Genres
                .Where(c => c.Id == id)
                .Select(c => new GenreDTO
                {
                    Id = c.Id,
                    Name = c.Translations!.FirstOrDefault(t => t.LanguageCode == currentLang) != null
                        ? c.Translations!.FirstOrDefault(t => t.LanguageCode == currentLang)!.Name
                        : c.Name
                }).FirstOrDefaultAsync();
        }
        public async Task<GenreDTO> CreateAsync(CreateGenreDTO genre)
        {
            var newGenre = new Genre
            {
                Name = genre.Name
            };
            _context.Genres.Add(newGenre);
            await _context.SaveChangesAsync();
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
