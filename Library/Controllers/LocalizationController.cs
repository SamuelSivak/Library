using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Library.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocalizationController : ControllerBase
    {
        private static readonly Dictionary<string, Dictionary<string, string>> Translations = new()
        {
            ["SK"] = new Dictionary<string, string>
            {
                ["nav.books"] = "Knihy",
                ["nav.authors"] = "Autori",
                ["nav.genres"] = "Žánre",
                ["nav.login"] = "Prihlásiť sa",
                ["nav.logout"] = "Odhlásiť sa",
                ["nav.admin"] = "Administrácia",
                ["search.placeholder"] = "Hľadať knihy, autorov, žánre...",
                ["search.results"] = "výsledkov",
                ["admin.title"] = "Administrácia kníh",
                ["admin.subtitle"] = "Správa katalógu kníh, nahrávanie obálok a úprava detailov.",
                ["admin.addBtn"] = "Pridať knihu",
                ["admin.cover"] = "Obálka",
                ["admin.title_header"] = "Názov",
                ["admin.author"] = "Autor",
                ["admin.genres"] = "Žánre",
                ["admin.pages"] = "Počet strán",
                ["admin.isbn"] = "ISBN",
                ["admin.published"] = "Vydané",
                ["admin.actions"] = "Akcie",
                ["admin.loading"] = "Načítavajú sa dáta z databázy...",
                ["admin.empty"] = "V databáze nie sú žiadne knihy.",
                ["admin.addTitle"] = "Pridať novú knihu",
                ["admin.editTitle"] = "Upraviť knihu",
                ["admin.bookTitleLabel"] = "Názov knihy *",
                ["admin.isbnLabel"] = "ISBN (13 číslic)",
                ["admin.pagesLabel"] = "Počet strán",
                ["admin.authorLabel"] = "Autor *",
                ["admin.publishedLabel"] = "Dátum vydania",
                ["admin.descriptionLabel"] = "Popis knihy",
                ["admin.descriptionPlaceholder"] = "Stručný obsah knihy...",
                ["admin.coverLabel"] = "Obrázok obálky (Nahrať na Blob Server / MinIO)",
                ["admin.uploadClick"] = "Kliknite pre nahratie obálky",
                ["admin.uploadStatus"] = "Nahrávanie na S3 Blob úložisko...",
                ["admin.uploadFailed"] = "Nahrávanie zlyhalo. Uistite sa, že Docker MinIO beží.",
                ["admin.imageUrlPlaceholder"] = "URL obrázka (z MinIO alebo externé)",
                ["admin.coverSelected"] = "Vybratá obálka z Blob servera",
                ["admin.genresLabel"] = "Žánre",
                ["admin.cancel"] = "Zrušiť",
                ["admin.save"] = "Uložiť knihu",
                ["details.pages"] = "Počet strán",
                ["details.isbn"] = "ISBN",
                ["details.published"] = "Vydané",
                ["details.no_desc"] = "Táto kniha nemá žiadny popis.",
                ["details.rating"] = "Celkové hodnotenie",
                ["details.reviews_count"] = "recenzií",
                ["details.reviews_title"] = "Recenzie čitateľov",
                ["details.no_reviews"] = "Pre túto knihu zatiaľ nie sú žiadne recenzie.",
                ["details.add_review_title"] = "Pridať recenziu",
                ["details.review_placeholder"] = "Sem napíšte svoju recenziu...",
                ["details.submit_review"] = "Odoslať recenziu",
                ["details.login_to_review"] = "Pre napísanie recenzie sa musíte prihlásiť."
            },
            ["EN"] = new Dictionary<string, string>
            {
                ["nav.books"] = "Books",
                ["nav.authors"] = "Authors",
                ["nav.genres"] = "Genres",
                ["nav.login"] = "Log In",
                ["nav.logout"] = "Sign Out",
                ["nav.admin"] = "Admin Panel",
                ["search.placeholder"] = "Search books, authors, genres...",
                ["search.results"] = "results",
                ["admin.title"] = "Book Administration",
                ["admin.subtitle"] = "Manage book catalog, upload covers and edit details.",
                ["admin.addBtn"] = "Add Book",
                ["admin.cover"] = "Cover",
                ["admin.title_header"] = "Title",
                ["admin.author"] = "Author",
                ["admin.genres"] = "Genres",
                ["admin.pages"] = "Page Count",
                ["admin.isbn"] = "ISBN",
                ["admin.published"] = "Published",
                ["admin.actions"] = "Actions",
                ["admin.loading"] = "Loading data from database...",
                ["admin.empty"] = "No books in the database.",
                ["admin.addTitle"] = "Add New Book",
                ["admin.editTitle"] = "Edit Book",
                ["admin.bookTitleLabel"] = "Book Title *",
                ["admin.isbnLabel"] = "ISBN (13 digits)",
                ["admin.pagesLabel"] = "Page Count",
                ["admin.authorLabel"] = "Author *",
                ["admin.publishedLabel"] = "Published Date",
                ["admin.descriptionLabel"] = "Book Description",
                ["admin.descriptionPlaceholder"] = "Brief content of the book...",
                ["admin.coverLabel"] = "Cover Image (Upload to Blob Server / MinIO)",
                ["admin.uploadClick"] = "Click to upload cover",
                ["admin.uploadStatus"] = "Uploading to S3 Blob Storage...",
                ["admin.uploadFailed"] = "Upload failed. Make sure Docker MinIO is running.",
                ["admin.imageUrlPlaceholder"] = "Image URL (from MinIO or external)",
                ["admin.coverSelected"] = "Selected cover from Blob server",
                ["admin.genresLabel"] = "Genres",
                ["admin.cancel"] = "Cancel",
                ["admin.save"] = "Save Book",
                ["details.pages"] = "Page Count",
                ["details.isbn"] = "ISBN",
                ["details.published"] = "Published",
                ["details.no_desc"] = "This book has no description.",
                ["details.rating"] = "Overall Rating",
                ["details.reviews_count"] = "reviews",
                ["details.reviews_title"] = "Reader Reviews",
                ["details.no_reviews"] = "There are no reviews for this book yet.",
                ["details.add_review_title"] = "Add a Review",
                ["details.review_placeholder"] = "Write your review here...",
                ["details.submit_review"] = "Submit Review",
                ["details.login_to_review"] = "You must log in to write a review."
            },
            ["GR"] = new Dictionary<string, string>
            {
                ["nav.books"] = "Βιβλία",
                ["nav.authors"] = "Συγγραφείς",
                ["nav.genres"] = "Κατηγορίες",
                ["nav.login"] = "Σύνδεση",
                ["nav.logout"] = "Αποσύνδεση",
                ["nav.admin"] = "Πίνακας Admin",
                ["search.placeholder"] = "Αναζήτηση βιβλίων, συγγραφέων, κατηγοριών...",
                ["search.results"] = "αποτελέσματα",
                ["admin.title"] = "Διαχείριση Βιβλίων",
                ["admin.subtitle"] = "Διαχειριστείτε τον κατάλογο, ανεβάστε εξώφυλλα και επεξεργαστείτε λεπτομέρειες.",
                ["admin.addBtn"] = "Προσθήκη Βιβλίου",
                ["admin.cover"] = "Εξώφυλλο",
                ["admin.title_header"] = "Τίτλος",
                ["admin.author"] = "Συγγραφέας",
                ["admin.genres"] = "Κατηγορίες",
                ["admin.pages"] = "Σελίδες",
                ["admin.isbn"] = "ISBN",
                ["admin.published"] = "Δημοσιεύθηκε",
                ["admin.actions"] = "Ενέργειες",
                ["admin.loading"] = "Φόρτωση δεδομένων...",
                ["admin.empty"] = "Δεν υπάρχουν βιβλία στη βάση δεδομένων.",
                ["admin.addTitle"] = "Προσθήκη Νέου Βιβλίου",
                ["admin.editTitle"] = "Επεξεργασία Βιβλίου",
                ["admin.bookTitleLabel"] = "Τίτλος Βιβλίου *",
                ["admin.isbnLabel"] = "ISBN (13 ψηφία)",
                ["admin.pagesLabel"] = "Αριθμός Σελίδων",
                ["admin.authorLabel"] = "Συγγραφέας *",
                ["admin.publishedLabel"] = "Ημερομηνία Δημοσίευσης",
                ["admin.descriptionLabel"] = "Περιγραφή Βιβλίου",
                ["admin.descriptionPlaceholder"] = "Σύντομη περιγραφή του βιβλίου...",
                ["admin.coverLabel"] = "Εικόνα Εξωφύλλου (Μεταφόρτωση σε Blob Server / MinIO)",
                ["admin.uploadClick"] = "Κάντε κλικ για να ανεβάσετε εξώφυλλο",
                ["admin.uploadStatus"] = "Μεταφόρτωση σε S3 Blob Storage...",
                ["admin.uploadFailed"] = "Η μεταφόρτωση απέτυχε. Βεβαιωθείτε ότι το MinIO εκτελείται.",
                ["admin.imageUrlPlaceholder"] = "URL Εικόνας (από MinIO ή εξωτερικό)",
                ["admin.coverSelected"] = "Επιλεγμένο εξώφυλλο από το Blob server",
                ["admin.genresLabel"] = "Κατηγορίες",
                ["admin.cancel"] = "Ακύρωση",
                ["admin.save"] = "Αποθήκευση Βιβλίου",
                ["details.pages"] = "Αριθμός Σελίδων",
                ["details.isbn"] = "ISBN",
                ["details.published"] = "Δημοσιεύθηκε",
                ["details.no_desc"] = "Αυτό το βιβλίο δεν έχει περιγραφή.",
                ["details.rating"] = "Συνολική Βαθμολογία",
                ["details.reviews_count"] = "κριτικές",
                ["details.reviews_title"] = "Κριτικές Αναγνωστών",
                ["details.no_reviews"] = "Δεν υπάρχουν κριτικές για αυτό το βιβλίο ακόμη.",
                ["details.add_review_title"] = "Προσθήκη Κριτικής",
                ["details.review_placeholder"] = "Γράψτε την κριτική σας εδώ...",
                ["details.submit_review"] = "Υποβολή Κριτικής",
                ["details.login_to_review"] = "Πρέπει να συνδεθείτε για να γράψετε κριτική."
            }
        };

        [HttpGet]
        public IActionResult Get([FromQuery] string lang = "EN")
        {
            var normalizedLang = lang?.ToUpper() ?? "EN";
            
            if (!Translations.ContainsKey(normalizedLang))
            {
                normalizedLang = "EN"; // Fallback to English
            }

            return Ok(Translations[normalizedLang]);
        }
    }
}
