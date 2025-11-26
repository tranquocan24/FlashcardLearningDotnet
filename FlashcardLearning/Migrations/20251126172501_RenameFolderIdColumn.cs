using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlashcardLearning.Migrations
{
    /// <inheritdoc />
    public partial class RenameFolderIdColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Decks_Folders_folder_id",
                table: "Decks");

            migrationBuilder.RenameColumn(
                name: "folder_id",
                table: "Decks",
                newName: "FolderId");

            migrationBuilder.RenameIndex(
                name: "IX_Decks_folder_id",
                table: "Decks",
                newName: "IX_Decks_FolderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Decks_Folders_FolderId",
                table: "Decks",
                column: "FolderId",
                principalTable: "Folders",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Decks_Folders_FolderId",
                table: "Decks");

            migrationBuilder.RenameColumn(
                name: "FolderId",
                table: "Decks",
                newName: "folder_id");

            migrationBuilder.RenameIndex(
                name: "IX_Decks_FolderId",
                table: "Decks",
                newName: "IX_Decks_folder_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Decks_Folders_folder_id",
                table: "Decks",
                column: "folder_id",
                principalTable: "Folders",
                principalColumn: "Id");
        }
    }
}
