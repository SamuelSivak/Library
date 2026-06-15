using Library.DataContext;
using Library.Models;
using Library.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Bogus;

namespace Library
{
    public static class Seed
    {
        // Helper to clear existing tables (excluding user/role tables)
        private static string ToRoman(int number)
        {
            if (number < 1) return string.Empty;
            if (number >= 100) return "C" + ToRoman(number - 100);
            if (number >= 90) return "XC" + ToRoman(number - 90);
            if (number >= 50) return "L" + ToRoman(number - 50);
            if (number >= 40) return "XL" + ToRoman(number - 40);
            if (number >= 10) return "X" + ToRoman(number - 10);
            if (number >= 9) return "IX" + ToRoman(number - 9);
            if (number >= 5) return "V" + ToRoman(number - 5);
            if (number >= 4) return "IV" + ToRoman(number - 4);
            if (number >= 1) return "I" + ToRoman(number - 1);
            throw new ArgumentOutOfRangeException(nameof(number));
        }

        private static async Task ClearDatabaseAsync(LibraryContext context)
        {
            Console.WriteLine("[Seed] Clearing old database tables using ExecuteDeleteAsync...");
            
            await context.BookGenres.ExecuteDeleteAsync();
            await context.Reviews.ExecuteDeleteAsync();
            await context.BookTranslations.ExecuteDeleteAsync();
            await context.AuthorTranslations.ExecuteDeleteAsync();
            await context.GenreTranslations.ExecuteDeleteAsync();
            await context.BiBookAnalytics.ExecuteDeleteAsync();
            
            await context.Books.ExecuteDeleteAsync();
            await context.Authors.ExecuteDeleteAsync();
            await context.Reviewers.ExecuteDeleteAsync();
            
            await context.Genres.ExecuteDeleteAsync();
            await context.Country.ExecuteDeleteAsync();

            // Reset SQL Server Identity Seeds to 0 (next inserted record gets ID 1)
            Console.WriteLine("[Seed] Resetting database identity seeds...");
            await context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Reviews', RESEED, 0);");
            await context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('BookTranslations', RESEED, 0);");
            await context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('AuthorTranslations', RESEED, 0);");
            await context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('GenreTranslations', RESEED, 0);");
            await context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Books', RESEED, 0);");
            await context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Authors', RESEED, 0);");
            await context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Reviewers', RESEED, 0);");
            await context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Genres', RESEED, 0);");
            await context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Country', RESEED, 0);");
            
            Console.WriteLine("[Seed] Database cleared and identity seeds reset successfully.");
        }

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

        public static async Task SeedDataAsync(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<LibraryContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await context.Database.MigrateAsync();

            try
            {
                await context.Database.ExecuteSqlRawAsync("ALTER DATABASE SCOPED CONFIGURATION SET IDENTITY_CACHE = OFF;");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Seed] Failed to disable identity cache: {ex.Message}");
            }

            // Seed Roles first
            var roles = new[] { "Admin", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                    Console.WriteLine($"[Seed] Seeded role: {role}");
                }
            }

            // Seed Admin User
            var adminUser = await userManager.FindByNameAsync("admin");
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = "admin",
                    Email = "admin@library.com"
                };
                var createResult = await userManager.CreateAsync(adminUser, "admin123");
                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    Console.WriteLine("[Seed] Seeded admin user: admin / admin123");
                }
                else
                {
                    Console.WriteLine($"[Seed] Failed to seed admin user: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                if (await userManager.HasPasswordAsync(adminUser))
                {
                    await userManager.RemovePasswordAsync(adminUser);
                }
                var addResult = await userManager.AddPasswordAsync(adminUser, "admin123");
                if (addResult.Succeeded)
                {
                    if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
                    {
                        await userManager.AddToRoleAsync(adminUser, "Admin");
                    }
                    Console.WriteLine("[Seed] Reset admin user password to: admin123");
                }
                else
                {
                    Console.WriteLine($"[Seed] Failed to reset admin user password: {string.Join(", ", addResult.Errors.Select(e => e.Description))}");
                }
            }

            // Check if we need to clear old database seed data.
            // We clear and reseed if the count of books, authors, or analytics is not correct, or if sampleBook Id is out of range.
            var hasOldOrLoremSeed = false;
            var bookCount = await context.Books.CountAsync();
            var authorCount = await context.Authors.CountAsync();
            var analyticsCount = await context.BiBookAnalytics.CountAsync();
            
            if (bookCount != 30000 || authorCount != 200 || analyticsCount != 30000)
            {
                hasOldOrLoremSeed = true;
            }
            else
            {
                var sampleBook = await context.Books.FirstOrDefaultAsync();
                if (sampleBook == null || sampleBook.Id > 30000)
                {
                    hasOldOrLoremSeed = true;
                }
                else
                {
                    var famousEnTitles = new HashSet<string>(new[]
                    {
                        "The Shadow of the Wind", "The Alchemist", "The Da Vinci Code", "The Little Prince", "The Great Gatsby",
                        "To Kill a Mockingbird", "One Hundred Years of Solitude", "Crime and Punishment", "The Hobbit",
                        "The Lord of the Rings", "Pride and Prejudice", "Wuthering Heights", "Jane Eyre", "The Picture of Dorian Gray",
                        "Brave New World", "Fahrenheit 451", "Dracula", "Frankenstein", "The Odyssey", "The Iliad",
                        "Les Misérables", "The Count of Monte Cristo", "Don Quixote", "The Divine Comedy", "The Stranger",
                        "The Plague", "Nausea", "Madame Bovary", "Great Expectations", "A Tale of Two Cities",
                        "Sense and Sensibility", "Ulysses", "Of Mice and Men", "East of Eden", "The Sound and the Fury",
                        "The Catcher in the Rye", "Siddhartha", "Steppenwolf", "Buddenbrooks", "The Magic Mountain",
                        "The Three Musketeers", "The Cherry Orchard", "A Doll's House", "Snow Country", "Animal Farm",
                        "The Metamorphosis", "The Trial", "The Brothers Karamazov", "Anna Karenina", "War and Peace",
                        "The Name of the Rose", "The Old Man and the Sea", "A Farewell to Arms", "For Whom the Bell Tolls",
                        "The Idiot", "Love in the Time of Cholera", "The Castle", "The Grapes of Wrath", "Ficciones", "Invisible Man",
                        "The Silmarillion", "A Game of Thrones", "A Clash of Kings", "A Storm of Swords", "A Feast for Crows",
                        "A Dance with Dragons", "The Winds of Winter", "A Dream of Spring", "The Fellowship of the Ring",
                        "The Two Towers", "The Return of the King", "The Green Mile", "It", "The Shining", "Misery", "Carrie",
                        "Dune", "Dune Messiah", "Children of Dune", "Neuromancer", "Snow Crash", "Foundation",
                        "Foundation and Empire", "Second Foundation", "The Catch-22", "Catching Fire", "Mockingjay",
                        "The Hunger Games", "The Road", "Blood Meridian", "No Country for Old Men", "The Great Hunt",
                        "The Dragon Reborn", "The Shadow Rising", "The Fires of Heaven", "Lord of Chaos", "A Crown of Swords",
                        "The Path of Daggers", "Winter's Heart"
                    });
                    
                    if (!famousEnTitles.Contains(sampleBook.Title))
                    {
                        hasOldOrLoremSeed = true;
                    }
                }
            }
            
            if (hasOldOrLoremSeed)
            {
                await ClearDatabaseAsync(context);
            }

            // If the database already has books (e.g. customized test data), we skip the Bogus seed to avoid duplicate database writes.
            if (await context.Books.AnyAsync())
            {
                Console.WriteLine("[Seed] Database already seeded. Skipping Bogus seed.");
                return;
            }

            Console.WriteLine("[Seed] Seeding database with Bogus...");

            // ── Countries (15 total) ───────────────────────────────────────────
            var cSK = await EnsureCountry(context, "Slovakia");
            var cUK = await EnsureCountry(context, "United Kingdom");
            var cUS = await EnsureCountry(context, "United States");
            var cDE = await EnsureCountry(context, "Germany");
            var cFR = await EnsureCountry(context, "France");
            var cCZ = await EnsureCountry(context, "Czech Republic");
            var cIT = await EnsureCountry(context, "Italy");
            var cES = await EnsureCountry(context, "Spain");
            var cJP = await EnsureCountry(context, "Japan");
            var cCO = await EnsureCountry(context, "Colombia");
            var cGR = await EnsureCountry(context, "Greece");
            var cPL = await EnsureCountry(context, "Poland");
            var cAT = await EnsureCountry(context, "Austria");
            var cCA = await EnsureCountry(context, "Canada");
            var cAU = await EnsureCountry(context, "Australia");

            // ── Genres & Translations (15 total) ───────────────────────────────
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
                ("Mystery", "Mysteriózne", "Μυστηρίου"),
                ("Biography", "Biografia", "Βιογραφία"),
                ("History", "História", "Ιστορία"),
                ("Poetry", "Poézia", "Ποίηση")
            };

            var genres = new List<Genre>();
            foreach (var (name, sk, gr) in genresList)
            {
                var genre = await EnsureGenre(context, name);
                
                if (!await context.GenreTranslations.AnyAsync(t => t.GenreId == genre.Id && t.LanguageCode == "SK"))
                {
                    context.GenreTranslations.Add(new GenreTranslation { GenreId = genre.Id, LanguageCode = "SK", Name = sk });
                }
                if (!await context.GenreTranslations.AnyAsync(t => t.GenreId == genre.Id && t.LanguageCode == "GR"))
                {
                    context.GenreTranslations.Add(new GenreTranslation { GenreId = genre.Id, LanguageCode = "GR", Name = gr });
                }
                genres.Add(genre);
            }
            await context.SaveChangesAsync();

            // Initialize Bogus Fakers
            var faker = new Faker();
            var fakerSk = new Faker("sk");
            var fakerEl = new Faker("el");

            // ── Authors (200 total) ────────────────────────────────────────────
            var countries = await context.Country.ToListAsync();
            var authors = new List<Author>();

            for (int i = 0; i < 200; i++)
            {
                var country = faker.PickRandom(countries);
                var author = new Author
                {
                    Name = faker.Name.FirstName(),
                    Surname = faker.Name.LastName(),
                    CountryId = country.Id,
                    Translations = new List<AuthorTranslation>()
                };

                author.Translations.Add(new AuthorTranslation
                {
                    LanguageCode = "SK",
                    Name = fakerSk.Name.FirstName(),
                    Surname = fakerSk.Name.LastName()
                });

                author.Translations.Add(new AuthorTranslation
                {
                    LanguageCode = "GR",
                    Name = fakerEl.Name.FirstName(),
                    Surname = fakerEl.Name.LastName()
                });

                authors.Add(author);
            }

            await context.Authors.AddRangeAsync(authors);
            await context.SaveChangesAsync();
            Console.WriteLine($"[Seed] Seeded {authors.Count} authors.");

            // ── Famous Realistic Book Titles pool (100 total) ──────────────────
            var famousTitles = new List<(string En, string Sk, string Gr)>
            {
                ("The Shadow of the Wind", "Tieň vetra", "Η Σκιά του Ανέμου"),
                ("The Alchemist", "Alchymista", "Ο Αλχημιστής"),
                ("The Da Vinci Code", "Da Vinciho kód", "Ο Κώδικας Ντα Βίντσι"),
                ("The Little Prince", "Malý princ", "Ο Μικρός Πρίγκιπας"),
                ("The Great Gatsby", "Veľký Gatsby", "Ο Υπέροχος Γκάτσμπυ"),
                ("To Kill a Mockingbird", "Nezabíjajte vtáčika", "Όταν σκοτώνουν τα κοτσύφια"),
                ("One Hundred Years of Solitude", "Sto rokov samoty", "Εκατό Χρόνια Μοναξιάς"),
                ("Crime and Punishment", "Zločin a trest", "Έγκλημα και Τιμωρία"),
                ("The Hobbit", "Hobit", "Το Χόμπιτ"),
                ("The Lord of the Rings", "Pán prsteňov", "Ο Άρχοντας των Δαχτυλιδιών"),
                ("Pride and Prejudice", "Pýcha a predsudok", "Περηφάνια και Προκατάληψη"),
                ("Wuthering Heights", "Búrlivé výšiny", "Ανεμοδαρμένα Ύψη"),
                ("Jane Eyre", "Jana Eyrová", "Τζέιν Έιρ"),
                ("The Picture of Dorian Gray", "Portrét Doriana Graya", "Το Πορτρέτο του Ντόριαν Γκρέι"),
                ("Brave New World", "Prekrásny nový svet", "Θαυμαστός Καινούριος Κόσμος"),
                ("Fahrenheit 451", "451 stupňov Fahrenheita", "Φαρενάιτ 451"),
                ("Dracula", "Dracula", "Δράκουλας"),
                ("Frankenstein", "Frankenstein", "Φρανκενστάιν"),
                ("The Odyssey", "Odysseia", "Οδύσσεια"),
                ("The Iliad", "Ilias", "Ιλιάδα"),
                ("Les Misérables", "Bedári", "Οι Άθλιοι"),
                ("The Count of Monte Cristo", "Gróf Monte Christo", "Ο Κόμης Μόντε Κρίστο"),
                ("Don Quixote", "Don Quijote", "Δον Κιχώτης"),
                ("The Divine Comedy", "Božská komédia", "Θεία Κωμωδία"),
                ("The Stranger", "Cudzinec", "Ο Ξένος"),
                ("The Plague", "Mor", "Η Πανούκλα"),
                ("Nausea", "Hnus", "Η Ναυčia"),
                ("Madame Bovary", "Pani Bovaryová", "Μαντάμ Μποβαρύ"),
                ("Great Expectations", "Veľké nádeje", "Μεγάλες Προσδοκίες"),
                ("A Tale of Two Cities", "Príbeh dvoch miest", "Ιστορία Δύο Πόλεων"),
                ("Sense and Sensibility", "Rozum a cit", "Λογική and Ευαισθησία"),
                ("Ulysses", "Ulysses", "Οδυσσέας"),
                ("Of Mice and Men", "O myšiach a ľuďoch", "Άνθρωποι και Ποντίκια"),
                ("East of Eden", "Na východ od raja", "Ανατολικά της Εδέμ"),
                ("The Sound and the Fury", "Blot a bes", "Η Βουή and Η Μανία"),
                ("The Catcher in the Rye", "Kto chytá v žite", "Ο Φύλακας στη Σίκαλη"),
                ("Siddhartha", "Siddhártha", "Σιντάρτα"),
                ("Steppenwolf", "Stepný vlk", "Ο Λύκος της Στέπας"),
                ("Buddenbrooks", "Buddenbrookovci", "Οι Μπούντενμπροοκ"),
                ("The Magic Mountain", "Čarovný vrch", "Το Μαγικό Βουνό"),
                ("The Three Musketeers", "Traja mušketieri", "Οι Τρεις Σωματοφύλακες"),
                ("The Cherry Orchard", "Višňový sad", "Ο Βυσσινόκηπος"),
                ("A Doll's House", "Nora", "Το Σπίτι της Κούκλας"),
                ("Snow Country", "Snežná krajina", "Η Χώρα του Χιονιού"),
                ("Animal Farm", "Zvieracia farma", "Η Φάρμα των Ζώων"),
                ("The Metamorphosis", "Premena", "Η Μεταμόρφωση"),
                ("The Trial", "Proces", "Η Δίκη"),
                ("The Brothers Karamazov", "Bratia Karamazovovci", "Αδελφοί Καραμάζοφ"),
                ("Anna Karenina", "Anna Kareninová", "Άνna Καρένινα"),
                ("War and Peace", "Vojna a mier", "Πόλεμος και Ειρήνη"),
                ("The Name of the Rose", "Meno ruže", "Το Όνομα του Ρόδου"),
                ("The Old Man and the Sea", "Starec a more", "Ο Γέρος and Η Θάλασσα"),
                ("A Farewell to Arms", "Zbohom zbraniam", "Αποχαιρετισμός στα Όπλα"),
                ("For Whom the Bell Tolls", "Komu zvonia do hrobu", "Για Ποιον Χτυπά η Καμπάνα"),
                ("The Idiot", "Idiot", "Ο Ηλίθιος"),
                ("Love in the Time of Cholera", "Láska v čase cholery", "Ο Έρωτας στα Χρόνια της Χολέρας"),
                ("The Castle", "Zámok", "Ο Πύργος"),
                ("The Grapes of Wrath", "Ovocie hnevu", "Τα Σταφύλια της Οργής"),
                ("Ficciones", "Fikcie", "Λαβύρινθοι"),
                ("Invisible Man", "Neviditeľný muž", "Ο Αόρατος Άνθρωπος"),
                ("The Silmarillion", "Silmarillion", "Το Σιλμαρίλλιον"),
                ("A Game of Thrones", "Hra o tróny", "Παιχνίδι του Στέμματος"),
                ("A Clash of Kings", "Súboj kráľov", "Σύγκρουση Βασιλέων"),
                ("A Storm of Swords", "Búrka mečov", "Θύελλα Σπαθιών"),
                ("A Feast for Crows", "Hostina pre vrany", "Βορρά Ορνίων"),
                ("A Dance with Dragons", "Tanec s drakmi", "Χορός με Δράκους"),
                ("The Winds of Winter", "Vetry zimy", "Οι Άνεμοι του Χειμώνα"),
                ("A Dream of Spring", "Sen o jari", "Όνειρο της Άνοιξης"),
                ("The Fellowship of the Ring", "Spoločenstvo Prsteňa", "Η Συντροφιά του Δαχτυλιδιού"),
                ("The Two Towers", "Dve veže", "Οι Δύο Πύργοι"),
                ("The Return of the King", "Návrat kráľa", "Η Επιστροφή του Βασιλιά"),
                ("The Green Mile", "Zelená míľa", "Το Πράσινο Μίλι"),
                ("It", "To", "Το Αυτό"),
                ("The Shining", "Osvietenie", "Η Λάμψη"),
                ("Misery", "Misery", "Μίζερι"),
                ("Carrie", "Carrie", "Κάρι"),
                ("Dune", "Duna", "Ντιουν"),
                ("Dune Messiah", "Spasiteľ Duny", "Ο Μεσσίας του Ντιουν"),
                ("Children of Dune", "Deti Duny", "Τα Παιδιά του Ντιουν"),
                ("Neuromancer", "Neuromancer", "Νευρομάντης"),
                ("Snow Crash", "Sneh", "Χιονοστιβάδα"),
                ("Foundation", "Nadácia", "Θεμελίωση"),
                ("Foundation and Empire", "Nadácia a Impérium", "Θεμελίωση και Αυτοκρατορία"),
                ("Second Foundation", "Druhá Nadácia", "Δεύτερη Θεμελίωση"),
                ("The Catch-22", "Hlava XXII", "Catch-22"),
                ("Catching Fire", "Skúška ohňom", "Φωτιά"),
                ("Mockingjay", "Drozdajka", "Κοτσυφόκισσα"),
                ("The Hunger Games", "Hry o život", "Αγώνες Πείνας"),
                ("The Road", "Cesta", "Ο Δρόμος"),
                ("Blood Meridian", "Krvavý poludník", "Ματωμένος Μεσημβρινός"),
                ("No Country for Old Men", "Táto krajina nie je pre starých", "Καμιά Πατρίδα για τους Μελλοθάνατους"),
                ("The Great Hunt", "Veľký lov", "Το Μεγάλο Κυνήγι"),
                ("The Dragon Reborn", "Znovuzrodený Drak", "Ο Αναγεννημένος Δράκος"),
                ("The Shadow Rising", "Stúpajúci tieň", "Η Άνοδος της Σκιάς"),
                ("The Fires of Heaven", "Ohne nebies", "Οι Φωτιές του Ουρανού"),
                ("Lord of Chaos", "Pán chaosu", "Ο Άρχοντας του Χάους"),
                ("A Crown of Swords", "Koruna mečov", "Ένα Στέμμα από Σπαθιά"),
                ("The Path of Daggers", "Cesta dýk", "Το Μονοπάτι των Εγχειριδίων"),
                ("Winter's Heart", "Srdce zimy", "Η Καρδιά του Χειμώνα")
            };

            // ── Books (30,000 total) ───────────────────────────────────────────
            var authorIds = await context.Authors.Select(a => a.Id).ToListAsync();
            var genreIds = await context.Genres.Select(g => g.Id).ToListAsync();

            Console.WriteLine("[Seed] Generating 30,000 books in batches...");
            int totalBooks = 30000;
            int bookBatchSize = 5000;

            for (int batchStart = 0; batchStart < totalBooks; batchStart += bookBatchSize)
            {
                var booksBatch = new List<Book>();
                int batchEnd = Math.Min(batchStart + bookBatchSize, totalBooks);

                for (int i = batchStart; i < batchEnd; i++)
                {
                    var baseTitle = famousTitles[i % famousTitles.Count];
                    int cycle = i / famousTitles.Count;
                    string suffix = cycle > 0 ? " " + ToRoman(cycle + 1) : "";

                    string titleEn = $"{baseTitle.En}{suffix}";
                    string titleSk = $"{baseTitle.Sk}{suffix}";
                    string titleGr = $"{baseTitle.Gr}{suffix}";

                    var authorId = faker.PickRandom(authorIds);
                    var isbn = faker.Commerce.Ean13();

                    var book = new Book
                    {
                        Title = titleEn,
                        ISBN = isbn,
                        PageCount = faker.Random.Number(80, 1000),
                        Published = faker.Date.Past(50),
                        Description = faker.Lorem.Paragraph(),
                        ImageUrl = $"https://picsum.photos/seed/{isbn}/400/600",
                        AuthorId = authorId,
                        BookGenres = new List<BookGenre>(),
                        Translations = new List<BookTranslation>()
                    };

                    // Assign 1 to 3 random genres
                    var selectedGenres = genreIds.OrderBy(_ => Guid.NewGuid()).Take(faker.Random.Number(1, 3));
                    foreach (var gId in selectedGenres)
                    {
                        book.BookGenres.Add(new BookGenre { GenreId = gId });
                    }

                    // Slovak translation
                    book.Translations.Add(new BookTranslation
                    {
                        LanguageCode = "SK",
                        Title = titleSk,
                        Description = fakerSk.Lorem.Paragraph()
                    });

                    // Greek translation
                    book.Translations.Add(new BookTranslation
                    {
                        LanguageCode = "GR",
                        Title = titleGr,
                        Description = fakerEl.Lorem.Paragraph()
                    });

                    booksBatch.Add(book);
                }

                await context.Books.AddRangeAsync(booksBatch);
                await context.SaveChangesAsync();
                context.ChangeTracker.Clear();
                Console.WriteLine($"[Seed] Seeded books {batchStart} to {batchEnd}...");
            }

            // ── Reviewers ──────────────────────────────────────────────────────
            var reviewers = new List<Reviewer>();
            for (int i = 0; i < 500; i++) // Scale reviewers to 500
            {
                reviewers.Add(new Reviewer
                {
                    Name = faker.Name.FullName()
                });
            }
            await context.Reviewers.AddRangeAsync(reviewers);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
            Console.WriteLine($"[Seed] Seeded {reviewers.Count} reviewers.");

            // ── Reviews (150,000 total) ────────────────────────────────────────
            var users = await userManager.Users.ToListAsync();
            var reviewerIds = await context.Reviewers.Select(r => r.Id).ToListAsync();
            var bookIds = await context.Books.Select(b => b.Id).ToListAsync();

            Console.WriteLine("[Seed] Generating 150,000 reviews in batches...");
            int totalReviews = 150000;
            int reviewBatchSize = 25000;

            // To compute analytics in-memory at the end, we can keep track of rating sums and counts for each bookId
            var ratingSums = new Dictionary<int, double>();
            var ratingCounts = new Dictionary<int, int>();

            foreach (var bId in bookIds)
            {
                ratingSums[bId] = 0.0;
                ratingCounts[bId] = 0;
            }

            for (int batchStart = 0; batchStart < totalReviews; batchStart += reviewBatchSize)
            {
                var reviewsBatch = new List<Review>();
                int batchEnd = Math.Min(batchStart + reviewBatchSize, totalReviews);

                for (int i = batchStart; i < batchEnd; i++)
                {
                    var bookId = faker.PickRandom(bookIds);
                    var reviewerId = faker.PickRandom(reviewerIds);
                    var user = faker.Random.Bool(0.3f) ? faker.PickRandom(users) : null;
                    var rating = faker.Random.Number(1, 5);

                    // Update in-memory stats
                    ratingSums[bookId] += rating;
                    ratingCounts[bookId]++;

                    reviewsBatch.Add(new Review
                    {
                        Text = faker.Lorem.Paragraph(1),
                        Rating = rating,
                        CreatedAt = faker.Date.Past(2),
                        BookId = bookId,
                        ReviewerId = reviewerId,
                        UserId = user?.Id
                    });
                }

                await context.Reviews.AddRangeAsync(reviewsBatch);
                await context.SaveChangesAsync();
                context.ChangeTracker.Clear();
                Console.WriteLine($"[Seed] Seeded reviews {batchStart} to {batchEnd}...");
            }

            // ── Generate Bi_BookAnalytics ─────────────────────────────────────
            Console.WriteLine("[Seed] Generating Bi_BookAnalytics...");
            var analyticsList = new List<BiBookAnalytics>();
            foreach (var bookId in bookIds)
            {
                double sum = ratingSums[bookId];
                int count = ratingCounts[bookId];
                double avg = count > 0 ? Math.Round(sum / count, 1) : 0.0;

                analyticsList.Add(new BiBookAnalytics
                {
                    BookId = bookId,
                    AverageRating = avg,
                    TotalReviews = count,
                    LastUpdated = DateTime.UtcNow
                });
            }

            int analyticsBatchSize = 5000;
            for (int i = 0; i < analyticsList.Count; i += analyticsBatchSize)
            {
                var batch = analyticsList.GetRange(i, Math.Min(analyticsBatchSize, analyticsList.Count - i));
                await context.BiBookAnalytics.AddRangeAsync(batch);
                await context.SaveChangesAsync();
                context.ChangeTracker.Clear();
            }
            Console.WriteLine("[Seed] Seeded Bi_BookAnalytics successfully.");

            // ── Migrate cover images to MinIO blob storage using pool ──────────
            try
            {
                var booksList = await context.Books.ToListAsync();
                var blobService = scope.ServiceProvider.GetRequiredService<IBlobService>();
                using var http = new HttpClient();
                http.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0");

                var minioImagePool = new List<string>();
                int poolSize = 100;

                Console.WriteLine($"[Seed] Preparing MinIO image pool of size {poolSize}...");
                for (int p = 0; p < poolSize && p < booksList.Count; p++)
                {
                    var book = booksList[p];
                    try
                    {
                        var downloadUrl = $"https://picsum.photos/seed/{book.ISBN}/400/600";
                        var response = await http.GetAsync(downloadUrl);
                        if (response.IsSuccessStatusCode)
                        {
                            var stream = await response.Content.ReadAsStreamAsync();
                            var id = await blobService.UploadFileAsync(stream, $"{book.ISBN ?? book.Title}.jpg", "image/jpeg");
                            var minioUrl = $"/api/blobs/{id}";
                            book.ImageUrl = minioUrl;
                            minioImagePool.Add(minioUrl);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Seed] Pool image download failed: {ex.Message}");
                    }
                }

                if (minioImagePool.Count == 0)
                {
                    minioImagePool.Add("/api/blobs/default-cover.jpg");
                }

                Console.WriteLine($"[Seed] MinIO image pool created with {minioImagePool.Count} images. Distributing to remaining books...");

                var random = new Random();
                for (int b = 0; b < booksList.Count; b++)
                {
                    if (b >= poolSize || string.IsNullOrWhiteSpace(booksList[b].ImageUrl) || !booksList[b].ImageUrl!.StartsWith("/api/blobs/"))
                    {
                        booksList[b].ImageUrl = minioImagePool[random.Next(minioImagePool.Count)];
                    }
                }

                await context.SaveChangesAsync();
                Console.WriteLine($"[Seed] Image migration completed using MinIO image pool.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Seed] Image migration error: {ex.Message}");
            }

            Console.WriteLine("[Seed] Database seeding completed successfully!");
        }
    }
}