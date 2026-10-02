using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Mnemo.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Username = table.Column<string>(type: "text", nullable: false),
                    RegisteredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RepetitionTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VocabularyEntryId = table.Column<int>(type: "integer", nullable: false),
                    EntryPartOfSpeech = table.Column<int>(type: "integer", nullable: true),
                    Prompt = table.Column<string>(type: "text", nullable: false),
                    UserAnswer = table.Column<string>(type: "text", nullable: false),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    ActionCounter = table.Column<int>(type: "integer", nullable: false),
                    ElapsedTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    OwnerId = table.Column<int>(type: "integer", nullable: false),
                    task_type = table.Column<string>(type: "character varying(21)", maxLength: 21, nullable: false),
                    Options = table.Column<List<string>>(type: "text[]", nullable: true),
                    CorrectOption = table.Column<string>(type: "text", nullable: true),
                    SentenceParts = table.Column<List<string>>(type: "text[]", nullable: true),
                    CorrectOrder = table.Column<string>(type: "text", nullable: true),
                    Syllables = table.Column<List<string>>(type: "text[]", nullable: true),
                    SyllableReorderRepetitionTask_CorrectOrder = table.Column<string>(type: "text", nullable: true),
                    CorrectAnswers = table.Column<List<string>>(type: "text[]", nullable: true),
                    Option = table.Column<string>(type: "text", nullable: true),
                    CorrectYesOrNo = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepetitionTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RepetitionTasks_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Vocabularies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Guid = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Visibility = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    OwnerId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vocabularies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vocabularies_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VocabularyEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EnrichmentStatus = table.Column<int>(type: "integer", nullable: false),
                    LastEnrichmentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OwnerId = table.Column<int>(type: "integer", nullable: false),
                    MergedFromId = table.Column<int>(type: "integer", nullable: true),
                    PartOfSpeech = table.Column<int>(type: "integer", nullable: true),
                    CEFR = table.Column<int>(type: "integer", nullable: true),
                    Foreign = table.Column<string>(type: "text", nullable: false),
                    Transcription = table.Column<string>(type: "text", nullable: true),
                    AudioUrl = table.Column<string>(type: "text", nullable: true),
                    Examples = table.Column<List<string>>(type: "text[]", nullable: false),
                    Translations = table.Column<List<string>>(type: "text[]", nullable: false),
                    Synonyms = table.Column<List<string>>(type: "text[]", nullable: false),
                    Antonyms = table.Column<List<string>>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VocabularyEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VocabularyEntries_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VocabularyEntries_Vocabularies_MergedFromId",
                        column: x => x.MergedFromId,
                        principalTable: "Vocabularies",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RepetitionStates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RepetitionCounter = table.Column<int>(type: "integer", nullable: false),
                    RepetitionInterval = table.Column<int>(type: "integer", nullable: false),
                    EasinessFactor = table.Column<double>(type: "double precision", nullable: false),
                    LastRepetitionAt = table.Column<DateOnly>(type: "date", nullable: false),
                    NextRepetitionAt = table.Column<DateOnly>(type: "date", nullable: false),
                    VocabularyEntryId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepetitionStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RepetitionStates_VocabularyEntries_VocabularyEntryId",
                        column: x => x.VocabularyEntryId,
                        principalTable: "VocabularyEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VocabularyEntryLinks",
                columns: table => new
                {
                    VocabularyId = table.Column<int>(type: "integer", nullable: false),
                    VocabularyEntryId = table.Column<int>(type: "integer", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
                name: "IX_RepetitionStates_VocabularyEntryId",
                table: "RepetitionStates",
                column: "VocabularyEntryId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RepetitionTasks_OwnerId",
                table: "RepetitionTasks",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Vocabularies_OwnerId",
                table: "Vocabularies",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_VocabularyEntries_MergedFromId",
                table: "VocabularyEntries",
                column: "MergedFromId");

            migrationBuilder.CreateIndex(
                name: "IX_VocabularyEntries_OwnerId_Foreign_PartOfSpeech",
                table: "VocabularyEntries",
                columns: new[] { "OwnerId", "Foreign", "PartOfSpeech" });

            migrationBuilder.CreateIndex(
                name: "IX_VocabularyEntries_OwnerId_MergedFromId",
                table: "VocabularyEntries",
                columns: new[] { "OwnerId", "MergedFromId" });

            migrationBuilder.CreateIndex(
                name: "IX_VocabularyEntryLinks_VocabularyEntryId",
                table: "VocabularyEntryLinks",
                column: "VocabularyEntryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RepetitionStates");

            migrationBuilder.DropTable(
                name: "RepetitionTasks");

            migrationBuilder.DropTable(
                name: "VocabularyEntryLinks");

            migrationBuilder.DropTable(
                name: "VocabularyEntries");

            migrationBuilder.DropTable(
                name: "Vocabularies");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
