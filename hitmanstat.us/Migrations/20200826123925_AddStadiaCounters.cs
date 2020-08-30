using Microsoft.EntityFrameworkCore.Migrations;

namespace hitmanstat.us.Migrations
{
    public partial class AddStadiaCounters : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "H1st",
                table: "UserReportCounter",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "H2st",
                table: "UserReportCounter",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "H1st",
                table: "UserReportCounter");

            migrationBuilder.DropColumn(
                name: "H2st",
                table: "UserReportCounter");
        }
    }
}
