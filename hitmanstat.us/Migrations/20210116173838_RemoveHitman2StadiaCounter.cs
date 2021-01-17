using Microsoft.EntityFrameworkCore.Migrations;

namespace hitmanstat.us.Migrations
{
    public partial class RemoveHitman2StadiaCounter : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "H2st",
                table: "UserReportCounter");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "H2st",
                table: "UserReportCounter",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
