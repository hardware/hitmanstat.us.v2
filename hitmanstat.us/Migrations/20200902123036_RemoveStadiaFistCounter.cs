using Microsoft.EntityFrameworkCore.Migrations;

namespace hitmanstat.us.Migrations
{
    public partial class RemoveStadiaFistCounter : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "H1st",
                table: "UserReportCounter");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "H1st",
                table: "UserReportCounter",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
