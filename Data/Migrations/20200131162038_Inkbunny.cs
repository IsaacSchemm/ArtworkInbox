using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ArtworkInbox.Data.Migrations
{
    public partial class Inkbunny : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "InkbunnyLastRead",
                table: "UserReadMarkers",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserInkbunnyTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    Sid = table.Column<string>(type: "varchar(max)", nullable: false),
                    Username = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserInkbunnyTokens", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_UserInkbunnyTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserInkbunnyTokens");

            migrationBuilder.DropColumn(
                name: "InkbunnyLastRead",
                table: "UserReadMarkers");
        }
    }
}
