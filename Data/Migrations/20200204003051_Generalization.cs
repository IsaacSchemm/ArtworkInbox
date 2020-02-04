using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ArtworkInbox.Data.Migrations
{
    public partial class Generalization : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastRead",
                table: "UserWeasylTokens",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastRead",
                table: "UserTwitterTokens",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastRead",
                table: "UserTumblrTokens",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastRead",
                table: "UserInkbunnyTokens",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastRead",
                table: "UserDeviantArtTokens",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserMastodonTokens",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(nullable: false),
                    Host = table.Column<string>(nullable: false),
                    AccessToken = table.Column<string>(type: "varchar(max)", nullable: true),
                    LastRead = table.Column<DateTimeOffset>(nullable: true)
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
                name: "LastRead",
                table: "UserWeasylTokens");

            migrationBuilder.DropColumn(
                name: "LastRead",
                table: "UserTwitterTokens");

            migrationBuilder.DropColumn(
                name: "LastRead",
                table: "UserTumblrTokens");

            migrationBuilder.DropColumn(
                name: "LastRead",
                table: "UserInkbunnyTokens");

            migrationBuilder.DropColumn(
                name: "LastRead",
                table: "UserDeviantArtTokens");
        }
    }
}
