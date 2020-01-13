using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ArtworkInbox.Data.Migrations
{
    public partial class Weasyl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "WeasylLastRead",
                table: "UserReadMarkers",
                nullable: true,
                defaultValue: null);

            migrationBuilder.AddColumn<string>(
                name: "WeasylApiKey",
                table: "AspNetUsers",
                type: "varchar(max)",
                nullable: true,
                defaultValue: null);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WeasylLastRead",
                table: "UserReadMarkers");

            migrationBuilder.DropColumn(
                name: "WeasylApiKey",
                table: "AspNetUsers");
        }
    }
}
