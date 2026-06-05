using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mnemo.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRepetitionResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Entries_RepetitionResults_RepetitionResultId",
                table: "Entries");

            migrationBuilder.DropTable(
                name: "RepetitionResults");

            migrationBuilder.DropIndex(
                name: "IX_Entries_RepetitionResultId",
                table: "Entries");

            migrationBuilder.DropColumn(
                name: "RepetitionResultId",
                table: "Entries");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RepetitionResultId",
                table: "Entries",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RepetitionResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Correct = table.Column<int>(type: "INTEGER", nullable: false),
                    FinishedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Percent = table.Column<int>(type: "INTEGER", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Total = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepetitionResults", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Entries_RepetitionResultId",
                table: "Entries",
                column: "RepetitionResultId");

            migrationBuilder.AddForeignKey(
                name: "FK_Entries_RepetitionResults_RepetitionResultId",
                table: "Entries",
                column: "RepetitionResultId",
                principalTable: "RepetitionResults",
                principalColumn: "Id");
        }
    }
}
