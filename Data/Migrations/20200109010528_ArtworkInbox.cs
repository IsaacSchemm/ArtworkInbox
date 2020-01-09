using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ArtworkInbox.Data.Migrations
{
    public partial class ArtworkInbox : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HideMature",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HideMatureThumbnails",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "HideReposts",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "UserDeviantArtTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    AccessToken = table.Column<string>(type: "varchar(max)", nullable: true),
                    RefreshToken = table.Column<string>(type: "varchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDeviantArtTokens", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_UserDeviantArtTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserReadMarkers",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    DeviantArtLastRead = table.Column<DateTimeOffset>(nullable: true),
                    TwitterLastRead = table.Column<DateTimeOffset>(nullable: true),
                    TumblrLastRead = table.Column<DateTimeOffset>(nullable: true)
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

            migrationBuilder.CreateTable(
                name: "UserTwitterTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    AccessToken = table.Column<string>(type: "varchar(max)", nullable: true),
                    AccessTokenSecret = table.Column<string>(type: "varchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTwitterTokens", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_UserTwitterTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserDeviantArtTokens");

            migrationBuilder.DropTable(
                name: "UserReadMarkers");

            migrationBuilder.DropTable(
                name: "UserTwitterTokens");

            migrationBuilder.DropColumn(
                name: "HideMature",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "HideMatureThumbnails",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "HideReposts",
                table: "AspNetUsers");
        }
    }
}
