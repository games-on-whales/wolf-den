using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WolfLeash.Migrations
{
    /// <inheritdoc />
    public partial class EnableCascadingDeleteForRunner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Apps_Runners_RunnerId",
                table: "Apps");

            migrationBuilder.DropIndex(
                name: "IX_Apps_RunnerId",
                table: "Apps");

            migrationBuilder.DropColumn(
                name: "RunnerId",
                table: "Apps");

            migrationBuilder.AddColumn<int>(
                name: "DbAppId",
                table: "Runners",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Runners_DbAppId",
                table: "Runners",
                column: "DbAppId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Runners_Apps_DbAppId",
                table: "Runners",
                column: "DbAppId",
                principalTable: "Apps",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Runners_Apps_DbAppId",
                table: "Runners");

            migrationBuilder.DropIndex(
                name: "IX_Runners_DbAppId",
                table: "Runners");

            migrationBuilder.DropColumn(
                name: "DbAppId",
                table: "Runners");

            migrationBuilder.AddColumn<int>(
                name: "RunnerId",
                table: "Apps",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Apps_RunnerId",
                table: "Apps",
                column: "RunnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Apps_Runners_RunnerId",
                table: "Apps",
                column: "RunnerId",
                principalTable: "Runners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
