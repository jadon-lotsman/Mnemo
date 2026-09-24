using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mnemo.Data.Migrations
{
    /// <inheritdoc />
    public partial class MigrateToLinked : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VocabularyEntries_Vocabularies_VocabularyId",
                table: "VocabularyEntries");

            migrationBuilder.RenameColumn(
                name: "VocabularyId",
                table: "VocabularyEntries",
                newName: "OwnerId");

            migrationBuilder.RenameIndex(
                name: "IX_VocabularyEntries_VocabularyId_MergedFromId",
                table: "VocabularyEntries",
                newName: "IX_VocabularyEntries_OwnerId_MergedFromId");

            migrationBuilder.RenameIndex(
                name: "IX_VocabularyEntries_VocabularyId_Foreign_PartOfSpeech",
                table: "VocabularyEntries",
                newName: "IX_VocabularyEntries_OwnerId_Foreign_PartOfSpeech");

            migrationBuilder.CreateTable(
                name: "VocabularyEntryLinks",
                columns: table => new
                {
                    VocabularyId = table.Column<int>(type: "INTEGER", nullable: false),
                    VocabularyEntryId = table.Column<int>(type: "INTEGER", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VocabularyEntryLinks", x => new { x.VocabularyId, x.VocabularyEntryId });
                    table.ForeignKey(
                        name: "FK_VocabularyEntryLinks_Vocabularies_VocabularyId",
                        column: x => x.VocabularyId,
                        principalTable: "Vocabularies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VocabularyEntryLinks_VocabularyEntries_VocabularyEntryId",
                        column: x => x.VocabularyEntryId,
                        principalTable: "VocabularyEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VocabularyEntryLinks_VocabularyEntryId",
                table: "VocabularyEntryLinks",
                column: "VocabularyEntryId");

            migrationBuilder.AddForeignKey(
                name: "FK_VocabularyEntries_Users_OwnerId",
                table: "VocabularyEntries",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VocabularyEntries_Users_OwnerId",
                table: "VocabularyEntries");

            migrationBuilder.DropTable(
                name: "VocabularyEntryLinks");

            migrationBuilder.RenameColumn(
                name: "OwnerId",
                table: "VocabularyEntries",
                newName: "VocabularyId");

            migrationBuilder.RenameIndex(
                name: "IX_VocabularyEntries_OwnerId_MergedFromId",
                table: "VocabularyEntries",
                newName: "IX_VocabularyEntries_VocabularyId_MergedFromId");

            migrationBuilder.RenameIndex(
                name: "IX_VocabularyEntries_OwnerId_Foreign_PartOfSpeech",
                table: "VocabularyEntries",
                newName: "IX_VocabularyEntries_VocabularyId_Foreign_PartOfSpeech");

            migrationBuilder.AddForeignKey(
                name: "FK_VocabularyEntries_Vocabularies_VocabularyId",
                table: "VocabularyEntries",
                column: "VocabularyId",
                principalTable: "Vocabularies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
