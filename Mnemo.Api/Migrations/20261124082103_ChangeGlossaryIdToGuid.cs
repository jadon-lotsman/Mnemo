using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Mnemo.Migrations
{
    /// <inheritdoc />
    public partial class ChangeGlossaryIdToGuid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RepetitionStates_VocabularyEntries_VocabularyEntryId",
                table: "RepetitionStates");

            migrationBuilder.DropTable(
                name: "VocabularyEntryLinks");

            migrationBuilder.DropTable(
                name: "VocabularyEntries");

            migrationBuilder.DropTable(
                name: "Vocabularies");

            migrationBuilder.RenameColumn(
                name: "VocabularyEntryId",
                table: "RepetitionTasks",
                newName: "EntryId");

            migrationBuilder.RenameColumn(
                name: "VocabularyEntryId",
                table: "RepetitionStates",
                newName: "EntryId");

            migrationBuilder.RenameIndex(
                name: "IX_RepetitionStates_VocabularyEntryId",
                table: "RepetitionStates",
                newName: "IX_RepetitionStates_EntryId");

            migrationBuilder.CreateTable(
                name: "Glossaries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuidv7()"),
                    ShareGuid = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OwnerId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Glossaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Glossaries_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GlossaryEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IsEnrolled = table.Column<bool>(type: "boolean", nullable: false),
                    EnrichmentStatus = table.Column<int>(type: "integer", nullable: false),
                    LastEnrichmentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OwnerId = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_GlossaryEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GlossaryEntries_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GlossaryLinks",
                columns: table => new
                {
                    GlossaryId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntryId = table.Column<int>(type: "integer", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlossaryLinks", x => new { x.GlossaryId, x.EntryId });
                    table.ForeignKey(
                        name: "FK_GlossaryLinks_Glossaries_GlossaryId",
                        column: x => x.GlossaryId,
                        principalTable: "Glossaries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GlossaryLinks_GlossaryEntries_EntryId",
                        column: x => x.EntryId,
                        principalTable: "GlossaryEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Glossaries_OwnerId",
                table: "Glossaries",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_GlossaryEntries_OwnerId_Foreign_PartOfSpeech",
                table: "GlossaryEntries",
                columns: new[] { "OwnerId", "Foreign", "PartOfSpeech" });

            migrationBuilder.CreateIndex(
                name: "IX_GlossaryLinks_EntryId",
                table: "GlossaryLinks",
                column: "EntryId");

            migrationBuilder.AddForeignKey(
                name: "FK_RepetitionStates_GlossaryEntries_EntryId",
                table: "RepetitionStates",
                column: "EntryId",
                principalTable: "GlossaryEntries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RepetitionStates_GlossaryEntries_EntryId",
                table: "RepetitionStates");

            migrationBuilder.DropTable(
                name: "GlossaryLinks");

            migrationBuilder.DropTable(
                name: "Glossaries");

            migrationBuilder.DropTable(
                name: "GlossaryEntries");

            migrationBuilder.RenameColumn(
                name: "EntryId",
                table: "RepetitionTasks",
                newName: "VocabularyEntryId");

            migrationBuilder.RenameColumn(
                name: "EntryId",
                table: "RepetitionStates",
                newName: "VocabularyEntryId");

            migrationBuilder.RenameIndex(
                name: "IX_RepetitionStates_EntryId",
                table: "RepetitionStates",
                newName: "IX_RepetitionStates_VocabularyEntryId");

            migrationBuilder.CreateTable(
                name: "Vocabularies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OwnerId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Guid = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Visibility = table.Column<int>(type: "integer", nullable: false)
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
                    MergedFromId = table.Column<int>(type: "integer", nullable: true),
                    OwnerId = table.Column<int>(type: "integer", nullable: false),
                    Antonyms = table.Column<List<string>>(type: "text[]", nullable: false),
                    AudioUrl = table.Column<string>(type: "text", nullable: true),
                    CEFR = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EnrichmentStatus = table.Column<int>(type: "integer", nullable: false),
                    Examples = table.Column<List<string>>(type: "text[]", nullable: false),
                    Foreign = table.Column<string>(type: "text", nullable: false),
                    LastEnrichmentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PartOfSpeech = table.Column<int>(type: "integer", nullable: true),
                    Synonyms = table.Column<List<string>>(type: "text[]", nullable: false),
                    Transcription = table.Column<string>(type: "text", nullable: true),
                    Translations = table.Column<List<string>>(type: "text[]", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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

            migrationBuilder.AddForeignKey(
                name: "FK_RepetitionStates_VocabularyEntries_VocabularyEntryId",
                table: "RepetitionStates",
                column: "VocabularyEntryId",
                principalTable: "VocabularyEntries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
