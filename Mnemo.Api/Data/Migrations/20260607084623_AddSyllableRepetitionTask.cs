using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mnemo.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSyllableRepetitionTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SyllableReorderRepetitionTask_CorrectOrder",
                table: "RepetitionTasks",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Syllables",
                table: "RepetitionTasks",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SyllableReorderRepetitionTask_CorrectOrder",
                table: "RepetitionTasks");

            migrationBuilder.DropColumn(
                name: "Syllables",
                table: "RepetitionTasks");
        }
    }
}
