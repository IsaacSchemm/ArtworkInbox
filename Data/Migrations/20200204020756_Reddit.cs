using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ArtworkInbox.Data.Migrations
{
    public partial class Reddit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserBotsinSpaceTokens");

            migrationBuilder.DropTable(
                name: "UserInkbunnyTokens");

            migrationBuilder.DropTable(
                name: "UserReadMarkers");

            migrationBuilder.CreateTable(
                name: "UserRedditTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    AccessToken = table.Column<string>(type: "varchar(max)", nullable: true),
                    RefreshToken = table.Column<string>(type: "varchar(max)", nullable: true),
                    LastRead = table.Column<DateTimeOffset>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRedditTokens", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_UserRedditTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserRedditTokens");

            migrationBuilder.CreateTable(
                name: "UserBotsinSpaceTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
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
                name: "UserInkbunnyTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LastRead = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Sid = table.Column<string>(type: "varchar(max)", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "UserReadMarkers",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BotsinSpaceLastRead = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeviantArtLastRead = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    InkbunnyLastRead = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    TumblrLastRead = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    TwitterLastRead = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    WeasylLastRead = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
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
    }
}
