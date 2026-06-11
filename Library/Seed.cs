using Library.DataContext;
using Library.Models;
using Library.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Library
{
    public static class Seed
    {
        // ── helpers ────────────────────────────────────────────────────────────
        private static async Task<Country> EnsureCountry(LibraryContext ctx, string name)
        {
            var c = await ctx.Country.FirstOrDefaultAsync(x => x.Name == name);
            if (c != null) return c;
            c = new Country { Name = name };
            ctx.Country.Add(c);
            await ctx.SaveChangesAsync();
            return c;
        }

        private static async Task<Genre> EnsureGenre(LibraryContext ctx, string name)
        {
            var g = await ctx.Genres.FirstOrDefaultAsync(x => x.Name == name);
            if (g != null) return g;
            g = new Genre { Name = name };
            ctx.Genres.Add(g);
            await ctx.SaveChangesAsync();
            return g;
        }

        private static async Task<Author> EnsureAuthor(LibraryContext ctx, string name, string surname, int countryId)
        {
            var a = await ctx.Authors.FirstOrDefaultAsync(x => x.Name == name && x.Surname == surname);
            if (a != null) return a;
            a = new Author { Name = name, Surname = surname, CountryId = countryId };
            ctx.Authors.Add(a);
            await ctx.SaveChangesAsync();
            return a;
        }

        private static async Task EnsureBook(LibraryContext ctx, Book book)
        {
            if (await ctx.Books.AnyAsync(b => b.ISBN == book.ISBN)) return;
            ctx.Books.Add(book);
            await ctx.SaveChangesAsync();
        }

        // ── main entry point ───────────────────────────────────────────────────
        public static async Task SeedDataAsync(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<LibraryContext>();

            await context.Database.MigrateAsync();

            try
            {
                await context.Database.ExecuteSqlRawAsync("ALTER DATABASE SCOPED CONFIGURATION SET IDENTITY_CACHE = OFF;");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Seed] Failed to disable identity cache: {ex.Message}");
            }

            // ── Countries ──────────────────────────────────────────────────────
            var cUK   = await EnsureCountry(context, "United Kingdom");
            var cUS   = await EnsureCountry(context, "United States");
            var cRU   = await EnsureCountry(context, "Russia");
            var cFR   = await EnsureCountry(context, "France");
            var cCZ   = await EnsureCountry(context, "Czech Republic");
            var cDE   = await EnsureCountry(context, "Germany");
            var cCO   = await EnsureCountry(context, "Colombia");
            var cIT   = await EnsureCountry(context, "Italy");
            var cIE   = await EnsureCountry(context, "Ireland");
            var cNO   = await EnsureCountry(context, "Norway");
            var cES   = await EnsureCountry(context, "Spain");
            var cJP   = await EnsureCountry(context, "Japan");

            // ── Genres ─────────────────────────────────────────────────────────
            var gClassic    = await EnsureGenre(context, "Classic");
            var gDystopian  = await EnsureGenre(context, "Dystopian");
            var gAdventure  = await EnsureGenre(context, "Adventure");
            var gHistorical = await EnsureGenre(context, "Historical Fiction");
            var gFantasy    = await EnsureGenre(context, "Fantasy");
            var gThriller   = await EnsureGenre(context, "Thriller");
            var gRomance    = await EnsureGenre(context, "Romance");
            var gSciFi      = await EnsureGenre(context, "Science Fiction");
            var gHorror     = await EnsureGenre(context, "Horror");
            var gPhilosophy = await EnsureGenre(context, "Philosophy");
            var gFiction    = await EnsureGenre(context, "Fiction");
            var gMystery    = await EnsureGenre(context, "Mystery");

            // ── Authors ────────────────────────────────────────────────────────
            var orwell      = await EnsureAuthor(context, "George",    "Orwell",          cUK.Id);
            var hemingway   = await EnsureAuthor(context, "Ernest",    "Hemingway",       cUS.Id);
            var tolstoy     = await EnsureAuthor(context, "Leo",       "Tolstoy",         cRU.Id);
            var hugo        = await EnsureAuthor(context, "Victor",    "Hugo",            cFR.Id);
            var kafka       = await EnsureAuthor(context, "Franz",     "Kafka",           cCZ.Id);
            var tolkien     = await EnsureAuthor(context, "J.R.R.",    "Tolkien",         cUK.Id);
            var dostoyevsky = await EnsureAuthor(context, "Fyodor",    "Dostoyevsky",     cRU.Id);
            var chekhov     = await EnsureAuthor(context, "Anton",     "Chekhov",         cRU.Id);
            var marquez     = await EnsureAuthor(context, "Gabriel",   "Garcia Marquez",  cCO.Id);
            var camus       = await EnsureAuthor(context, "Albert",    "Camus",           cFR.Id);
            var sartre      = await EnsureAuthor(context, "Jean-Paul", "Sartre",          cFR.Id);
            var flaubert    = await EnsureAuthor(context, "Gustave",   "Flaubert",        cFR.Id);
            var dickens     = await EnsureAuthor(context, "Charles",   "Dickens",         cUK.Id);
            var austen      = await EnsureAuthor(context, "Jane",      "Austen",          cUK.Id);
            var wilde       = await EnsureAuthor(context, "Oscar",     "Wilde",           cIE.Id);
            var joyce       = await EnsureAuthor(context, "James",     "Joyce",           cIE.Id);
            var steinbeck   = await EnsureAuthor(context, "John",      "Steinbeck",       cUS.Id);
            var fitzgerald  = await EnsureAuthor(context, "F. Scott",  "Fitzgerald",      cUS.Id);
            var faulkner    = await EnsureAuthor(context, "William",   "Faulkner",        cUS.Id);
            var salinger    = await EnsureAuthor(context, "J.D.",      "Salinger",        cUS.Id);
            var hesse       = await EnsureAuthor(context, "Hermann",   "Hesse",           cDE.Id);
            var mann        = await EnsureAuthor(context, "Thomas",    "Mann",            cDE.Id);
            var dumas       = await EnsureAuthor(context, "Alexandre", "Dumas",           cFR.Id);
            var cervantes   = await EnsureAuthor(context, "Miguel de", "Cervantes",       cES.Id);
            var dante       = await EnsureAuthor(context, "Dante",     "Alighieri",       cIT.Id);
            var huxley      = await EnsureAuthor(context, "Aldous",    "Huxley",          cUK.Id);
            var bradbury    = await EnsureAuthor(context, "Ray",       "Bradbury",        cUS.Id);
            var stoker      = await EnsureAuthor(context, "Bram",      "Stoker",          cIE.Id);
            var shelley     = await EnsureAuthor(context, "Mary",      "Shelley",         cUK.Id);
            var ibsen       = await EnsureAuthor(context, "Henrik",    "Ibsen",           cNO.Id);
            var kawabata    = await EnsureAuthor(context, "Yasunari",  "Kawabata",        cJP.Id);

            // ── Books ──────────────────────────────────────────────────────────
            // George Orwell
            await EnsureBook(context, new Book {
                Title = "1984", ISBN = "9780451524935", PageCount = 328,
                Published = new DateTime(1949, 6, 8), AuthorId = orwell.Id,
                Description = "A dystopian novel about totalitarianism and surveillance.",
                ImageUrl = "https://covers.openlibrary.org/b/isbn/9780451524935-L.jpg",
                BookGenres = new List<BookGenre> { new() { GenreId = gDystopian.Id }, new() { GenreId = gClassic.Id } }
            });
            await EnsureBook(context, new Book {
                Title = "Animal Farm", ISBN = "9780451526342", PageCount = 112,
                Published = new DateTime(1945, 8, 17), AuthorId = orwell.Id,
                Description = "A satirical allegory of Soviet totalitarianism.",
                ImageUrl = "https://covers.openlibrary.org/b/isbn/9780451526342-L.jpg",
                BookGenres = new List<BookGenre> { new() { GenreId = gDystopian.Id }, new() { GenreId = gClassic.Id } }
            });

            // Ernest Hemingway
            await EnsureBook(context, new Book {
                Title = "The Old Man and the Sea", ISBN = "9780684801223", PageCount = 127,
                Published = new DateTime(1952, 9, 1), AuthorId = hemingway.Id,
                Description = "A short novel about an aging Cuban fisherman's epic struggle.",
                ImageUrl = "https://covers.openlibrary.org/b/isbn/9780684801223-L.jpg",
                BookGenres = new List<BookGenre> { new() { GenreId = gAdventure.Id }, new() { GenreId = gClassic.Id } }
            });
            await EnsureBook(context, new Book {
                Title = "A Farewell to Arms", ISBN = "9780684801469", PageCount = 352,
                Published = new DateTime(1929, 9, 27), AuthorId = hemingway.Id,
                Description = "A novel of love and loss set during World War I.",
                ImageUrl = "https://covers.openlibrary.org/b/isbn/9780684801469-L.jpg",
                BookGenres = new List<BookGenre> { new() { GenreId = gHistorical.Id }, new() { GenreId = gClassic.Id } }
            });
            await EnsureBook(context, new Book {
                Title = "For Whom the Bell Tolls", ISBN = "9780684803357", PageCount = 480,
                Published = new DateTime(1940, 10, 21), AuthorId = hemingway.Id,
                Description = "An American fights alongside Spanish guerrillas during the Civil War.",
                ImageUrl = "https://covers.openlibrary.org/b/isbn/9780684803357-L.jpg",
                BookGenres = new List<BookGenre> { new() { GenreId = gHistorical.Id }, new() { GenreId = gAdventure.Id } }
            });

            // Leo Tolstoy
            await EnsureBook(context, new Book {
                Title = "War and Peace", ISBN = "9780199232765", PageCount = 1225,
                Published = new DateTime(1869, 1, 1), AuthorId = tolstoy.Id,
                Description = "An epic novel chronicling Russian society during the Napoleonic era.",
                ImageUrl = "https://covers.openlibrary.org/b/isbn/9780199232765-L.jpg",
                BookGenres = new List<BookGenre> { new() { GenreId = gHistorical.Id }, new() { GenreId = gClassic.Id } }
            });
            await EnsureBook(context, new Book {
                Title = "Anna Karenina", ISBN = "9780140449174", PageCount = 864,
                Published = new DateTime(1878, 1, 1), AuthorId = tolstoy.Id,
                Description = "A tragic story of love and society in Imperial Russia.",
                ImageUrl = "https://covers.openlibrary.org/b/isbn/9780140449174-L.jpg",
                BookGenres = new List<BookGenre> { new() { GenreId = gRomance.Id }, new() { GenreId = gClassic.Id } }
            });

            // Victor Hugo
            await EnsureBook(context, new Book {
                Title = "Les Misérables", ISBN = "9780451419439", PageCount = 1463,
                Published = new DateTime(1862, 1, 1), AuthorId = hugo.Id,
                Description = "A sweeping historical novel of redemption and revolution in France.",
                ImageUrl = "https://covers.openlibrary.org/b/isbn/9780451419439-L.jpg",
                BookGenres = new List<BookGenre> { new() { GenreId = gHistorical.Id }, new() { GenreId = gClassic.Id } }
            });
            await EnsureBook(context, new Book {
                Title = "The Hunchback of Notre-Dame", ISBN = "9780140443530", PageCount = 940,
                Published = new DateTime(1831, 1, 1), AuthorId = hugo.Id,
                Description = "A gothic novel centered on Quasimodo and the great cathedral of Paris.",
                ImageUrl = "https://covers.openlibrary.org/b/isbn/9780140443530-L.jpg",
                BookGenres = new List<BookGenre> { new() { GenreId = gHistorical.Id }, new() { GenreId = gClassic.Id } }
            });

            // Franz Kafka
            await EnsureBook(context, new Book {
                Title = "The Metamorphosis", ISBN = "9780553213690", PageCount = 201,
                Published = new DateTime(1915, 1, 1), AuthorId = kafka.Id,
                Description = "A man wakes up transformed into a giant insect and faces abandonment.",
                ImageUrl = "https://covers.openlibrary.org/b/isbn/9780553213690-L.jpg",
                BookGenres = new List<BookGenre> { new() { GenreId = gClassic.Id }, new() { GenreId = gFiction.Id } }
            });
            await EnsureBook(context, new Book {
                Title = "The Trial", ISBN = "9780805210408", PageCount = 255,
                Published = new DateTime(1925, 1, 1), AuthorId = kafka.Id,
                Description = "A man is prosecuted by an inaccessible authority for an unspecified crime.",
                ImageUrl = "https://covers.openlibrary.org/b/isbn/9780805210408-L.jpg",
                BookGenres = new List<BookGenre> { new() { GenreId = gClassic.Id }, new() { GenreId = gThriller.Id } }
            });

            // J.R.R. Tolkien
            await EnsureBook(context, new Book {
                Title = "The Lord of the Rings", ISBN = "9780618640157", PageCount = 1178,
                Published = new DateTime(1954, 7, 29), AuthorId = tolkien.Id,
                Description = "An epic high-fantasy tale of the struggle against the Dark Lord Sauron.",
                ImageUrl = "https://covers.openlibrary.org/b/isbn/9780618640157-L.jpg",
                BookGenres = new List<BookGenre> { new() { GenreId = gFantasy.Id }, new() { GenreId = gAdventure.Id } }
            });
            await EnsureBook(context, new Book {
                Title = "The Hobbit", ISBN = "9780618968633", PageCount = 310,
                Published = new DateTime(1937, 9, 21), AuthorId = tolkien.Id,
                Description = "Bilbo Baggins is swept into an epic quest to reclaim the dwarves' homeland.",
                ImageUrl = "https://covers.openlibrary.org/b/isbn/9780618968633-L.jpg",
                BookGenres = new List<BookGenre> { new() { GenreId = gFantasy.Id }, new() { GenreId = gAdventure.Id } }
            });

            // Fyodor Dostoyevsky
            await EnsureBook(context, new Book {
                Title = "Crime and Punishment", ISBN = "9780140449136", PageCount = 671,
                Published = new DateTime(1866, 1, 1), AuthorId = dostoyevsky.Id,
                Description = "A student murders a pawnbroker and wrestles with guilt and redemption.",
                ImageUrl = "https://covers.openlibrary.org/b/isbn/9780140449136-L.jpg",
                BookGenres = new List<BookGenre> { new() { GenreId = gClassic.Id }, new() { GenreId = gThriller.Id } }
            });
            await EnsureBook(context, new Book {
                Title = "The Idiot", ISBN = "9780140447927", PageCount = 656,
                Published = new DateTime(1869, 1, 1), AuthorId = dostoyevsky.Id,
                Description = "A kind-hearted, naive prince returns to Russia and is corrupted by society.",
                ImageUrl = "https://covers.openlibrary.org/b/isbn/9780140447927-L.jpg",
                BookGenres = new List<BookGenre> { new() { GenreId = gClassic.Id }, new() { GenreId = gFiction.Id } }
            });
            await EnsureBook(context, new Book {
                Title = "The Brothers Karamazov", ISBN = "9780374528379", PageCount = 796,
                Published = new DateTime(1880, 1, 1), AuthorId = dostoyevsky.Id,
                Description = "Three brothers clash over faith, doubt, morality, and a father's murder.",
                ImageUrl = "https://covers.openlibrary.org/b/isbn/9780374528379-L.jpg",
                BookGenres = new List<BookGenre> { new() { GenreId = gClassic.Id }, new() { GenreId = gPhilosophy.Id } }
            });

            // Gabriel García Márquez
            await EnsureBook(context, new Book {
                Title = "One Hundred Years of Solitude", ISBN = "9780060883287", PageCount = 422,
                Published = new DateTime(1967, 5, 30), AuthorId = marquez.Id,
                Description = "The Buendía family saga spanning seven generations in magical Macondo.",
                ImageUrl = "https://covers.openlibrary.org/b/isbn/9780060883287-L.jpg",
                BookGenres = new List<BookGenre> { new() { GenreId = gFiction.Id }, new() { GenreId = gHistorical.Id } }
            });
            await EnsureBook(context, new Book {
                Title = "Love in the Time of Cholera", ISBN = "9780307389732", PageCount = 348,
                Published = new DateTime(1985, 1, 1), AuthorId = marquez.Id,
                Description = "A man waits over fifty years to reunite with his unrequited love.",
                ImageUrl = "https://covers.openlibrary.org/b/isbn/9780307389732-L.jpg",
                BookGenres = new List<BookGenre> { new() { GenreId = gRomance.Id }, new() { GenreId = gFiction.Id } }
            });

            // Albert Camus
            await EnsureBook(context, new Book {
                Title = "The Stranger", ISBN = "9780679720201", PageCount = 123,
                Published = new DateTime(1942, 1, 1), AuthorId = camus.Id,
                Description = "A detached Algerian man commits a senseless murder and faces his fate.",
                ImageUrl = "https://covers.openlibrary.org/b/isbn/9780679720201-L.jpg",
                BookGenres = new List<BookGenre> { new() { GenreId = gClassic.Id }, new() { GenreId = gPhilosophy.Id } }
            });
            await EnsureBook(context, new Book {
                Title = "The Plague", ISBN = "9780679720218", PageCount = 308,
                Published = new DateTime(1947, 1, 1), AuthorId = camus.Id,
                Description = "A bubonic plague grips an Algerian city, an allegory of human suffering.",
                ImageUrl = "https://covers.openlibrary.org/b/isbn/9780679720218-L.jpg",
                BookGenres = new List<BookGenre> { new() { GenreId = gFiction.Id }, new() { GenreId = gPhilosophy.Id } }
            });

            // Jean-Paul Sartre
            await EnsureBook(context, new Book {
                Title = "Nausea", ISBN = "9780811201224", PageCount = 178,
                Published = new DateTime(1938, 1, 1), AuthorId = sartre.Id,
                Description = "A historian in a provincial French town is overcome by existential dread.",
                ImageUrl = "https://covers.openlibrary.org/b/isbn/9780811201224-L.jpg",
                BookGenres = new List<BookGenre> { new() { GenreId = gClassic.Id }, new() { GenreId = gPhilosophy.Id } }
            });

            // Gustave Flaubert
            await EnsureBook(context, new Book {
                Title = "Madame Bovary", ISBN = "9780140449129", PageCount = 329,
                Published = new DateTime(1857, 1, 1), AuthorId = flaubert.Id,
                Description = "A provincial doctor's wife seeks escape from boredom through romance and luxury.",
                ImageUrl = "https://covers.openlibrary.org/b/isbn/9780140449129-L.jpg",
                BookGenres = new List<BookGenre> { new() { GenreId = gRomance.Id }, new() { GenreId = gClassic.Id } }
            });

            // Charles Dickens
            await EnsureBook(context, new Book {
                Title = "Great Expectations", ISBN = "9780141439563", PageCount = 544,
                Published = new DateTime(1861, 1, 1), AuthorId = dickens.Id,
                Description = "An orphan's journey from poverty to wealth and self-discovery in Victorian England.",
                ImageUrl = "https://covers.openlibrary.org/b/isbn/9780141439563-L.jpg",
                BookGenres = new List<BookGenre> { new() { GenreId = gClassic.Id }, new() { GenreId = gHistorical.Id } }
            });
            await EnsureBook(context, new Book {
                Title = "A Tale of Two Cities", ISBN = "9780141439600", PageCount = 489,
                Published = new DateTime(1859, 1, 1), AuthorId = dickens.Id,
                Description = "Set against the French Revolution — a story of sacrifice, love and resurrection.",
                ImageUrl = "https://covers.openlibrary.org/b/isbn/9780141439600-L.jpg",
                BookGenres = new List<BookGenre> { new() { GenreId = gHistorical.Id }, new() { GenreId = gClassic.Id } }
            });

            // Jane Austen
            await EnsureBook(context, new Book {
                Title = "Pride and Prejudice", ISBN = "9780141439518", PageCount = 432,
                Published = new DateTime(1813, 1, 28), AuthorId = austen.Id,
                Description = "A spirited romance between Elizabeth Bennet and the proud Mr. Darcy.",
                ImageUrl = "https://covers.openlibrary.org/b/isbn/9780141439518-L.jpg",
                BookGenres = new List<BookGenre> { new() { GenreId = gRomance.Id }, new() { GenreId = gClassic.Id } }
            });
            await EnsureBook(context, new Book {
                Title = "Sense and Sensibility", ISBN = "9780141439662", PageCount = 374,
                Published = new DateTime(1811, 1, 1), AuthorId = austen.Id,
                Description = "Two sisters navigate love, heartbreak and societal expectations in Regency England.",
                ImageUrl = "https://covers.openlibrary.org/b/isbn/9780141439662-L.jpg",
                BookGenres = new List<BookGenre> { new() { GenreId = gRomance.Id }, new() { GenreId = gClassic.Id } }
            });

            // Oscar Wilde
            await EnsureBook(context, new Book {
                Title = "The Picture of Dorian Gray", ISBN = "9780141439570", PageCount = 254,
                Published = new DateTime(1890, 1, 1), AuthorId = wilde.Id,
                Description = "A beautiful young man sells his soul for eternal youth while his portrait ages.",
                ImageUrl = "https://covers.openlibrary.org/b/isbn/9780141439570-L.jpg",
                BookGenres = new List<BookGenre> { new() { GenreId = gClassic.Id }, new() { GenreId = gHorror.Id } }
            });

            // James Joyce
            await EnsureBook(context, new Book {
                Title = "Ulysses", ISBN = "9780141182803", PageCount = 730,
                Published = new DateTime(1922, 2, 2), AuthorId = joyce.Id,
                Description = "A day in the life of Leopold Bloom, wandering Dublin on 16 June 1904.",
                ImageUrl = "https://covers.openlibrary.org/b/isbn/9780141182803-L.jpg",
                BookGenres = new List<BookGenre> { new() { GenreId = gClassic.Id }, new() { GenreId = gFiction.Id } }
            });

            // John Steinbeck
            await EnsureBook(context, new Book {
                Title = "Of Mice and Men", ISBN = "9780140177398", PageCount = 112,
                Published = new DateTime(1937, 1, 1), AuthorId = steinbeck.Id,
                Description = "Two displaced ranch workers dream of owning land during the Great Depression.",
                ImageUrl = "https://covers.openlibrary.org/b/isbn/9780140177398-L.jpg",
                BookGenres = new List<BookGenre> { new() { GenreId = gClassic.Id }, new() { GenreId = gFiction.Id } }
            });
            await EnsureBook(context, new Book {
                Title = "East of Eden", ISBN = "9780142004234", PageCount = 601,
                Published = new DateTime(1952, 1, 1), AuthorId = steinbeck.Id,
                Description = "An epic tale of good, evil, and free will across generations in California.",
                ImageUrl = "https://covers.openlibrary.org/b/isbn/9780142004234-L.jpg",
                BookGenres = new List<BookGenre> { new() { GenreId = gHistorical.Id }, new() { GenreId = gClassic.Id } }
            });

            // F. Scott Fitzgerald
            await EnsureBook(context, new Book {
                Title = "The Great Gatsby", ISBN = "9780743273565", PageCount = 180,
                Published = new DateTime(1925, 4, 10), AuthorId = fitzgerald.Id,
                Description = "The Jazz Age dream, wealth and illusion in 1920s Long Island.",
                ImageUrl = "https://covers.openlibrary.org/b/isbn/9780743273565-L.jpg",
                BookGenres = new List<BookGenre> { new() { GenreId = gClassic.Id }, new() { GenreId = gFiction.Id } }
            });

            // William Faulkner
            await EnsureBook(context, new Book {
                Title = "The Sound and the Fury", ISBN = "9780679732242", PageCount = 326,
                Published = new DateTime(1929, 1, 1), AuthorId = faulkner.Id,
                Description = "The decline of the Compson family told through four different perspectives.",
                ImageUrl = "https://covers.openlibrary.org/b/isbn/9780679732242-L.jpg",
                BookGenres = new List<BookGenre> { new() { GenreId = gClassic.Id }, new() { GenreId = gFiction.Id } }
            });

            // J.D. Salinger
            await EnsureBook(context, new Book {
                Title = "The Catcher in the Rye", ISBN = "9780316769174", PageCount = 277,
                Published = new DateTime(1951, 7, 16), AuthorId = salinger.Id,
                Description = "Holden Caulfield's rebellious coming-of-age story set in New York City.",
                ImageUrl = "https://covers.openlibrary.org/b/isbn/9780316769174-L.jpg",
                BookGenres = new List<BookGenre> { new() { GenreId = gClassic.Id }, new() { GenreId = gFiction.Id } }
            });

            // Hermann Hesse
            await EnsureBook(context, new Book {
                Title = "Siddhartha", ISBN = "9780553208849", PageCount = 152,
                Published = new DateTime(1922, 1, 1), AuthorId = hesse.Id,
                Description = "A spiritual journey of a young Brahmin toward enlightenment in ancient India.",
                ImageUrl = "https://covers.openlibrary.org/b/isbn/9780553208849-L.jpg",
                BookGenres = new List<BookGenre> { new() { GenreId = gClassic.Id }, new() { GenreId = gPhilosophy.Id } }
            });
            await EnsureBook(context, new Book {
                Title = "Steppenwolf", ISBN = "9780312278908", PageCount = 218,
                Published = new DateTime(1927, 1, 1), AuthorId = hesse.Id,
                Description = "A middle-aged intellectual torn between bourgeois conventions and wild impulses.",
                ImageUrl = "https://covers.openlibrary.org/b/isbn/9780312278908-L.jpg",
                BookGenres = new List<BookGenre> { new() { GenreId = gClassic.Id }, new() { GenreId = gFiction.Id } }
            });

            // Thomas Mann
            await EnsureBook(context, new Book {
                Title = "Buddenbrooks", ISBN = "9780375751608", PageCount = 731,
                Published = new DateTime(1901, 1, 1), AuthorId = mann.Id,
                Description = "The decline of a wealthy German merchant family across four generations.",
                ImageUrl = "https://covers.openlibrary.org/b/isbn/9780375751608-L.jpg",
                BookGenres = new List<BookGenre> { new() { GenreId = gHistorical.Id }, new() { GenreId = gClassic.Id } }
            });
            await EnsureBook(context, new Book {
                Title = "The Magic Mountain", ISBN = "9780679772873", PageCount = 706,
                Published = new DateTime(1924, 1, 1), AuthorId = mann.Id,
                Description = "A young engineer spends seven years in a Swiss sanatorium before World War I.",
                ImageUrl = "https://covers.openlibrary.org/b/isbn/9780679772873-L.jpg",
                BookGenres = new List<BookGenre> { new() { GenreId = gHistorical.Id }, new() { GenreId = gPhilosophy.Id } }
            });

            // Alexandre Dumas
            await EnsureBook(context, new Book {
                Title = "The Three Musketeers", ISBN = "9780140449266", PageCount = 704,
                Published = new DateTime(1844, 3, 1), AuthorId = dumas.Id,
                Description = "D'Artagnan joins three musketeers in swashbuckling adventures in 17th-century France.",
                ImageUrl = "https://covers.openlibrary.org/b/isbn/9780140449266-L.jpg",
                BookGenres = new List<BookGenre> { new() { GenreId = gAdventure.Id }, new() { GenreId = gHistorical.Id } }
            });
            await EnsureBook(context, new Book {
                Title = "The Count of Monte Cristo", ISBN = "9780140449266X", PageCount = 1276,
                Published = new DateTime(1845, 1, 1), AuthorId = dumas.Id,
                Description = "A wrongfully imprisoned man escapes and enacts an elaborate plan of revenge.",
                ImageUrl = "https://covers.openlibrary.org/b/isbn/9780140449264-L.jpg",
                BookGenres = new List<BookGenre> { new() { GenreId = gAdventure.Id }, new() { GenreId = gThriller.Id } }
            });

            // Miguel de Cervantes
            await EnsureBook(context, new Book {
                Title = "Don Quixote", ISBN = "9780060934347", PageCount = 1072,
                Published = new DateTime(1605, 1, 16), AuthorId = cervantes.Id,
                Description = "A nobleman, his mind warped by chivalric romances, sets out as a knight-errant.",
                ImageUrl = "https://covers.openlibrary.org/b/isbn/9780060934347-L.jpg",
                BookGenres = new List<BookGenre> { new() { GenreId = gAdventure.Id }, new() { GenreId = gClassic.Id } }
            });

            // Dante Alighieri
            await EnsureBook(context, new Book {
                Title = "The Divine Comedy", ISBN = "9780142437223", PageCount = 798,
                Published = new DateTime(1320, 1, 1), AuthorId = dante.Id,
                Description = "An epic poem journey through Hell, Purgatory, and Paradise guided by Virgil.",
                ImageUrl = "https://covers.openlibrary.org/b/isbn/9780142437223-L.jpg",
                BookGenres = new List<BookGenre> { new() { GenreId = gClassic.Id }, new() { GenreId = gPhilosophy.Id } }
            });

            // Aldous Huxley
            await EnsureBook(context, new Book {
                Title = "Brave New World", ISBN = "9780060850524", PageCount = 311,
                Published = new DateTime(1932, 1, 1), AuthorId = huxley.Id,
                Description = "A futuristic society where happiness is engineered and freedom is abolished.",
                ImageUrl = "https://covers.openlibrary.org/b/isbn/9780060850524-L.jpg",
                BookGenres = new List<BookGenre> { new() { GenreId = gDystopian.Id }, new() { GenreId = gSciFi.Id } }
            });

            // Ray Bradbury
            await EnsureBook(context, new Book {
                Title = "Fahrenheit 451", ISBN = "9781451673319", PageCount = 256,
                Published = new DateTime(1953, 10, 19), AuthorId = bradbury.Id,
                Description = "In a future America, firemen burn books and one begins to question why.",
                ImageUrl = "https://covers.openlibrary.org/b/isbn/9781451673319-L.jpg",
                BookGenres = new List<BookGenre> { new() { GenreId = gDystopian.Id }, new() { GenreId = gSciFi.Id } }
            });

            // Bram Stoker
            await EnsureBook(context, new Book {
                Title = "Dracula", ISBN = "9780486411095", PageCount = 418,
                Published = new DateTime(1897, 5, 26), AuthorId = stoker.Id,
                Description = "A Transylvanian vampire stalks Victorian England in this Gothic horror classic.",
                ImageUrl = "https://covers.openlibrary.org/b/isbn/9780486411095-L.jpg",
                BookGenres = new List<BookGenre> { new() { GenreId = gHorror.Id }, new() { GenreId = gClassic.Id } }
            });

            // Mary Shelley
            await EnsureBook(context, new Book {
                Title = "Frankenstein", ISBN = "9780486282114", PageCount = 280,
                Published = new DateTime(1818, 1, 1), AuthorId = shelley.Id,
                Description = "A scientist creates life from dead matter and faces the horrifying consequences.",
                ImageUrl = "https://covers.openlibrary.org/b/isbn/9780486282114-L.jpg",
                BookGenres = new List<BookGenre> { new() { GenreId = gHorror.Id }, new() { GenreId = gSciFi.Id } }
            });

            // Anton Chekhov
            await EnsureBook(context, new Book {
                Title = "The Cherry Orchard", ISBN = "9780140447958", PageCount = 112,
                Published = new DateTime(1904, 1, 17), AuthorId = chekhov.Id,
                Description = "An aristocratic family loses their beloved estate as Russia modernises.",
                ImageUrl = "https://covers.openlibrary.org/b/isbn/9780140447958-L.jpg",
                BookGenres = new List<BookGenre> { new() { GenreId = gClassic.Id }, new() { GenreId = gHistorical.Id } }
            });

            // Henrik Ibsen
            await EnsureBook(context, new Book {
                Title = "A Doll's House", ISBN = "9780486270623", PageCount = 80,
                Published = new DateTime(1879, 12, 4), AuthorId = ibsen.Id,
                Description = "A woman abandons her husband and children to find her true identity.",
                ImageUrl = "https://covers.openlibrary.org/b/isbn/9780486270623-L.jpg",
                BookGenres = new List<BookGenre> { new() { GenreId = gClassic.Id }, new() { GenreId = gFiction.Id } }
            });

            // Yasunari Kawabata
            await EnsureBook(context, new Book {
                Title = "Snow Country", ISBN = "9780679761020", PageCount = 175,
                Published = new DateTime(1956, 1, 1), AuthorId = kawabata.Id,
                Description = "A wealthy dilettante has a fleeting affair with a rural geisha in snowy Japan.",
                ImageUrl = "https://covers.openlibrary.org/b/isbn/9780679761020-L.jpg",
                BookGenres = new List<BookGenre> { new() { GenreId = gRomance.Id }, new() { GenreId = gFiction.Id } }
            });

            // ── Reviewers ──────────────────────────────────────────────────────
            if (!await context.Reviewers.AnyAsync())
            {
                await context.Reviewers.AddRangeAsync(new List<Reviewer>
                {
                    new Reviewer { Name = "John Smith" },
                    new Reviewer { Name = "Jane Doe" },
                    new Reviewer { Name = "Peter Brown" },
                    new Reviewer { Name = "Emily Clark" }
                });
                await context.SaveChangesAsync();
            }

            // ── Reviews ────────────────────────────────────────────────────────
            if (!await context.Reviews.AnyAsync())
            {
                var rJohn  = await context.Reviewers.FirstAsync(r => r.Name == "John Smith");
                var rJane  = await context.Reviewers.FirstAsync(r => r.Name == "Jane Doe");
                var rPeter = await context.Reviewers.FirstAsync(r => r.Name == "Peter Brown");
                var rEmily = await context.Reviewers.FirstAsync(r => r.Name == "Emily Clark");

                var allBooks = await context.Books.ToListAsync();
                var reviews  = new List<Review>();
                var rnd      = new Random(42);

                var texts = new[]
                {
                    "An absolute masterpiece — required reading.", "Haunting and unforgettable.", "Changed the way I see literature.",
                    "Dense but deeply rewarding.", "A timeless classic.", "Beautifully written from start to finish.",
                    "Gripping and thought-provoking.", "One of the greatest novels ever written.", "Poetic and emotionally resonant.",
                    "Dark, complex, and brilliant.", "A surprisingly accessible classic.", "Stays with you long after the last page."
                };
                var reviewers = new[] { rJohn, rJane, rPeter, rEmily };

                foreach (var book in allBooks)
                {
                    int count = rnd.Next(1, 4);
                    var usedReviewers = reviewers.OrderBy(_ => rnd.Next()).Take(count).ToList();
                    foreach (var reviewer in usedReviewers)
                    {
                        reviews.Add(new Review
                        {
                            Text       = texts[rnd.Next(texts.Length)],
                            Rating     = rnd.Next(3, 6),
                            CreatedAt  = DateTime.UtcNow.AddDays(-rnd.Next(30, 3000)),
                            BookId     = book.Id,
                            ReviewerId = reviewer.Id
                        });
                    }
                }

                await context.Reviews.AddRangeAsync(reviews);
                await context.SaveChangesAsync();
            }

            // ── Migrate cover images to MinIO blob storage ─────────────────────
            try
            {
                var books      = await context.Books.ToListAsync();
                var blobService = scope.ServiceProvider.GetRequiredService<IBlobService>();
                using var http = new HttpClient();
                http.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0");

                bool changed = false;
                foreach (var book in books)
                {
                    if (string.IsNullOrWhiteSpace(book.ImageUrl)) continue;

                    string? downloadUrl = null;

                    if (book.ImageUrl.StartsWith("http") && book.ImageUrl.Contains("openlibrary.org"))
                        downloadUrl = book.ImageUrl;
                    else if (book.ImageUrl.StartsWith("/api/blobs/"))
                    {
                        var fileId = book.ImageUrl.Replace("/api/blobs/", "");
                        if (!await blobService.FileExistsAsync(fileId))
                            downloadUrl = $"https://covers.openlibrary.org/b/isbn/{book.ISBN}-L.jpg";
                    }

                    if (downloadUrl == null) continue;

                    try
                    {
                        var response = await http.GetAsync(downloadUrl);
                        if (response.IsSuccessStatusCode)
                        {
                            var stream = await response.Content.ReadAsStreamAsync();
                            var id     = await blobService.UploadFileAsync(stream, $"{book.ISBN ?? book.Title}.jpg", "image/jpeg");
                            book.ImageUrl = $"/api/blobs/{id}";
                            changed = true;
                            Console.WriteLine($"[Seed] Uploaded cover for '{book.Title}'");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Seed] Cover upload failed for '{book.Title}': {ex.Message}");
                    }
                }

                if (changed) await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Seed] Image migration error: {ex.Message}");
            }

            var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Username == "admin");
            if (adminUser == null)
            {
                adminUser = new User
                {
                    Username = "admin",
                    Email = "admin@library.com",
                    PasswordHash = PasswordHasher.HashPassword("admin123"),
                    Role = "Admin"
                };
                context.Users.Add(adminUser);
                Console.WriteLine("[Seed] Seeded admin user: admin / admin123");
            }
            else
            {
                adminUser.PasswordHash = PasswordHasher.HashPassword("admin123");
                adminUser.Role = "Admin";
                Console.WriteLine("[Seed] Reset admin user password to: admin123");
            }
            await context.SaveChangesAsync();

            // Seed localized database content
            await SeedTranslationsAsync(context);
        }

        private static async Task SeedTranslationsAsync(LibraryContext context)
        {
            Console.WriteLine("[Seed] Seeding database translation tables...");

            // 1. Genres
            var genresList = new List<(string Name, string Sk, string Gr)>
            {
                ("Classic", "Klasika", "Κλασικό"),
                ("Dystopian", "Dystopia", "Δυστοπικό"),
                ("Adventure", "Dobrodružné", "Περιπέτεια"),
                ("Historical Fiction", "Historická fikcia", "Ιστορικό Μυθιστόρημα"),
                ("Fantasy", "Fantasy", "Φαντασίας"),
                ("Thriller", "Triler", "Θρίλερ"),
                ("Romance", "Romantické", "Ρομαντικό"),
                ("Science Fiction", "Sci-fi", "Επιστημονικής Φαντασίας"),
                ("Horror", "Horor", "Τρόμου"),
                ("Philosophy", "Filozofia", "Φιλοσοφία"),
                ("Fiction", "Fikcia", "Μυθοπλασία"),
                ("Mystery", "Mysteriózne", "Μυστηρίου")
            };

            foreach (var (name, sk, gr) in genresList)
            {
                var genre = await context.Genres.FirstOrDefaultAsync(g => g.Name == name);
                if (genre != null)
                {
                    if (!await context.GenreTranslations.AnyAsync(t => t.GenreId == genre.Id && t.LanguageCode == "SK"))
                    {
                        context.GenreTranslations.Add(new GenreTranslation { GenreId = genre.Id, LanguageCode = "SK", Name = sk });
                    }
                    if (!await context.GenreTranslations.AnyAsync(t => t.GenreId == genre.Id && t.LanguageCode == "GR"))
                    {
                        context.GenreTranslations.Add(new GenreTranslation { GenreId = genre.Id, LanguageCode = "GR", Name = gr });
                    }
                }
            }

            // 2. Authors
            var authorsList = new List<(string Name, string Surname, string SkName, string SkSurname, string GrName, string GrSurname)>
            {
                ("George", "Orwell", "George", "Orwell", "Τζορτζ", "Όργουελ"),
                ("Ernest", "Hemingway", "Ernest", "Hemingway", "Έrnest", "Χέμινγουεϊ"),
                ("Leo", "Tolstoy", "Lev Nikolajevič", "Tolstoj", "Λέων", "Τολστόι"),
                ("Victor", "Hugo", "Victor", "Hugo", "Βίκτωρ", "Ουγκώ"),
                ("Franz", "Kafka", "Franz", "Kafka", "Φραντς", "Κάφκα"),
                ("J.R.R.", "Tolkien", "J.R.R.", "Tolkien", "Τζ. Ρ. Ρ.", "Τόλκιν"),
                ("Fyodor", "Dostoyevsky", "Fjodor Michajlovič", "Dostojevskij", "Φιόντορ", "Ντοστογιέφσκι"),
                ("Gabriel", "Garcia Marquez", "Gabriel García", "Márquez", "Γκαμπριέλ Γκαρσία", "Μάρκες"),
                ("Albert", "Camus", "Albert", "Camus", "Αλμπέρ", "Καμύ"),
                ("Jane", "Austen", "Jane", "Austen", "Τζέιν", "Όστεν")
            };

            foreach (var (name, surname, skName, skSurname, grName, grSurname) in authorsList)
            {
                var author = await context.Authors.FirstOrDefaultAsync(a => a.Name == name && a.Surname == surname);
                if (author != null)
                {
                    if (!await context.AuthorTranslations.AnyAsync(t => t.AuthorId == author.Id && t.LanguageCode == "SK"))
                    {
                        context.AuthorTranslations.Add(new AuthorTranslation { AuthorId = author.Id, LanguageCode = "SK", Name = skName, Surname = skSurname });
                    }
                    if (!await context.AuthorTranslations.AnyAsync(t => t.AuthorId == author.Id && t.LanguageCode == "GR"))
                    {
                        context.AuthorTranslations.Add(new AuthorTranslation { AuthorId = author.Id, LanguageCode = "GR", Name = grName, Surname = grSurname });
                    }
                }
            }

            // 3. Books
            var booksList = new List<(string Title, string SkTitle, string SkDesc, string GrTitle, string GrDesc)>
            {
                ("1984", "1984", "Dystopický román o totalitarizme a sledovaní.", "1984", "Ένα δυστοπικό μυθιστόρημα για τον ολοκληρωτισμό και την παρακολούθηση."),
                ("Animal Farm", "Zvieracia farma", "Satirická alegória sovietskeho totalitarizmu.", "Η Φάρμα των Ζώων", "Μια σατιρική αλληγορία του σοβιετικού ολοκληρωτισμού."),
                ("The Old Man and the Sea", "Starec a more", "Príbeh o boji starnúceho kubánskeho rybára s obrovským marlínom.", "Ο Γέρος και η Θάλασσα", "Η ιστορία του αγώνα ενός ηλικιωμένου Κουβανού ψαρά με έναν τεράστιο μαρλίνο."),
                ("War and Peace", "Vojna a mier", "Ruská klasika odohrávajúca sa počas napoleonskej éry.", "Πόλεμος και Ειρήνη", "Ένα ρωσικό κλασικό έργο που διαδραματίζεται κατά τη διάρκεια της ναπολεόντειας εποχής."),
                ("Les Misérables", "Bedári", "Príbeh lásky, vykúpenia a revolúcie v Paríži 19. storočia.", "Οι Άθλιοι", "Μια ιστορία αγάπης, λύτρωσης a επανάστασης στο Παρίσι του 19ου αιώνα."),
                ("The Metamorphosis", "Premena", "Absurdný a existenciálny príbeh muža, ktorý sa premení na hmyz.", "Η Μεταμόρφωση", "Μια παράλογη και υπαρξιακή ιστορία ενός ανθρώπου που μεταμορφώνεται σε έντομο."),
                ("The Hobbit", "Hobit", "Fantasy dobrodružstvo v Stredozemi.", "Χόμπιτ", "Μια φανταστική περιπέτεια στη Μέση Γη."),
                ("Crime and Punishment", "Zločin a trest", "Psychologická dráma o vine, treste a vykúpení v Petrohrade.", "Έγκλημα και Τιμωρία", "Ένα ψυχολογικό δράμα για την ενοχή, την τιμωρία και τη λύτρωση στην Αγία Πετρούπολη."),
                ("One Hundred Years of Solitude", "Sto rokov samoty", "Rodinná sága v mestečku Macondo.", "Εκατό Χρόνια Μοναξιάς", "Ένα οικογενειακό έπος στην πόλη Μακόντο."),
                ("The Stranger", "Cudzinec", "Existenciálny román odohrávajúci sa v Alžírsku.", "Ο Ξένος", "Έna υπαρξιακό μυθιστόρημα που διαδραματίζεται στην Αλγερία.")
            };

            foreach (var (title, skTitle, skDesc, grTitle, grDesc) in booksList)
            {
                var book = await context.Books.FirstOrDefaultAsync(b => b.Title == title);
                if (book != null)
                {
                    if (!await context.BookTranslations.AnyAsync(t => t.BookId == book.Id && t.LanguageCode == "SK"))
                    {
                        context.BookTranslations.Add(new BookTranslation { BookId = book.Id, LanguageCode = "SK", Title = skTitle, Description = skDesc });
                    }
                    if (!await context.BookTranslations.AnyAsync(t => t.BookId == book.Id && t.LanguageCode == "GR"))
                    {
                        context.BookTranslations.Add(new BookTranslation { BookId = book.Id, LanguageCode = "GR", Title = grTitle, Description = grDesc });
                    }
                }
            }

            await context.SaveChangesAsync();
            Console.WriteLine("[Seed] Database translation tables seeded successfully.");
        }
    }
}