using Library.DataContext;
using Library.DTOs;
using Microsoft.EntityFrameworkCore;
using Library.Models;
using Library.Services;

namespace Library.Repositories
{
    public class AuthorRepository : IAuthorRepository
    {
        private readonly LibraryContext _context;
        private readonly ILanguageService _languageService;
        private readonly ICacheService _cache;

        public AuthorRepository(LibraryContext context, ILanguageService languageService, ICacheService cache)
        {
            _context = context;
            _languageService = languageService;
            _cache = cache;
        }

        public async Task<IEnumerable<AuthorDTO>> GetAllAsync()
        {
            var currentLang = _languageService.GetCurrentLanguage();
            var cacheKey = $"authors_all_{currentLang}";
            var cached = await _cache.GetAsync<IEnumerable<AuthorDTO>>(cacheKey);
            if (cached != null)
            {
                return cached;
            }

            var result = await _context.Authors
                .Select(a => new AuthorDTO
                {
                    Id = a.Id,
                    Name = a.Translations!.FirstOrDefault(t => t.LanguageCode == currentLang) != null
                        ? a.Translations!.FirstOrDefault(t => t.LanguageCode == currentLang)!.Name
                        : a.Name,
                    Surname = a.Translations!.FirstOrDefault(t => t.LanguageCode == currentLang) != null
                        ? a.Translations!.FirstOrDefault(t => t.LanguageCode == currentLang)!.Surname
                        : a.Surname,
                    Country = a.Country!.Name,
                    Books = a.Books!.Select(b => 
                        b.Translations!.FirstOrDefault(t => t.LanguageCode == currentLang) != null
                            ? b.Translations!.FirstOrDefault(t => t.LanguageCode == currentLang)!.Title
                            : b.Title
                    ).ToList()
                })
                .ToListAsync();

            await _cache.SetAsync(cacheKey, (IEnumerable<AuthorDTO>)result, TimeSpan.FromMinutes(30));
            return result;
        }

        public async Task<AuthorDTO?> GetByIdAsync(int id)
        {
            var currentLang = _languageService.GetCurrentLanguage();
            var cacheKey = $"author_{id}_{currentLang}";
            var cached = await _cache.GetAsync<AuthorDTO>(cacheKey);
            if (cached != null)
            {
                return cached;
            }

            var author = await _context.Authors
                .Where(a => a.Id == id)
                .Select(a => new AuthorDTO
                {
                    Id = a.Id,
                    Name = a.Translations!.FirstOrDefault(t => t.LanguageCode == currentLang) != null
                        ? a.Translations!.FirstOrDefault(t => t.LanguageCode == currentLang)!.Name
                        : a.Name,
                    Surname = a.Translations!.FirstOrDefault(t => t.LanguageCode == currentLang) != null
                        ? a.Translations!.FirstOrDefault(t => t.LanguageCode == currentLang)!.Surname
                        : a.Surname,
                    Country = a.Country!.Name,
                    Books = a.Books!.Select(b => 
                        b.Translations!.FirstOrDefault(t => t.LanguageCode == currentLang) != null
                            ? b.Translations!.FirstOrDefault(t => t.LanguageCode == currentLang)!.Title
                            : b.Title
                    ).ToList()
                })
                .FirstOrDefaultAsync();

            if (author != null)
            {
                await _cache.SetAsync(cacheKey, author, TimeSpan.FromMinutes(30));
            }
            return author;
        }

        public async Task<AuthorDTO> CreateAsync(CreateAuthorDTO author)
        {
            var newAuthor = new Author
            {
                Name = author.Name,
                Surname = author.Surname,
                CountryId = author.CountryId
            };
            _context.Authors.Add(newAuthor);
            await _context.SaveChangesAsync();

            foreach (var lang in new[] { "SK", "EN", "GR" })
            {
                await _cache.RemoveAsync($"authors_all_{lang}");
            }

            return (await GetByIdAsync(newAuthor.Id))!;
        }
        public async Task<bool> ExistsAsync(string name, string surname)
        {
            return await _context.Authors
                .AnyAsync(a => a.Name.ToLower() == name.ToLower() && a.Surname.ToLower() == surname.ToLower());
        }
    }
}