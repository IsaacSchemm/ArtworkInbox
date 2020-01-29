using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ArtworkInbox.Data.Migrations
{
    public partial class Mastodon : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "MastodonLastRead",
                table: "UserReadMarkers",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserMastodonTokens",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Hostname = table.Column<string>(nullable: false),
                    UserId = table.Column<string>(nullable: false),
                    AccessToken = table.Column<string>(type: "varchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMastodonTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserMastodonTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserMastodonTokens_UserId",
                table: "UserMastodonTokens",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserMastodonTokens");

            migrationBuilder.DropColumn(
                name: "MastodonLastRead",
                table: "UserReadMarkers");
        }
    }
}
