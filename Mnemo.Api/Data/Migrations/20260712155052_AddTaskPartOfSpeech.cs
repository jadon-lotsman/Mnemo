using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mnemo.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskPartOfSpeech : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AsessmentEntryId",
                table: "RepetitionTasks",
                newName: "VocabularyEntryId");

            migrationBuilder.AddColumn<int>(
                name: "EntryPartOfSpeech",
                table: "RepetitionTasks",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EntryPartOfSpeech",
                table: "RepetitionTasks");

            migrationBuilder.RenameColumn(
                name: "VocabularyEntryId",
                table: "RepetitionTasks",
                newName: "AsessmentEntryId");
        }
    }
}
