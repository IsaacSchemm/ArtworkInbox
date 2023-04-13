using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArtworkInbox.Data.Migrations
{
    /// <inheritdoc />
    public partial class YouTube : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserInoreaderTokens");

            migrationBuilder.CreateTable(
                name: "UserYouTubeTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AccessToken = table.Column<string>(type: "varchar(max)", nullable: true),
                    RefreshToken = table.Column<string>(type: "varchar(max)", nullable: true),
                    LastRead = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserYouTubeTokens", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_UserYouTubeTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserYouTubeTokens");

            migrationBuilder.CreateTable(
                name: "UserInoreaderTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AccessToken = table.Column<string>(type: "varchar(max)", nullable: true),
                    AllAsText = table.Column<bool>(type: "bit", nullable: false),
                    LastRead = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RefreshToken = table.Column<string>(type: "varchar(max)", nullable: true),
                    UnreadOnly = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserInoreaderTokens", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_UserInoreaderTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }
    }
}
