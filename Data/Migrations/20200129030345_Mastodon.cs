using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ArtworkInbox.Data.Migrations
{
    public partial class Mastodon : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "BotsinSpaceLastRead",
                table: "UserReadMarkers",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "MastodonTechnologyLastRead",
                table: "UserReadMarkers",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserBotsinSpaceTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    AccessToken = table.Column<string>(type: "varchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserBotsinSpaceTokens", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_UserBotsinSpaceTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserMastodonTechnologyTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    AccessToken = table.Column<string>(type: "varchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMastodonTechnologyTokens", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_UserMastodonTechnologyTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserBotsinSpaceTokens");

            migrationBuilder.DropTable(
                name: "UserMastodonTechnologyTokens");

            migrationBuilder.DropColumn(
                name: "BotsinSpaceLastRead",
                table: "UserReadMarkers");

            migrationBuilder.DropColumn(
                name: "MastodonTechnologyLastRead",
                table: "UserReadMarkers");
        }
    }
}
