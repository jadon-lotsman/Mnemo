using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mnemo.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RepetitionResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Correct = table.Column<int>(type: "INTEGER", nullable: false),
                    Total = table.Column<int>(type: "INTEGER", nullable: false),
                    Percent = table.Column<int>(type: "INTEGER", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FinishedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepetitionResults", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    RegisteredAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Entries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Foreign = table.Column<string>(type: "TEXT", nullable: false),
                    Transcription = table.Column<string>(type: "TEXT", nullable: false),
                    Examples = table.Column<string>(type: "TEXT", nullable: false),
                    Translations = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    RepetitionResultId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Entries_RepetitionResults_RepetitionResultId",
                        column: x => x.RepetitionResultId,
                        principalTable: "RepetitionResults",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Entries_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RepetitionTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AsessmentEntryId = table.Column<int>(type: "INTEGER", nullable: false),
                    Prompt = table.Column<string>(type: "TEXT", nullable: false),
                    UserAnswer = table.Column<string>(type: "TEXT", nullable: false),
                    ActionCounter = table.Column<int>(type: "INTEGER", nullable: false),
                    ElapsedTime = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    task_type = table.Column<string>(type: "TEXT", maxLength: 21, nullable: false),
                    Options = table.Column<string>(type: "TEXT", nullable: true),
                    CorrectOption = table.Column<string>(type: "TEXT", nullable: true),
                    SentenceParts = table.Column<string>(type: "TEXT", nullable: true),
                    CorrectOrder = table.Column<string>(type: "TEXT", nullable: true),
                    CorrectAnswers = table.Column<string>(type: "TEXT", nullable: true),
                    Option = table.Column<string>(type: "TEXT", nullable: true),
                    CorrectYesOrNo = table.Column<bool>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepetitionTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RepetitionTasks_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RepetitionStates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RepetitionCounter = table.Column<int>(type: "INTEGER", nullable: false),
                    RepetitionInterval = table.Column<int>(type: "INTEGER", nullable: false),
                    EasinessFactor = table.Column<double>(type: "REAL", nullable: false),
                    CanSelfAssess = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastRepetitionAt = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    NextRepetitionAt = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    VocabularyEntryId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepetitionStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RepetitionStates_Entries_VocabularyEntryId",
                        column: x => x.VocabularyEntryId,
                        principalTable: "Entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RepetitionStates_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Entries_RepetitionResultId",
                table: "Entries",
                column: "RepetitionResultId");

            migrationBuilder.CreateIndex(
                name: "IX_Entries_UserId",
                table: "Entries",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RepetitionStates_UserId",
                table: "RepetitionStates",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RepetitionStates_VocabularyEntryId",
                table: "RepetitionStates",
                column: "VocabularyEntryId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RepetitionTasks_UserId",
                table: "RepetitionTasks",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RepetitionStates");

            migrationBuilder.DropTable(
                name: "RepetitionTasks");

            migrationBuilder.DropTable(
                name: "Entries");

            migrationBuilder.DropTable(
                name: "RepetitionResults");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
