using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mnemo.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEntryMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Antonyms",
                table: "Entries",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<int>(
                name: "PartOfSpeech",
                table: "Entries",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Synonyms",
                table: "Entries",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "TranscriptionAudioUrl",
                table: "Entries",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Antonyms",
                table: "Entries");

            migrationBuilder.DropColumn(
                name: "PartOfSpeech",
                table: "Entries");

            migrationBuilder.DropColumn(
                name: "Synonyms",
                table: "Entries");

            migrationBuilder.DropColumn(
                name: "TranscriptionAudioUrl",
                table: "Entries");
        }
    }
}
