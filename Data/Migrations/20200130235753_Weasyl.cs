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
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserWeasylTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    ApiKey = table.Column<string>(type: "varchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserWeasylTokens", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_UserWeasylTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserWeasylTokens");

            migrationBuilder.DropColumn(
                name: "WeasylLastRead",
                table: "UserReadMarkers");
        }
    }
}
