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

        public AuthorRepository(LibraryContext context, ILanguageService languageService)
        {
            _context = context;
            _languageService = languageService;
        }

        public async Task<IEnumerable<AuthorDTO>> GetAllAsync()
        {
            var currentLang = _languageService.GetCurrentLanguage();
            return await _context.Authors
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
        }

        public async Task<AuthorDTO?> GetByIdAsync(int id)
        {
            var currentLang = _languageService.GetCurrentLanguage();
            return await _context.Authors
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
            return (await GetByIdAsync(newAuthor.Id))!;
        }
        public async Task<bool> ExistsAsync(string name, string surname)
        {
            return await _context.Authors
                .AnyAsync(a => a.Name.ToLower() == name.ToLower() && a.Surname.ToLower() == surname.ToLower());
        }
    }
}