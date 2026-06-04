using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mnemo.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUserFromRepetitionStateAndRenameCanSelfAssess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RepetitionStates_Users_UserId",
                table: "RepetitionStates");

            migrationBuilder.RenameColumn(
                name: "CanSelfAssess",
                table: "RepetitionStates",
                newName: "CanAdjustToday");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "RepetitionStates",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddForeignKey(
                name: "FK_RepetitionStates_Users_UserId",
                table: "RepetitionStates",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RepetitionStates_Users_UserId",
                table: "RepetitionStates");

            migrationBuilder.RenameColumn(
                name: "CanAdjustToday",
                table: "RepetitionStates",
                newName: "CanSelfAssess");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "RepetitionStates",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RepetitionStates_Users_UserId",
                table: "RepetitionStates",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
