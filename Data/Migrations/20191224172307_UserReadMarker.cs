using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DANotify.Data.Migrations
{
    public partial class UserReadMarker : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserReadMarkers",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    DeviantArtLastRead = table.Column<DateTimeOffset>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserReadMarkers", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_UserReadMarkers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserReadMarkers");
        }
    }
}
