using Microsoft.EntityFrameworkCore.Migrations;

namespace ArtworkInbox.Data.Migrations
{
    public partial class TwitterTokens : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "RefreshToken",
                table: "UserDeviantArtTokens",
                type: "varchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "char(40)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AccessToken",
                table: "UserDeviantArtTokens",
                type: "varchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "char(50)",
                oldNullable: true);

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
                name: "UserTwitterTokens");

            migrationBuilder.AlterColumn<string>(
                name: "RefreshToken",
                table: "UserDeviantArtTokens",
                type: "char(40)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AccessToken",
                table: "UserDeviantArtTokens",
                type: "char(50)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(max)",
                oldNullable: true);
        }
    }
}
