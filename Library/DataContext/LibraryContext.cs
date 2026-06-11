using Library.DTOs;
using Library.Logging;
using Microsoft.EntityFrameworkCore;
namespace Library.DataContext
{
    public class LibraryContext : DbContext
    {
        public LibraryContext(DbContextOptions<LibraryContext> options) : base(options) { }

        public DbSet<Models.Book> Books { get; set; }
        public DbSet<Models.Author> Authors { get; set; }
        public DbSet<Models.Genre> Genres { get; set; }
        public DbSet<Models.BookGenre> BookGenres { get; set; }
        public DbSet<Models.Country> Country { get; set; }
        public DbSet<Models.Review> Reviews { get; set; }
        public DbSet<Models.Reviewer> Reviewers { get; set; }
        public DbSet<Models.User> Users { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<Models.BookTranslation> BookTranslations { get; set; }
        public DbSet<Models.GenreTranslation> GenreTranslations { get; set; }
        public DbSet<Models.AuthorTranslation> AuthorTranslations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Models.BookGenre>()
                .HasKey(book => new { book.BookId, book.GenreId });

            modelBuilder.Entity<Models.Book>()
                .HasIndex(b => new { b.Title, b.AuthorId })
                .IsUnique();

            modelBuilder.Entity<Models.BookTranslation>()
                .HasIndex(bt => new { bt.BookId, bt.LanguageCode })
                .IsUnique();

            modelBuilder.Entity<Models.GenreTranslation>()
                .HasIndex(gt => new { gt.GenreId, gt.LanguageCode })
                .IsUnique();

            modelBuilder.Entity<Models.AuthorTranslation>()
                .HasIndex(at => new { at.AuthorId, at.LanguageCode })
                .IsUnique();

            modelBuilder.Entity<Models.Review>()
                .Property(r => r.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<Models.User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<Models.User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Models.User>()
                .Property(u => u.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");
        }
    }
}
