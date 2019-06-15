using Microsoft.EntityFrameworkCore.Migrations;

namespace hitmanstat.us.Migrations
{
    public partial class AddMessageAndStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Service",
                table: "Event",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Message",
                table: "Event",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "Event",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Message",
                table: "Event");

            migrationBuilder.DropColumn(
                name: "State",
                table: "Event");

            migrationBuilder.AlterColumn<string>(
                name: "Service",
                table: "Event",
                nullable: true,
                oldClrType: typeof(string));
        }
    }
}
