using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Library.Migrations
{
    
    public partial class AddSortingColumnsToAnalytics : Migration
    {
        
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Clicks",
                table: "Bi_BookAnalytics",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NegativeReviews",
                table: "Bi_BookAnalytics",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PositiveReviews",
                table: "Bi_BookAnalytics",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Clicks",
                table: "Bi_BookAnalytics");

            migrationBuilder.DropColumn(
                name: "NegativeReviews",
                table: "Bi_BookAnalytics");

            migrationBuilder.DropColumn(
                name: "PositiveReviews",
                table: "Bi_BookAnalytics");
        }
    }
}
