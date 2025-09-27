using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class ConvertYearPublishedToDateTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add a temporary column for the new DateTime value
            migrationBuilder.AddColumn<DateTime>(
                name: "YearPublishedTemp",
                table: "Books",
                type: "datetime2",
                nullable: true);

            // Convert existing int year values to DateTime (January 1st of that year)
            migrationBuilder.Sql(@"
                UPDATE Books 
                SET YearPublishedTemp = DATEFROMPARTS(YearPublished, 1, 1) 
                WHERE YearPublished IS NOT NULL AND YearPublished > 0");

            // Drop the old column
            migrationBuilder.DropColumn(
                name: "YearPublished",
                table: "Books");

            // Rename the temporary column to the original name
            migrationBuilder.RenameColumn(
                name: "YearPublishedTemp",
                table: "Books",
                newName: "YearPublished");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Add a temporary column for the int value
            migrationBuilder.AddColumn<int>(
                name: "YearPublishedTemp",
                table: "Books",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Convert DateTime values back to int (extract year)
            migrationBuilder.Sql(@"
                UPDATE Books 
                SET YearPublishedTemp = YEAR(YearPublished) 
                WHERE YearPublished IS NOT NULL");

            // Drop the DateTime column
            migrationBuilder.DropColumn(
                name: "YearPublished",
                table: "Books");

            // Rename the temporary column to the original name
            migrationBuilder.RenameColumn(
                name: "YearPublishedTemp",
                table: "Books",
                newName: "YearPublished");
        }
    }
}
