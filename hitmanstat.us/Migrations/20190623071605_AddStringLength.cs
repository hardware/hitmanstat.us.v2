using Microsoft.EntityFrameworkCore.Migrations;

namespace hitmanstat.us.Migrations
{
    public partial class AddStringLength : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Fingerprint",
                table: "UserReport",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(string));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Fingerprint",
                table: "UserReport",
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 32);
        }
    }
}
