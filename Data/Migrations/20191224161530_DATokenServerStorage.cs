using Microsoft.EntityFrameworkCore.Migrations;

namespace DANotify.Data.Migrations
{
    public partial class DATokenServerStorage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserDeviantArtTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    AccessToken = table.Column<string>(type: "char(50)", nullable: true),
                    RefreshToken = table.Column<string>(type: "char(40)", nullable: true)
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserDeviantArtTokens");
        }
    }
}
