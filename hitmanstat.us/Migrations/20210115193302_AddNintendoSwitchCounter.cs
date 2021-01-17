using Microsoft.EntityFrameworkCore.Migrations;

namespace hitmanstat.us.Migrations
{
    public partial class AddNintendoSwitchCounter : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "H3sw",
                table: "UserReportCounter",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "H3sw",
                table: "UserReportCounter");
        }
    }
}
