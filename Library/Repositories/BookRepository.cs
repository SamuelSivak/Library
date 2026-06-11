using Library.DataContext;
using Microsoft.EntityFrameworkCore;
using Library.Models;
using Library.DTOs;
using Library.Services;

namespace Library.Repositories
{
    public class BookRepository : IBookRepository
    {
        private readonly LibraryContext _context;
        private readonly ILanguageService _languageService;

        public BookRepository(LibraryContext context, ILanguageService languageService)
        {
            _context = context;
            _languageService = languageService;
        }
        
        public async Task<IEnumerable<BookDTO>> GetAllAsync(string? search = null, string? genre = null)
        {
            var currentLang = _languageService.GetCurrentLanguage();
            var query = _context.Books.AsQueryable();

            if (!string.IsNullOrWhiteSpace(genre))
            {
                query = query.Where(b => b.BookGenres!.Any(bg => 
                    bg.Genre!.Name.ToLower() == genre.ToLower() ||
                    bg.Genre.Translations!.Any(t => t.LanguageCode == currentLang && t.Name.ToLower() == genre.ToLower())
                ));
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(b => 
                    b.Title.ToLower().Contains(searchLower) ||
                    b.Translations!.Any(t => t.LanguageCode == currentLang && t.Title.ToLower().Contains(searchLower)) ||
                    (b.Author != null && (
                        (b.Author.Name + " " + b.Author.Surname).ToLower().Contains(searchLower) ||
                        b.Author.Translations!.Any(t => t.LanguageCode == currentLang && (t.Name + " " + t.Surname).ToLower().Contains(searchLower))
                    )) ||
                    b.BookGenres!.Any(bg => 
                        bg.Genre!.Name.ToLower().Contains(searchLower) ||
                        bg.Genre.Translations!.Any(t => t.LanguageCode == currentLang && t.Name.ToLower().Contains(searchLower))
                    )
                );
            }

            return await query
                .Select(b => new BookDTO
                {
                    Id = b.Id,
                    Title = b.Translations!.FirstOrDefault(t => t.LanguageCode == currentLang) != null 
                        ? b.Translations!.FirstOrDefault(t => t.LanguageCode == currentLang)!.Title 
                        : b.Title,
                    Description = b.Translations!.FirstOrDefault(t => t.LanguageCode == currentLang) != null 
                        ? b.Translations!.FirstOrDefault(t => t.LanguageCode == currentLang)!.Description 
                        : b.Description,
                    ISBN = b.ISBN,
                    PageCount = b.PageCount,
                    ImageUrl = b.ImageUrl,
                    AuthorName = b.Author != null 
                        ? (b.Author.Translations!.FirstOrDefault(t => t.LanguageCode == currentLang) != null
                            ? $"{b.Author.Translations!.FirstOrDefault(t => t.LanguageCode == currentLang)!.Name} {b.Author.Translations!.FirstOrDefault(t => t.LanguageCode == currentLang)!.Surname}"
                            : $"{b.Author.Name} {b.Author.Surname}")
                        : "Unknown Author",
                    Published = b.Published,
                    Genres = b.BookGenres!.Select(bg => 
                        bg.Genre!.Translations!.FirstOrDefault(t => t.LanguageCode == currentLang) != null
                            ? bg.Genre.Translations!.FirstOrDefault(t => t.LanguageCode == currentLang)!.Name
                            : bg.Genre!.Name
                    ).ToList(),
                    Reviews = b.Reviews!.Select(r => new ReviewDTO
                    {
                        Id = r.Id,
                        Text = r.Text!,
                        Rating = r.Rating,
                        CreatedAt = r.CreatedAt,
                        BookId = r.BookId,
                        ReviewerId = r.ReviewerId,
                        ReviewerName = r.Reviewer != null ? r.Reviewer.Name : "Anonymous"
                    }).ToList(),
                    AverageRating = b.Reviews!.Any() ? Math.Round(b.Reviews!.Average(r => r.Rating), 1) : 0
                }).ToListAsync();
        }

        public async Task<BookDTO?> GetByIdAsync(int id)
        {
            var currentLang = _languageService.GetCurrentLanguage();
            return await _context.Books
                .Where(b => b.Id == id)
                .Select(b => new BookDTO
            {
                Id = b.Id,
                Title = b.Translations!.FirstOrDefault(t => t.LanguageCode == currentLang) != null 
                    ? b.Translations!.FirstOrDefault(t => t.LanguageCode == currentLang)!.Title 
                    : b.Title,
                Description = b.Translations!.FirstOrDefault(t => t.LanguageCode == currentLang) != null 
                    ? b.Translations!.FirstOrDefault(t => t.LanguageCode == currentLang)!.Description 
                    : b.Description,
                ISBN = b.ISBN,
                PageCount = b.PageCount,
                ImageUrl = b.ImageUrl,
                AuthorName = b.Author != null 
                    ? (b.Author.Translations!.FirstOrDefault(t => t.LanguageCode == currentLang) != null
                        ? $"{b.Author.Translations!.FirstOrDefault(t => t.LanguageCode == currentLang)!.Name} {b.Author.Translations!.FirstOrDefault(t => t.LanguageCode == currentLang)!.Surname}"
                        : $"{b.Author.Name} {b.Author.Surname}")
                    : "Unknown Author",
                Published = b.Published,
                Genres = b.BookGenres!.Select(bg => 
                    bg.Genre!.Translations!.FirstOrDefault(t => t.LanguageCode == currentLang) != null
                        ? bg.Genre.Translations!.FirstOrDefault(t => t.LanguageCode == currentLang)!.Name
                        : bg.Genre!.Name
                ).ToList(),
                Reviews = b.Reviews!.Select(r => new ReviewDTO
                {
                    Id = r.Id,
                    Text = r.Text!,
                    Rating = r.Rating,
                    CreatedAt = r.CreatedAt,
                    BookId = r.BookId,
                    ReviewerId = r.ReviewerId,
                    ReviewerName = r.Reviewer != null ? r.Reviewer.Name : "Anonymous"
                }).ToList(),
                AverageRating = b.Reviews!.Any() ? Math.Round(b.Reviews!.Average(r => r.Rating), 1) : 0
            }).FirstOrDefaultAsync();
        }

        public async Task<BookDTO> CreateAsync(CreateBookDTO book)
        {
            var newBook = new Book
            { 
                Title = book.Title,
                Description = book.Description,
                ISBN = book.ISBN,
                PageCount = book.PageCount,
                ImageUrl = book.ImageUrl,
                Published = book.Published,
                AuthorId = book.AuthorId,
                BookGenres = book.GenreIds!.Select(id => new BookGenre
                {
                    GenreId = id
                }).ToList()
            };

            _context.Books.Add(newBook);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(newBook.Id)
                ?? throw new InvalidOperationException($"Book with id {newBook.Id} not found after creation.");
        }

        public async Task<BookDTO?> UpdateAsync(int id, UpdateBookDTO book)
        {
            var existingBook = await _context.Books
                .Include(b => b.BookGenres)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (existingBook == null) return null;

            existingBook.Title = book.Title;
            existingBook.Description = book.Description;
            existingBook.ISBN = book.ISBN;
            existingBook.PageCount = book.PageCount;
            existingBook.ImageUrl = book.ImageUrl;
            existingBook.Published = book.Published;
            existingBook.AuthorId = book.AuthorId;

            // Clear old genres and add new ones
            if (existingBook.BookGenres != null)
            {
                _context.BookGenres.RemoveRange(existingBook.BookGenres);
            }

            existingBook.BookGenres = book.GenreIds?.Select(gid => new BookGenre
            {
                BookId = id,
                GenreId = gid
            }).ToList() ?? new List<BookGenre>();

            await _context.SaveChangesAsync();
            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return false;

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(string title, int authorId)
        {
            return await _context.Books
                .AnyAsync(b => b.Title.ToLower() == title.ToLower() && b.AuthorId == authorId);
        }
    }
}