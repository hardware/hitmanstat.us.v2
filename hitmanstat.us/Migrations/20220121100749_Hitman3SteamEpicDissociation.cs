using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace hitmanstat.us.Migrations
{
    public partial class Hitman3SteamEpicDissociation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "H3pc",
                table: "UserReportCounter",
                newName: "H3steam");

            migrationBuilder.AddColumn<int>(
                name: "H3epic",
                table: "UserReportCounter",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "H3epic",
                table: "UserReportCounter");

            migrationBuilder.RenameColumn(
                name: "H3steam",
                table: "UserReportCounter",
                newName: "H3pc");
        }
    }
}
