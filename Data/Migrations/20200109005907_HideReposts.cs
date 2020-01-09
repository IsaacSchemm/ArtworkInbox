using Microsoft.EntityFrameworkCore.Migrations;

namespace ArtworkInbox.Data.Migrations
{
    public partial class HideReposts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HideReposts",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HideReposts",
                table: "AspNetUsers");
        }
    }
}
