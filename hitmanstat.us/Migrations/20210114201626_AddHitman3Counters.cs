using Microsoft.EntityFrameworkCore.Migrations;

namespace hitmanstat.us.Migrations
{
    public partial class AddHitman3Counters : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "H3pc",
                table: "UserReportCounter",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "H3ps",
                table: "UserReportCounter",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "H3st",
                table: "UserReportCounter",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "H3xb",
                table: "UserReportCounter",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "H3pc",
                table: "UserReportCounter");

            migrationBuilder.DropColumn(
                name: "H3ps",
                table: "UserReportCounter");

            migrationBuilder.DropColumn(
                name: "H3st",
                table: "UserReportCounter");

            migrationBuilder.DropColumn(
                name: "H3xb",
                table: "UserReportCounter");
        }
    }
}
