using Library.DTOs;
using Library.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Library.DataContext
{
    public class LibraryContext : IdentityDbContext<ApplicationUser>
    {
        public LibraryContext(DbContextOptions<LibraryContext> options) : base(options) { }

        public DbSet<Models.Book> Books { get; set; }
        public DbSet<Models.Author> Authors { get; set; }
        public DbSet<Models.Genre> Genres { get; set; }
        public DbSet<Models.BookGenre> BookGenres { get; set; }
        public DbSet<Models.Country> Country { get; set; }
        public DbSet<Models.Review> Reviews { get; set; }
        public DbSet<Models.Reviewer> Reviewers { get; set; }
        public DbSet<Models.BookTranslation> BookTranslations { get; set; }
        public DbSet<Models.GenreTranslation> GenreTranslations { get; set; }
        public DbSet<Models.AuthorTranslation> AuthorTranslations { get; set; }
        public DbSet<Models.BiBookAnalytics> BiBookAnalytics { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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

            modelBuilder.Entity<Models.BiBookAnalytics>()
                .ToTable("Bi_BookAnalytics")
                .HasKey(book => book.BookId);

            modelBuilder.Entity<Models.BiBookAnalytics>()
                .HasOne(ba => ba.Book)
                .WithOne(b => b.Analytics)
                .HasForeignKey<Models.BiBookAnalytics>(ba => ba.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Models.Book>()
                .HasIndex(b => b.AuthorId);

            modelBuilder.Entity<Models.Review>()
                .HasIndex(r => r.BookId);

            modelBuilder.Entity<Models.Review>()
                .HasIndex(r => r.UserId);
        }
    }
}
