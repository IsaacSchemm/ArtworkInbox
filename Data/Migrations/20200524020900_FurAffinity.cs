using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ArtworkInbox.Data.Migrations
{
    public partial class FurAffinity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserFurAffinityTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    FA_COOKIE = table.Column<string>(nullable: false),
                    LastRead = table.Column<DateTimeOffset>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFurAffinityTokens", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_UserFurAffinityTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserFurAffinityTokens");
        }
    }
}
