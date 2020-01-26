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

            migrationBuilder.AddColumn<string>(
                name: "InkbunnySessionId",
                table: "AspNetUsers",
                type: "varchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InkbunnyLastRead",
                table: "UserReadMarkers");

            migrationBuilder.DropColumn(
                name: "InkbunnySessionId",
                table: "AspNetUsers");
        }
    }
}
