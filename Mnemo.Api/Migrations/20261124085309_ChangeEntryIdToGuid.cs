using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Mnemo.Migrations
{
    /// <inheritdoc />
    public partial class ChangeEntryIdToGuid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("TRUNCATE TABLE \"GlossaryEntries\", \"RepetitionTasks\", \"RepetitionStates\", \"GlossaryLinks\" CASCADE;");

            migrationBuilder.DropColumn(name: "EntryId", table: "RepetitionTasks");
            migrationBuilder.DropColumn(name: "EntryId", table: "RepetitionStates");
            migrationBuilder.DropColumn(name: "EntryId", table: "GlossaryLinks");
            migrationBuilder.DropColumn(name: "Id", table: "GlossaryEntries");

            migrationBuilder.AddColumn<Guid>(
                name: "EntryId",
                table: "RepetitionTasks",
                type: "uuid",
                nullable: false);

            migrationBuilder.AddColumn<Guid>(
                name: "EntryId",
                table: "RepetitionStates",
                type: "uuid",
                nullable: false);

            migrationBuilder.AddColumn<Guid>(
                name: "EntryId",
                table: "GlossaryLinks",
                type: "uuid",
                nullable: false);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "GlossaryEntries",
                type: "uuid",
                nullable: false,
                defaultValueSql: "uuidv7()");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GlossaryEntries",
                table: "GlossaryEntries",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "EntryId",
                table: "RepetitionTasks",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "EntryId",
                table: "RepetitionStates",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "EntryId",
                table: "GlossaryLinks",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "GlossaryEntries",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "uuidv7()")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        }
    }
}
