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
        private readonly ICacheService _cache;

        public BookRepository(LibraryContext context, ILanguageService languageService, ICacheService cache)
        {
            _context = context;
            _languageService = languageService;
            _cache = cache;
        }
        
        public async Task<IEnumerable<BookDTO>> GetAllAsync(string? search = null, string? genre = null, int page = 1, int pageSize = 20, string? sortBy = null)
        {
            var currentLang = _languageService.GetCurrentLanguage();
            var cacheKey = $"books_all_{search ?? ""}_{genre ?? ""}_{page}_{pageSize}_{sortBy ?? ""}_{currentLang}";
            var cached = await _cache.GetAsync<IEnumerable<BookDTO>>(cacheKey);
            if (cached != null)
            {
                return cached;
            }

            List<int> bookIds;

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                var searchPattern = $"\"{search}*\"";

                var matchingAuthorIds = await _context.Authors
                    .AsNoTracking()
                    .Where(a => 
                        (a.Name + " " + a.Surname).ToLower().Contains(searchLower) ||
                        a.Translations!.Any(t => t.LanguageCode == currentLang && (t.Name + " " + t.Surname).ToLower().Contains(searchLower))
                    )
                    .Select(a => a.Id)
                    .ToListAsync();

                var matchingGenreIds = await _context.Genres
                    .AsNoTracking()
                    .Where(g => 
                        g.Name.ToLower().Contains(searchLower) ||
                        g.Translations!.Any(t => t.LanguageCode == currentLang && t.Name.ToLower().Contains(searchLower))
                    )
                    .Select(g => g.Id)
                    .ToListAsync();

                var q1 = _context.Books.AsNoTracking()
                    .Where(b => EF.Functions.Contains(b.Title, searchPattern))
                    .Select(b => b.Id);

                var q2 = _context.BookTranslations.AsNoTracking()
                    .Where(t => t.LanguageCode == currentLang && EF.Functions.Contains(t.Title, searchPattern))
                    .Select(t => t.BookId);

                var q3 = _context.Books.AsNoTracking()
                    .Where(b => matchingAuthorIds.Contains(b.AuthorId))
                    .Select(b => b.Id);

                var q4 = _context.BookGenres.AsNoTracking()
                    .Where(bg => matchingGenreIds.Contains(bg.GenreId))
                    .Select(bg => bg.BookId);

                var combinedQuery = q1.Union(q2).Union(q3).Union(q4);

                var bookQuery = _context.Books.AsNoTracking().Where(b => combinedQuery.Contains(b.Id));

                if (!string.IsNullOrWhiteSpace(genre))
                {
                    bookQuery = bookQuery.Where(b => b.BookGenres!.Any(bg => 
                        bg.Genre!.Name.ToLower() == genre.ToLower() ||
                        bg.Genre.Translations!.Any(t => t.LanguageCode == currentLang && t.Name.ToLower() == genre.ToLower())
                    ));
                }

                var skipValue = (page - 1) * pageSize;
                var exactMatchOrdered = bookQuery.OrderByDescending(b => 
                    b.Title.ToLower() == searchLower || 
                    b.Translations!.Any(t => t.LanguageCode == currentLang && t.Title.ToLower() == searchLower)
                );

                bookIds = await ApplySorting(exactMatchOrdered, sortBy, currentLang, true)
                    .Select(b => b.Id)
                    .Skip(skipValue)
                    .Take(pageSize)
                    .ToListAsync();
            }
            else
            {
                var query = _context.Books.AsNoTracking().AsQueryable();
                if (!string.IsNullOrWhiteSpace(genre))
                {
                    query = query.Where(b => b.BookGenres!.Any(bg => 
                        bg.Genre!.Name.ToLower() == genre.ToLower() ||
                        bg.Genre.Translations!.Any(t => t.LanguageCode == currentLang && t.Name.ToLower() == genre.ToLower())
                    ));
                }

                var skipValue = (page - 1) * pageSize;
                bookIds = await ApplySorting(query, sortBy, currentLang, false)
                    .Select(b => b.Id)
                    .Skip(skipValue)
                    .Take(pageSize)
                    .ToListAsync();
            }

            if (!bookIds.Any())
            {
                return Enumerable.Empty<BookDTO>();
            }

            
            var books = await _context.Books.AsNoTracking()
                .Include(b => b.Analytics)
                .Include(b => b.Author)
                    .ThenInclude(a => a!.Translations)
                .Include(b => b.BookGenres!)
                    .ThenInclude(bg => bg.Genre!)
                        .ThenInclude(g => g.Translations)
                .Include(b => b.Translations)
                .Where(b => bookIds.Contains(b.Id))
                .ToListAsync();

            
            var booksMap = books.ToDictionary(b => b.Id);
            var sortedBooks = bookIds
                .Where(id => booksMap.ContainsKey(id))
                .Select(id => booksMap[id]);

            var result = sortedBooks.Select(b => new BookDTO
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
                Reviews = null,
                AverageRating = b.Analytics != null ? b.Analytics.AverageRating : 0
            }).ToList();

            await _cache.SetAsync(cacheKey, (IEnumerable<BookDTO>)result, TimeSpan.FromSeconds(15));
            return result;
        }

        public async Task<BookDTO?> GetByIdAsync(int id)
        {
            var currentLang = _languageService.GetCurrentLanguage();
            var cacheKey = $"book_detail_{id}_{currentLang}";
            var cached = await _cache.GetAsync<BookDTO>(cacheKey);
            if (cached != null)
            {
                return cached;
            }

            var book = await _context.Books
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

            if (book != null)
            {
                await _cache.SetAsync(cacheKey, book, TimeSpan.FromSeconds(15));
            }
            return book;
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
            await InvalidateCacheAsync(newBook.Id);

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
            await InvalidateCacheAsync(id);
            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return false;

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            await InvalidateCacheAsync(id);
            return true;
        }

        private async Task InvalidateCacheAsync(int? bookId = null)
        {
            foreach (var lang in new[] { "SK", "EN", "GR" })
            {
                await _cache.RemoveAsync($"books_count___{lang}");
                await _cache.RemoveAsync($"books_all____1_20__{lang}");
                await _cache.RemoveAsync($"books_all____1_20_rating_{lang}");
                await _cache.RemoveAsync($"books_all____1_20_popularity_{lang}");
                if (bookId.HasValue)
                {
                    await _cache.RemoveAsync($"book_detail_{bookId.Value}_{lang}");
                }
            }
        }

        public async Task<bool> ExistsAsync(string title, int authorId)
        {
            return await _context.Books
                .AnyAsync(b => b.Title.ToLower() == title.ToLower() && b.AuthorId == authorId);
        }

        public async Task<int> GetTotalCountAsync(string? search = null, string? genre = null)
        {
            var currentLang = _languageService.GetCurrentLanguage();
            var cacheKey = $"books_count_{search ?? ""}_{genre ?? ""}_{currentLang}";
            var cachedCount = await _cache.GetAsync<int?>(cacheKey);
            if (cachedCount.HasValue)
            {
                return cachedCount.Value;
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                var searchPattern = $"\"{search}*\"";

                
                var matchingAuthorIds = await _context.Authors
                    .AsNoTracking()
                    .Where(a => 
                        (a.Name + " " + a.Surname).ToLower().Contains(searchLower) ||
                        a.Translations!.Any(t => t.LanguageCode == currentLang && (t.Name + " " + t.Surname).ToLower().Contains(searchLower))
                    )
                    .Select(a => a.Id)
                    .ToListAsync();

                
                var matchingGenreIds = await _context.Genres
                    .AsNoTracking()
                    .Where(g => 
                        g.Name.ToLower().Contains(searchLower) ||
                        g.Translations!.Any(t => t.LanguageCode == currentLang && t.Name.ToLower().Contains(searchLower))
                    )
                    .Select(g => g.Id)
                    .ToListAsync();

                
                var q1 = _context.Books.AsNoTracking()
                    .Where(b => EF.Functions.Contains(b.Title, searchPattern))
                    .Select(b => b.Id);

                var q2 = _context.BookTranslations.AsNoTracking()
                    .Where(t => t.LanguageCode == currentLang && EF.Functions.Contains(t.Title, searchPattern))
                    .Select(t => t.BookId);

                var q3 = _context.Books.AsNoTracking()
                    .Where(b => matchingAuthorIds.Contains(b.AuthorId))
                    .Select(b => b.Id);

                var q4 = _context.BookGenres.AsNoTracking()
                    .Where(bg => matchingGenreIds.Contains(bg.GenreId))
                    .Select(bg => bg.BookId);

                
                var combinedQuery = q1.Union(q2).Union(q3).Union(q4);

                var bookQuery = _context.Books.AsNoTracking().Where(b => combinedQuery.Contains(b.Id));

                
                if (!string.IsNullOrWhiteSpace(genre))
                {
                    bookQuery = bookQuery.Where(b => b.BookGenres!.Any(bg => 
                        bg.Genre!.Name.ToLower() == genre.ToLower() ||
                        bg.Genre.Translations!.Any(t => t.LanguageCode == currentLang && t.Name.ToLower() == genre.ToLower())
                    ));
                }

                var count = await bookQuery.CountAsync();
                await _cache.SetAsync(cacheKey, (int?)count, TimeSpan.FromSeconds(15));
                return count;
            }
            else
            {
                
                var query = _context.Books.AsNoTracking().AsQueryable();
                if (!string.IsNullOrWhiteSpace(genre))
                {
                    query = query.Where(b => b.BookGenres!.Any(bg => 
                        bg.Genre!.Name.ToLower() == genre.ToLower() ||
                        bg.Genre.Translations!.Any(t => t.LanguageCode == currentLang && t.Name.ToLower() == genre.ToLower())
                    ));
                }

                var count = await query.CountAsync();
                await _cache.SetAsync(cacheKey, (int?)count, TimeSpan.FromSeconds(15));
                return count;
            }
        }

        public async Task<IEnumerable<BookDTO>> GetTopRatedAsync(int limit)
        {
            var currentLang = _languageService.GetCurrentLanguage();
            return await _context.BiBookAnalytics
                .Include(ba => ba.Book)
                .ThenInclude(b => b!.Author)
                .OrderByDescending(ba => ba.AverageRating)
                .ThenByDescending(ba => ba.TotalReviews)
                .Take(limit)
                .Select(ba => new BookDTO
                {
                    Id = ba.BookId,
                    Title = ba.Book!.Translations!.FirstOrDefault(t => t.LanguageCode == currentLang) != null 
                        ? ba.Book.Translations!.FirstOrDefault(t => t.LanguageCode == currentLang)!.Title 
                        : ba.Book.Title,
                    Description = ba.Book.Translations!.FirstOrDefault(t => t.LanguageCode == currentLang) != null 
                        ? ba.Book.Translations!.FirstOrDefault(t => t.LanguageCode == currentLang)!.Description 
                        : ba.Book.Description,
                    ISBN = ba.Book.ISBN,
                    PageCount = ba.Book.PageCount,
                    ImageUrl = ba.Book.ImageUrl,
                    AuthorName = ba.Book.Author != null 
                        ? (ba.Book.Author.Translations!.FirstOrDefault(t => t.LanguageCode == currentLang) != null
                            ? $"{ba.Book.Author.Translations!.FirstOrDefault(t => t.LanguageCode == currentLang)!.Name} {ba.Book.Author.Translations!.FirstOrDefault(t => t.LanguageCode == currentLang)!.Surname}"
                            : $"{ba.Book.Author.Name} {ba.Book.Author.Surname}")
                        : "Unknown Author",
                    Published = ba.Book.Published,
                    Genres = ba.Book.BookGenres!.Select(bg => 
                        bg.Genre!.Translations!.FirstOrDefault(t => t.LanguageCode == currentLang) != null
                            ? bg.Genre.Translations!.FirstOrDefault(t => t.LanguageCode == currentLang)!.Name
                            : bg.Genre!.Name
                    ).ToList(),
                    Reviews = null,
                    AverageRating = ba.AverageRating
                })
                .ToListAsync();
        }

        private IOrderedQueryable<Book> ApplySorting(IQueryable<Book> query, string? sortBy, string currentLang, bool isSecondary)
        {
            sortBy = sortBy?.ToLower() ?? "rating";

            if (isSecondary)
            {
                var ordered = (IOrderedQueryable<Book>)query;
                return sortBy switch
                {
                    "popularity" => ordered.ThenByDescending(b => b.Analytics != null ? b.Analytics.Clicks : 0),
                    "rating" => ordered.ThenByDescending(b => b.Analytics != null ? b.Analytics.AverageRating : 0.0),
                    "positivereviews" => ordered.ThenByDescending(b => b.Analytics != null ? b.Analytics.PositiveReviews : 0),
                    "negativereviews" => ordered.ThenByDescending(b => b.Analytics != null ? b.Analytics.NegativeReviews : 0),
                    "published" => ordered.ThenByDescending(b => b.Published),
                    "pages" => ordered.ThenByDescending(b => b.PageCount),
                    "alphabetical" => ordered.ThenBy(b => b.Translations!.FirstOrDefault(t => t.LanguageCode == currentLang) != null 
                        ? b.Translations!.FirstOrDefault(t => t.LanguageCode == currentLang)!.Title 
                        : b.Title),
                    _ => ordered.ThenByDescending(b => b.Analytics != null ? b.Analytics.AverageRating : 0.0)
                };
            }
            else
            {
                return sortBy switch
                {
                    "popularity" => query.OrderByDescending(b => b.Analytics != null ? b.Analytics.Clicks : 0),
                    "rating" => query.OrderByDescending(b => b.Analytics != null ? b.Analytics.AverageRating : 0.0),
                    "positivereviews" => query.OrderByDescending(b => b.Analytics != null ? b.Analytics.PositiveReviews : 0),
                    "negativereviews" => query.OrderByDescending(b => b.Analytics != null ? b.Analytics.NegativeReviews : 0),
                    "published" => query.OrderByDescending(b => b.Published),
                    "pages" => query.OrderByDescending(b => b.PageCount),
                    "alphabetical" => query.OrderBy(b => b.Translations!.FirstOrDefault(t => t.LanguageCode == currentLang) != null 
                        ? b.Translations!.FirstOrDefault(t => t.LanguageCode == currentLang)!.Title 
                        : b.Title),
                    _ => query.OrderByDescending(b => b.Analytics != null ? b.Analytics.AverageRating : 0.0)
                };
            }
        }
    }
}