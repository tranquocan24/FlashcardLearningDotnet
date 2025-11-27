using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlashcardLearning.Migrations
{
    /// <inheritdoc />
    public partial class AddDictionaryEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DictionaryEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Word = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Meaning = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CachedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DictionaryEntries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DictionaryEntries_Word",
                table: "DictionaryEntries",
                column: "Word",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DictionaryEntries");
        }
    }
}
