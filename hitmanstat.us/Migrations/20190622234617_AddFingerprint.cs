using Microsoft.EntityFrameworkCore.Migrations;

namespace hitmanstat.us.Migrations
{
    public partial class AddFingerprint : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Fingerprint",
                table: "UserReport",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Fingerprint",
                table: "UserReport");
        }
    }
}
