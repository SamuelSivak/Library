using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Library.Migrations
{
    
    public partial class AddFullTextSearch : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.fulltext_catalogs WHERE name = 'LibraryCatalog')
                BEGIN
                    CREATE FULLTEXT CATALOG LibraryCatalog AS DEFAULT;
                END", suppressTransaction: true);

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID('Books'))
                BEGIN
                    CREATE FULLTEXT INDEX ON Books(Title) KEY INDEX PK_Books ON LibraryCatalog WITH STOPLIST = SYSTEM;
                END", suppressTransaction: true);

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID('BookTranslations'))
                BEGIN
                    CREATE FULLTEXT INDEX ON BookTranslations(Title) KEY INDEX PK_BookTranslations ON LibraryCatalog WITH STOPLIST = SYSTEM;
                END", suppressTransaction: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID('BookTranslations'))
                BEGIN
                    DROP FULLTEXT INDEX ON BookTranslations;
                END", suppressTransaction: true);

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID('Books'))
                BEGIN
                    DROP FULLTEXT INDEX ON Books;
                END", suppressTransaction: true);

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.fulltext_catalogs WHERE name = 'LibraryCatalog')
                BEGIN
                    DROP FULLTEXT CATALOG LibraryCatalog;
                END", suppressTransaction: true);
        }
    }
}
