using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace hitmanstat.us.Migrations
{
    public partial class AddUserReportCounter : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserReportCounter",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    H1pc = table.Column<int>(nullable: false, defaultValue: 0),
                    H1xb = table.Column<int>(nullable: false, defaultValue: 0),
                    H1ps = table.Column<int>(nullable: false, defaultValue: 0),
                    H2pc = table.Column<int>(nullable: false, defaultValue: 0),
                    H2xb = table.Column<int>(nullable: false, defaultValue: 0),
                    H2ps = table.Column<int>(nullable: false, defaultValue: 0),
                    Date = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserReportCounter", x => x.ID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserReportCounter");
        }
    }
}
