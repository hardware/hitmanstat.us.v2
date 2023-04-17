using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace hitmanstat.us.Migrations
{
    /// <inheritdoc />
    public partial class RemoveOldServices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "H1pc",
                table: "UserReportCounter");

            migrationBuilder.DropColumn(
                name: "H1ps",
                table: "UserReportCounter");

            migrationBuilder.DropColumn(
                name: "H1xb",
                table: "UserReportCounter");

            migrationBuilder.DropColumn(
                name: "H2pc",
                table: "UserReportCounter");

            migrationBuilder.DropColumn(
                name: "H2ps",
                table: "UserReportCounter");

            migrationBuilder.DropColumn(
                name: "H2xb",
                table: "UserReportCounter");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "H1pc",
                table: "UserReportCounter",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "H1ps",
                table: "UserReportCounter",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "H1xb",
                table: "UserReportCounter",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "H2pc",
                table: "UserReportCounter",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "H2ps",
                table: "UserReportCounter",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "H2xb",
                table: "UserReportCounter",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
