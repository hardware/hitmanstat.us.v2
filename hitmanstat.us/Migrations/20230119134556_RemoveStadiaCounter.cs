using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace hitmanstat.us.Migrations
{
    /// <inheritdoc />
    public partial class RemoveStadiaCounter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "H3st",
                table: "UserReportCounter");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "H3st",
                table: "UserReportCounter",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
