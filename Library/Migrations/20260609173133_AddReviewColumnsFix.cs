using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Library.Migrations
{
    
    public partial class AddReviewColumnsFix : Migration
    {
        
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Rating",
                table: "Reviews",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Reviews",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");
        }

        
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rating",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Reviews");
        }
    }
}
