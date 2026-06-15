using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Library.Migrations
{
    /// <inheritdoc />
    public partial class AddSortingColumnsToAnalytics : Migration
    {
        /// <inheritdoc />
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

        /// <inheritdoc />
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
