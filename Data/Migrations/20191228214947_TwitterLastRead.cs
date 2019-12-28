using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DANotify.Data.Migrations
{
    public partial class TwitterLastRead : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "TumblrLastRead",
                table: "UserReadMarkers",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "TwitterLastRead",
                table: "UserReadMarkers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TumblrLastRead",
                table: "UserReadMarkers");

            migrationBuilder.DropColumn(
                name: "TwitterLastRead",
                table: "UserReadMarkers");
        }
    }
}
