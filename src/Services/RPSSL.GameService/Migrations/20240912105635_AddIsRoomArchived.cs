using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RPSSL.GameService.Migrations
{
    /// <inheritdoc />
    public partial class AddIsRoomArchived : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Rooms",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "Rooms");
        }
    }
}
