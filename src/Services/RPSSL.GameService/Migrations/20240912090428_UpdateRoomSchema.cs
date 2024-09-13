using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RPSSL.GameService.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRoomSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Games_RoomId",
                table: "Games");

            migrationBuilder.CreateIndex(
                name: "IX_Games_RoomId",
                table: "Games",
                column: "RoomId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Games_RoomId",
                table: "Games");

            migrationBuilder.CreateIndex(
                name: "IX_Games_RoomId",
                table: "Games",
                column: "RoomId",
                unique: true);
        }
    }
}
